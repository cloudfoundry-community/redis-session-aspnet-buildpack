using System;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitHub;
using Nuke.Common.Tools.NuGet;
using Nuke.Common.Utilities.Collections;
using Octokit;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using FileMode = System.IO.FileMode;
using ZipFile = System.IO.Compression.ZipFile;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public enum StackType
    {
        Windows,
        Linux
    }
    public static int Main() => Execute<Build>(x => x.Publish);
    const string BuildpackProjectName = "Pivotal.Redis.Aspnet.Session.Buildpack";
    const string MajorMinorPatch = "1.0.7"; //todo: another way to get version from git?
    string PackageZipName => $"{BuildpackProjectName}-{Runtime}-{MajorMinorPatch}.zip";

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Target CF stack type - 'windows' or 'linux'. Determines buildpack runtime (Framework or Core). Default is 'windows'")]
    readonly StackType Stack = StackType.Windows;

    [Parameter("GitHub personal access token with access to the repo")]
    string GitHubToken;

    [Parameter("If this release should be marked as a pre-release")]
    bool IsPreRelease = false;

    [Parameter("Build Version Number")]
    readonly string BuildVersion = string.Empty;

    string Runtime => Stack == StackType.Windows ? "win-x64" : "linux-x64";
    // string Framework => Stack == StackType.Windows ? "net47" : "netcoreapp3.1";
    string Framework => "netcoreapp3.1";


    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath TestsDirectory => RootDirectory / "tests";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath VersionFile => RootDirectory / "release.version";

    Target Clean => _ => _
        .Description("Cleans up **/bin and **/obj folders")
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteFile);
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteFile);
            EnsureCleanDirectory(ArtifactsDirectory);
        });

    Target SetPackageZipName => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            if (!GitRepository.IsGitHubRepository())
                throw new Exception("SetPackageZipName supported when this is in a git repository");

            Logger.Log(LogLevel.Normal, $"Updating version file {VersionFile} with name {PackageZipName}");

            File.WriteAllText(VersionFile, $"{PackageZipName}");
        });

    Target Restore => _ => _
        .Description("Restores NuGet dependencies for the buildpack")
        .DependsOn(SetPackageZipName)
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution)
                .SetRuntime(Runtime));
        });

    Target Compile => _ => _
        .Description("Compiles the buildpack")
        .DependsOn(Restore)
        .Executes(() =>
        {
            Logger.Info(Stack);
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetFramework(Framework)
                .SetRuntime(Runtime)
                .EnableNoRestore());
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetConfiguration(Configuration)
                .SetNoBuild(IsLocalBuild)
                .ResetVerbosity()
                .SetFramework(Framework)
                .SetRuntime(Runtime)
                .CombineWith(
                    Solution.GetProjects("*Tests"), (cs, v) => cs
                        .SetProjectFile(v)));
        });

    Target Publish => _ => _
        .Description("Packages buildpack in Cloud Foundry expected format into /artifacts directory")
        .DependsOn(Test)
        .Executes(() =>
        {
            DotNetPublish(s => s
                .SetProject(Solution)
                .SetConfiguration(Configuration)
                .SetFramework(Framework)
                .SetRuntime(Runtime)
                .EnableNoRestore());

            var workDirectory = TemporaryDirectory / "pack";
            EnsureCleanDirectory(TemporaryDirectory);
            var buildpackProject = Solution.GetProject(BuildpackProjectName);
            var publishDirectory = buildpackProject.Directory / "bin" / Configuration / Framework / Runtime / "publish";
            var workBinDirectory = workDirectory / "bin";
            var scriptsDirectory = RootDirectory / "scripts";

            CopyDirectoryRecursively(publishDirectory, workBinDirectory, DirectoryExistsPolicy.Merge);
            CopyDirectoryRecursively(scriptsDirectory, workBinDirectory, DirectoryExistsPolicy.Merge);

            var tempZipFile = TemporaryDirectory / PackageZipName;

            ZipFile.CreateFromDirectory(workDirectory, tempZipFile);
            MakeFilesInZipUnixExecutable(tempZipFile);
            CopyFileToDirectory(tempZipFile, ArtifactsDirectory, FileExistsPolicy.Overwrite);
            Logger.Block(ArtifactsDirectory / PackageZipName);

        });


    Target Release => _ => _
        .Description("Creates a GitHub release (or ammends existing) and uploads buildpack artifact")
        .Requires(() => GitHubToken)
        .Requires(() => BuildVersion)
        .Executes(async () =>
        {
            if (!GitRepository.IsGitHubRepository())
                throw new Exception("Only supported when git repo remote is github");

            var client = new GitHubClient(new ProductHeaderValue(BuildpackProjectName))
            {
                Credentials = new Credentials(GitHubToken, AuthenticationType.Bearer)
            };

            Logger.Log(LogLevel.Normal, $"Releasing in Github {client.BaseAddress}");

            var gitIdParts = GitRepository.Identifier.Split("/");
            var owner = gitIdParts[0];
            var repoName = gitIdParts[1];

            var packageFileNamewithoutExtension = Path.GetFileNameWithoutExtension(GetPackageZipNameFromVersionFile());
            var majorMinorPatch = packageFileNamewithoutExtension.Split('-')[3];

            var releaseName = IsPreRelease ? $"v{majorMinorPatch}-prerelease" : $"v{majorMinorPatch}";

            Release release;
            try
            {
                Logger.Log(LogLevel.Normal, $"Checking for existence of release with name {releaseName}...");
                release = await client.Repository.Release.Get(owner, repoName, releaseName);
                Logger.Log(LogLevel.Normal, $"Found release {releaseName} at {release.AssetsUrl}");
            }
            catch (Exception)
            {
                Logger.Log(LogLevel.Normal, $"Release with name {releaseName} not found.. so creating new...");

                var newRelease = new NewRelease(releaseName)
                {
                    Name = releaseName,
                    Draft = false,
                    Prerelease = IsPreRelease,
                    Body = $"Build Version: {(string.IsNullOrWhiteSpace(BuildVersion) ? "Unknown" : BuildVersion)}"
                };
                release = await client.Repository.Release.Create(owner, repoName, newRelease);
            }

            var targetPackageName = IsPreRelease ? $"{packageFileNamewithoutExtension}-prerelease.zip" : GetPackageZipNameFromVersionFile();

            var existingAsset = release.Assets.FirstOrDefault(x => x.Name == targetPackageName);
            if (existingAsset != null)
            {
                Logger.Log(LogLevel.Normal, $"Deleting assert {existingAsset.Name}...");
                await client.Repository.Release.DeleteAsset(owner, repoName, existingAsset.Id);
            }

            var zipPackageLocation = ArtifactsDirectory / GetPackageZipNameFromVersionFile();
            var targetZipPackageLocation = ArtifactsDirectory / targetPackageName;

            if (string.Compare(zipPackageLocation, targetZipPackageLocation) != 0)
                File.Copy(zipPackageLocation, targetZipPackageLocation, true);

            var releaseAssetUpload = new ReleaseAssetUpload(targetPackageName, "application/zip", File.OpenRead(targetZipPackageLocation), null);

            Logger.Log(LogLevel.Normal, $"Uploading assert {releaseAssetUpload.FileName}...");

            var releaseAsset = await client.Repository.Release.UploadAsset(release, releaseAssetUpload);

            Logger.Block(releaseAsset.BrowserDownloadUrl);

            Logger.Log(LogLevel.Normal, $"Released in Github {client.BaseAddress}, successfully");
        });

    public static void MakeFilesInZipUnixExecutable(AbsolutePath zipFile)
    {
        var tmpFileName = zipFile + ".tmp";
        using (var input = new ZipInputStream(File.Open(zipFile, FileMode.Open)))
        using (var output = new ZipOutputStream(File.Open(tmpFileName, FileMode.Create)))
        {
            output.SetLevel(9);
            ZipEntry entry;

            while ((entry = input.GetNextEntry()) != null)
            {
                var outEntry = new ZipEntry(entry.Name);
                outEntry.HostSystem = (int)HostSystemID.Unix;
                outEntry.ExternalFileAttributes = -2115174400;
                output.PutNextEntry(outEntry);
                input.CopyTo(output);
            }
            output.Finish();
            output.Flush();
        }

        DeleteFile(zipFile);
        RenameFile(tmpFileName, zipFile, FileExistsPolicy.Overwrite);
    }

    public string GetPackageZipNameFromVersionFile()
    {
        if (!File.Exists(VersionFile))
            throw new FileNotFoundException(VersionFile);

        return File.ReadAllText(VersionFile);
    }
}
