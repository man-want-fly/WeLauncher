# WeLauncher

WeLauncher 是一个简化的 Windows 原生 Launcher（C# + WPF），专注于核心功能：壳的自我更新、子程序的下载与运行、以及防止外部启动的保护机制。

## 三大核心功能

1.  **壳及壳本身的更新**
    *   壳（Shell）负责读取配置、展示应用列表。
    *   壳自身通过 MSIX / AppInstaller 机制或简单的 Zip 替换进行更新。

2.  **下载解压子程序并运行**
    *   根据 Manifest 下载应用 Zip 包。
    *   校验 SHA256 并解压到本地。
    *   通过 Wrapper 启动子程序。

3.  **Wrapper 防止在壳以外启动**
    *   每个子程序都配有一个 Wrapper。
    *   Wrapper 会检查 WeLauncher 是否在运行，防止用户直接运行子程序。
    *   Wrapper 自动定位并启动真正的应用程序。

## Manifest 格式

`manifest.json` 极其精简，去除了不必要的参数：

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
      "wrapperRelativePath": "wrapper/WrapperApp.exe"
    },
    {
      "id": "libX",
      "name": "依赖库X",
      "version": "2.0.1",
      "downloadUrl": "https://cdn.example.com/apps/libX/2.0.1/libX-2.0.1.zip",
      "sha256": "e2fc714c4727ee9395f324cd2e7f331f",
      "wrapperRelativePath": "wrapper/WrapperApp.exe"
    }
  ]
}
```

## 使用方法

### 1. 打包子程序 (Package Subprogram)

只需要将子程序拖入本地文件夹，一个文件夹代表一个子程序。
**推荐位置**：在项目根目录下创建一个 `apps` 文件夹，将你的子程序文件夹放入其中。例如：`WeLauncher/apps/MyApp/`。

使用 GitHub Actions **Package Subprogram**：
*   **输入**: 子程序所在的文件夹路径 (例如 `apps/MyApp`)。
*   **输入**: 输出文件名。
*   **输入**: (可选) 入口程序名称 (例如 `MyApp.exe`)。如果不填，Wrapper 会自动查找文件夹内的 `.exe`。
*   **输出**: 一个包含 `app/` (原程序) 和 `wrapper/` (保护壳) 的 Zip 包。

### 2. 打包壳 (Package Shell)

使用 GitHub Actions **Package Shell**：
*   **输入**: `manifest.json` 的内容（或者仓库内的路径）。
*   **输出**: 包含编译好的 WeLauncher 和 `manifest.json` 的 Zip 包。

### 3. 本地运行

1.  下载 **Shell Zip** 并解压。
2.  确保 `manifest.json` 配置正确（指向你的子程序下载地址）。
3.  运行 `WeLauncher.exe`。
4.  点击应用，Launcher 会自动下载、解压并运行。

## 目录结构

*   `src/WeLauncher`: WPF 壳源码。
*   `wrappers/MinimalWrapper`: 极简 Wrapper 源码。
*   `tooling/`: 打包脚本 (`package-subprogram.ps1`, `package-shell.ps1`)。
*   `.github/workflows/`: 自动化工作流。

## 开发与构建

*   **Wrapper**:
    *   逻辑：检查 `WeLauncher` 进程 -> 查找 `meta/app.json` 或 `app/*.exe` -> 启动。
*   **Shell**:
    *   逻辑：读取 `manifest.json` -> 下载 Zip -> 解压 -> 运行 `wrapper/WrapperApp.exe`。

无需复杂的 Token 验证或启动参数配置，一切保持简单有效。
