# SS.NuGet.Publish

Automate publishing a project's NuGet package during build process.  

[![Build status](https://ci.appveyor.com/api/projects/status/59u4d4pxlk4o4fhi/branch/master?svg=true)](https://ci.appveyor.com/project/SimplerSoftware/ss-nuget-publish/branch/master)

## Usage

To use the package, you need to add the [SS.NuGet.Publish](https://www.nuget.org/packages/SS.NuGet.Publish/) NuGet package to your project. Then configure accordingly in the .csproj file.

#### Project file properties
* **NuGetPublishVersion** *(Optional)*
  - Version of Nuget.exe to use. Defaults to 6.8.0
* **NuGetPublishPath** *(Optional)*
  - Path to cache the Nuget.exe locally. Defaults to *%UserProfile%\\.nuget\\{NuGetPublishVersion}\\*
* **NuGetPublishType** *(Optional)*
  - Either *remote* or *local*. Will try to auto detect if NuGetPublishLocation is a url (begins with http), defaults to local if NuGetPublishLocation is not a url.
* **NuGetPublishLocation**
  - Valid URL or path to publish the NuGet package to
* **NuGetPublishSkipDuplicate**
  - Set to true to add the `-SkipDuplicate` command line option for nuget push command.

## How it works

- If not already cached, it will download a copy of nuget.exe from https://dist.nuget.org/win-x86-commandline/v{NuGetPublishVersion}/nuget.exe into `NuGetPublishPath`
- Publish
  - If `NuGetPublishType = remote` it executes nuget.exe using *push* command to `NuGetPublishLocation` as the source
  - If `NuGetPublishType = local` it executes nuget.exe using *add* command to `NuGetPublishLocation` as the source
  
> **Note:**  
> If `NuGetPublishLocation` is not defined, no publish is attempted.  
> If `NuGetPublishType` is set to remote, then `NuGetPublishLocation` **must** be a valid URL to a NuGet repo.  
> If `NuGetPublishType` is set to local, then `NuGetPublishLocation` **must** be a valid local folder or UNC path.  
> If you're publishing to a remote site, like NuGet.org, you will need to set your API Key prior to publishing. Follow the [instructions here](https://docs.microsoft.com/en-us/nuget/reference/cli-reference/cli-ref-setapikey) to learn how to accomplish this.

## Examples
```xml
<PropertyGroup>
  <NuGetPublishType>remote</NuGetPublishType>
  <NuGetPublishLocation>https://api.nuget.org/v3/index.json</NuGetPublishLocation>
</PropertyGroup>
```
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup Condition="'$(Configuration)|$(Platform)'=='Debug|AnyCPU'">
    <NuGetPublishLocation>D:\references\packages</NuGetPublishLocation>
  </PropertyGroup>
  <PropertyGroup Condition="'$(Configuration)|$(Platform)'=='Release|AnyCPU'">
    <NuGetPublishType>remote</NuGetPublishType>
    <NuGetPublishLocation>https://api.nuget.org/v3/index.json</NuGetPublishLocation>
  </PropertyGroup>
</project>
```
