# AllLive.UWP

运行项目需要添加FFmpeg引用

1、支持Https:

下载编译好的FFmpeg

https://xiaoyaocz.lanzoui.com/iozmnpl46wd

修改AllLive.UWP.csproj里的FFmpeg路径

```
<ItemGroup>
   <Content Include="$(SolutionDir)\FFmpegBuild\$(PlatformTarget)\bin\*.dll" />
</ItemGroup>
```

2、不支持Https

直接Nuget添加FFmpegInteropX.FFmpegUWP包

https://www.nuget.org/packages/FFmpegInteropX.FFmpegUWP