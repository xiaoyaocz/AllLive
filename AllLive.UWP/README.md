# AllLive.UWP


UWP不支持QuickJS,打包Release时需要修改AllLive.Core项目的csproj文件:

```
<PackageReference Include="QuickJS.NET" Version="0.0.3" />
```

至

```
<PackageReference Include="QuickJS.NET" Version="0.0.3" PrivateAssets="all" />
```

修改AllLive.Core属性
添加Release条件编译符号：WINDOWS_UWP