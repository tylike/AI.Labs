using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using System.Diagnostics;

namespace AI.Labs.Module.BusinessObjects.ChatInfo
{
    public class BatchChatViewController : ObjectViewController<ListView, BatchChat>
    {
        public BatchChatViewController()
        {
            var send = new SimpleAction(this, "批量询问", null);
            send.Execute += Send_Execute;
        }

        private async void Send_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            if (ViewCurrentObject.Model != null)
            {
                var items = e.SelectedObjects.OfType<BatchChat>().ToList();
                List<Action> actions = new List<Action>();
                var rst = Parallel.ForEach(items, async item =>
                {
                    var c = item;
                    var msg = "";
                    var start = DateTime.Now;
                    await AIHelper.Ask(item.问题, p =>
                    {
                        msg += p.Content;
                        Debug.WriteLine(c.Oid + ":" + p.Content);
                    },
                    aiModel: ViewCurrentObject.Model,
                    systemPrompt: "回答用户问题:"
                    );
                    var end = DateTime.Now;
                    actions.Add(() =>
                    {
                        c.回答 = msg;
                        c.首次响应时间 = start;
                        c.完成响应时间 = end;
                    });
                });

                foreach (var item in actions)
                {
                    item();
                }

                ObjectSpace.CommitChanges();

                await Task.CompletedTask;
            }
            else
            {
                throw new UserFriendlyException("没有选择语言模型!");
            }
        }
    }

    [NavigationItem("业务场景")]
    public class BatchChat : XPObject
    {
        public BatchChat(Session s) : base(s)
        {

        }


        public AIModel Model
        {
            get { return GetPropertyValue<AIModel>(nameof(Model)); }
            set { SetPropertyValue(nameof(Model), value); }
        }


        [Size(500)]
        public string 问题
        {
            get { return GetPropertyValue<string>(nameof(问题)); }
            set { SetPropertyValue(nameof(问题), value); }
        }

        [Size(-1)]
        public string 回答
        {
            get { return GetPropertyValue<string>(nameof(回答)); }
            set { SetPropertyValue(nameof(回答), value); }
        }

        public string Temp { get; set; }

        [ModelDefault("DisplayFormat", "{0:HH:mm:ss.fff}")]
        public DateTime 发送时间
        {
            get { return GetPropertyValue<DateTime>(nameof(发送时间)); }
            set { SetPropertyValue(nameof(发送时间), value); }
        }

        [ModelDefault("DisplayFormat", "{0:HH:mm:ss.fff}")]
        public DateTime? 首次响应时间
        {
            get { return GetPropertyValue<DateTime?>(nameof(首次响应时间)); }
            set { SetPropertyValue(nameof(首次响应时间), value); }
        }
        [ModelDefault("DisplayFormat", "{0:HH:mm:ss.fff}")]
        public DateTime 完成响应时间
        {
            get { return GetPropertyValue<DateTime>(nameof(完成响应时间)); }
            set { SetPropertyValue(nameof(完成响应时间), value); }
        }

        public int 耗时
        {
            get
            {
                if (首次响应时间.HasValue)
                    return (int)(完成响应时间 - 首次响应时间.Value).TotalMilliseconds;
                return 0;
            }
        }

    }
}
