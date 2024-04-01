// 选择要监视变化的目标元素
const target = document.getElementById('prompt-textarea').parentElement;
console.log("开始填加监控!");
//document.querySelector('.flex.flex-col.pb-9.text-sm'); // 监视整个文档

//debugger;
//if (target && !target.id) {
//    // 如果 target 存在并且没有设置 id，则设置 id 为 'history'
//    target.id = 'history';
//    alert('id设置成功!');
//} else {
//    alert('目标元素不存在或已经设置了 ID');
//}

// 创建一个MutationObserver实例，并定义回调函数
const observer = new MutationObserver((mutations) => {

    mutations.forEach((mutation) => {
        debugger;
        // 在这里检查是否添加了 data-test-id 为 xx 的元素，并且该元素处于禁用状态
        const addedElements = Array.from(mutation.addedNodes);
        const targetElement = addedElements.find(element => element instanceof Element && element.dataset.testid === 'send-button' && element.disabled);

        if (targetElement) {
            // 发送消息给 C#
            //webViewWindow.postMessage('MessageEnd', '*');
            //window.chrome.webview.postMessage('MessageEnd');
            // 在JavaScript中选择元素并发送信息
            debugger;
            window.setTimeout(NotifyMessage, 800);
            //window.chrome.webview.postMessageWithAdditionalObjects('MessageEnd', elementInfos);
        }
    });


    mutations.forEach((mutation) => {
        const changedElement = mutation.target;
        //console.log('变化的元素:', changedElement);
        var changePath = getElementPath(changedElement);
        if (mutation.type === 'childList') {
            var sum = "";
            if (mutation.addedNodes.length > 0) {
                sum = "add:" + mutation.addedNodes.length;
            }
            if (mutation.removedNodes.length > 0) {
                sum += "remove:" + mutation.removedNodes.length;
            }
            console.group('子级元素发生变化', changePath, sum);
            //console.log('变化的元素路径:', getElementPath(changedElement));
            console.log('添加的节点:', mutation.addedNodes);
            console.log('移除的节点:', mutation.removedNodes);
            console.groupEnd();
        } else if (mutation.type === 'attributes') {
            console.group('属性发生变化', changePath, mutation.attributeName);
            console.log('变化的元素路径:', getElementPath(changedElement));
            console.log('所有属性:', getAllAttributes(changedElement));
            console.log('发生变化的属性名:', mutation.attributeName);
            console.log('属性变化前的值:', mutation.oldValue);
            console.log('当前属性值:', changedElement.getAttribute(mutation.attributeName));
            console.groupEnd();
        }
    });
});

// 配置MutationObserver以监视所有变化
const config = { childList: true, subtree: true, attributes: true, attributeOldValue: true };

// 启动观察器并传入目标节点和配置
observer.observe(target, config);
console.log("填加监控完成!");
function NotifyMessage() {
    var elements = document.querySelectorAll('[data-message-author-role]');
    var elementInfos = Array.from(elements).map(element => {
        return {
            HTML: element.innerHTML,
            Text: element.innerText,
            Role: element.getAttribute('data-message-author-role')
        };
    });
    window.chrome.webview.postMessage({ Message: 'MessageEnd', History: elementInfos });

}
function getElementPath(element) {
    const path = [];
    while (element && element.nodeType === Node.ELEMENT_NODE) {
        const id = element.id;
        const testId = element.getAttribute('data-testid');
        const dmar = element.getAttribute('data-message-author-role');
        if (id) {
            path.unshift(`#${id}`);
            return path.join('\\');
        } else if (testId) {
            path.unshift(`[data-testid="${testId}"]`);
            return path.join('\\');
        } else if (dmar) {
            path.unshift(`[data-message-author-role="${dmar}"]`);
            return path.join('\\');
        }

        const name = element.name;
        const selector = name ? `[name="${name}"]` : element.nodeName.toLowerCase();
        path.unshift(selector);
        element = element.parentNode;
    }
    return path.join('\\');
}


// 获取元素所有属性的辅助函数
function getAllAttributes(element) {
    const attributes = element.attributes;
    const attributeList = [];
    for (let i = 0; i < attributes.length; i++) {
        const attributeName = attributes[i].name;
        const attributeValue = attributes[i].value;
        attributeList.push({ name: attributeName, value: attributeValue });
    }
    return attributeList;
}