global using WebView2 = Microsoft.Web.WebView2.Wpf.WebView2;

namespace MultiAIClient
{
    internal partial class InjectTextIntoInput
    {
       
        public static async Task<bool> RobustChatgptInjection(WebView2 webView, string text)
        {
            await Task.Delay(1000); // 额外等待1秒确保动态内容加载
            string currentUrl = webView.Source?.ToString().ToLower() ?? "";
            string cssSelector = GetSelectorByUrl(currentUrl);
            text = EscapeJavaScriptText(text);

        string script = $@"
        (function() {{
            {GetInjectJS_Chatgpt(currentUrl,cssSelector, text)}
        }})();
    ";

            try
            {
                var result = await webView.CoreWebView2.ExecuteScriptAsync(script);
                Console.WriteLine($"{currentUrl}注入脚本返回: {result}");

                await Task.Delay(2000);
                return result == "true";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{currentUrl}脚本执行异常: {ex.Message}");
                return false;
            }
        }

        public static async Task InjectIntoChatgptWithRetry(WebView2 webView, string text)
        {
            string currentUrl = webView.Source?.ToString().ToLower() ?? "";

            // 最大重试次数
            const int maxRetries = 5;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                Console.WriteLine($"尝试注入{currentUrl} (第{attempt}次)...");

                bool success = await RobustChatgptInjection(webView, text);
                

                if (success)
                {
                    Console.WriteLine($"{currentUrl}注入成功！");
                    return;
                }
                else
                {
                    Console.WriteLine($"{currentUrl}第{attempt}次尝试失败");
                    if (attempt < maxRetries)
                    {
                        // 等待后重试
                        await Task.Delay(2000);
                    }
                }
            }

            Console.WriteLine($"{currentUrl}所有注入尝试均失败");
        }
    
        private static string  GetInjectJS_Chatgpt(string currentUrl,string cssSelector,string injectText)
        {
            return @"
            console.log('开始注入##currentUrl##...');
            
            // 方法1: 直接操作DOM元素
            function method1() {
                const editor = document.querySelector(`##cssSelector##`);
                if (!editor) {
                    console.log('方法1: 未找到编辑器');
                    return false;
                }
                
                try {
                    // 清除现有内容
                    editor.innerHTML = '<p></p>';
                    
                    // 设置新内容
                    const pElement = editor.querySelector('p');
                    pElement.textContent = `##injectText##`;
                    
                    // 触发所有可能的事件
                    triggerAllEvents(editor);
                    console.log('方法1: 文本设置成功');
                    return true;
                } catch (e) {
                    console.log('方法1失败:', e.message);
                    return false;
                }
            }
            
            // 方法2: 使用execCommand（兼容旧方法）
            function method2() {
                const editor = document.querySelector(`##cssSelector##`);
                if (!editor) return false;
                
                try {
                    editor.focus();
                    document.execCommand('selectAll', false, null);
                    document.execCommand('delete', false, null);
                    document.execCommand('insertText', false, `##injectText##`);
                    console.log('方法2: execCommand成功');
                    return true;
                } catch (e) {
                    console.log('方法2失败:', e.message);
                    return false;
                }
            }
            
            // 方法3: 模拟真实键盘输入（最可靠但最慢）
            function method3() {
                const editor = document.querySelector(`##cssSelector##`);
                if (!editor) return false;
                
                try {
                    editor.focus();
                    editor.innerHTML = '<p></p>';
                    
                    // 模拟逐个字符输入（更真实但较慢）
                    simulateTyping(editor,`##injectText##`);
                    return true;
                } catch (e) {
                    console.log('方法3失败:', e.message);
                    return false;
                }
            }
            
            // 触发完整的事件序列
            function triggerAllEvents(element) {
                if (!element) return;
                
                element.focus();
                
                const events = [
                    'focus', 'blur', 'input', 'change', 'keydown', 'keyup', 
                    'keypress', 'click', 'mousedown', 'mouseup', 'compositionstart',
                    'compositionend', 'textInput', 'paste', 'cut', 'select'
                ];
                
                events.forEach(eventType => {
                    try {
                        let event;
                        if (eventType === 'paste') {
                            event = new ClipboardEvent('paste', { bubbles: true, cancelable: true });
                        } else {
                            event = new Event(eventType, { bubbles: true, cancelable: true });
                        }
                        element.dispatchEvent(event);
                    } catch (e) {
                        // 忽略事件创建错误
                    }
                });
            }
            
            // 模拟真实输入
            function simulateTyping(element, text) {
                let index = 0;
                const typeNextChar = () => {
                    if (index < text.length) {
                        const char = text.charAt(index);
                        const pElement = element.querySelector('p') || element;
                        pElement.textContent += char;
                        
                        // 触发输入事件
                        const inputEvent = new InputEvent('input', {
                            inputType: 'insertText',
                            data: char,
                            bubbles: true,
                            cancelable: true
                        });
                        element.dispatchEvent(inputEvent);
                        
                        index++;
                        setTimeout(typeNextChar, 50); // 50ms间隔模拟输入
                    } else {
                        console.log('模拟输入完成');
                        // 输入完成后尝试发送
                        setTimeout(tryToSend, 200);
                    }
                };
                
                typeNextChar();
            }
            
            // 尝试发送消息
            function tryToSend() {
                console.log('尝试发送消息...');
                
                // 查找发送按钮的多种方式
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
                // 多种查找策略
                const strategies = [
                    () => document.querySelector('button[aria-label*=""发送""]'),
                    () => document.querySelector('button[aria-label*=""Send"" i]'),
                    () => document.querySelector('button[data-icon=""send""]'),
                    () => document.querySelector('button.send-button'),
                    () => document.querySelector('button[class*=""send""]'),
                    () => {
                        // 通过样式查找蓝色按钮
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
                        // 通过文本内容查找
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
            
            // 模拟回车键
            function simulateEnterKey() {
                const editor = document.querySelector(`##cssSelector##`);
                if (editor) {
                    const enterEvent = new KeyboardEvent('keydown', {
                        key: 'Enter',
                        code: 'Enter',
                        keyCode: 13,
                        which: 13,
                        bubbles: true,
                        cancelable: true
                    });
                    editor.dispatchEvent(enterEvent);
                    
                    setTimeout(() => {
                        const enterUpEvent = new KeyboardEvent('keyup', {
                            key: 'Enter',
                            code: 'Enter',
                            keyCode: 13,
                            which: 13,
                            bubbles: true,
                            cancelable: true
                        });
                        editor.dispatchEvent(enterUpEvent);
                    }, 100);
                }
            }
            
            // 主执行逻辑 - 按顺序尝试各种方法
            console.log('尝试方法1...');
            if (method1()) {
                setTimeout(tryToSend, 500);
                return true;
            }
            
            console.log('方法1失败，尝试方法2...');
            if (method2()) {
                setTimeout(tryToSend, 500);
                return true;
            }
            
            console.log('方法2失败，尝试方法3...');
            return method3(); // 方法3内部会处理发送

"
.Replace("##currentUrl##", currentUrl)
.Replace("##cssSelector##", cssSelector)
.Replace("##injectText##", injectText);
        }

    }

}
