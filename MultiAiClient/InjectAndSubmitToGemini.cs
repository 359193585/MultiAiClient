using Microsoft.Web.WebView2.Wpf;
using System.Diagnostics;

namespace MultiAIClient
{
    internal partial class InjectTextIntoInput
    {
        public static async Task<bool> RobustGeminiInjection(WebView2 webView, string text)
        {
            await Task.Delay(1000); // 额外等待1秒确保动态内容加载
            string currentUrl = webView.Source?.ToString().ToLower() ?? "";
            string cssSelector = GetSelector.GetSelectorByUrl(currentUrl);
            text = EscapeJavaScriptText(text);
            string script = $@"
                    (function() {{
                        {GetGeminiInjectionScript(cssSelector, text)}
                    }})();
                ";
            

            try
            {
                // 使用更详细的执行结果检查
                var result = await webView.CoreWebView2.ExecuteScriptAsync(script);
                Console.WriteLine($"Gemini注入脚本返回: {result}");

                // 等待一段时间让操作完成
                await Task.Delay(2000);

                return result == "true";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Gemini脚本执行异常: {ex.Message}");
                return false;
            }
        }

        public static async Task InjectIntoGeminiWithRetry(WebView2 webView, string text)
        {
            // 最大重试次数
            const int maxRetries = 5;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                Console.WriteLine($"尝试注入Gemini (第{attempt}次)...");

                bool success = await RobustGeminiInjection(webView, text);

                if (success)
                {
                    Console.WriteLine("Gemini注入成功！");
                    return;
                }
                else
                {
                    Console.WriteLine($"第{attempt}次尝试失败");
                    if (attempt < maxRetries)
                    {
                        // 等待后重试
                        await Task.Delay(2000);
                    }
                }
            }

            Console.WriteLine("所有注入尝试均失败");
        }
        private static string GetGeminiInjectionScript(string cssSelector,string injectText)
        {
            return @"

// 方法1: 使用execCommand
        function method1() {
            const editor = document.querySelector(`##cssSelector##`);
            if (!editor){
                console.log('editor not found');
                return false;
            }
            console.log('方法1: 找到编辑器');
            try {
                editor.focus();
                document.execCommand('selectAll', false, null);
                document.execCommand('delete', false, null);
                console.log(' 处理多行文本：将换行符转换为<br>标签');
                const processedText = processMultiLineText(`##injectText##`);
                console.log(processedText);
                document.execCommand('insertText', false,processedText);
                console.log('方法1: execCommand成功');
                            return true;
                
            } catch (e) {
                console.log('方法1失败:', e.message);
                return false;
            }
        }


// 处理多行文本的函数
        function processMultiLineText(text) {
            
            if (text.includes('\n')) {
                console.log('是多行文本，将换行符转换为HTML段落');
                const lines = text.split('\n');
                let htmlContent = '';
                for (let line of lines) {
                    if (line.trim() !== '') {
                        htmlContent += ' ' + escapeHtml(line) ;
                    }
                }
                console.log('多行文本已转换为HTML:', htmlContent);
                return htmlContent;
            } else {
                console.log('单行文本');
                return escapeHtml(text);
            }
        }


// HTML转义函数
        function escapeHtml(unsafe) {
            return unsafe
                .replace(/&/g, '&amp;')
                .replace(/</g, '&lt;')
                .replace(/>/g, '&gt;')
                .replace(/""/g, '&quot;')
                .replace(/'/g, '&#039;');
        }

// 尝试发送消息
        function tryToSend() {
            console.log('尝试发送消息...');
            
            const sendButton = findSendButton();
            if (sendButton && !sendButton.disabled) {
                sendButton.click();
                console.log('成功点击发送按钮');
                return true;
            } else {
                console.log('未找到可用发送按钮，尝试回车键');
                simulateEnterKey();
                return false;
            }
        }

 // 查找发送按钮
        function findSendButton() {
            const strategies = [
                () => document.querySelector('button[aria-label*=""发送""]'),
                () => document.querySelector('button[aria-label*=""Send"" i]'),
                () => document.querySelector('button[data-icon=""send""]'),
                () => document.querySelector('button.send-button'),
                () => document.querySelector('button[class*=""send""]'),
                () => {
                    const buttons = document.querySelectorAll('button');
                    for (let btn of buttons) {
                        const style = window.getComputedStyle(btn);
                        if (style.backgroundColor.includes('rgb(26, 115, 232)') ||
                            style.backgroundColor.includes('#1a73e8')) {
                            return btn;
                        }
                    }
                    return null;
                },
                () => {
                    const buttons = document.querySelectorAll('button');
                    for (let btn of buttons) {
                        if (btn.textContent.includes('发送') || btn.textContent.includes('Send')) {
                            return btn;
                        }
                    }
                    return null;
                }
            ];
            
            for (let strategy of strategies) {
                const button = strategy();
                if (button) {
                    console.log('找到发送按钮:', button);
                    return button;
                }
            }
            return null;
        }


        // 主程序开始
        console.log('尝试方法1...');
        if (method1()) {
            setTimeout(tryToSend, 500);
            return true;
        }
        

        "
        .Replace("##cssSelector##", cssSelector)
        .Replace("##injectText##", injectText);
        }


    }
}
