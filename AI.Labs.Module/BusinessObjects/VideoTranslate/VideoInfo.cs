using DevExpress.ExpressApp.DC;
using DevExpress.Xpo;
using System.Text;
//using SubtitlesParser.Classes; // 引入SubtitlesParser的命名空间
using System.Text.RegularExpressions;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;
using System.Windows.Forms;
using System.Drawing;
using DevExpress.Persistent.Validation;
using System.IO;
using Newtonsoft.Json;
using com.sun.org.apache.bcel.@internal.generic;
using AI.Labs.Module.BusinessObjects.AudioBooks;
using AI.Labs.Module.BusinessObjects.STT;
using System.Security.Cryptography;
using AI.Labs.Module.BusinessObjects.Helper;
//using SubtitlesParser.Classes.Parsers;
namespace AI.Labs.Module.BusinessObjects.VideoTranslate
{
    public class SrtChecker
    {
        public static string CheckSrtFile(string filePath)
        {
            var sb = new StringBuilder();
            var lines = File.ReadAllLines(filePath);
            var regex = new Regex(@"^\d+$");
            var timeRegex = new Regex(@"\d{2}:\d{2}:\d{2},\d{3} --> \d{2}:\d{2}:\d{2},\d{3}");

            for (int i = 0; i < lines.Length; i++)
            {
                if (i % 4 == 0 && !regex.IsMatch(lines[i]))
                {
                    sb.AppendLine($"错误行号: {i + 1}: {lines[i]} 不是有效的行号.");
                }
                else if (i % 4 == 1 && !timeRegex.IsMatch(lines[i]))
                {
                    sb.AppendLine($"错误行号: {i + 1}: {lines[i]} 不是有效的时间格式.");
                }
            }
            return sb.ToString();
        }
        public static string CheckSrt(string input)
        {
            var sb = new StringBuilder();
            var lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var regex = new Regex(@"^\d+$");
            var timeRegex = new Regex(@"\d{2}:\d{2}:\d{2},\d{3} --> \d{2}:\d{2}:\d{2},\d{3}");

            for (int i = 0; i < lines.Length; i++)
            {
                if (i % 4 == 0 && !regex.IsMatch(lines[i]))
                {
                    sb.AppendLine($"错误行号: {i + 1}: {lines[i]} 不是有效的行号.");
                }
                else if (i % 4 == 1 && !timeRegex.IsMatch(lines[i]))
                {
                    sb.AppendLine($"错误行号: {i + 1}: {lines[i]} 不是有效的时间格式.");
                }
            }
            return sb.ToString();
        }
    }
    public class SrtFixer
    {
        public static string AutoFixSrtFormat(string input)
        {
            var lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var sb = new StringBuilder();
            for (int i = 0; i < lines.Length; i++)
            {
                if (i > 0 && IsNumeric(lines[i]) && !string.IsNullOrWhiteSpace(lines[i - 1]))
                {
                    sb.AppendLine();
                }
                sb.AppendLine(lines[i]);
            }
            return sb.ToString();
        }

