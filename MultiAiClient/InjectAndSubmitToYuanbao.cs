using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiAIClient
{
    internal partial class InjectTextIntoInput
    {

        public static  async Task<bool> InjectAndSubmitToYuanbao(WebView2 targetWebView, string text)
        {
            string currentUrl = targetWebView.Source?.ToString().ToLower() ?? "";
            string cssSelector = GetSelectorByUrl(currentUrl);
            text = EscapeJavaScriptText(text);
            string script = $@"
        (function() {{
           {GetInjectJS_Yuanbao(cssSelector, text)}
        }})();
    ";

            try
            {
                var result = await targetWebView.CoreWebView2.ExecuteScriptAsync(script);
                Console.WriteLine($"腾讯元宝操作结果: {result}");
                return result == "true";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"腾讯元宝脚本执行异常: {ex.Message}");
                return false;
            }
        }

        private static string GetInjectJS_Yuanbao(string cssSelector,string injectText)
        {
            return @"

            console.log('开始处理腾讯元宝...');
            const MAX_ATTEMPTS = 10;
            const RETRY_DELAY_MS = 300;
            let attempts = 0;

            function tryCompleteOperation() {
                attempts++;
                
                // 1. 查找腾讯元宝输入框
                const inputElement = document.querySelector(`##cssSelector##`);
                if (!inputElement) {
                    console.log('Attempt ' + attempts + ': 腾讯元宝输入框未找到');
                    if (attempts < MAX_ATTEMPTS) {
                        setTimeout(tryCompleteOperation, RETRY_DELAY_MS);
                        return false;
                    }
                    console.error('腾讯元宝输入框未找到');
                    return false;
                }

                // 2. 注入文本（针对富文本编辑器）
                console.log('找到腾讯元宝输入框');
                try {
                    inputElement.innerHTML = '<p></p>';
                    const pElement = inputElement.querySelector('p');
                    pElement.textContent = `##injectText##`;
                    
                    // 3. 触发富文本编辑器事件
                    triggerYuanbaoEvents(inputElement);
                    
                    // 4. 查找并点击发送按钮
                    setTimeout(() => {
                        clickYuanbaoSendButton();
                    }, 200);
                    
                    console.log('腾讯元宝文本注入成功');
                    return true;
                } catch (e) {
                    console.error('腾讯元宝注入失败:', e.message);
                    return false;
                }
            }

            function triggerYuanbaoEvents(element) {
                element.focus();
                
                // 腾讯元宝作为富文本编辑器可能需要更多事件
                const events = [
                    'focus', 'blur', 'input', 'change', 'keydown', 'keyup', 
                    'keypress', 'click', 'compositionstart', 'compositionend'
                ];
                
                events.forEach(eventType => {
                    const event = new Event(eventType, { bubbles: true, cancelable: true });
                    element.dispatchEvent(event);
                });
            }

            function clickYuanbaoSendButton() {
                console.log('查找腾讯元宝发送按钮...');
                
                // 腾讯元宝发送按钮的多种查找策略
                const sendButtonSelectors = [
                    'button:contains(""发送"")', // 通过文本内容
                    'button[aria-label*=""发送""]',
                    'button[class*=""send""]',
                    'button[class*=""submit""]',
                    'button.ti-button', // 腾讯元宝可能使用的类名
                    'button[type=""submit""]',
                ];

                for (let selector of sendButtonSelectors) {
                    try {
                        // 自定义contains选择器
                        if (selector.includes('contains')) {
                            const buttons = document.querySelectorAll('button');
                            for (let button of buttons) {
                                if (button.textContent.includes('发送')) {
                                    console.log('找到发送按钮(文本匹配)');
                                    button.click();
                                    return true;
                                }
                            }
                        } else {
                            const button = document.querySelector(selector);
                            if (button && button.offsetParent !== null) {
                                console.log('找到腾讯元宝发送按钮:', selector);
                                button.click();
                                return true;
                            }
                        }
                    } catch (e) {
                        console.log('选择器执行错误:', selector, e.message);
                    }
                }

                // 备用方案：通过样式查找蓝色按钮
                const buttons = document.querySelectorAll('button');
                for (let button of buttons) {
                    const style = window.getComputedStyle(button);
                    if (style.backgroundColor && 
                        (style.backgroundColor.includes('rgb') || style.backgroundColor.includes('#')) &&
                        !style.backgroundColor.includes('transparent') &&
                        !style.backgroundColor.includes('rgba(0, 0, 0, 0)')) {
                        console.log('通过样式找到可能的主按钮');
                        button.click();
                        return true;
                    }
                }

                // 如果找不到按钮，尝试回车键
                console.log('未找到发送按钮，尝试回车键');
                simulateYuanbaoEnterKey();
                return false;
            }

            function simulateYuanbaoEnterKey() {
                const inputElement = document.querySelector(`##cssSelector##`);
                if (inputElement) {
                    const enterEvent = new KeyboardEvent('keydown', {
                        key: 'Enter',
                        code: 'Enter',
                        keyCode: 13,
                        which: 13,
                        bubbles: true,
                        cancelable: true
                    });
                    inputElement.dispatchEvent(enterEvent);
                }
            }

            return tryCompleteOperation();

            "
            .Replace("##cssSelector##", cssSelector).Replace("##injectText##", injectText);
        }
    }
}
