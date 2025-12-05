using Microsoft.Web.WebView2.Wpf;

namespace MultiAIClient
{
    internal partial class InjectTextIntoInput
    {

        public static async Task<bool> InjectAndSubmitToDeepSeek1(WebView2 targetWebView, string text)
        {
            string currentUrl = targetWebView.Source?.ToString().ToLower() ?? "";
            string cssSelector = GetSelector.GetSelectorByUrl(currentUrl);
            try
            {
                string script = $@"
                (function() {{
                    console.log('🚀 开始DeepSeek注入流程...');
                    
                    const DEEPSEEK_SELECTORS = {{
                        // 主要选择器：基于您提供的类名组合
                        INPUT: '{cssSelector}',
                        INPUT_PLACEHOLDER: 'textarea[placeholder=""给 DeepSeek 发送消息""]',
                        
                        // 发送按钮选择器（可能需要根据实际调整）
                        SEND_BUTTON: 'button[type=""submit""]',
                        SEND_BUTTON_ALT: 'button:has(svg)',
                        SEND_BUTTON_TEXT: 'button:contains(""发送"")',
                    }};
                    
                    let attempts = 0;
                    const MAX_ATTEMPTS = 8;
                    const RETRY_DELAY = 400;
                    
                    function tryInjectAndSubmit() {{
                        attempts++;
                        console.log(`尝试第${{attempts}}次查找DeepSeek输入框...`);
                        
                        // 1. 多种策略查找输入框
                        let inputElement = findDeepSeekInput();
                        
                        if (!inputElement) {{
                            if (attempts < MAX_ATTEMPTS) {{
                                console.log('输入框未找到，继续重试...');
                                setTimeout(tryInjectAndSubmit, RETRY_DELAY);
                                return false;
                            }}
                            console.error('❌ 多次尝试后仍未找到DeepSeek输入框');
                            return false;
                        }}
                        
                        console.log('✅ 成功找到DeepSeek输入框');
                        
                        // 2. 执行注入和提交
                        return executeInjection(inputElement);
                    }}
                    
                    function findDeepSeekInput() {{
                        // 策略1: 精确类名组合
                        let element = document.querySelector(DEEPSEEK_SELECTORS.INPUT);
                        if (element) {{
                            console.log('通过精确类名找到输入框');
                            return element;
                        }}
                        
                        // 策略2: 通过placeholder属性
                        element = document.querySelector(DEEPSEEK_SELECTORS.INPUT_PLACEHOLDER);
                        if (element) {{
                            console.log('通过placeholder找到输入框');
                            return element;
                        }}
                        
                        // 策略3: 查找所有textarea并筛选
                        const allTextareas = document.querySelectorAll('textarea');
                        for (let textarea of allTextareas) {{
                            if (textarea.offsetParent !== null && 
                                (textarea.placeholder.includes('DeepSeek') || 
                                 textarea.placeholder.includes('发送消息'))) {{
                                console.log('通过文本内容筛选找到输入框');
                                return textarea;
                            }}
                        }}
                        
                        return null;
                    }}
                    
                    function executeInjection(inputElement) {{
                        try {{
                            // 确保输入框可见可操作
                            inputElement.scrollIntoView({{ behavior: 'smooth', block: 'center' }});
                            
                            // 聚焦输入框
                            inputElement.focus();
                            
                            // 清除现有内容
                            inputElement.value = '';
                            
                            // 设置新内容 - 使用更可靠的方法
                            const nativeTextareaValueSetter = Object.getOwnPropertyDescriptor(
                                window.HTMLTextAreaElement.prototype, ""value"").set;
                            nativeTextareaValueSetter.call(inputElement, `{EscapeJavaScriptText(text)}`);
                            
                            // 3. 触发完整的事件序列（DeepSeek可能需要特定事件）
                            triggerDeepSeekEvents(inputElement);
                            
                            // 4. 短暂延迟后尝试提交
                            setTimeout(() => {{
                                attemptSubmission();
                            }}, 500);
                            
                            console.log('✅ DeepSeek文本注入成功');
                            return true;
                            
                        }} catch (error) {{
                            console.error('❌ 注入过程出错:', error.message);
                            return false;
                        }}
                    }}
                    
                    function triggerDeepSeekEvents(element) {{
                        // DeepSeek可能需要的事件类型
                        const eventTypes = [
                            'input', 'change', 'keydown', 'keyup', 'keypress',
                            'focus', 'blur', 'compositionstart', 'compositionend'
                        ];
                        
                        eventTypes.forEach(eventType => {{
                            try {{
                                let event;
                                if (eventType.includes('key')) {{
                                    event = new KeyboardEvent(eventType, {{
                                        key: eventType === 'keydown' ? 'a' : '',
                                        code: 'KeyA',
                                        keyCode: 65,
                                        bubbles: true,
                                        cancelable: true
                                    }});
                                }} else {{
                                    event = new Event(eventType, {{ bubbles: true, cancelable: true }});
                                }}
                                element.dispatchEvent(event);
                            }} catch (e) {{
                                // 忽略事件创建错误
                            }}
                        }});
                        
                        // 特别确保input事件被触发
                        const inputEvent = new InputEvent('input', {{
                            inputType: 'insertText',
                            data: `{EscapeJavaScriptText(text)}`,
                            bubbles: true,
                            cancelable: true
                        }});
                        element.dispatchEvent(inputEvent);
                    }}
                    
                    function attemptSubmission() {{
                        console.log('🔍 查找DeepSeek发送按钮...');
                        
                        let sendButton = findSendButton();
                        
                        if (sendButton && !isButtonDisabled(sendButton)) {{
                            console.log('✅ 找到可用发送按钮，点击提交');
                            sendButton.click();
                        }} else {{
                            console.log('尝试模拟回车键提交');
                            simulateEnterKey();
                        }}
                    }}
                    
                    function findSendButton() {{
                        // 多种查找策略
                        const strategies = [
                            () => document.querySelector(DEEPSEEK_SELECTORS.SEND_BUTTON),
                            () => document.querySelector(DEEPSEEK_SELECTORS.SEND_BUTTON_ALT),
                            () => document.querySelector(DEEPSEEK_SELECTORS.SEND_BUTTON_TEXT),
                            () => {{
                                // 通过SVG图标查找（常见发送按钮）
                                const buttons = document.querySelectorAll('button');
                                for (let button of buttons) {{
                                    if (button.innerHTML.includes('send') || 
                                        button.querySelector('svg')) {{
                                        return button;
                                    }}
                                }}
                                return null;
                            }},
                            () => {{
                                // 查找最近的表单提交按钮
                                const forms = document.querySelectorAll('form');
                                for (let form of forms) {{
                                    const submitBtn = form.querySelector('button[type=""submit""]');
                                    if (submitBtn) return submitBtn;
                                }}
                                return null;
                            }}
                        ];
                        
                        for (let strategy of strategies) {{
                            const button = strategy();
                            if (button) {{
                                console.log('找到发送按钮:', strategy.toString().substring(0, 50));
                                return button;
                            }}
                        }}
                        
                        return null;
                    }}
                    
                    function isButtonDisabled(button) {{
                        return button.disabled || 
                               button.getAttribute('aria-disabled') === 'true' ||
                               button.classList.contains('disabled') ||
                               button.style.display === 'none' ||
                               button.offsetParent === null;
                    }}
                    
                    function simulateEnterKey() {{
                        const inputElement = findDeepSeekInput();
                        if (inputElement) {{
                            const enterEvent = new KeyboardEvent('keydown', {{
                                key: 'Enter',
                                code: 'Enter',
                                keyCode: 13,
                                which: 13,
                                bubbles: true,
                                cancelable: true,
                                composed: true
                            }});
                            
                            inputElement.dispatchEvent(enterEvent);
                            
                            // 同时触发keyup
                            setTimeout(() => {{
                                const enterUpEvent = new KeyboardEvent('keyup', {{
                                    key: 'Enter',
                                    code: 'Enter',
                                    keyCode: 13,
                                    which: 13,
                                    bubbles: true,
                                    cancelable: true
                                }});
                                inputElement.dispatchEvent(enterUpEvent);
                            }}, 50);
                        }}
                    }}
                    
                    return tryInjectAndSubmit();
                }})();
            ";

                // 执行脚本
                var result = await targetWebView.CoreWebView2.ExecuteScriptAsync(script);
                Console.WriteLine($"DeepSeek注入结果: {result}");

                // 等待操作完成
                await Task.Delay(1000);

                return result == "true";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeepSeek注入异常: {ex.Message}");
                return false;
            }
        }
        public static async Task<bool> InjectAndSubmitToDeepSeek(WebView2 targetWebView, string text)
        {
            string currentUrl = targetWebView.Source?.ToString().ToLower() ?? "";
            string cssSelector = GetSelector.GetSelectorByUrl(currentUrl);
            text = EscapeJavaScriptText(text);
            try
            {
                string script = $@"
                    (function() {{
                        {GetDeepSeekInjectionScript(cssSelector, text)}
                    }})();
                ";

                // 执行脚本
                var result = await targetWebView.CoreWebView2.ExecuteScriptAsync(script);
                // 等待操作完成
                await Task.Delay(1000);

                string scriptButtonSend = FindAndClickSendButton();
                var sendResult = await targetWebView.CoreWebView2.ExecuteScriptAsync(scriptButtonSend);
                await Task.Delay(1000);

                Console.WriteLine($"DeepSeek注入结果: {sendResult}");
                return sendResult == "true";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeepSeek注入异常: {ex.Message}");
                return false;
            }
        }
        public static async Task InjectIntoDeepseekWithRetry(WebView2 webView, string text)
        {
            const int maxRetries = 1; // 最大重试次数
            string aiName = "Deepseek";
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                Console.WriteLine($"尝试注入{aiName}(第{attempt}次)...");
                bool success = await InjectAndSubmitToDeepSeek(webView, text);
                if (success)
                {
                    Console.WriteLine($"{aiName}注入成功！");
                    return;
                }
                else
                {
                    Console.WriteLine($"Inject{aiName}第{attempt}次尝试失败");
                    if (attempt < maxRetries)
                    {
                        await Task.Delay(2000);
                    }
                }
            }
            Console.WriteLine($"所有{aiName}注入尝试均失败");
        }
        // 返回DeepSeek注入脚本的C#方法
        private static string GetDeepSeekInjectionScript(string cssSelector, string injectText)
        {
            return @"
        console.log('🚀 开始DeepSeek注入流程...');
        const MAX_ATTEMPTS = 10;
        const RETRY_DELAY_MS = 300;
        let attempts = 0;

        function tryInject() {
            attempts++;
            console.log('尝试第' + attempts + '次查找输入框...');
            
            // 查找DeepSeek输入框
            const allTextareas = document.querySelectorAll(`##cssSelector##`);
            for (let textarea of allTextareas) {
                if (textarea.offsetParent !== null && 
                    (textarea.placeholder.includes('DeepSeek') || 
                     textarea.placeholder.includes('发送消息'))) {
                    console.log('通过文本内容筛选找到输入框');
                    
                    // 注入文本
                    textarea.value = '##injectText##'+ '\r\n';
                    
                    // 触发事件
                    const events = ['input', 'change', 'keydown', 'keyup', 'focus'];
                    events.forEach(eventType => {
                        const event = new Event(eventType, { bubbles: true });
                        textarea.dispatchEvent(event);
                    });
                    console.log('文本传输到输入框内');
                    console.log( textarea.value);











                    // 模拟回车键发送
                    //setTimeout(() => {
                    //    const enterEvent = new KeyboardEvent('keydown', {
                    //        key: 'Enter',
                    //        code: 'Enter', 
                    //        keyCode: 13,
                    //        which: 13,
                    //        bubbles: true
                    //    });
                    //    textarea.dispatchEvent(enterEvent);
                    //}, 100);
                    









                    return true;
                }
            }
            
            if (attempts < MAX_ATTEMPTS) {
                setTimeout(tryInject, RETRY_DELAY_MS);
                return false;
            }
            
            return false;
        }
        
        return tryInject();
    "
        .Replace("##cssSelector##", cssSelector)
        .Replace("##injectText##", injectText);
        }

