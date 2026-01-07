# WeLauncher

Windows 原生 Launcher（C# + WPF），通过 manifest 驱动分发与更新，使用 MSIX（AppInstaller）进行壳的更新，统一 zip 分发应用与依赖，简化 Wrapper 校验。

## 功能
- 下载、校验并解压应用 zip（包含依赖与资源）
- 通过 Wrapper 验证后启动应用，仅允许从壳内启动
- 依赖隐藏展示，UI 网格仅显示可见应用
- 壳更新采用 MSIX（AppInstaller）

## 项目结构
- src/WeLauncher：WPF 壳（.NET 8）
- wrappers/MinimalWrapper：简化 Wrapper（控制台）
- tooling/wrappergen：通用 Wrapper 模板与自动打包脚本
- manifests/manifest.sample.json：manifest 示例
- packaging/WeLauncher.appinstaller：AppInstaller 示例

## 构建与发布
- 本地构建（Windows）：
  - dotnet build src/WeLauncher/WeLauncher.csproj -c Release
  - dotnet publish src/WeLauncher/WeLauncher.csproj -c Release -r win-x64
- GitHub Actions：
  - 推送到 build 分支后自动构建并上传 WeLauncher-win-x64.zip 工件

## Wrapper 自动生成与打包
- 输入：子程序可运行文件与依赖目录（PayloadDir）、版本号等
- 执行：
  - pwsh tooling/wrappergen/wrapper-make.ps1 -AppId appA -Version 1.0.0 -PayloadDir "C:\\path\\to\\appA" -EntryExe "App.exe" -Args @("--mode=prod") -Env @{ APP_ENV="prod" } -Name "示例应用A" -BaseUrl "https://cdn.example.com"
- 输出：
  - dist/apps/<appId>/<version>/<appId>-<version>.zip
  - zip 内含 app/、wrapper/WrapperApp.exe、meta/app.json
  - 控制台输出 SHA256 与生成 manifest 片段 dist/manifests/<appId>-<version>.json，直接可合并到服务端 manifest.json

示例 manifest 片段：

```json
{
  "id": "appA",
  "name": "示例应用A",
  "version": "1.0.0",
  "downloadUrl": "https://cdn.example.com/apps/appA/1.0.0/appA-1.0.0.zip",
  "sha256": "<自动计算>",
  "visible": true,
  "launchArgs": ["--mode=prod"],
  "env": { "APP_ENV": "prod" },
  "wrapperRelativePath": "wrapper/WrapperApp.exe"
}
```

## 使用说明
- 启动壳，选择应用卡片进行“安装/启动”
- zip 包结构示例：
  - app/App.exe
  - app/resources/...
  - app/config/...
  - wrapper/WrapperApp.exe
  - meta/app.json

## Zip 包内文件目录
- 顶层目录：
  - app/：主程序与资源
  - wrapper/：对应应用的 Wrapper 可执行文件（每个子程序都有自己的 Wrapper，随 zip 分发）
  - meta/：应用私有元信息
