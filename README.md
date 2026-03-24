# 小鹏 PDF 阅读器

一款基于 Avalonia UI 开发的跨平台 PDF 阅读器，支持 Windows、Linux 和 macOS。

## 功能特性

- **文件操作**：打开、关闭 PDF 文件，打印功能
- **视图控制**：缩放（放大/缩小）、适应宽度、适应页面、全屏模式
- **页面导航**：上一页/下一页、跳转指定页码
- **搜索功能**：在 PDF 文档中搜索文本，高亮显示搜索结果
- **书签管理**：添加、删除、跳转书签
- **大纲导航**：显示 PDF 文档大纲结构，快速定位章节
- **缩略图预览**：页面缩略图快速预览
- **最近文件**：记录并快速打开最近访问的 PDF 文件
- **主题切换**：支持浅色/深色主题切换

## 技术栈

- **框架**：Avalonia UI 11.2.0（跨平台 UI 框架）
- **语言**：C# / .NET 9.0
- **架构**：MVVM（使用 CommunityToolkit.Mvvm）
- **PDF 渲染**：Pdfium
- **日志**：Serilog

## 项目结构

```
XiaoPengPDF/
├── src/
│   ├── XiaoPengPDF/                    # 主应用程序
│   │   ├── Views/                      # 视图层
│   │   ├── ViewModels/                # 视图模型层
│   │   └── Styles/                    # 样式资源
│   ├── XiaoPengPDF.Core/              # 核心接口与模型
│   │   ├── Interfaces/                # 服务接口
│   │   └── Models/                    # 数据模型
│   ├── XiaoPengPDF.Infrastructure/     # 基础设施层
│   │   ├── Configuration/              # 配置
│   │   └── Logging/                   # 日志服务
│   ├── XiaoPengPDF.Pdfium/            # Pdfium 渲染实现
│   └── XiaoPengPDF.Services/          # 业务服务层
└── XiaoPengPDF.sln                    # 解决方案文件
```

## 快速开始

### 环境要求

- .NET 9.0 SDK 或更高版本

### 构建项目

```bash
cd src/XiaoPengPDF
dotnet build
```

### 运行项目

```bash
dotnet run
```

### 发布应用

```bash
dotnet publish -c Release -r win-x64 --self-contained
```

## 快捷键

| 快捷键 | 功能 |
|--------|------|
| Ctrl+O | 打开文件 |
| - | 缩小 |
| + | 放大 |

## 许可证

本项目仅供学习交流使用。
