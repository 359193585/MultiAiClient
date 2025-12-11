// ====== Inject Begin  ======


(function () {
    // 创建或复用全局容器
    window.__my_injected__ = window.__my_injected__ || {};

    // 如果已经注册同名模块则跳过
    if (window.__my_injected__.geminiSend) return;
    console.log('window.__my_injected__.geminiSend is successful');

    const MAX_ATTEMPTS = 10;
    const RETRY_DELAY_MS = 300;
    let attempts = 0;

    function tryCompleteOperation(cssSelector, injectText, sendBtnSelector) {
        console.log('开始处理...');
        attempts++;
        debugger;
        // 1. 查找输入框
        const inputElement = document.querySelector(cssSelector);
        if (!inputElement) {
            console.log('Attempt ' + attempts + ': 输入框未找到');
            if (attempts < MAX_ATTEMPTS) {
                setTimeout(function () { tryCompleteOperation(cssSelector, injectText, sendBtnSelector); }, RETRY_DELAY_MS);
                return false;
            }
            console.error('输入框未找到');
            return false;
        }

        // 2. 注入文本（针对富文本编辑器）
        console.log('找到输入框');
        try {
            inputElement.textContent = injectText;

            // 3. 触发富文本编辑器事件
            triggerHumanLikeInput(inputElement);

            // 4. 查找并点击发送按钮
            setTimeout(() => {
                clickSendButton(sendBtnSelector);
            }, 200);

            console.log('文本注入成功');
            return true;
        } catch (e) {
            console.error('注入失败:', e.message);
            return false;
        }
    }

    // 富文本编辑器可能需要更多事件
    function triggerEvents(element) {
        element.focus();
        const events = [
            'focus', 'blur', 'input', 'change', 'keydown', 'keyup',
            'keypress', 'click', 'compositionstart', 'compositionend'
        ];

        events.forEach(eventType => {
            const event = new Event(eventType, { bubbles: true, cancelable: true });
            element.dispatchEvent(event);
        });
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



    //查找并点击发送按钮
    function clickSendButton(sendBtnSelector) {
        console.log('查找发送按钮...');
        const sendButton = document.querySelector(sendBtnSelector);

        if (sendButton) {
            console.log(`通过ID找到发送按钮`);
            sendButton.click();
            return true;
        } else {
            console.log(`未找到ID为` + sendBtnSelector + `的按钮`);
        }

        console.log('未找到发送按钮，尝试回车键');
        simulateYuanbaoEnterKey();
        return false;
    }

    //重新选择输入框，发送回车键
    function simulateYuanbaoEnterKey() {
        const inputElement = document.querySelector(cssSelector);
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
    // 暴露受控接口方法到共享命名空间
    window.__my_injected__.geminiSend = {
        tryCompleteOperation,
        sendBtnSelector: () => sendBtnSelector,
        cssSelector: () => cssSelector
    };

})();


// ====== End ======
//# sourceURL=Inject_geminiSend.js