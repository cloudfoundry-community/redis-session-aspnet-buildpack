### Redis Session Buildpack

This is a supply buildpack that will detect the bounded redis service instance and modifies the `web.config` with the `sessionState` and `machineKey` sections.
- Any existing `sessionState` section(s) will be replaced with a custom one with valid connection string
- Any existing `machineKey` section(s) will be replaced with new validation and decryption keys

### Benefits of using this buildpack
- No code change required to persist session to redis, when pushing any ASP.NET application to PCF 
- So it reduces the effort in lifting and shifting a legacy ASP.NET application to PCF

### Usage Instructions

To enable redis backed session in the application, please follow the below steps.
- Install nuget package `Microsoft.Web.RedisSessionStateProvider` in your application, preferrably the latest one.
- Add the buildpack in your application `manifest.yml` as in the example below. You can pick the latest release from https://github.com/alfusinigoj/redis-session-aspnet-buildpack/releases

```yaml
applications:
- name: my_sample_application_using_redis_session
  stack: windows2016
  buildpacks: 
    - https://github.com/alfusinigoj/redis-session-aspnet-buildpack/releases/download/1.0.0/redis-session-buildpack-win-x64.zip
    - hwc_buildpack
```

*If you come across any issues, kindly raise an issue at https://github.com/alfusinigoj/redis-session-aspnet-buildpack/issues. You are also welcome to contribute through pull requests.*