// ====== Inject Begin  ======

(function () {
    window.__my_injected__ = window.__my_injected__ || {};
    if (window.__my_injected__.commonSend) return;
    console.log('🚀 开始注入流程...');

    const USER_SELECTORS = {
        // 输入框选择器：基于类名组合
        INPUT: 'textarea',
        INPUT_PLACEHOLDER: 'textarea[data-testid="chat_input_input"][placeholder]',
        INPUT_PLACEHOLDER_STR: ['发送消息', '发送', '发消息'],


        // 发送按钮选择器（可能需要根据实际调整）
        SEND_BUTTON: 'button',

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
            if (textarea.offsetParent !== null) {
                const isPlaceholderMatch = USER_SELECTORS.INPUT_PLACEHOLDER_STR.some(
                    str => textarea.placeholder.toLowerCase().includes(str)
                );
            };
            if (isPlaceholderMatch) {
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

        console.log('🔍 查找发送按钮...');
        let sendButton = findSendButton();
        if (sendButton && !isButtonDisabled(sendButton[0])) {
            console.log('✅ 找到可用发送按钮');
            sendButton[0].click();
            console.log('✅ 点击提交');
        } else {
            console.log('尝试模拟回车键提交');
            simulateEnterKey();
        }


    }

    function findSendButton() {
        const buttons = Array.from(document.querySelectorAll('button')).filter(button => {
            const ariaLabel = button.getAttribute('aria-label');
            const testId = button.getAttribute('data-testid');

            // 条件1: aria-label 有 "发送" 或 "发送消息"
            const ariaMatch = ariaLabel && (
                ariaLabel.includes('发')
                || ariaLabel.includes('消息')
            );

            // 条件2: data-testid 包含 "send" 或 "input"
            const testIdMatch = testId && (testId.includes('send')
                || testId.includes('input')

            );

            // console.log(`ariaMatch=${ariaMatch}`)
            // console.log(testIdMatch)
            return ariaMatch && testIdMatch;

        });

        console.log(buttons);
        if (buttons.length === 0) {
            return null;
        }
        else {
            return buttons;[0];
        }
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
    console.log('✅ 接口方法commonSend注册到共享命名空间完成');
})();

//# sourceURL=Inject_commonSend.js