using DevExpress.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using NAudio.CoreAudioApi;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using DevExpress.PivotGrid.Criteria;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.Utils.Serializing;
using DevExpress.DashboardCommon.DataProcessing;

namespace AI.Labs.Module.BusinessObjects.TTS
{
    [NonPersistent]
    public abstract class TTSBase : XPObject
    {
        public TTSBase(Session s) : base(s)
        {

        }

        [XafDisplayName("文字")]
        [Size(-1)]
        public string Text
        {
            get { return GetPropertyValue<string>(nameof(Text)); }
            set { SetPropertyValue(nameof(Text), value); }
        }

        [XafDisplayName("音频内容")]
        [Size(-1)]
        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        public byte[] FileContent
        {
            get { return GetPropertyValue<byte[]>(nameof(FileContent)); }
            set { SetPropertyValue(nameof(FileContent), value); }
        }

        [XafDisplayName("状态")]
        [ModelDefault("AllowEdit", "False")]
        public TTSState State
        {
            get { return GetPropertyValue<TTSState>(nameof(State)); }
            set { SetPropertyValue(nameof(State), value); }
        }

        [XafDisplayName("音频生成用时(毫秒)")]
        [ModelDefault("AllowEdit", "False")]
        public int ElapsedMilliseconds
        {
            get { return GetPropertyValue<int>(nameof(ElapsedMilliseconds)); }
            set { SetPropertyValue(nameof(ElapsedMilliseconds), value); }
        }

        [XafDisplayName("声音方案")]
        [ToolTip("是指从文本生成语音时对TTS Engine使用的配置参数的集合")]
        public VoiceSolution Solution
        {
            get { return GetPropertyValue<VoiceSolution>(nameof(Solution)); }
            set { SetPropertyValue(nameof(Solution), value); }
        }
    }

    [NavigationItem("应用场景")]
    [XafDisplayName("文本阅读")]
    public class ReadTextInfo : TTSBase
    {
        public ReadTextInfo(Session s) : base(s)
        {

        }
    }

    public class AudioBookRole : XPObject
    {
        public AudioBookRole(Session s) : base(s)
        {

        }
        public string Name
        {
            get { return GetPropertyValue<string>(nameof(Name)); }
            set { SetPropertyValue(nameof(Name), value); }
        }

        public VoiceSolution VoiceSolution
        {
            get { return GetPropertyValue<VoiceSolution>(nameof(VoiceSolution)); }
            set { SetPropertyValue(nameof(VoiceSolution), value); }
        }

        [Association]
        public AudioBook AudioBook
        {
            get { return GetPropertyValue<AudioBook>(nameof(AudioBook)); }
            set { SetPropertyValue(nameof(AudioBook), value); }
        }

        [Association]
        public XPCollection<AudioBookSpreakItem> SpreakItems
        {
            get => GetCollection<AudioBookSpreakItem>(nameof(SpreakItems));
        }

        public int SpreakItemsCount { get => SpreakItems.Count; }

    }

    [NavigationItem("应用场景")]
    [XafDisplayName("有声书籍")]
    public class AudioBook : XPObject
    {
        #region Properties
        [ToolTip("简单模式:说话前的内容,与说话内容都使用一种声音播放")]
        public bool SimpleMode
        {
            get { return GetPropertyValue<bool>(nameof(SimpleMode)); }
            set { SetPropertyValue(nameof(SimpleMode), value); }
        }

        public AudioBook(Session s) : base(s)
        {
        }

        public string Name
        {
            get { return GetPropertyValue<string>(nameof(Name)); }
            set { SetPropertyValue(nameof(Name), value); }
        }

        [Size(-1)]
        public string Content
        {
            get { return GetPropertyValue<string>(nameof(Content)); }
            set { SetPropertyValue(nameof(Content), value); }
        }


        [Size(-1)]
        public string Description
        {
            get { return GetPropertyValue<string>(nameof(Description)); }
            set { SetPropertyValue(nameof(Description), value); }
        }
        public VoiceSolution Narration
        {
            get { return GetPropertyValue<VoiceSolution>(nameof(Narration)); }
            set { SetPropertyValue(nameof(Narration), value); }
        }
        #endregion

        #region Paragraphs

        [Action]
        public void ParseParagerahs()
        {
            var input = this.Content;
            string pattern = "”(?![\r\n\r\n])";
            string replacement = "”\r\n\r\n";

            string result = Regex.Replace(input, pattern, replacement);
            var t = result.Split("\r\n\r\n", StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in t)
            {
                var p = new AudioBookSpreakItem(Session) { ArticleText = item, Index = this.SpreakItems.Count + 1 };
                this.SpreakItems.Add(p);
            }
        }

        //段落
        //[Association, DevExpress.Xpo.Aggregated]
        //public XPCollection<AudioBookParagraphItem> Paragraphs { get => GetCollection<AudioBookParagraphItem>(nameof(Paragraphs)); }
        #endregion

