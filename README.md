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
- manifests/manifest.sample.json：manifest 示例
- packaging/WeLauncher.appinstaller：AppInstaller 示例

## 构建与发布
- 本地构建（Windows）：
  - dotnet build src/WeLauncher/WeLauncher.csproj -c Release
  - dotnet publish src/WeLauncher/WeLauncher.csproj -c Release -r win-x64
- GitHub Actions：
  - 推送到 build 分支后自动构建并上传 WeLauncher-win-x64.zip 工件

## 使用说明
- 启动壳，选择应用卡片进行“安装/启动”
- zip 包结构示例：
  - app/App.exe
  - wrapper/WrapperApp.exe
  - app/resources/...
  - meta/app.json

