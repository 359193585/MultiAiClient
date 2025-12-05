# 一个将多个web端访问的ai聊天进行集成的工具，可以多页签并行打开多个ai聊天窗口，在统一的输入框内输入你的提示词，一键发送至所有ai。
程序支持快速上下定位问题，统计你的提问次数，并可以支持导出全部问题，分析你和ai互动的过程，优化你的提示词。
项目使用c#和wpf框架，以及webview2编写，通过js注入方式实现页面交互。

## AI聊天服务配置
通过配置文件config.json,可以设置你自己常用的ai工具连接，默认设置了5个，使用json格式，内容如下：
> 
[
 {
    "name": "腾讯元宝",
    "url": "https://yuanbao.tencent.com/chat/",
    "dataFolderName": "Data_tencent.yuanbao",
    "iconPath": "Icons/yuanbao_logo.png"
  },
  {
    "name": "ChatGPT",
    "url": "https://chatgpt.com/",
    "dataFolderName": "Data_ChatGPT",
    "iconPath": "Icons/chartgpt_logo.png"
  },
  {
    "name": "DeepSeek AI",
    "url": "https://chat.deepseek.com/",
    "dataFolderName": "Data_Deepseek",
    "iconPath": "Icons/deepseek_logo.png"
  },
  {
    "name": "Gemini AI",
    "url": "https://gemini.google.com/app",
    "dataFolderName": "Data_Gemini",
    "iconPath": "Icons/gemini_logo.png"
  },
  {
    "name": "腾讯元器智能体",
    "url": "https://yuanqi.tencent.com/my-creation/agent",
    "dataFolderName": "Data_tencent.yuanqi",
    "iconPath": "Icons/yuanqi_logo.png"
  }
]


# A tool that integrates multiple web-based AI chat services, allowing users to open multiple AI chat windows in parallel via multiple tabs. Users can enter their prompts in a unified input box and send them to all AIs with one click.
The program supports locating issues quickly, tracking the number of your questions, exporting all questions, analyzing the process of your interaction with AI, and optimizing your prompts.
Developed using C#, the WPF framework, and WebView2, the project enables page interaction through JS injection.

