using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Base.General;
using DevExpress.Xpo;
using OpenAI.Managers;
using OpenAI.Tokenizer.GPT3;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI.Labs.Module.BusinessObjects.AutoComplexTask
{

    [NavigationItem("自动任务")]
    public class ComplexTask : SimpleXPObject, ITreeNode
    {
        public ComplexTask(Session s) : base(s)
        {

        }

        [XafDisplayName("标题")]
        public string Title
        {
            get { return GetPropertyValue<string>(nameof(Title)); }
            set { SetPropertyValue(nameof(Title), value); }
        }

        [XafDisplayName("任务说明")]
        [Size(-1)]
        public string TaskMemo
        {
            get { return GetPropertyValue<string>(nameof(TaskMemo)); }
            set { SetPropertyValue(nameof(TaskMemo), value); }
        }
        [XafDisplayName("反馈")]
        public string Response
        {
            get { return GetPropertyValue<string>(nameof(Response)); }
            set { SetPropertyValue(nameof(Response), value); }
        }


        public int TokenCount
        {
            get
            {
                if (string.IsNullOrEmpty(TaskMemo))
                    return 0;
                return TokenizerGpt3.TokenCount(TaskMemo);
            }
        }

        string ITreeNode.Name { get => Title; }
        ITreeNode ITreeNode.Parent { get => Parent; }

        [Association]
        [XafDisplayName("上级任务")]
        public ComplexTask Parent
        {
            get { return GetPropertyValue<ComplexTask>(nameof(Parent)); }
            set { SetPropertyValue(nameof(Parent), value); }
        }

        IBindingList ITreeNode.Children { get => Items; }

        [Association, DevExpress.Xpo.Aggregated]
        [XafDisplayName("子级任务")]
        public XPCollection<ComplexTask> Items
        {
            get
            {
                return GetCollection<ComplexTask>(nameof(Items));
            }
        }

        [Action]
        public void SplitItems()
        {
            var ps = TaskMemo.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            int i = 0;
            foreach (var item in ps)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    i++;
                    var task = new ComplexTask(Session)
                    {
                        Title = i.ToString("000"),
                        TaskMemo = item,
                    };
                    Items.Add(task);
                }
            }
        }

        [Action]
        public async void AddSymbol()
        {
            var uiContext = SynchronizationContext.Current;
            foreach (var item in Items)
            {
                await AIHelper.Ask(item.TaskMemo, t =>
                {
                    item.Response += t.Content;
                },
                systemPrompt: "不要改写用户内容,将用户提供的内容加上标点符号.",
                uiContext: uiContext
                );
            }
        }

        [Size(-1)]
        [ModelDefault("RowCount", "2")]
        [XafDisplayName("总结提示")]
        public string SummaryPrompt
        {
            get { return GetPropertyValue<string>(nameof(SummaryPrompt)); }
            set { SetPropertyValue(nameof(SummaryPrompt), value); }
        }

        [XafDisplayName("总结结果")]
        [Size(-1)]
        public string SummaryResponse
        {
            get { return GetPropertyValue<string>(nameof(SummaryResponse)); }
            set { SetPropertyValue(nameof(SummaryResponse), value); }
        }


        [Action]
        public async void Summary()
        {
            var systemPrompt = SummaryPrompt;
            if (string.IsNullOrEmpty(systemPrompt))
            {
                systemPrompt = this.Parent?.SummaryPrompt;
            }

            if (string.IsNullOrEmpty(systemPrompt))
            {
                systemPrompt = @"总结用户提供的内容:
1.写作方法
2.写作技巧
3.找出幽默元素:如引用的歇后语、俏皮话、网络用话语、网络流行语、口语化的表达等等.
4.是否使用了一些修辞手法,如比喻、拟人、排比、夸张等等.
";
            }

            var uiContext = SynchronizationContext.Current;
            await AIHelper.Ask(Response, t =>
            {
                SummaryResponse += t.Content;
            },
                systemPrompt: systemPrompt,
                uiContext: uiContext
                );

        }

        [Action(ToolTip = "清空子任务", ConfirmationMessage = "请确认是否要执行清空子任务,请谨慎操作,保存后不可恢复!")]
        public void ClearItems()
        {
            Session.Delete(Items);
        }


    }
}
