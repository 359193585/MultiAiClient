(function () {

    // 定义需要移除的广告元素的选择器数组
    const selectorsToRemove = [
        '.index_content__Ya5lt',
        '.index_download-multi-btn__btn__nYjRo',
        '.agent-dialogue__tool__download',
        '.download_hint__wrap'
    ];

    function removeElements() {
        selectorsToRemove.forEach(selector => {
            const element = document.querySelector(selector);
            if (element) {
                element.remove();
                console.log('Removed element with selector: ' + selector);
            }
        });
    }

    // 防抖函数
    function debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }
    // 创建防抖版的移除函数
    const debouncedRemove = debounce(removeElements, 100);

    // 等待DOMContentLoaded事件
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () {
            // 初始移除
            removeElements();
            // 创建 MutationObserver 来监听 DOM 变化，使用防抖
            const observer = new MutationObserver(debouncedRemove);
            observer.observe(document.body, {
                childList: true,
                subtree: true
            });
            // 10秒后停止观察
            setTimeout(() => {
                observer.disconnect();
                console.log('MutationObserver disconnected 1-1.');
            }, 10000);
        });
    } else {
        // 如果DOMContentLoaded已经触发，直接执行
        removeElements();
        const observer = new MutationObserver(debouncedRemove);
        observer.observe(document.body, {
            childList: true,
            subtree: true
        });
        setTimeout(() => {
            observer.disconnect();
            console.log('MutationObserver disconnected 1-2.');
        }, 10000);
    }

})();
//# sourceURL=Inject_yuanbaoRemoveAds.js