        #region SpreakItem
        [Action]
        public async void GenerateRoles()
        {
            var roleNames = SpreakItems.Select(t => t.Spreaker).GroupBy(t => t).Distinct();
            foreach (var role in roleNames)
            {
                await CreateAudioRole(role.Key);
            }
            await Task.CompletedTask;
        }

        public const string SystemPrompt =  @"#任务标题
将小说内容解析为结构化json数据.
#任务要求:
1.说话内容中不包含开始和结束的引号、双引号。
2.识别给出的小说内容,返回说话人(有可能为“旁白”即无说话人或作者的表达)、说话前内容、说话内容、说话后内容、情感、音量，也可能有多个人在说话，如下格式内容,返回的结果是json,json示例如下:
{
    Text:'张三说:“这不好吧!”笑了一下',
    Spreaker:'张三',
    SpreakBefore:'张三说',
    SpreakContent:'这不好吧!',
    SpreakAfter:'笑了一下',
    Emotion:'尴尬',
    Volume:80
}";

        [Action]
        public async void ParseSpreakItems()
        {
            //Session.Delete(this.SpreakItems);
            //int idx = 0;
            foreach (var item in this.SpreakItems.OrderBy(t => t.Index))
            {
                await item.ParseArticleText();
            }
        }
        //说话项目、旁白
        [Association, DevExpress.Xpo.Aggregated, ModelDefault("AllowEdit", "True")]
        public XPCollection<AudioBookSpreakItem> SpreakItems { get => GetCollection<AudioBookSpreakItem>(nameof(SpreakItems)); } 

        //说话角色
        [Association, DevExpress.Xpo.Aggregated]
        public XPCollection<AudioBookRole> Roles { get => GetCollection<AudioBookRole>(nameof(Roles)); }
        #endregion

        #region AudioItems

        /// <summary>
        /// 本书被分成了N段音频内容
        /// </summary>
        [Association, DevExpress.Xpo.Aggregated]
        public XPCollection<AudioBookTextAudioItem> AudioItems
        {
            get => GetCollection<AudioBookTextAudioItem>(nameof(AudioItems));
        }

        #endregion
        public async Task<AudioBookRole> CreateAudioRole(string roleName)
        {
            var find = this.Roles.FirstOrDefault(t => t.Name == roleName);
            if (find == null)
            {
                find = new AudioBookRole(Session);
                find.Name = roleName;
                this.Roles.Add(find);
            }
            await Task.CompletedTask;
            return find;
        }

        [Action]
        public async void CreateAudioBook()
        {
            var index = 0;
            foreach (var item in SpreakItems.OrderBy(t => t.Index))
            {
                Debug.WriteLine($"正在处理:{item.Index},Oid:{item.Oid}");
                if (!SimpleMode)
                {
                    //xxx说:旁白
                    if (!string.IsNullOrEmpty(item.SpreakBefore))
                    {
                        var t = new AudioBookTextAudioItem(Session) { Index = index, Solution = Narration };
                        AudioItems.Add(t);
                        //var t = new BookTextAudio(Session) { Text = item.SpreakBefore,Index = index };
                        t.Text = item.SpreakBefore;
                        t.FileContent = TTSEngine.GetTextToSpeechData(item.SpreakBefore, Narration?.DisplayName);
                        index++;

                    }

                    //内容
                    if (!string.IsNullOrEmpty(item.SpreakContent))
                    {
                        var t = new AudioBookTextAudioItem(Session) { Index = index, Solution = item.AudioRole.VoiceSolution };
                        AudioItems.Add(t);
                        t.Text = item.SpreakContent;
                        t.FileContent = TTSEngine.GetTextToSpeechData(item.SpreakContent, item.AudioRole.VoiceSolution.DisplayName);
                        index++;
                    }
                }
                else
                {
                    var t = new AudioBookTextAudioItem(Session) { Index = index, Solution = item.AudioRole.VoiceSolution };
                    AudioItems.Add(t);
                    t.Text = item.ArticleText;
                    t.FileContent = TTSEngine.GetTextToSpeechData(item.ArticleText, item.AudioRole.VoiceSolution.DisplayName);
                    index++;
                }
            }
            Debug.WriteLine("处理完成!");
            await Task.CompletedTask;
        }

