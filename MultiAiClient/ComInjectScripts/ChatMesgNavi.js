// ====== Chat Message Navigator ======

// 创建或复用全局容器（只用一个简单名字）
window.__my_injected__ = window.__my_injected__ || {};

// 如果已经注册同名模块则跳过（避免重复注入）
if (window.__my_injected__.chatNavigator) {
    console.log('chatNavigator module already exists.');
} else {

    // 私有变量:选择器，当前索引, 问题总数
    const state = {
        messagesSelector: null,
        index: -1,
        messageCount: 0
    }
    // 初始化方法，用于设置选择器
    function init(selector) {
        state.messagesSelector = selector;
        state.index = -1;
        console.log('Chat navigator initialized with selector: ' + selector);
    };

    // 获取所有用户消息元素
    function getUserMessages() {
        return Array.from(
            document.querySelectorAll(state.messagesSelector)
        );
    }

    // 1. 滚动到指定 index
    function scrollToMessage(i) {
        const msgs = getUserMessages();
        if (i < 0 || i >= msgs.length) {
            state.index = -1;
            return false;
        }
        const target = msgs[i];
        target.scrollIntoView({ behavior: 'smooth', block: 'center' });
        // 可视化高亮（先清理）
        msgs.forEach(m => m.style.outline = '');
        target.style.outline = '2px solid #4A90E2';
        state.index = i;
        state.messageCount = (state.index + 1) + '/' + msgs.length;
        return true;
    }

    // 2. 跳到上一个问题
    function goToPrevUserMessage() {
        const msgs = getUserMessages();
        if (!msgs.length) return;
        if (state.index === -1) state.index = msgs.length - 1;
        else state.index = Math.max(0, state.index - 1);
        scrollToMessage(state.index);
        return true;
    }


    // 3. 跳到下一个问题
    function goToNextUserMessage() {
        const msgs = getUserMessages();
        if (!msgs.length) return;
        if (state.index === -1) state.index = msgs.length - 1;
        else state.index = Math.min(msgs.length - 1, state.index + 1);
        scrollToMessage(state.index);
        return true;
    }

    // 4. 跳到最新的问题
    function goToBottomUserMessage() {
        const msgs = getUserMessages();
        if (!msgs.length) return;
        scrollToMessage(msgs.length-1);
        return true;
    }

    // 5. 跳到最早的问题
    function goToTopUserMessage() {
        const msgs = getUserMessages();
        if (!msgs.length) return;
        scrollToMessage(0);
        return true;
    }
    function getMesgStr() {
        const msgs = getUserMessages();
        const messageContents = msgs.map(msg => msg.textContent.trim());
        return JSON.stringify(messageContents);
    }

    // 暴露受控接口方法到共享命名空间
    window.__my_injected__.chatNavigator = {
        init,
        goToPrevUserMessage,
        goToNextUserMessage,
        goToTopUserMessage,
        goToBottomUserMessage,
        getMesgStr,
        messageCount: () => state.messageCount,
        // 可选：获取当前 index，用于 UI 同步
        getIndex: () => state.index,
        // 可选：重置或重扫描
        reset: () => { state.index = -1; }
    }


}
// ====== End ======
//# sourceURL=chatNavigator.js

