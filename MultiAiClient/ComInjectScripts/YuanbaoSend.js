// ====== Inject yuanbaoSend  ======


(function () {
    // 创建或复用全局容器（只用一个简单名字）
    window.__my_injected__ = window.__my_injected__ || {};

    // 如果已经注册同名模块则跳过（避免重复注入）
    if (window.__my_injected__.yuanbaoSend) return;
    console.log('window.__my_injected__.yuanbaoSend is successful');

    const MAX_ATTEMPTS = 10;
    const RETRY_DELAY_MS = 300;
    let attempts = 0;

    function tryCompleteOperation(cssSelector, injectText, sendBtnSelector) {
        console.log('开始处理腾讯元宝...');
        attempts++;

        // 1. 查找腾讯元宝输入框
        const inputElement = document.querySelector(cssSelector);
        if (!inputElement) {
            console.log('Attempt ' + attempts + ': 腾讯元宝输入框未找到');
            if (attempts < MAX_ATTEMPTS) {
                setTimeout(function () { tryCompleteOperation(cssSelector, injectText, sendBtnSelector); }, RETRY_DELAY_MS);
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
            pElement.textContent = injectText;

            // 3. 触发富文本编辑器事件
            triggerYuanbaoEvents(inputElement);

            // 4. 查找并点击发送按钮
            setTimeout(() => {
                clickYuanbaoSendButton(sendBtnSelector);
            }, 200);

            console.log('腾讯元宝文本注入成功');
            return true;
        } catch (e) {
            console.error('腾讯元宝注入失败:', e.message);
            return false;
        }
    }

    // 腾讯元宝富文本编辑器可能需要更多事件
    function triggerYuanbaoEvents(element) {
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

    //查找并点击发送按钮
    function clickYuanbaoSendButton(sendBtnSelector) {
        console.log('查找腾讯元宝发送按钮...');
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
    window.__my_injected__.yuanbaoSend = {
        tryCompleteOperation
    };

})();


// ====== End ======
//# sourceURL=Inject_yuanbaoSend.js