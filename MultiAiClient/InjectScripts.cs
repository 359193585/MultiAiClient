using Microsoft.Web.WebView2.Wpf;
using System.Diagnostics;
using System.Text.Json;
using System.Windows.Controls.Primitives;

namespace MultiAIClient
{
    internal partial class InjectTextIntoInput
    {
        public static async Task InjectTextIntoInputBox(WebView2 targetWebView, string text)
        {
            string currentUrl = targetWebView.Source?.ToString().ToLower() ?? "";
            string cssSelector = GetSelectorByUrl(currentUrl);
            if (string.IsNullOrEmpty(cssSelector)) return;
            string[] candidateSelectors = new string[] {
        cssSelector, // 主选方案
        "div[contenteditable=\"true\"]", // 备选方案
        "textarea[contenteditable=\"true\"]" // 备选方案
    };
            // 使用JavaScript来定位输入框并设置值
            string script = $@"
        (function() {{
            const MAX_ATTEMPTS = 10;
            const RETRY_DELAY_MS = 300;
            let attempts = 0;

            function tryInject() {{
                const inputElement = document.querySelector('{cssSelector}');
                if (inputElement) {{
                    // 1. 聚焦
                    inputElement.focus();
                    // 2. 清除原内容
                    inputElement.innerHTML = ''; // 或 inputElement.value = ''
                    // 3. 设置新内容
                    inputElement.innerHTML = `{text}`;
                    // 4. 触发事件链
                    ['input', 'change', 'keydown', 'keyup'].forEach(eventType => {{
                        const event = new Event(eventType, {{ bubbles: true }});
                        inputElement.dispatchEvent(event);
                    }});
                    return true;
                }}

                attempts++;
                if (attempts < MAX_ATTEMPTS) {{
                    setTimeout(tryInject, RETRY_DELAY_MS);
                    return false;
                }} else {{
                    console.error('Element not found after retries.');
                    return false;
                }}
            }}

            return tryInject();
        }})();
    ";

            // 在WebView2中执行脚本
            try
            {
                // 执行脚本并等待结果
                var result = await targetWebView.CoreWebView2.ExecuteScriptAsync(script);

                // 检查结果是否为true（成功）
                if (result == "true")
                {
                    Debug.WriteLine($"成功注入文本到: {currentUrl}");
                }
                else
                {
                    Debug.WriteLine($"注入失败: {currentUrl}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"脚本执行错误: {ex.Message}");
            }
        }
       
        public static string ChatMessageNavi_Debug_Minified(string cssSelector = "")
        {
            if (string.IsNullOrEmpty(cssSelector)) { return cssSelector; }
            var selJson = JsonSerializer.Serialize(cssSelector);
            return @"
(function(){var w=window;w.__my_injected__=w.__my_injected__||{};if(w.__my_injected__.cn)return;var cn={};var s=##SEL##;var x=-1,m='0/0';function S(){return Array.from(document.querySelectorAll(s))}function u(i){var a=S();if(i<0||i>=a.length){x=-1;return false}var t=a[i];try{t.scrollIntoView({behavior:'smooth',block:'center'})}catch(e){}a.forEach(function(z){try{z.style.outline=''}catch(e){}});try{t.style.outline='2px solid #4A90E2'}catch(e){}x=i;m=(x+1)+'/'+a.length;return true}function p(){var a=S();if(!a.length)return; if(x===-1)x=a.length-1;else x=Math.max(0,x-1);return u(x)}function n(){var a=S();if(!a.length)return; if(x===-1)x=a.length-1;else x=Math.min(a.length-1,x+1);return u(x)}function g(){try{return JSON.stringify(S().map(function(q){return(q&&q.textContent)?q.textContent.trim():''}))}catch(e){return'[]'}}function c(){return m}function i(){return x}function r(){x=-1}cn.n=n;cn.p=p;cn.s=u;cn.g=g;cn.c=c;cn.i=i;cn.r=r;w.__my_injected__.cn=cn})();//# sourceURL=chatNavigator.js"
            .Replace("##SEL##", selJson);
        }


        public static string ChatMessageNavi_Debug(string cssSelector = "")
        {
            if ( string.IsNullOrEmpty( cssSelector ) ) { return cssSelector; }
            return @"


// ====== Chat Message Navigator ======

(function () {
    // 创建或复用全局容器（只用一个简单名字）
    window.__my_injected__ = window.__my_injected__ || {};

    // 如果已经注册同名模块则跳过（避免重复注入）
    if (window.__my_injected__.chatNavigator) return;

// 私有变量:选择器，当前索引
    const messagesSelector = `##cssSelector##`;
    let index = -1;
    let messageCount=0;

// 获取所有用户消息元素
function getUserMessages() {
    return Array.from(
        document.querySelectorAll(messagesSelector)
    );
}

// 1. 滚动到指定 index
function scrollToMessage(i) {
        const msgs = getUserMessages();
        if (i < 0 || i >= msgs.length){index=-1;return false;}
        const target = msgs[i];
        target.scrollIntoView({ behavior: 'smooth', block: 'center' });
        // 可视化高亮（先清理）
        msgs.forEach(m => m.style.outline = '');
        target.style.outline = '2px solid #4A90E2';
        index = i;
        messageCount= (index+1) + '/' + msgs.length;
        return true;
    }

// 2. 跳到上一个问题
    function goToPrevUserMessage() {
        const msgs = getUserMessages();
        if (!msgs.length) return;
        if (index === -1) index = msgs.length - 1;
        else index = Math.max(0, index - 1);
        scrollToMessage(index);
        return true;
    }


// 3. 跳到下一个问题
    function goToNextUserMessage() {
        const msgs = getUserMessages();
        if (!msgs.length) return;
        if (index === -1) index = msgs.length - 1;
        else index = Math.min(msgs.length - 1, index + 1);
        scrollToMessage(index);
        return true;
    }

function getMesgStr() {
    const msgs = getUserMessages();
    const messageContents = msgs.map(msg => msg.textContent.trim());
    return JSON.stringify(messageContents);
}

 // 暴露受控接口方法到共享命名空间
    window.__my_injected__.chatNavigator = {
        goToPrevUserMessage,
        goToNextUserMessage,
        messageCount: ()=> messageCount,
        // 可选：获取当前 index，用于 UI 同步
        getIndex: () => index,
        // 可选：重置或重扫描
        reset: () => { index = -1; },
        getMesgStr
    };
})();


// ====== End ======
//# sourceURL=chatNavigator.js


"
            .Replace("##cssSelector##", cssSelector);
        }
        private static string NewMethod2(string text, string[] candidateSelectors)
        {
            return $@"
        (function() {{
            const maxAttempts = 10; // 增加重试次数
            const retryDelayMs = 200;
            let attempt = 0;
            let targetElement = null;

            function tryFindAndInject() {{
                attempt++;
                
                // 1. 尝试所有备选选择器，直到找到一个
                const selectors = {JsonSerializer.Serialize(candidateSelectors)};
                for (let selector of selectors) {{
                    let element = document.querySelector(selector);
                    if (element && element.isContentEditable) {{
                        targetElement = element;
                        break; // 找到第一个匹配的元素就退出循环
                    }}
                }}
                
                // 2. 如果找到目标元素
                if (targetElement) {{
                    // 清除可能存在的空白提示
                    if (targetElement.classList.contains('ql-blank')) {{
                        targetElement.innerHTML = '';
                    }}
                    
                    // 聚焦元素
                    targetElement.focus();
                    
                    // 创建一个TextEvent来模拟输入，这比直接设置textContent更能触发编辑器的响应
                    // 注意：某些环境可能限制创建TextEvent，因此准备备选方案
                    try {{
                        const textEvent = document.createEvent('TextEvent');
                        textEvent.initTextEvent('textInput', true, true, window, '{text.Replace("'", "\\'")}', 0, 'en-US');
                        targetElement.dispatchEvent(textEvent);
                    }} catch (e) {{
                        // 如果TextEvent不可用，则使用execCommand作为备选（兼容旧版浏览器）
                        targetElement.focus();
                        document.execCommand('selectAll', false, null);
                        document.execCommand('delete', false, null);
                        document.execCommand('insertText', false, '{text.Replace("'", "\\'")}');
                    }}
                    
                    // 触发一系列可能被监听的事件
                    ['input', 'change', 'keydown', 'keyup', 'blur', 'focus'].forEach(eventType => {{
                        const event = new Event(eventType, {{ bubbles: true }});
                        targetElement.dispatchEvent(event);
                    }});
                    
                    return true;
                }}
                
                // 3. 重试逻辑
                if (attempt < maxAttempts) {{
                    console.log('Attempt ' + attempt + ': Element not found, retrying...');
                    setTimeout(tryFindAndInject, retryDelayMs);
                    return false;
                }} else {{
                    console.error('Failed to find the input element after ' + maxAttempts + ' attempts.');
                    return false;
                }}
            }}
            
            // 开始尝试
            return tryFindAndInject();
        }})();
    ";
        }

        private static string NewMethod1(string text, string cssSelector)
        {
            return $@"
        (function() {{
            const MAX_ATTEMPTS = 5;
            const RETRY_DELAY = 300;
            let attempts = 0;
            
            function tryInject() {{
                // 1. 尝试直接查找元素
                let inputElement = document.querySelector('{cssSelector}');
                
                // 2. 如果找不到，尝试在Shadow DOM中查找
                if (!inputElement) {{
                    const shadowHosts = document.querySelectorAll('*');
                    for (const host of shadowHosts) {{
                        if (host.shadowRoot) {{
                            inputElement = host.shadowRoot.querySelector('{cssSelector}');
                            if (inputElement) break;
                        }}
                    }}
                }}
                
                // 3. 如果找到元素，设置值并触发事件
                if (inputElement) {{
                    inputElement.value = `{text.Replace("`", "\\`")}`;
                    
                    // 触发所有可能的事件
                    const events = ['input', 'change', 'keydown', 'keyup', 'keypress'];
                    for (const eventType of events) {{
                        const event = new Event(eventType, {{ bubbles: true }});
                        inputElement.dispatchEvent(event);
                    }}
                    
                    inputElement.focus();
                    return true;
                }}
                
                // 4. 重试逻辑
                attempts++;
                if (attempts < MAX_ATTEMPTS) {{
                    setTimeout(tryInject, RETRY_DELAY);
                    return false;
                }}
                
                console.error('元素未找到: {cssSelector}');
                return false;
            }}
            
            // 5. 如果文档已加载完成，直接尝试
            if (document.readyState === 'complete') {{
                return tryInject();
            }}
            
            // 6. 否则等待文档加载完成
            document.addEventListener('DOMContentLoaded', tryInject);
            return false;
        }})();
    ";
        }

        private static string NewMethod(string text, string cssSelector)
        {
            return $@"
        (function() {{
            var inputElement = document.querySelector('{cssSelector}');
            if (inputElement) {{
                inputElement.value = `{text.Replace("`", "\\`")}`; // 设置值，注意转义反引号
                // 触发input事件，让网站知道值已改变（很重要！）
                var event = new Event('input', {{ bubbles: true }});
                inputElement.dispatchEvent(event);
                // 聚焦到输入框
                inputElement.focus();
                return true; // 表示成功
            }}
            return false; // 表示未找到元素
        }})();
    ";
        }
        
        // 根据URL获取选择器
        public static string GetSelectorByUrl(string url)
        {
            if (url.Contains("yuanbao.tencent.com")) return ".ql-editor";
            if (url.Contains("gemini.google.com")) return "div.ql-editor.textarea.new-input-ui";
            if (url.Contains("chatgpt.com")) return "#prompt-textarea";
            if (url.Contains("deepseek.com")) return "textarea";



            if (url.Contains("chat.openai.com")) return "#prompt-textarea";
            if (url.Contains("claude.ai")) return ".prose textarea";
            return "";
        }
        public static string[] GetSelectorsByUrl(string url)
        {
            if (url.Contains("yuanbao.tencent.com"))
                return new string[] { ".ql-editor", ".hyc-component-text .hyc-content-text" };

            if (url.Contains("gemini.google.com"))
                return new string[] { "div.ql-editor.textarea.new-input-ui", "" };

            if (url.Contains("chatgpt.com"))
                return new string[] { "#prompt-textarea", "div[data-message-author-role=\"user\"]" };

            if (url.Contains("deepseek.com"))
                return new string[] { "textarea", ".ds-message .fbb737a4" };


            return new string[] { "", "" };
        }
        internal static async Task InjectRunIndexQuestion(WebView2 webView, string command)
        {
            string currentUrl = webView.Source?.ToString().ToLower() ?? "";
            // 在WebView2中执行脚本
            try
            {
                var result = await webView.CoreWebView2.ExecuteScriptAsync(command);
                await Task.Delay(1000); // 额外等待1秒确保动态内容加载

                if (result == "true")
                {
                    Debug.WriteLine($"运行定位成功: {currentUrl}");
                }
                else
                {
                    Debug.WriteLine($"运行定位失败: {currentUrl}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"脚本执行错误: {ex.Message}");
            }

        }
        #region 辅助方法
        // 转义JavaScript文本
        private static string EscapeJavaScriptText(string text)
        {
            return text.Replace("\\", "\\\\")
                      .Replace("'", "\\'")
                      .Replace("\"", "\\\"")
                      .Replace("`", "\\`")
                      .Replace("\r", "\\r")
                      .Replace("\n", "\\n");
        }
       
        #endregion
    }
}
