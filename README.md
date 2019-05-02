### Redis Session Buildpack

This is a supply buildpack that will detect the bounded redis service instance and modifies the `web.config` with the `sessionState` and `machineKey` sections.
- Any existing `sessionState` section(s) will be replaced with a custom one with valid connection string
- Any existing `machineKey` section(s) will be replaced with new validation and decryption keys

### Benefits of using this buildpack
- No code change required to persist session to redis, when pushing any ASP.NET application to PCF 
- So it reduces the effort in lifting and shifting a legacy ASP.NET application to PCF

### Pre-requisites
- PCF environment with redis tile in market place
- A redis service instance created
- `cf push` access to the PCF enviromnment

### Usage Instructions

To enable redis backed session in the application, please follow the below steps.
- Install nuget package `Microsoft.Web.RedisSessionStateProvider` in your application, preferrably the latest one.
- Add the buildpack in your application `manifest.yml` as in the example below. You can pick the latest release from https://github.com/alfusinigoj/redis-session-aspnet-buildpack/releases

```yaml
---
applications:
- name: redis-buildpack-sample
  memory: 1024M
  stack: windows2016
  buildpacks:
   - https://github.com/alfusinigoj/redis-session-aspnet-buildpack/releases/download/1.0.0/redis-session-buildpack-win-x64.zip
   - https://github.com/cloudfoundry/hwc-buildpack/releases/download/v3.1.8/hwc-buildpack-windows2016-v3.1.8.zip
  services:
   - my_redis_service
```
- To bind the application to the redis service to the application `manifest.yml` as above
- Push the application to PCF, you will be seeing logs as below

```text
=================== Redis Session Buildpack execution started ==================
================================================================================
-----> Removing existing machineKey configuration...
-----> Creating machineKey section with new validation, decryption keys and SHA1 validation...
-----> Removing existing session configurations...
-----> Found redis connection 'xxxxxxxxxxxxxxx,password=xxxxxxxxxxxxxxxxxxx,allowAdmin=false,abortConnect=true,resolveDns=false,ssl=false'
================================================================================
=================== Redis Session Buildpack execution completed ================
```

*If you come across any issues, kindly raise an issue at https://github.com/alfusinigoj/redis-session-aspnet-buildpack/issues. You are also welcome to contribute through pull requests.*