
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using WebView2.DevTools.Dom;

namespace Browser
{
    public partial class FrmChatGPT : Form
    {
        public FrmChatGPT()
        {
            InitializeComponent();
            InitializeAsync();
            this.txtURL.Text = "https://chat.openai.com";
        }
        async void InitializeAsync()
        {
            await webView.EnsureCoreWebView2Async(null);

            // 启用WebMessageReceived事件
            webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

            btnGO.PerformClick();

        }

        public bool IsStartMonit { get; set; }

        public async Task Send(string message,bool send = true)
        {
            try
            {
                //var dev = await webView.CoreWebView2.CreateDevToolsContextAsync();

                //var history =await dev.QuerySelectorAllAsync<HtmlDivElement>("");
                //var t = history.First().GetChildNodesAsync();

                //var textBox = await dev.QuerySelectorAllAsync<HtmlTextAreaElement>("#prompt-textarea");
                //var value = textBox.First().GetValueAsync();
                //await textBox.First().SetValueAsync(message);
                message = message.Replace("\r", "");
                message = message.Replace("\n", "\\n");
                message = message.Replace("'", "\\'");

                if (send)
                {
                    //var rst = await this.webView.CoreWebView2.ExecuteScriptAsync("document.getElementById('prompt-textarea').value");
                    string script = @$"
debugger;
var textarea = document.querySelector('#prompt-textarea');
textarea.value = '{message}';
textarea.focus();
document.execCommand('insertText', false, ' ');

//var eee = new Event('change');
//textarea.dispatchEvent(eee);
document.querySelector('[data-testid=""send-button""]').click();
//return true;
";
                    var rst = await webView.ExecuteScriptAsync(script);
                    Debug.WriteLine("发送成功:"+rst);
                }
            }catch (Exception ex)
            {
                throw ex;
            }
            //dev.querysel
        }
        public event EventHandler<MessageInfo> MessageRecived;
        private void CoreWebView2_WebMessageReceived(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            //Debug.WriteLine("<hr>"+e.TryGetWebMessageAsString());
            var message = JsonConvert.DeserializeObject<MessageInfo>(e.WebMessageAsJson);
            MessageRecived?.Invoke(this, message);
            //var obj = JsonConvert.DeserializeObject(e.WebMessageAsJson);            
        }

        private void btnGO_Click(object sender, EventArgs e)
        {
            this.webView.CoreWebView2.Navigate(txtURL.Text);
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            var dev = await webView.CoreWebView2.CreateDevToolsContextAsync();

            // 使用querySelector获取特定类名的div元素
            var divElements = await dev.QuerySelectorAllAsync<HtmlDivElement>("[data-message-author-role]");

            // 遍历divElements获取具体内容
            foreach (var div in divElements)
            {
                var att = await div.GetAttributeAsync("data-message-author-role");
                var innerText = await div.GetInnerTextAsync();
                var innerHtml = await div.GetInnerHtmlAsync();

                var firstChild = (HtmlDivElement)(await div.GetFirstChildAsync());
                var css = await firstChild.GetClassListAsync();
                var cssArray = await css.ToArrayAsync();

                Console.WriteLine(innerText);
                Debug.WriteLine(innerHtml);
            }


            //var history =await dev.QuerySelectorAllAsync<HtmlDivElement>("");
            //var t = history.First().GetChildNodesAsync();

            //var btn =await dev.QuerySelectorAllAsync<HtmlTextAreaElement>("prompt-textarea");
            //var value = btn.First().GetValueAsync();
            //btn.First().SetValueAsync("");
            //var rst = await this.webView.CoreWebView2.ExecuteScriptAsync("document.getElementById('prompt-textarea').value");
            //string script = "document.querySelector('[data-testid=\"send-button\"]').click();";
            //await webView.ExecuteScriptAsync(script);

            //dev.querysel

        }

        private async void btnWatch_Click(object sender, EventArgs e)
        {
            await StartMonit();
            //                @"
            //// 选择要监视变化的目标元素
            //const target = document.documentElement; // 监视整个文档

            //// 创建一个MutationObserver实例，并定义回调函数
            //const observer = new MutationObserver((mutations) => {
            //  mutations.forEach((mutation) => {
            //    const changedElement = mutation.target;
            //    console.log('变化的元素:', changedElement);

            //    if (mutation.type === 'childList') {

            //      console.group('子级元素发生变化');
            //      console.log('变化的元素路径:', getElementPath(changedElement));
            //      console.log('添加的节点:', mutation.addedNodes);
            //      console.log('移除的节点:', mutation.removedNodes);
            //      console.groupEnd();
            //    } else if (mutation.type === 'attributes') {
            //      console.group('属性发生变化');
            //      console.log('变化的元素路径:', getElementPath(changedElement));
            //      console.log('所有属性:', getAllAttributes(changedElement));
            //      console.log('发生变化的属性名:', mutation.attributeName);
            //      console.log('属性变化前的值:', mutation.oldValue);
            //      console.log('当前属性值:', changedElement.getAttribute(mutation.attributeName));
            //      console.groupEnd();
            //    }
            //  });
            //});

            //// 配置MutationObserver以监视所有变化
            //const config = { childList: true, subtree: true, attributes: true, attributeOldValue: true };

            //// 启动观察器并传入目标节点和配置
            //observer.observe(target, config);

            //// 获取元素路径的辅助函数
            //function getElementPath(element) {
            //  const path = [];
            //  while (element && element.nodeType === Node.ELEMENT_NODE) {
            //    const selector = element.nodeName.toLowerCase();
            //    const className = element.className.trim();
            //    const elementInfo = className ? `${selector}.${className}` : selector;
            //    path.unshift(elementInfo);
            //    element = element.parentNode;
            //  }
            //  return path.join('\\');
            //}

            //// 获取元素所有属性的辅助函数
            //function getAllAttributes(element) {
            //  const attributes = element.attributes;
            //  const attributeList = [];
            //  for (let i = 0; i < attributes.length; i++) {
            //    const attributeName = attributes[i].name;
            //    const attributeValue = attributes[i].value;
            //    attributeList.push({ name: attributeName, value: attributeValue });
            //  }
            //  return attributeList;
            //}
            //";
        }

        public async Task StartMonit()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "inject.js");
            var script = System.IO.File.ReadAllText(path);
            //await webView.CoreWebView2.ExecuteScriptAsync("var script = document.createElement('script');" +
            //                                 $"script.src = '{path}';" +
            //                                 "document.head.appendChild(script);");
            //string script = System.IO.File.ReadAllText(  );
            await webView.CoreWebView2.ExecuteScriptAsync(script);
            this.IsStartMonit = true;
        }
    }


}