        [Action]
        public async void PlayAudioBook()
        {
            foreach (var item in AudioItems.OrderBy(t => t.Index))
            {
                if (item.FileContent != null)
                {
                    TTSEngine.Play(item.FileContent);
                }
            }
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// 有声书的最小条目
    /// </summary>
    public class AudioBookTextAudioItem : TTSBase
    {
        public AudioBookTextAudioItem(Session s) : base(s)
        {

        }

        public int Index
        {
            get { return GetPropertyValue<int>(nameof(Index)); }
            set { SetPropertyValue(nameof(Index), value); }
        }

        [Association]
        public AudioBook Book
        {
            get { return GetPropertyValue<AudioBook>(nameof(Book)); }
            set { SetPropertyValue(nameof(Book), value); }
        }
    }

    public class ParseResult
    {
        public string Text;
        public string Spreaker;
        public string SpreakBefore;
        public string SpreakContent;
        public string SpreakAfter;
        public string Emotion;
        public int Volume;
    }
    /// <summary>
    /// 有声书一个章节的一次说话
    /// 旁白也算一条
    /// </summary>
    [Appearance("没有赋值声音角色", Criteria = "AudioRole is null", BackColor = "#FF0000", TargetItems = nameof(AudioRole))]
    public class AudioBookSpreakItem : XPObject
    {
        public AudioBookSpreakItem(Session s) : base(s)
        {

        }

        [Association]
        public AudioBook AudioBook
        {
            get { return GetPropertyValue<AudioBook>(nameof(AudioBook)); }
            set { SetPropertyValue(nameof(AudioBook), value); }
        }


        

        public int Index
        {
            get { return GetPropertyValue<int>(nameof(Index)); }
            set { SetPropertyValue(nameof(Index), value); }
        }

        //public AudioBookParagraphItem Paragraph
        //{
        //    get { return GetPropertyValue<AudioBookParagraphItem>(nameof(Paragraph)); }
        //    set { SetPropertyValue(nameof(Paragraph), value); }
        //}

        //
        //public Role Role
        //{
        //    get { return GetPropertyValue<Role>(nameof(Role)); }
        //    set { SetPropertyValue(nameof(Role), value); }
        //}

        [Association]
        [DataSourceProperty("AudioBook.Roles")]
        public AudioBookRole AudioRole
        {
            get { return GetPropertyValue<AudioBookRole>(nameof(AudioRole)); }
            set { SetPropertyValue(nameof(AudioRole), value); }
        }

        [XafDisplayName("文章原文")]
        [Size(-1)]
        public string ArticleText
        {
            get { return GetPropertyValue<string>(nameof(ArticleText)); }
            set { SetPropertyValue(nameof(ArticleText), value); }
        }


        public string Spreaker
        {
            get { return GetPropertyValue<string>(nameof(Spreaker)); }
            set { SetPropertyValue(nameof(Spreaker), value); }
        }

        [Size(-1)]
        [Persistent("Content")]
        public string SpreakContent
        {
            get { return GetPropertyValue<string>(nameof(SpreakContent)); }
            set { SetPropertyValue(nameof(SpreakContent), value); }
        }

        //[Size(-1)]
        //public string JSON
        //{
        //    get { return GetPropertyValue<string>(nameof(JSON)); }
        //    set { SetPropertyValue(nameof(JSON), value); }
        //}

        [Size(-1)]
        public string SpreakBefore
        {
            get { return GetPropertyValue<string>(nameof(SpreakBefore)); }
            set { SetPropertyValue(nameof(SpreakBefore), value); }
        }


        public string Emotion
        {
            get { return GetPropertyValue<string>(nameof(Emotion)); }
            set { SetPropertyValue(nameof(Emotion), value); }
        }

        public int Volumn
        {
            get { return GetPropertyValue<int>(nameof(Volumn)); }
            set { SetPropertyValue(nameof(Volumn), value); }
        }

        [Action]
        public async Task ParseArticleText()
        {
            var p = @"
#要识别的小说内容:
" + ArticleText;
            Debug.WriteLine(new string('=', 80));
            Debug.WriteLine(p);
            var rst =await  AIHelper.Ask(AudioBook.SystemPrompt, p, "http://localhost:8000");
            if (!rst.IsError)
            {
                //idx++;
                //var n = new AudioBookSpreakItem(Session);
                //n.Index = idx;
                //n.Paragraph = item;
                //this.SpreakItems.Add(n);
                var n = this;
                try
                {
                    var json = JsonConvert.DeserializeObject<ParseResult>(rst.Message);
                    n.Spreaker = json.Spreaker;
                    n.SpreakContent = json.SpreakContent;
                    n.SpreakBefore = json.SpreakBefore;
                    n.Emotion = json.Emotion;
                    n.Volumn = json.Volume;
                    n.AudioRole = await AudioBook.CreateAudioRole(n.Spreaker);
                }
                catch (Exception ex)
                {
                    n.SpreakContent = rst.Message;
                    n.SpreakBefore = "报错了:" + ex.Message;
                }
            }
            Debug.WriteLine(new string('-', 80));
            Debug.WriteLine(rst.Message);
        }
    }

    ///// <summary>
    ///// 某章节中的一个段落
    ///// </summary>
    //public class AudioBookParagraphItem : XPObject
    //{
    //    public AudioBookParagraphItem(Session s) : base(s)
    //    {

    //    }
    //    [Association]
    //    public AudioBook AudioBook
    //    {
    //        get { return GetPropertyValue<AudioBook>(nameof(AudioBook)); }
    //        set { SetPropertyValue(nameof(AudioBook), value); }
    //    }

    //    public int Index
    //    {
    //        get { return GetPropertyValue<int>(nameof(Index)); }
    //        set { SetPropertyValue(nameof(Index), value); }
    //    }


    //    [Size(-1)]
    //    public string Text
    //    {
    //        get { return GetPropertyValue<string>(nameof(Text)); }
    //        set { SetPropertyValue(nameof(Text), value); }
    //    }
    //}
}