        private static string FindAndClickSendButton()
        {
            return @"
(function() {
    const MAX_ATTEMPTS = 1;
    const RETRY_DELAY_MS = 400;
    let attempts = 0;

    function findAndClickSendButton() {
        attempts++;
        console.log(`[DeepSeek Inject] 尝试定位发送按钮 (${attempts}/${MAX_ATTEMPTS})`);

        // 核心选择器：通过类名和角色属性精准定位
        //const potentialButtons = document.querySelectorAll('._7436101[role=""button""]');
        //const potentialButtons = document.querySelectorAll('.ds-focus-ring');
        //const potentialButtons = document.querySelectorAll('.ds-icon-button[role=""button""]');
        const potentialButtons = document.querySelectorAll('.ds-icon-button[role=""button""]');
        console.log(`[DeepSeek Inject] 找到 ${potentialButtons.length} 个潜在按钮`);

        let foundValidButton = false;
        
        let targetButton = null;
        // 发送图标路径（部分匹配即可，因为路径字符串很长）
        const sendIconPathSegment = 'M8.3125 0.981587';
        for (let button of potentialButtons) {
        // 在按钮内部查找SVG中的path元素
            const pathElement = button.querySelector('path');
            if (pathElement) {
                const pathData = pathElement.getAttribute('d') || '';
                // 如果路径数据包含发送图标的特征段，则认为是目标按钮
                if (pathData.includes(sendIconPathSegment)) {
                    targetButton = button;
                    break;
                    }
                }
            }

        
        if(targetButton!=null){
            console.log(`[DeepSeek Inject] 找到潜在按钮`);
            try{ targetButton.click();foundValidButton = true;console.log(`[DeepSeek Inject] 发送按钮点击成功`);return true;}
            catch (error) {console.error(`[DeepSeek Inject] 点击按钮时出错:`, error);}
            
            
        }
        
        if (foundValidButton) {
            return true;
        }

        // 重试逻辑
        if (attempts < MAX_ATTEMPTS) {
            console.log(`[DeepSeek Inject] 未找到有效按钮，${RETRY_DELAY_MS}ms后重试`);
            setTimeout(findAndClickSendButton, RETRY_DELAY_MS);
        } else {
            console.error(`[DeepSeek Inject] 达到最大重试次数，未能点击发送按钮`);
            // 备选方案，比如尝试键盘事件等
            attemptAlternativeSendMethod();
        }
    }

    function attemptAlternativeSendMethod() {
        console.log(`[DeepSeek Inject] 尝试备选发送方案`);
        
        // 备选方案1: 尝试模拟键盘回车事件
        const activeElement = document.activeElement;
        if (activeElement && (activeElement.tagName === 'TEXTAREA' || activeElement.tagName === 'INPUT')) {
            console.log(`[DeepSeek Inject] 尝试通过回车键发送`);
            const enterEvent = new KeyboardEvent('keydown', {
                key: 'Enter',
                code: 'Enter',
                keyCode: 13,
                which: 13,
                bubbles: true
            });
            activeElement.dispatchEvent(enterEvent);
        }
        
        // 备选方案2: 尝试查找并点击可能的发送图标
        const sendIcons = document.querySelectorAll('svg');
        sendIcons.forEach(icon => {
            const iconHtml = icon.innerHTML;
            if (iconHtml.includes('M8.3125 0.981587')) { // DeepSeek发送图标的路径
                const parentButton = icon.closest('[role=""button""]');
                if (parentButton) {
                    parentButton.click();
                    console.log(`[DeepSeek Inject] 通过图标找到并点击发送按钮`);
                }
            }
        });
    }

    // 启动查找流程
    findAndClickSendButton();
})();
";
        }
       
    }
}
