# Astrolabe

File manager toolkit module for ShuRuk — folder icon custom, file notes, history versioning, incremental snapshots, batch rename, quick preview.

ShuRuk 的文件管理工具集模块——文件夹图标自定义、文件备注、历史版本追踪、增量快照、批量重命名、快速预览。

## Structure / 结构

```
Astrolabe/
├─ .github/workflows/release.yml      # CI：构建 MSIX + 便携 exe
├─ build/                             # 构建变体配置
├─ src/
│  ├─ Astrolabe.App/                  # 模块主程序（WinUI3 MSIX）
│  ├─ Astrolabe.Core/                 # 模块级公共逻辑
│  ├─ Astrolabe.FolderPalette/        # 轻量子模块（文件夹图标自定义）
│  │  ├─ Astrolabe.FolderPalette.Core/        # 业务逻辑
│  │  └─ Astrolabe.FolderPalette.PortableExe/  # 便携版
│  └─ Astrolabe.WinRTExtension/       # CsWinRT 扩展组件
├─ ShuRuk.module.json                 # 模块元数据清单
└─ README.md
```

## Submodule / 子模块

- **FolderPalette**（轻量）：文件夹图标自定义，独立小窗口 + 右键菜单唤起，提供便携版。

## Prerequisites / 前置条件

引用 ShuRuk.Runtime 需配置 GitHub Packages 认证（一次性）：

1. 创建 GitHub Classic PAT（https://github.com/settings/tokens），勾选 `read:packages` scope
2. 运行以下命令，将认证写入全局 NuGet 配置：
   ```powershell
   dotnet nuget add source "https://nuget.pkg.github.com/FSYShu/index.json" --name github --username FSYShu --password ghp_yourPAT --store-password-in-clear-text
   ```

## Build / 构建

```powershell
# MSIX
dotnet build src/Astrolabe.App/Astrolabe.App.csproj -c Release

# 便携版
dotnet publish src/Astrolabe.FolderPalette/Astrolabe.FolderPalette.PortableExe/Astrolabe.FolderPalette.PortableExe.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```
## Run / 运行

运行 MSIX 主程序：

```powershell
winapp run src/Astrolabe.App
```

运行便携版（非打包，也可直接 `dotnet run`）：

```powershell
winapp run src/Astrolabe.FolderPalette/Astrolabe.FolderPalette.PortableExe
```

