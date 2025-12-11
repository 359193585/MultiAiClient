# 🤖 AI 聊天集成工具 (C# / WPF / WebView2)

这是一个将多个 **Web 端 AI 聊天服务** 集成到统一界面的工具。它旨在提升您与多个 AI 同时交互的效率，并通过统一的输入和分析功能来优化您的提示词工程（Prompt Engineering）流程。

## ✨ 核心功能

* **多页签并行访问**：
    * 允许用户在多个页签中并行打开不同的 AI 聊天窗口。
* **统一输入与一键群发**：
    * 提供一个统一的输入框，用于编写提示词。
    * 支持**一键发送**，将相同的提示词同时发送至所有打开的 AI 聊天服务。
* **效率与分析工具**：
    * **快速定位**：支持快速向上或向下定位历史提问。
    * **数据统计**：自动统计您的提问次数。
    * **数据导出**：支持导出所有问题和与 AI 的互动过程。
    * **提示词优化**：您可以根据导出的提示词，改进和优化提示。

## 🛠️ 技术实现

本项目采用以下技术栈开发：

* **主框架**：C# & WPF (Windows Presentation Foundation)
* **Web 视图**：使用 **WebView2** 嵌入 Web 端的 AI 聊天页面。
* **页面交互**：通过 **JavaScript (JS) 注入** 方式，实现对网页元素的自动化操作和交互。

---

## ⚙️ AI 聊天服务配置

AI 服务连接通过配置文件 `config.json` 进行管理和设置。您可以添加或修改常用的 AI 聊天工具连接。

配置文件使用标准 **JSON 格式**，以下是默认配置示例：

```json
[
  {
    "name": "腾讯元宝",
    "url": "[https://yuanbao.tencent.com/chat/](https://yuanbao.tencent.com/chat/)",
    "dataFolderName": "Data_tencent.yuanbao",
    "iconPath": "Icons/yuanbao_logo.png"
  },
  {
    "name": "ChatGPT",
    "url": "[https://chatgpt.com/](https://chatgpt.com/)",
    "dataFolderName": "Data_ChatGPT",
    "iconPath": "Icons/chartgpt_logo.png"
  },
  {
    "name": "DeepSeek AI",
    "url": "[https://chat.deepseek.com/](https://chat.deepseek.com/)",
    "dataFolderName": "Data_Deepseek",
    "iconPath": "Icons/deepseek_logo.png"
  },
  {
    "name": "Gemini AI",
    "url": "[https://gemini.google.com/app](https://gemini.google.com/app)",
    "dataFolderName": "Data_Gemini",
    "iconPath": "Icons/gemini_logo.png"
  },
  {
    "name": "腾讯元器智能体",
    "url": "[https://yuanqi.tencent.com/my-creation/agent](https://yuanqi.tencent.com/my-creation/agent)",
    "dataFolderName": "Data_tencent.yuanqi",
    "iconPath": "Icons/yuanqi_logo.png"
  }
]

```

# English Introduce
A tool that integrates multiple web-based AI chat services, allowing users to open multiple AI chat windows in parallel via multiple tabs. Users can enter their prompts in a unified input box and send them to all AIs with one click.
The program supports locating issues quickly, tracking the number of your questions, exporting all questions, analyzing the process of your interaction with AI, and optimizing your prompts.
Developed using C#, the WPF framework, and WebView2, the project enables page interaction through JS injection.