- 详细结构：
  - app/App.exe
  - app/resources/*
  - app/config/*
  - wrapper/WrapperApp.exe（通用实现，但每个应用包都会携带一份）
  - meta/app.json（包含入口、版本、图标、启动参数、环境变量）

## Manifest 文件目录
- 服务端目录建议：
  - /manifests/manifest.json（全局应用列表）
  - /apps/<appId>/<version>/<appId>-<version>.zip（应用包）
  - /shell/WeLauncher.appinstaller（壳的 AppInstaller）
  - /shell/WeLauncher_<version>_x64.msix（MSIX 包）
- 本地缓存目录（默认）：
  - %ProgramData%/WeLauncher/
    - manifest/manifest.json
    - apps/<appId>/versions/<version>/
    - cache/downloads/
    - launch/（壳与 Wrapper 令牌文件）
    - logs/

## 服务器文件目录（推荐）

```
cdn.example.com/
├─ manifests/
│  └─ manifest.json
├─ apps/
│  ├─ appA/
│  │  └─ 1.0.0/
│  │     └─ appA-1.0.0.zip
│  └─ libX/
│     └─ 2.0.1/
│        └─ libX-2.0.1.zip
├─ shell/
│  ├─ WeLauncher.appinstaller
│  └─ WeLauncher_1.0.0.0_x64.msix
└─ assets/            # 可选
   ├─ icons/
   │  └─ appA.png
   └─ checksums/
      └─ sha256.txt
```

- URL 约定：
  - Manifest: https://cdn.example.com/manifests/manifest.json
  - App 包：https://cdn.example.com/apps/<appId>/<version>/<appId>-<version>.zip
  - AppInstaller: https://cdn.example.com/shell/WeLauncher.appinstaller
  - MSIX 包：https://cdn.example.com/shell/WeLauncher_<version>_x64.msix
- 建议：
  - 使用 HTTPS 与缓存控制（ETag/Cache-Control）
  - 正确 MIME 类型（.appinstaller 为 application/xml 或 text/xml；.msix 为 application/vns.ms-appx）
  - 文件版本化路径，避免缓存污染

## 示例 manifest.json

```json
{
  "schemaVersion": 1,
  "apps": [
    {
      "id": "appA",
      "name": "示例应用A",
      "version": "1.0.0",
      "downloadUrl": "https://cdn.example.com/apps/appA/1.0.0/appA-1.0.0.zip",
      "sha256": "d41d8cd98f00b204e9800998ecf8427e",
      "visible": true,
      "launchArgs": ["--mode=prod"],
      "env": { "APP_ENV": "prod" },
      "wrapperRelativePath": "wrapper/WrapperApp.exe"
    },
    {
      "id": "libX",
      "name": "依赖库X",
      "version": "2.0.1",
      "downloadUrl": "https://cdn.example.com/apps/libX/2.0.1/libX-2.0.1.zip",
      "sha256": "e2fc714c4727ee9395f324cd2e7f331f",
      "visible": false,
      "launchArgs": [],
      "env": {},
      "wrapperRelativePath": "wrapper/WrapperApp.exe"
    }
  ]
}
```

## 示例 zip 包目录与 meta/app.json

目录结构：

```
app/
  App.exe
  resources/
    ...
  config/
    ...
wrapper/
  WrapperApp.exe
meta/
  app.json
```

meta/app.json 示例：

```json
{
  "EntryExe": "App.exe",
  "Version": "1.0.0",
  "Icon": "app/resources/icon.png"
}
```

## Wrapper 策略
- 每一个子程序都拥有一个独立的 Wrapper，可执行文件位于该子程序 zip 的 wrapper/WrapperApp.exe。
- Wrapper 的实现可通用复用，但随每个应用包自包含分发，避免与壳或其他应用产生版本耦合。
- 壳启动应用时，会根据 manifest 中的 wrapperRelativePath 在对应应用版本目录下定位 Wrapper 并传入令牌与启动规格。

## 项目根目录结构示例

```
WeLauncher/
├─ src/
│  └─ WeLauncher/
│     ├─ App.xaml
│     ├─ App.xaml.cs
│     ├─ MainWindow.xaml
│     ├─ MainWindow.xaml.cs
│     ├─ WeLauncher.csproj
│     ├─ ViewModels/
│     │  └─ MainViewModel.cs
│     ├─ Services/
│     │  ├─ AppInstallerService.cs
│     │  ├─ ConfigService.cs
│     │  ├─ DownloadService.cs
│     │  ├─ InstallService.cs
│     │  ├─ LaunchService.cs
│     │  └─ ZipService.cs
│     ├─ Models/
│     │  ├─ AppDescriptor.cs
│     │  ├─ Manifest.cs
│     │  ├─ LaunchSpec.cs
│     │  └─ MetaApp.cs
│     └─ Utils/
│        └─ RelayCommand.cs
├─ wrappers/
│  └─ MinimalWrapper/
│     ├─ MinimalWrapper.csproj
│     └─ Program.cs
├─ manifests/
│  └─ manifest.sample.json
├─ packaging/
│  └─ WeLauncher.appinstaller
├─ .github/
│  └─ workflows/
│     └─ build.yml
└─ README.md
```

## 从 Release 资产生成 zip（不提交 exe 到仓库）
- 上传你的 payload 压缩包到本仓库的 Release（例如 npp-1.0.0-payload.zip，内容即应用目录：npp.exe/DLL/plugins/…）
- 在 Actions 选择“Package App Zip From Release”，点击“Run workflow”，填写：
  - app_id、version、name、entry_exe、args、env_json、base_url、visible、release_tag、asset_name
- 工作流会：
  - 下载指定 Release 的资产并解压到临时目录
  - 编译通用 Wrapper，生成 wrapper/WrapperApp.exe
  - 生成 meta/app.json
  - 打包 zip 到 dist/apps/<appId>/<version>/<appId>-<version>.zip
  - 生成 manifest 片段 dist/manifests/<appId>-<version>.json
  - 上传两个工件可直接下载使用
