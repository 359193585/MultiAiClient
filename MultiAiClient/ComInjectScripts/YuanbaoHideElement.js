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
    removeElements();

    // 创建 MutationObserver 来监听 DOM 变化
    const observer = new MutationObserver(function (mutations) {
        removeElements();
    });

    // 开始观察 body 元素，监听子节点的变化（包括添加和移除）
    observer.observe(document.body, {
        childList: true,
        subtree: true // 监控所有后代节点
    });

    // （可选）一段时间后停止观察，例如 10 秒后
    setTimeout(() => {
        observer.disconnect();
        console.log('MutationObserver disconnected.');
    }, 10000);

})();
//# sourceURL=Inject_yuanbaoRemoveAds.js