using DevExpress.ExpressApp.DC;
using DevExpress.Xpo;
//using SubtitlesParser.Classes; // 引入SubtitlesParser的命名空间
using DevExpress.Persistent.Base;
using System.Windows.Forms;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DiffPlex.DiffBuilder;
using DiffPlex;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
//using SubtitlesParser.Classes.Parsers;
namespace AI.Labs.Module.BusinessObjects.VideoTranslate
{

    public class NSubtitleItem : XPObject
    {
        [Appearance("NSubtitleItem", AppearanceItemType = "ViewItem",Criteria ="时间重合警告", TargetItems = "时间重合警告", BackColor = "DarkRed", FontColor = "White")]
        public bool 时间重合警告
        {
            get
            {
                if (SourceItems.Count == 0)
                    return true;
                if (End == TimeSpan.Zero && End == TimeSpan.Zero)
                    return true;

                if (Task == null)
                    return true;
                //return false;

                var pre = Task.NSubtitleItems.FirstOrDefault(t => t.Index == Index - 1);
                if (pre != null)
                {
                    if(Start<pre.End)
                    {
                        return true;
                    }
                    //if (Math.Abs(pre.End.TotalSeconds - this.Start.TotalSeconds) > 1)
                    //    return true;
                }
                return false;
            }
        }
        [XafDisplayName("行数")]
        public int RowCount
        {
            get { return GetPropertyValue<int>(nameof(RowCount)); }
            set { SetPropertyValue(nameof(RowCount), value); }
        }

        [XafDisplayName("匹配数量")]
        public int MatchCount
        {
            get { return GetPropertyValue<int>(nameof(MatchCount)); }
            set { SetPropertyValue(nameof(MatchCount), value); }
        }

        protected override void OnChanged(string propertyName, object oldValue, object newValue)
        {
            base.OnChanged(propertyName, oldValue, newValue);
            if (!IsLoading)
            {
                if(propertyName == nameof(EnglishText))
                {
                    RowCount = EnglishText.Trim().Split('\n',StringSplitOptions.RemoveEmptyEntries).Length;
                }
                if (propertyName == nameof(SourceItemOids))
                {
                    MatchCount = SourceItemOids.Split(',').Distinct().Count();
                }
            }
        }


        public string Memo
        {
            get { return GetPropertyValue<string>(nameof(Memo)); }
            set { SetPropertyValue(nameof(Memo), value); }
        }



        [Size(-1)]
        [EditorAlias(AI.Labs.Module.LabsModule.HtmlPropertyEditor)]
        public string 比较详情
        {
            get { return GetPropertyValue<string>(nameof(比较详情)); }
            set { SetPropertyValue(nameof(比较详情), value); }
        }



        public NSubtitleItem(Session s) : base(s)
        {

        }
        [Association]
        public TranslateTask Task
        {
            get { return GetPropertyValue<TranslateTask>(nameof(Task)); }
            set { SetPropertyValue(nameof(Task), value); }
        }

        [XafDisplayName("英文字幕")]
        [ToolTip("指带标点的一句话")]
        [Size(-1)]
        [ModelDefault("RowCount","2")]
        public string EnglishText
        {
            get { return GetPropertyValue<string>(nameof(EnglishText)); }
            set { SetPropertyValue(nameof(EnglishText), value); }
        }

        [XafDisplayName("中文")]
        [Size(-1)]
        [ModelDefault("RowCount", "2")]
        public string ChineseText
        {
            get { return GetPropertyValue<string>(nameof(ChineseText)); }
            set { SetPropertyValue(nameof(ChineseText), value); }
        }


        public int Index
        {
            get { return GetPropertyValue<int>(nameof(Index)); }
            set { SetPropertyValue(nameof(Index), value); }
        }

        //[ModelDefault("DisplayFormat", @"{0:hh\:mm\:ss\,fff}")]
        public TimeSpan Start
        {
            get { return GetPropertyValue<TimeSpan>(nameof(Start)); }
            set { SetPropertyValue(nameof(Start), value); }
        }

        public TimeSpan End
        {
            get { return GetPropertyValue<TimeSpan>(nameof(End)); }
            set { SetPropertyValue(nameof(End), value); }
        }

        //public TimeSpan Start
        //{
        //    get
        //    {
        //        if (SourceItems.Any())
        //            return SourceItems.Min(t => t.StartTime);
        //        return TimeSpan.Zero;
        //    }
        //}
        //public TimeSpan End
        //{
        //    get
        //    {
        //        if (SourceItems.Any())
        //            return SourceItems.Max(t => t.EndTime);
        //        return TimeSpan.Zero;
        //    }
        //}

        public int Second => (int)End.TotalSeconds - (int)Start.TotalSeconds;

        [Size(-1)]
        [ModelDefault("RowCount", "2")]
        public string SourceItemOids
        {
            get { return GetPropertyValue<string>(nameof(SourceItemOids)); }
            set { SetPropertyValue(nameof(SourceItemOids), value); }
        }


        //[Association]
        public List<SubtitleItem> SourceItems
        {
            get
            {
                if (!string.IsNullOrEmpty(SourceItemOids))
                {
                    var oids = SourceItemOids.Split(',').Select(t => int.Parse(t)).ToArray();
                    return Task.SubtitleItems.Where(t => oids.Contains(t.Oid)).ToList();
                }
                else
                {
                    return new List<SubtitleItem>();
                }
            }
        }

    }

    public class NSubtitleItemViewController : ObjectViewController<ObjectView, NSubtitleItem>
    {
        public NSubtitleItemViewController()
        {
            var matchSourceSubtitle = new SimpleAction(this, "MatchSourceSubtitle", null);
            matchSourceSubtitle.Caption = "匹配源字幕";
            matchSourceSubtitle.Execute += MatchSourceSubtitle_Execute;

            var checkMatchResult = new SimpleAction(this, "NSubtitleItem.CheckMatchResult", null);
            checkMatchResult.Caption = "检查匹配结果";
            checkMatchResult.Execute += CheckMatchResult_Execute;

        }

        private void CheckMatchResult_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var all = e.SelectedObjects.OfType<NSubtitleItem>().OrderBy(t => t.Index).ToList();
            //如果是第一个,则匹配所有的
            var task = all.First().Task;
            foreach (NSubtitleItem item in all)
            {
                TextCompare.CheckMatchResult(item);
            }
        }

        private void MatchSourceSubtitle_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var all = e.SelectedObjects.OfType<NSubtitleItem>().OrderBy(t => t.Index).ToList();
            //如果是第一个,则匹配所有的
            var task = all.First().Task;
            foreach (NSubtitleItem item in all)
            {
                TextCompare.MatchSourceSubtitles(item);
            }

        }
    }

}
