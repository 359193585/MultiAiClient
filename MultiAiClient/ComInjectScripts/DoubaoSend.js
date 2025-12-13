// ====== Inject Begin  ======

(function () {
    window.__my_injected__ = window.__my_injected__ || {};
    if (window.__my_injected__.commonSend) return;
    console.log('🚀 开始注入流程...');

    const USER_SELECTORS = {
        // 主要选择器：基于类名组合
        INPUT: 'textarea',
        INPUT_PLACEHOLDER: 'textarea[data-testid="chat_input_input"][placeholder]',

        // 发送按钮选择器（可能需要根据实际调整）
        SEND_BUTTON: '.ds-icon-button.ds-icon-button--l.ds-icon-button--sizing-container',
        SEND_BUTTON_ALT: 'button:has(svg)',
        SEND_BUTTON_TEXT: 'button:contains("发送")',

    };


    const MAX_ATTEMPTS = 3;
    const RETRY_DELAY = 300;
    let attempts = 0;

    function tryCompleteOperation(cssSelector, injectText, sendBtnSelector) {
        //console.log(`cssSelector= ${cssSelector} `);
        //console.log(`injectText= ${injectText} `);
        //console.log(`sendBtnSelector= ${sendBtnSelector} `);

        USER_SELECTORS.INPUT = cssSelector;
        USER_SELECTORS.SEND_BUTTON = sendBtnSelector;
        attempts++;
        console.log(`尝试第${attempts}次查找输入框...`);

        // 1. 多种策略查找输入框
        let inputElement = findInput();

        if (!inputElement) {
            if (attempts < MAX_ATTEMPTS) {
                console.log('输入框未找到，继续重试...');
                setTimeout(tryCompleteOperation(cssSelector, injectText, sendBtnSelector), RETRY_DELAY);
                return false;
            }
            console.error('❌ 多次尝试后仍未找到输入框');
            return false;
        }

        console.log('✅ 成功找到输入框');

        // 2. 执行注入和提交
        return executeInjection(inputElement, injectText);
    }

    function findInput() {

        // 策略: 精确类名组合
        let element = document.querySelector(USER_SELECTORS.INPUT);
        if (element) {
            console.log('通过精确类名找到输入框');
            return element;
        }

        // 策略: 通过placeholder属性
        element = document.querySelector(USER_SELECTORS.INPUT_PLACEHOLDER);
        if (element) {
            console.log('通过placeholder找到输入框');
            return element;
        }

        // 策略: 查找所有textarea并筛选
        const allTextareas = document.querySelectorAll('textarea');
        for (let textarea of allTextareas) {
            if (textarea.offsetParent !== null &&
                (textarea.placeholder.includes('Doubao') ||
                    textarea.placeholder.includes('发消息'))) {
                console.log('通过文本内容筛选找到输入框');
                return textarea;
            }
        }


        return null;

    }

    function executeInjection(inputElement, injectText) {


        console.log(`injectText= ${injectText} `);


        try {
            // 确保输入框可见可操作
            inputElement.scrollIntoView({ behavior: 'smooth', block: 'center' });

            // 聚焦输入框
            inputElement.focus();

            // 清除现有内容
            inputElement.value = '';

            // 设置新内容
            const nativeTextareaValueSetter = Object.getOwnPropertyDescriptor(
                window.HTMLTextAreaElement.prototype, "value").set;
            nativeTextareaValueSetter.call(inputElement, injectText);

            // 3. 触发完整的事件序列
            triggerHumanLikeInput(inputElement);

            // 4. 短暂延迟后尝试提交
            setTimeout(() => {
                attemptSubmission();
            }, 500);

            console.log('✅  文本注入成功');
            return true;

        } catch (error) {
            console.error('❌ 注入过程出错:', error.message);
            return false;
        }
    }

    function triggerHumanLikeInput(element) {
        // 1. 模拟聚焦
        element.focus();
        element.dispatchEvent(new Event('focus', { bubbles: true }));

        // 2. 模拟输入过程：keydown -> input -> keyup
        // 引入 50ms 到 150ms 的随机延迟，模拟人类打字速度
        const randomDelay = Math.floor(Math.random() * 100) + 50;

        // 使用 setTimeout 来模拟事件的时序和延迟
        setTimeout(() => {
            // keydown 和 keypress (模拟按键)
            element.dispatchEvent(new Event('keydown', { bubbles: true, keyCode: 13 })); // 模拟回车键或其他键
            element.dispatchEvent(new Event('keypress', { bubbles: true, keyCode: 13 }));

            // input (核心事件，触发内容变化监听)
            element.dispatchEvent(new Event('input', { bubbles: true }));

            // keyup (模拟释放按键)
            element.dispatchEvent(new Event('keyup', { bubbles: true, keyCode: 13 }));

            // change (用于表单控件，但富文本编辑器也可能监听)
            element.dispatchEvent(new Event('change', { bubbles: true }));

            // compositionend (针对中文/日文等输入法的最终确认)
            element.dispatchEvent(new Event('compositionend', { bubbles: true }));

            console.log('Human-like input events triggered.');

        }, randomDelay);

        // 3. 模拟失焦 (可选，但可以模拟操作结束)
        setTimeout(() => {
            element.blur();
            element.dispatchEvent(new Event('blur', { bubbles: true }));
        }, randomDelay + 100); // 在输入完成 100ms 后失焦
    }

    function attemptSubmission() {
        //console.log('尝试模拟回车键提交');
        //simulateEnterKey();

        console.log('🔍 查找发送按钮...');
        let sendButton = findSendButton();
        if (sendButton && !isButtonDisabled(sendButton)) {
            console.log('✅ 找到可用发送按钮，点击提交');
            sendButton.click();
        } else {
            console.log('尝试模拟回车键提交');
            simulateEnterKey();
        }
    }

    function findSendButton() {
        // 多种查找策略
        const strategies = [
            () => document.querySelector(USER_SELECTORS.SEND_BUTTON),
            () => document.querySelector(USER_SELECTORS.SEND_BUTTON_ALT),
            () => document.querySelector(USER_SELECTORS.SEND_BUTTON_TEXT),
            () => {

                // 通过SVG图标查找（常见发送按钮）
                const buttons = document.querySelectorAll('button');
                for (let button of buttons) {
                    if (button.innerHTML.includes('send') ||
                        button.querySelector('svg')) {
                        return button;
                    }
                }
                return null;

            },
            () => {
                // 查找最近的表单提交按钮
                const forms = document.querySelectorAll('form');
                for (let form of forms) {
                    const submitBtn = form.querySelector('button[type=""submit""]');
                    if (submitBtn) return submitBtn;
                }
                return null;

            }
        ];

        for (let strategy of strategies) {
            const button = strategy();
            if (button) {
                console.log('找到发送按钮:', strategy.toString().substring(0, 50));
                return button;
            }
        }

        return null;
    }

    function isButtonDisabled(button) {
        return button.disabled ||
            button.getAttribute('aria-disabled') === 'true' ||
            button.classList.contains('disabled') ||
            button.style.display === 'none' ||
            button.offsetParent === null;
    }

    function simulateEnterKey() {
        const inputElement = findInput();
        if (inputElement) {
            const enterEvent = new KeyboardEvent('keydown', {
                key: 'Enter',
                code: 'Enter',
                keyCode: 13,
                which: 13,
                bubbles: true,
                cancelable: true,
                composed: true
            });

            inputElement.dispatchEvent(enterEvent);

            // 同时触发keyup
            setTimeout(() => {
                const enterUpEvent = new KeyboardEvent('keyup', {
                    key: 'Enter',
                    code: 'Enter',
                    keyCode: 13,
                    which: 13,
                    bubbles: true,
                    cancelable: true
                });
                inputElement.dispatchEvent(enterUpEvent);
            }, 50);
        }
    }

    // 暴露受控接口方法到共享命名空间
    window.__my_injected__.commonSend = {
        tryCompleteOperation,
        cssSelector: () => USER_SELECTORS
    };
    console.log('暴露受控接口方法到共享命名空间完成');
})();

//# sourceURL=Inject_commonSend.js