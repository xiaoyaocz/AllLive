# AllLive

获取各个网站的直播信息及弹幕。

支持以下网站：

- 虎牙直播

- 斗鱼直播

- 哔哩哔哩直播

## TODO:

- 企鹅电竞 [×]

- DanmakuHelper [×]

## AllLive.Core

项目核心，.NET Standard 2.0类库。用于获取各个网站的信息及弹幕实现。

## AllLive.ConsoleApp

基于AllLive.Core的控制台程序。

输入直播间链接获取信息及播放地址：

```
AllLive.ConsoleApp -i [URL]
```

输入直播间链接获取弹幕：

```
AllLive.ConsoleApp -d [URL]
```

## AllLive.UWP

UWP客户端，简单看直播。

[微软商店下载](https://www.microsoft.com/store/apps/9N1TWG2G84VD)

![UWP1](https://raw.githubusercontent.com/xiaoyaocz/AllLive/master/Screenshots/UWP1.png)


## AllLive.DanmakuHelper

弹幕助手，实时显示弹幕。

## 参考及引用
[https://github.com/wbt5/real-url](https://github.com/wbt5/real-url)

[https://github.com/lovelyyoshino/Bilibili-Live-API/blob/master/API.WebSocket.md](https://github.com/lovelyyoshino/Bilibili-Live-API/blob/master/API.WebSocket.md)

[https://github.com/IsoaSFlus/danmaku](https://github.com/IsoaSFlus/danmaku)

[https://www.cnblogs.com/sdflysha/p/20210117-douyu-barrage-with-dotnet.html](https://www.cnblogs.com/sdflysha/p/20210117-douyu-barrage-with-dotnet.html)

[https://github.com/BacooTang/huya-danmu](https://github.com/BacooTang/huya-danmu)

[https://github.com/TarsCloud/Tars](https://github.com/TarsCloud/Tars)