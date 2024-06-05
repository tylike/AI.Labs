using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Base.General;
using DevExpress.Xpo;
using DevExpress.XtraReports.Native.Interaction;
using DevExpress.XtraSpreadsheet.Model;
using OpenAI.Managers;
using OpenAI.Tokenizer.GPT3;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using OpenAI.ObjectModels.RequestModels;
using Newtonsoft.Json;
namespace AI.Labs.Module.BusinessObjects.AutoComplexTask
{
    [NavigationItem("自动任务")]
    public class ComplexTask : SimpleXPObject, ITreeNode
    {
        public ComplexTask(Session s) : base(s)
        {

        }

        [XafDisplayName("最大轮次")]
        public int MaxRound
        {
            get { return GetPropertyValue<int>(nameof(MaxRound)); }
            set { SetPropertyValue(nameof(MaxRound), value); }
        }


        public List<ChatMessage> History { get; set; } = new List<ChatMessage>();

        public List<ChatMessage> GetHistory()
        {
            return History;
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
        public void ParseSRT()
        {
            //var srt = new SRTFile();
            //srt.LoadFromText(TaskMemo);
            //将所有时间不连续的字幕合并

            //按照中间间隔时间分段
            //List<SRT> paragraphs = SplitByStopTime(srt);
            //Debug.WriteLine($"分为了几段:{paragraphs.Count}");
            //按照上限token数分段，并保证结尾的一定是句号，即：完整的句子
            //List<SRT> paragraphs2 = SplitByTokenCount(srt.Texts);
            //var spliter = new SententTextSplitter();
            //var ps = TaskMemo.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            //var newParagraphs =SententTextSplitter.SegmentText(TaskMemo, 1000);

        }


        private static List<SRT> SplitByStopTime(SRTFile srt)
        {
            var paragraphs = new List<SRT>();
            SRT lastNew = null;
            SRT preSource = null;
            foreach (var item in srt.Texts)
            {
                //如果上一个字幕的结束时间不等于当前字幕的开始时间,则合并
                if (preSource != null && item.StartTime != preSource.EndTime)
                {
                    lastNew.EndTime = preSource.EndTime;
                    lastNew.BeforeGap = item.StartTime - preSource.EndTime;
                    lastNew = null;
                }

                if (lastNew == null)
                {
                    lastNew = new SRT();
                    paragraphs.Add(lastNew);
                    lastNew.Index = paragraphs.Count;
                    lastNew.StartTime = item.StartTime;
                    lastNew.Text = item.Text;
                }
                else
                {
                    lastNew.Text += Environment.NewLine + item.Text;
                }
                preSource = item;
            }
            lastNew.EndTime = preSource.EndTime;
            return paragraphs;
        }

        [Action]
        public void SplitItems()
        {
            //var ps = TaskMemo.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            var psLines = TaskMemo.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            var ps = SentenceTextSplitter.SegmentText(TaskMemo, 1000);

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
            await AIHelper.Ask(TaskMemo, t =>
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

        [Association, DevExpress.Xpo.Aggregated]
        public XPCollection<Agent> Agents
        {
            get => GetCollection<Agent>(nameof(Agents));
        }
    }
    public class JsonMessage
    {
        public string Content { get; set; }
        public string Agent { get; set; }
        public State State { get; set; }
    }

    public enum State
    {
        Processing,
        Complete
    }
    public class ComplexTaskViewController : ObjectViewController<ObjectView, ComplexTask>
    {
        public ComplexTaskViewController()
        {
            var run = new SimpleAction(this, "Run", null);
            run.Execute += Run_Execute;
        }

        private async void Run_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var currentRound = 0;
            var maxRound = Math.Max(ViewCurrentObject.MaxRound, 1);
            var nextSpeaker = ViewCurrentObject.Agents.First(t => t.Name == "admin");
            ViewCurrentObject.History.Clear();
            while (currentRound < maxRound)
            {
                var rst = await nextSpeaker.Send(ViewCurrentObject.TaskMemo);
                //如果rst以```json开头，以```结束，则去掉。
                if (rst.StartsWith("```json"))
                {
                    rst = rst.Substring(7, rst.Length - 7);
                }
                if (rst.EndsWith("```"))
                {
                    rst = rst.Substring(0, rst.Length - 3);
                }

                ViewCurrentObject.History.Add(ChatMessage.FromAssistant(rst, nextSpeaker.Name));

                var json = JsonConvert.DeserializeObject<JsonMessage>(rst);
                if (json.State == State.Complete)
                {
                    ViewCurrentObject.Response = json.Content;
                    break;
                }
                else
                {
                    ViewCurrentObject.Response = json.Content;
                    nextSpeaker = ViewCurrentObject.Agents.First(t => t.Name == json.Agent);
                }
                currentRound++;
            }
            //await AITask.RunAsync();
        }
    }

}
