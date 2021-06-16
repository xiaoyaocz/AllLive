# AllLive.UWP


UWP不支持QuickJS,生成Release时需要修改AllLive.Core项目的csproj文件:

1、引用修改

```
<PackageReference Include="QuickJS.NET" Version="0.0.3" />
<!-- 修改为以下内容，或去除QuickJS.NET引用 -->
<PackageReference Include="QuickJS.NET" Version="0.0.3" PrivateAssets="all" />
```

2、添加条件编译符号：WINDOWS_UWP

```
<PropertyGroup Condition="'$(Configuration)|$(Platform)'=='Release|AnyCPU'">
   <DefineConstants>TRACE;WINDOWS_UWP</DefineConstants>
</PropertyGroup>
```