        public static void AutoFixSrtFileFormat(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            using (var sw = new StreamWriter(filePath))
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    if (i > 0 && IsNumeric(lines[i]) && !string.IsNullOrWhiteSpace(lines[i - 1]))
                    {
                        sw.WriteLine();
                    }
                    sw.WriteLine(lines[i]);
                }
            }
        }

        private static bool IsNumeric(string line)
        {
            return Regex.IsMatch(line, @"^\d+$");
        }
    }

    public static class StringExtensions
    {
        //c#中，如何将一个字符串按长度分成N个组
        public static IEnumerable<string> SplitIntoGroups(this string input, int groupSize)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException("Input string cannot be null or empty.");

            if (groupSize <= 0)
                throw new ArgumentException("Group size must be greater than zero.");

            for (int i = 0; i < input.Length; i += groupSize)
            {
                yield return input.Substring(i, Math.Min(groupSize, input.Length - i));
            }
        }
    }

    public class YoutubeChannel : SimpleXPObject
    {
        public YoutubeChannel(Session s) : base(s)
        {

        }
        [RuleRequiredField]
        public string ChannelID
        {
            get { return GetPropertyValue<string>(nameof(ChannelID)); }
            set { SetPropertyValue(nameof(ChannelID), value); }
        }

        public string ChannelName
        {
            get { return GetPropertyValue<string>(nameof(ChannelName)); }
            set { SetPropertyValue(nameof(ChannelName), value); }
        }
        [RuleRequiredField]
        [Size(-1)]
        public string ChannelUrl
        {
            get { return GetPropertyValue<string>(nameof(ChannelUrl)); }
            set { SetPropertyValue(nameof(ChannelUrl), value); }
        }
    }

    public class FFmpegScript : SimpleXPObject
    {
        public FFmpegScript(Session s) : base(s)
        {

        }


        public VideoInfo Video
        {
            get { return GetPropertyValue<VideoInfo>(nameof(Video)); }
            set { SetPropertyValue(nameof(Video), value); }
        }


        [Size(-1)]
        [ModelDefault("RowCount","0")]
        public string StartCommand
        {
            get { return GetPropertyValue<string>(nameof(StartCommand)); }
            set { SetPropertyValue(nameof(StartCommand), value); }
        }

        [Size(-1)]
        public string FilterComplexText
        {
            get { return GetPropertyValue<string>(nameof(FilterComplexText)); }
            set { SetPropertyValue(nameof(FilterComplexText), value); }
        }

        [Size(-1)]
        public string Output
        {
            get { return GetPropertyValue<string>(nameof(Output)); }
            set { SetPropertyValue(nameof(Output), value); }
        }
    }

    [XafDisplayName("视频")]
    [NavigationItem("视频翻译")]
    public class VideoInfo : SimpleXPObject
    {
#warning 4.一键完成所有工作    手动创建剪映项目，导出视频，然后上传，准备的标题等
#warning 5.最终实现全部的自动化，可以自动下载一个频道的所有视频，然后自动翻译，自动上传到剪映
        public VideoInfo(Session s) : base(s)
        {

        }

        #region 基本信息
        [Size(-1)]
        [ModelDefault("RowCount", "0")]
        [XafDisplayName("来源视频")]
        public string VideoURL
        {
            get { return GetPropertyValue<string>(nameof(VideoURL)); }
            set { SetPropertyValue(nameof(VideoURL), value); }
        }
        [XafDisplayName("项目路径")]
        [Size(-1)]
        [ModelDefault("RowCount", "1")]
        //[RuleRequiredField]
        public string ProjectPath
        {
            get { return GetPropertyValue<string>(nameof(ProjectPath)); }
            set { SetPropertyValue(nameof(ProjectPath), value); }
        }

        [XafDisplayName("标题")]
        public string Title
        {
            get { return GetPropertyValue<string>(nameof(Title)); }
            set { SetPropertyValue(nameof(Title), value); }
        }

        [XafDisplayName("中文标题")]
        public string TitleCn
        {
            get { return GetPropertyValue<string>(nameof(TitleCn)); }
            set { SetPropertyValue(nameof(TitleCn), value); }
        }


        [Size(-1)]
        [XafDisplayName("内容说明")]
        public string Description
        {
            get { return GetPropertyValue<string>(nameof(Description)); }
            set { SetPropertyValue(nameof(Description), value); }
        }

        [Size(-1)]
        [XafDisplayName("中文内容")]
        public string DescriptionCn
        {
            get { return GetPropertyValue<string>(nameof(DescriptionCn)); }
            set { SetPropertyValue(nameof(DescriptionCn), value); }
        }


        [Size(-1)]
        [XafDisplayName("关键词")]
        public string Keywords
        {
            get { return GetPropertyValue<string>(nameof(Keywords)); }
            set { SetPropertyValue(nameof(Keywords), value); }
        }

        [Size(-1)]
        [XafDisplayName("中文关键词")]
        public string KeywordsCn
        {
            get { return GetPropertyValue<string>(nameof(KeywordsCn)); }
            set { SetPropertyValue(nameof(KeywordsCn), value); }
        }
        #endregion

        #region 视频文件
        [XafDisplayName("视频文件")]
        [Size(-1)]
        [ModelDefault("RowCount", "1")]
        //[RuleRequiredField]
        public string VideoFile
        {
            get { return GetPropertyValue<string>(nameof(VideoFile)); }
            set { SetPropertyValue(nameof(VideoFile), value); }
        }

        [XafDisplayName("下载进度")]
        [ModelDefault("DisplayFormat", "P")]
        public double DownloadProgress
        {
            get { return GetPropertyValue<double>(nameof(DownloadProgress)); }
            set { SetPropertyValue(nameof(DownloadProgress), value); }
        }


        [XafDisplayName("中文视频")]
        [Size(-1)]
        [ModelDefault("RowCount", "1")]
        public string VideoFileCn
        {
            get { return GetPropertyValue<string>(nameof(VideoFileCn)); }
            set { SetPropertyValue(nameof(VideoFileCn), value); }
        }
        #endregion

        #region 音频文件
        [XafDisplayName("中文音频方案")]
        public AudioBook CnAudioSolution
        {
            get { return GetPropertyValue<AudioBook>(nameof(CnAudioSolution)); }
            set { SetPropertyValue(nameof(CnAudioSolution), value); }
        }

        [XafDisplayName("音频文件")]
        [Size(-1)]
        [ModelDefault("RowCount", "1")]
        public string AudioFile
        {
            get { return GetPropertyValue<string>(nameof(AudioFile)); }
            set { SetPropertyValue(nameof(AudioFile), value); }
        }

        [XafDisplayName("中文音频")]
        [Size(-1)]
        [ModelDefault("RowCount", "1")]
        public string AudioFileCn
        {
            get { return GetPropertyValue<string>(nameof(AudioFileCn)); }
            set { SetPropertyValue(nameof(AudioFileCn), value); }
        }
        #endregion

        #region 字幕文件
        [XafDisplayName("识别说话人")]
        public bool ParseSpreaker
        {
            get { return GetPropertyValue<bool>(nameof(ParseSpreaker)); }
            set { SetPropertyValue(nameof(ParseSpreaker), value); }
        }

        [XafDisplayName("字幕文件")]
        [Size(-1)]
        [ModelDefault("RowCount", "1")]
        //[RuleRequiredField]
        public string VideoDefaultSRT
        {
            get { return GetPropertyValue<string>(nameof(VideoDefaultSRT)); }
            set { SetPropertyValue(nameof(VideoDefaultSRT), value); }
        }

        [XafDisplayName("详细字幕")]
        [Size(-1)]
        [ModelDefault("RowCount", "1")]
        public string VideoJsonSRT
        {
            get { return GetPropertyValue<string>(nameof(VideoJsonSRT)); }
            set { SetPropertyValue(nameof(VideoJsonSRT), value); }
        }


        [XafDisplayName("中文字幕")]
        [Size(-1)]
        [ModelDefault("RowCount", "1")]
        public string VideoChineseSRT
        {
            get
            {
                var rst = GetPropertyValue<string>(nameof(VideoChineseSRT));
                if (string.IsNullOrEmpty(rst))
                {
                    if (!string.IsNullOrEmpty(ProjectPath))
                    {
                        rst = Path.Combine(ProjectPath, $"{this.Oid.ToString()}.cn.srt");
                    }
                }
                return rst;
            }
            set
            {
                SetPropertyValue(nameof(VideoChineseSRT), value);
            }
        }
        #endregion

        #region 来源信息
        [XafDisplayName("作者")]
        public YoutubeChannel Channel
        {
            get { return GetPropertyValue<YoutubeChannel>(nameof(Channel)); }
            set { SetPropertyValue(nameof(Channel), value); }
        }



        [XafDisplayName("时长")]
        public string Duration
        {
            get { return GetPropertyValue<string>(nameof(Duration)); }
            set { SetPropertyValue(nameof(Duration), value); }
        }

        #region 播放统计
        [XafDisplayName("播放次数")]
        public int ViewCount
        {
            get { return GetPropertyValue<int>(nameof(ViewCount)); }
            set { SetPropertyValue(nameof(ViewCount), value); }
        }
        [XafDisplayName("喜欢")]
        public int Like
        {
            get { return GetPropertyValue<int>(nameof(Like)); }
            set { SetPropertyValue(nameof(Like), value); }
        }
        [XafDisplayName("不喜欢")]
        public int DisLike
        {
            get { return GetPropertyValue<int>(nameof(DisLike)); }
            set { SetPropertyValue(nameof(DisLike), value); }
        }

        [XafDisplayName("评星")]
        public decimal AverageRating
        {
            get { return GetPropertyValue<decimal>(nameof(AverageRating)); }
            set { SetPropertyValue(nameof(AverageRating), value); }
        }
        #endregion

        [XafDisplayName("上传日期")]
        public DateTime? UploadDate
        {
            get { return GetPropertyValue<DateTime?>(nameof(UploadDate)); }
            set { SetPropertyValue(nameof(UploadDate), value); }
        }

        [Size(-1)]
        [XafDisplayName("封面图片")]
        [ModelDefault("RowCount", "1")]
        public string ImageTitle
        {
            get { return GetPropertyValue<string>(nameof(ImageTitle)); }
            set { SetPropertyValue(nameof(ImageTitle), value); }
        }
        #endregion

        #region 使用大模型对话来修复字幕，如去掉so/now等无意义重复内容
        //[EditorAlias(EditorAliases.RichTextPropertyEditor)]
        [Size(-1)]
        public string FixSRTPrompt
        {
            get { return GetPropertyValue<string>(nameof(FixSRTPrompt)); }
            set { SetPropertyValue(nameof(FixSRTPrompt), value); }
        }


        public bool FixSRTIncludeContext
        {
            get { return GetPropertyValue<bool>(nameof(FixSRTIncludeContext)); }
            set { SetPropertyValue(nameof(FixSRTIncludeContext), value); }
        }
        #endregion

        #region old
        //[XafDisplayName("修复开始")]
        //public int FixSubtitleStartIndex
        //{
        //    get { return GetPropertyValue<int>(nameof(FixSubtitleStartIndex)); }
        //    set { SetPropertyValue(nameof(FixSubtitleStartIndex), value); }
        //}
        //[EditorAlias(EditorAliases.SpreadsheetPropertyEditor)]
        //public byte[] SubtitleSheetEditor
        //{
        //    get { return GetPropertyValue<byte[]>(nameof(SubtitleSheetEditor)); }
        //    set { SetPropertyValue(nameof(SubtitleSheetEditor), value); }
        //}

        //[XafDisplayName("修复条数")]
        //[RuleRange(1, 1000)]
        //public int FixSubtitleBatchCount
        //{
        //    get { return GetPropertyValue<int>(nameof(FixSubtitleBatchCount)); }
        //    set { SetPropertyValue(nameof(FixSubtitleBatchCount), value); }
        //}

        //[XafDisplayName("拆分合并")]
        //[EditorAlias(EditorAliases.SpreadsheetPropertyEditor)]
        //public byte[] SplitMerge
        //{
        //    get { return GetPropertyValue<byte[]>(nameof(SplitMerge)); }
        //    set { SetPropertyValue(nameof(SplitMerge), value); }
        //} 
        #endregion


        public FFmpegScript VideoScript
        {
            get 
            {
                return GetPropertyValue<FFmpegScript>(nameof(VideoScript));
            }
            set 
            {
                SetPropertyValue(nameof(VideoScript), value); 
            }
        }


        [XafDisplayName("语言模型")]
        public AIModel Model
        {
            get { return GetPropertyValue<AIModel>(nameof(Model)); }
            set { SetPropertyValue(nameof(Model), value); }
        }

        [XafDisplayName("语音识别")]
        public STTModel STTModel
        {
            get { return GetPropertyValue<STTModel>(nameof(STTModel)); }
            set { SetPropertyValue(nameof(STTModel), value); }
        }

        [XafDisplayName("识别提示")]
        [ToolTip("用于从语音中识别字幕时的提示词")]
        [Size(-1)]
        public string STTPrompt
        {
            get
            {
                return GetPropertyValue<string>(nameof(STTPrompt)) ?? "按语义填加标点符号,尽量每个完整句子为一段";
            }
            set { SetPropertyValue(nameof(STTPrompt), value); }
        }



        public bool TinyDiarize
        {
            get { return GetPropertyValue<bool>(nameof(TinyDiarize)); }
            set { SetPropertyValue(nameof(TinyDiarize), value); }
        }


        string GetDefaultJianYingProjectPath()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return @$"{path}\AppData\Local\JianyingPro\User Data\Projects\com.lveditor.draft\你的项目名称\draft_content.json";
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            TranslateTaskPrompt = @"请将英文字成为中文,专用的名词不需要翻译,如Transformers,Llama,Lang,LlamaIndex等";
            //AddSymbolPrompt = @"将以下内容加上标点符号,保持原有格式，保留换行，不要合并多条，不要移动单词位置，一段字幕没有表达完整时末尾加上省略号，不要输出除字幕以外的解释，结果是纯文本格式:";
            //FixSubtitleBatchCount = 30;
            //Environment.SpecialFolder


            JianYingProjectFile = GetDefaultJianYingProjectPath();
            Model = Session.Query<AIModel>().FirstOrDefault(t => t.IsDefault);
            CreateScriptObject();
        }
        protected override void OnLoaded()
        {
            base.OnLoaded();
            CreateScriptObject();
        }
        private void CreateScriptObject()
        {
            if (this.VideoScript == null)
            {
                this.VideoScript = new FFmpegScript(Session);
                this.VideoScript.Video = this;
            }
        }

        [XafDisplayName("忽略翻译")]
        [ToolTip("在翻译时应该忽略这些单词")]
        [Size(-1)]
        public string TranslateIgnoreWords
        {
            get { return GetPropertyValue<string>(nameof(TranslateIgnoreWords)); }
            set { SetPropertyValue(nameof(TranslateIgnoreWords), value); }
        }

        [XafDisplayName("翻译要求")]
        [Size(-1)]
        public string TranslateTaskPrompt
        {
            get { return GetPropertyValue<string>(nameof(TranslateTaskPrompt)); }
            set { SetPropertyValue(nameof(TranslateTaskPrompt), value); }
        }

        //[XafDisplayName("标点要求")]
        //[Size(-1)]
        //[RuleRequiredField]
        //public string AddSymbolPrompt
        //{
        //    get { return GetPropertyValue<string>(nameof(AddSymbolPrompt)); }
        //    set { SetPropertyValue(nameof(AddSymbolPrompt), value); }
        //}
        //[Action(Caption = "生成加标点任务")]
        //public void CreateAddSymbolTask()
        //{
        //    int index = 0;
        //    var task = new TranslateTask(Session);
        //    task.Index = index;
        //    index++;
        //    this.TranslateTasks.Add(task);
        //    var limitLength = 2000;
        //    foreach (var item in Subtitles)
        //    {
        //        if ((task.Text + "").Length + item.Lines.Length < limitLength)
        //        {
        //            task.Text += " " + item.Lines;
        //            task.SubtitleItems.Add(item);
        //        }
        //        else
        //        {
        //            task = new TranslateTask(Session);
        //            task.Index = index;
        //            this.TranslateTasks.Add(task);
        //            index++;
        //        }
        //    }
        //}
        //static string[] symbols = new string[] { ".", ",", "!", "?", "。", ",", "！", "？" };
        //bool IsSymbol(string text)
        //{
        //    foreach (var x in symbols)
        //    {
        //        if (text == x)
        //        {
        //            return true;
        //        }
        //        if (text.EndsWith(x))
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}
        //[Action]
        //public void SaveToFile()
        //{
        //    File.WriteAllText(VideoDefaultSRT + ".onlytext.txt", ContentEn);
        //}

        //[Action(Caption = "生成翻译任务")]
        //public void GenerateTranslateTask()
        //{
        //    if (PerTaskTranslateCount < 0)
        //    {
        //        throw new UserFriendlyException("错误:没有设置任务翻译的数量!");
        //    }

        //    int pageSize = PerTaskTranslateCount;
        //    var source = Subtitles.OrderBy(t => t.Index).ToArray();
        //    int totalRecords = source.Count();
        //    int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
        //    for (int page = 1; page <= totalPages; page++)
        //    {
        //        var pagedData = source.Skip((page - 1) * pageSize).Take(pageSize);
        //        // 对数据进行相应的处理
        //        var task = new TranslateTask(Session);
        //        task.Index = page;
        //        task.SubtitleItems.AddRange(pagedData);
        //        this.TranslateTasks.Add(task);
        //    }
        //}

        //[XafDisplayName("任务翻译")]
        //[ToolTip("生成的每个任务的翻译字幕的条数")]
        //public int PerTaskTranslateCount
        //{
        //    get { return GetPropertyValue<int>(nameof(PerTaskTranslateCount)); }
        //    set { SetPropertyValue(nameof(PerTaskTranslateCount), value); }
        //}

        #region 视频内容总结-根据字幕
        [XafDisplayName("字幕内容")]
        [ModelDefault("RowCount", "10")]

        public string ContentEn
        {
            get => string.Join(" ", Subtitles.Select(t => t.Lines));
        }

        [ModelDefault("RowCount", "10")]
        [XafDisplayName("中文内容")]
        public string ContentCn
        {
            get => string.Join(" ", Subtitles.Select(t => t.CnText));
        }
        #endregion

        [XafDisplayName("剪映文件")]
        [Size(-1)]
        [ModelDefault("RowCount", "1")]
        public string JianYingProjectFile
        {
            get
            {
                return GetPropertyValue<string>(nameof(JianYingProjectFile)) ?? GetDefaultJianYingProjectPath();
            }
            set { SetPropertyValue(nameof(JianYingProjectFile), value); }
        }

        #region 子级集合

        public XPCollection<AudioBookTextAudioItem> Audios
        {
            get => CnAudioSolution?.AudioItems;
        }

        [Association]
        [XafDisplayName("字幕")]
        public XPCollection<SubtitleItem> Subtitles
        {
            get { return GetCollection<SubtitleItem>(nameof(Subtitles)); }
        }
        //[Association]
        //public XPCollection<TranslateTask> TranslateTasks
        //{
        //    get => GetCollection<TranslateTask>(nameof(TranslateTasks));
        //}

        [XafDisplayName("视频信息")]
        [Association, DevExpress.Xpo.Aggregated]
        public XPCollection<YoutubeVideoInfo> Infos
        {
            get => GetCollection<YoutubeVideoInfo>(nameof(Infos));
        }

        [Association, DevExpress.Xpo.Aggregated]
        public XPCollection<Log> Logs
        {
            get => GetCollection<Log>(nameof(Logs));
        }
        #endregion
        public static XafApplication Application { get; set; }

        public void AddLog(string title, string content)
        {
            Application.UIThreadInvoke(() =>
            {
                var log = new Log(Session);
                log.Title = title;
                log.Content = content;
                log.Video = this;
                Logs.Add(log);
            });
        }
        public void Output(string str)
        {
            this.VideoScript.Output += $"{Environment.NewLine} {DateTime.Now.TimeOfDay} {str}";
        }

        public void SaveSRTToFile(SrtLanguage lang,string addationName = "",bool saveFixed = false)
        {
            var t = this;
            //保存翻译结果
            var fileName = Path.Combine(t.ProjectPath, $"{t.Oid}.{lang.ToString()}{addationName}.srt");
            SRTHelper.SaveToSrtFile(t.Subtitles, fileName, lang,saveFixed);
            t.VideoChineseSRT = fileName;
        }
    }

    public class Log:XPObject
    {

       public Log(Session s) : base(s)
        {

        }

        [Association]
        public VideoInfo Video
        {
            get { return GetPropertyValue<VideoInfo>(nameof(Video)); }
            set { SetPropertyValue(nameof(Video), value); }
        }


        public string Title
        {
            get { return GetPropertyValue<string>(nameof(Title)); }
            set { SetPropertyValue(nameof(Title), value); }
        }

        [Size(-1)]
        public string Content
        {
            get { return GetPropertyValue<string>(nameof(Content)); }
            set { SetPropertyValue(nameof(Content), value); }
        }
    }
    public class YoutubeVideoInfo : SimpleXPObject
    {
        public YoutubeVideoInfo(Session s) : base(s)
        {

        }

        [Association]
        public VideoInfo VideoInfo
        {
            get { return GetPropertyValue<VideoInfo>(nameof(VideoInfo)); }
            set { SetPropertyValue(nameof(VideoInfo), value); }
        }

        [XafDisplayName("网址")]
        public string Url
        {
            get { return GetPropertyValue<string>(nameof(Url)); }
            set { SetPropertyValue(nameof(Url), value); }
        }

        [XafDisplayName("类型")]
        public YoutubeVideoCategory 类型
        {
            get { return GetPropertyValue<YoutubeVideoCategory>(nameof(类型)); }
            set { SetPropertyValue(nameof(类型), value); }
        }

        public string 分辨率
        {
            get { return GetPropertyValue<string>(nameof(分辨率)); }
            set { SetPropertyValue(nameof(分辨率), value); }
        }

        public string 格式
        {
            get { return GetPropertyValue<string>(nameof(格式)); }
            set { SetPropertyValue(nameof(格式), value); }
        }

        public string 尺寸
        {
            get { return GetPropertyValue<string>(nameof(尺寸)); }
            set { SetPropertyValue(nameof(尺寸), value); }
        }


    }
    public enum YoutubeVideoCategory
    {
        仅有视频,
        仅有音频,
        完整视频
    }
    public static class ListExtensions
    {
        // 分页扩展方法
        public static IEnumerable<IEnumerable<T>> Paginate<T>(this IEnumerable<T> source, int pageSize)
        {
            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");
            }

            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    yield return GetPage(enumerator, pageSize);
                }
            }
        }

        // 用于从迭代器中获取单个分页的帮助器方法
        private static IEnumerable<T> GetPage<T>(IEnumerator<T> source, int pageSize)
        {
            do
            {
                yield return source.Current;
            }
            while (--pageSize > 0 && source.MoveNext());
        }
    }

}
