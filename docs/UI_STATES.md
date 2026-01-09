# Shell UI 状态展示 (UI States)

WeLauncher 的壳界面在不同阶段会展示不同的状态，以提供清晰的用户反馈。

## 1. 启动与下载 (Startup & Downloading)

当用户首次启动或有新版本更新时，壳会自动开始下载子程序包。

*   **状态描述**: 应用卡片显示“正在下载...”或进度条。
*   **视觉表现**:
    *   应用图标可能变暗或覆盖一层遮罩。
    *   显示下载进度百分比 (例如: `Downloading: 45%`)。
    *   此时点击卡片无效。

## 2. 解压中 (Unzipping)

下载完成后，壳会自动解压 Zip 包到指定目录。

*   **状态描述**: 应用卡片显示“正在解压...”或“正在安装...”。
*   **视觉表现**:
    *   显示旋转的加载动画 (Spinner) 或进度条。
    *   文本提示: `Installing...` / `Unzipping...`。
    *   此时点击卡片无效。

## 3. 就绪状态 (Ready / Grid View)

所有准备工作完成后，应用进入就绪状态，可以随时启动。

*   **布局**: Grid 网格布局。
*   **卡片样式**:
    *   **尺寸**: `64x64` 像素。
    *   **形状**: 圆角矩形 (Rounded Rectangle)。
    *   **内容**: 显示应用图标。
    *   **交互**:
        *   鼠标悬停 (Hover): 可能有轻微上浮或高亮效果。
        *   点击 (Click): 启动应用 (Launch)。

---

### 示意图 (Conceptual Wireframe)

```mermaid
graph TD
    subgraph Grid_Layout [Grid 布局 (64x64 Items)]
        A[App A<br/>(Ready)]
        B[App B<br/>(Downloading 50%)]
        C[App C<br/>(Unzipping...)]
        D[App D<br/>(Ready)]
    end
    
    style A fill:#4CAF50,stroke:#333,stroke-width:2px,rx:10,ry:10
    style B fill:#FFC107,stroke:#333,stroke-width:2px,rx:10,ry:10
    style C fill:#2196F3,stroke:#333,stroke-width:2px,rx:10,ry:10
    style D fill:#4CAF50,stroke:#333,stroke-width:2px,rx:10,ry:10
```
