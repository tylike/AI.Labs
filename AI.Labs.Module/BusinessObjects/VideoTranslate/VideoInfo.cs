﻿using DevExpress.ExpressApp.DC;
using DevExpress.Xpo;
//using SubtitlesParser.Classes; // 引入SubtitlesParser的命名空间
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;

using AI.Labs.Module.BusinessObjects.AudioBooks;
using AI.Labs.Module.BusinessObjects.STT;
using System.Security.Cryptography;
using AI.Labs.Module.BusinessObjects.Helper;
using System.Text;
using AI.Labs.Module.BusinessObjects.TTS;
using DevExpress.Persistent.Validation;
using YoutubeExplode.Demo.Cli;
//using SubtitlesParser.Classes.Parsers;
namespace AI.Labs.Module.BusinessObjects.VideoTranslate
{

    [XafDisplayName("视频")]
    [NavigationItem("视频翻译")]    
    public partial class VideoInfo : SimpleXPObject
    {
        [Action(Caption ="下载字幕")]
        public async void DownloadClosedCaption()
        {
            准备();
            await YE.DownloadClosedCaption(this.VideoURL, this.ProjectPath, p => this.DownloadProgress = p, null);
        }

        public void 准备()
        {
            if (string.IsNullOrEmpty(ProjectPath))
            {
                ProjectPath = Path.Combine(@"d:\VideoInfo", Oid.ToString());
                //创建空白方案
                var audio = GetCnAudioSolution();
            }

            if (!Directory.Exists(ProjectPath))
            {
                Directory.CreateDirectory(ProjectPath);
            }

            EncodingProvider provider = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(provider);
        }

        [Action(Caption ="下载音频")]
        public async void DownloadAuduio()
        {
            准备();
            await YE.DownloadAudio(this.VideoURL, this.ProjectPath, p => this.DownloadProgress = p, null);
        }

#warning 5.最终实现全部的自动化，可以自动下载一个频道的所有视频，然后自动翻译，自动上传到剪映
        public VideoInfo(Session s) : base(s)
        {

        }

        public AudioBook GetCnAudioSolution()
        {
            if (!IsLoading)
            {
                if (this.CnAudioSolution == null)
                {

                    var audioSolution = new AudioBook(Session);
                    audioSolution.VideoInfo = this;
                    audioSolution.Content = ContentCn;
                    audioSolution.Name = Title;

                    audioSolution.OutputPath = Path.Combine(ProjectPath, $"Audio");
                    audioSolution.CheckOutputPath();
                    this.CnAudioSolution = audioSolution;
                }

                return this.CnAudioSolution;
            }
            throw new Exception("加载时不要调用此方法!");
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
        [Association]
        public YoutubeChannel Channel
        {
            get { return GetPropertyValue<YoutubeChannel>(nameof(Channel)); }
            set { SetPropertyValue(nameof(Channel), value); }
        }

        [XafDisplayName("时长")]
        [Persistent("DurationTime")]
        public TimeSpan Duration
        {
            get { return GetPropertyValue<TimeSpan>(nameof(Duration)); }
            set { SetPropertyValue(nameof(Duration), value); }
        }

        [XafDisplayName("中文视频时长")]
        public TimeSpan CnVideoDuration
        {
            get { return GetPropertyValue<TimeSpan>(nameof(CnVideoDuration)); }
            set { SetPropertyValue(nameof(CnVideoDuration), value); }
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

        //IMediaInfo info;
        //public async Task<IMediaInfo> GetMediaInfo()
        //{
        //    if (info == null)
        //    {
        //        try
        //        {
        //            //FFMpegCore.FFMpeg.
        //            info = await FFmpeg.GetMediaInfo(VideoFile);
        //        }
        //        catch (Exception ex)
        //        {
        //            throw ex;
        //        }
        //    }
        //    return info;
        //}

        public int Width
        {
            get
            {
                return GetPropertyValue<int>(nameof(Width));
            }
            set
            {
                SetPropertyValue(nameof(Width), value);
            }
        }

        public int Height
        {
            get { return GetPropertyValue<int>(nameof(Height)); }
            set { SetPropertyValue(nameof(Height), value); }
        }
        public async Task GetVideoScreenSize()
        {
            if (!string.IsNullOrEmpty(VideoFile) && File.Exists(VideoFile))
            {
#warning 没有去真正计算视频的大小
                if (Width == 0)
                    this.Width = 1280;// (await GetMediaInfo()).VideoStreams.First().Width;
                if (Height == 0)
                    this.Height = 720;// (await GetMediaInfo()).VideoStreams.First().Height;
            }
        }

        public VisualFilterComplexScript VideoScript
        {
            get
            {
                return GetPropertyValue<VisualFilterComplexScript>(nameof(VideoScript));
            }
            set
            {
                SetPropertyValue(nameof(VideoScript), value);
            }
        }

        #region srt
        public static string InsertNewlines(string text, int maxLineLength)
        {
            StringBuilder sb = new StringBuilder();
            int currentLineLength = 0;

            foreach (char c in text)
            {
                if (currentLineLength >= maxLineLength && !char.IsWhiteSpace(c))
                {
                    sb.Append("\n");
                    currentLineLength = 0;
                }

                sb.Append(c);
                currentLineLength++;

                // Reset line length after newline
                if (c == '\n' || c == '\r' || c == '\\')
                {
                    currentLineLength = 0;
                }
            }

            return sb.ToString();
        }

        public (string CnSRT, string EnSRT) SaveFixedSRT()
        {
            var cnSrtFile = new SRTFile() { FileName = Path.Combine(ProjectPath, "cnsrt.fix.srt"), UseIndex = true };
            var enSrtFile = new SRTFile() { FileName = Path.Combine(ProjectPath, "ensrt.fix.srt"), UseIndex = true };

            foreach (var item in Audios)
            {
                #region 写字幕文件
                var cnText = item.Subtitle.CnText;
                var cnSrtItem = new SRT();

                if (!string.IsNullOrEmpty(cnText))
                {
                    cnText = InsertNewlines(cnText.Replace("\n", ""), 40);
                    cnSrtItem.Index = item.Index;
                    cnSrtItem.StartTime = item.Subtitle.FixedStartTime;
                    cnSrtItem.EndTime = item.Subtitle.FixedEndTime;
                    cnSrtItem.Text = cnText;
                    cnSrtFile.Texts.Add(cnSrtItem);
                }

                enSrtFile.Texts.Add(new SRT
                {
                    Index = item.Index,
                    StartTime = cnSrtItem.StartTime,
                    EndTime = cnSrtItem.EndTime,
                    Text = item.Subtitle.PlainText
                });
                #endregion
            }
            cnSrtFile.Save();
            enSrtFile.Save();
            return (cnSrtFile.FileName, enSrtFile.FileName);
        }
        #endregion

        [XafDisplayName("语言模型")]
        public AIModel Model
        {
            get => TranslateAgent?.Model;
        }

        //[XafDisplayName("语音识别")]
        //public STTModel STTModel
        //{
        //    get { return GetPropertyValue<STTModel>(nameof(STTModel)); }
        //    set { SetPropertyValue(nameof(STTModel), value); }
        //}

        //[XafDisplayName("识别提示")]
        //[ToolTip("用于从语音中识别字幕时的提示词")]
        //[Size(-1)]
        //public string STTPrompt
        //{
        //    get
        //    {
        //        return GetPropertyValue<string>(nameof(STTPrompt)) ?? "按语义填加标点符号,尽量每个完整句子为一段";
        //    }
        //    set { SetPropertyValue(nameof(STTPrompt), value); }
        //}

        [XafDisplayName("转录设置")]
        public SubtitleTranscriptionAgent SubtitleTranscriptionAgent
        {
            get { return GetPropertyValue<SubtitleTranscriptionAgent>(nameof(SubtitleTranscriptionAgent)); }
            set { SetPropertyValue(nameof(SubtitleTranscriptionAgent), value); }
        }

        [XafDisplayName("强制时长")]
        public int ForceDuration
        {
            get { return GetPropertyValue<int>(nameof(ForceDuration)); }
            set { SetPropertyValue(nameof(ForceDuration), value); }
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
            //TranslateTaskPrompt = @"用口语化的风格将英文字翻译为中文,专用的名词不需要翻译,如Transformers,Llama,Lang,LlamaIndex等
            //不要啰嗦,接地气,调侃式的文风";
            //AddSymbolPrompt = @"将以下内容加上标点符号,保持原有格式，保留换行，不要合并多条，不要移动单词位置，一段字幕没有表达完整时末尾加上省略号，不要输出除字幕以外的解释，结果是纯文本格式:";
            //FixSubtitleBatchCount = 30;
            //Environment.SpecialFolder
            //STTPrompt = "";

            JianYingProjectFile = GetDefaultJianYingProjectPath();
            //Model = Session.Query<AIModel>().FirstOrDefault(t => t.IsDefault);
            TranslateAgent = Session.Query<TranslateAgent>().FirstOrDefault(t => t.IsDefault);
            SubtitleTranscriptionAgent = Session.Query<SubtitleTranscriptionAgent>().FirstOrDefault(t => t.IsDefault);
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
                this.VideoScript = new VisualFilterComplexScript(Session);
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

        [XafDisplayName("翻译设置")]
        public TranslateAgent TranslateAgent
        {
            get { return GetPropertyValue<TranslateAgent>(nameof(TranslateAgent)); }
            set { SetPropertyValue(nameof(TranslateAgent), value); }
        }


        //[XafDisplayName("翻译要求")]
        //[Size(-1)]
        //public string TranslateTaskPrompt
        //{
        //    get { return GetPropertyValue<string>(nameof(TranslateTaskPrompt)); }
        //    set { SetPropertyValue(nameof(TranslateTaskPrompt), value); }
        //}

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
        [Association, DevExpress.Xpo.Aggregated, XafDisplayName("视频章节")]
        public XPCollection<Chapter> Chapters => GetCollection<Chapter>(nameof(Chapters));

        [XafDisplayName("音频")]
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

        public void SaveSRTToFile(SrtLanguage lang, string addationName = "", bool saveFixed = false)
        {
            var t = this;
            //保存翻译结果
            var fileName = Path.Combine(t.ProjectPath, $"{t.Oid}.{lang.ToString()}{addationName}.srt");
            SRTHelper.SaveToSrtFile(t.Subtitles, fileName, lang, saveFixed);
            if (lang == SrtLanguage.中文)
            {
                t.VideoChineseSRT = fileName;
            }
        }

        [XafDisplayName("识别开始时间")]
        [ToolTip("在识别音频时的开始时间（秒）")]
        public int AudioStartTime
        {
            get { return GetPropertyValue<int>(nameof(AudioStartTime)); }
            set { SetPropertyValue(nameof(AudioStartTime), value); }
        }


        [XafDisplayName("识别结束时间")]
        [ToolTip("在识别音频时的结束时间（秒）")]
        public int AudioEndTime
        {
            get { return GetPropertyValue<int>(nameof(AudioEndTime)); }
            set { SetPropertyValue(nameof(AudioEndTime), value); }
        }

    }

    public class Log : XPObject
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

    [XafDefaultProperty(nameof(Name))]
    [NavigationItem("视频翻译")]
    [XafDisplayName("转录设置")]
    public class SubtitleTranscriptionAgent : SimpleXPObject
    {
        public SubtitleTranscriptionAgent(Session s) : base(s)
        {

        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            Model = Session.Query<STTModel>().FirstOrDefault(t => t.IsDefault);
            Prompt = "按语义填加标点符号,尽量每个完整句子为一段,*** 注意正确的使用句号、逗号、感叹号、问号等标点符号。";
        }

        [XafDisplayName("名称")]
        [RuleRequiredField]
        public string Name
        {
            get { return GetPropertyValue<string>(nameof(Name)); }
            set { SetPropertyValue(nameof(Name), value); }
        }


        [XafDisplayName("模型")]
        [RuleRequiredField]
        public STTModel Model
        {
            get { return GetPropertyValue<STTModel>(nameof(Model)); }
            set { SetPropertyValue(nameof(Model), value); }
        }

        [XafDisplayName("要求提示")]
        [RuleRequiredField]
        [Size(-1)]
        public string Prompt
        {
            get { return GetPropertyValue<string>(nameof(Prompt)); }
            set { SetPropertyValue(nameof(Prompt), value); }
        }

        [XafDisplayName("默认")]
        public bool IsDefault
        {
            get { return GetPropertyValue<bool>(nameof(IsDefault)); }
            set { SetPropertyValue(nameof(IsDefault), value); }
        }
    }

    [XafDefaultProperty(nameof(Name))]
    [NavigationItem("视频翻译")]
    [XafDisplayName("翻译设置")]
    public class TranslateAgent : SimpleXPObject
    {
        public TranslateAgent(Session s) : base(s)
        {

        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            UserPrompt = @"用口语化的风格将英文字翻译为中文,专用的名词不需要翻译,如Transformers,Llama,Lang,LlamaIndex等
不要啰嗦,接地气,调侃式的文风";
            this.ContextCount = 10;
            //AddSymbolPrompt = @"将以下内容加上标点符号,保持原有格式，保留换行，不要合并多条，不要移动单词位置，一段字幕没有表达完整时末尾加上省略号，不要输出除字幕以外的解释，结果是纯文本格式:";
            //FixSubtitleBatchCount = 30;
            //Environment.SpecialFolder
            //STTPrompt = "";
            Model = Session.Query<AIModel>().FirstOrDefault(t => t.IsDefault);
        }


        [XafDisplayName("名称")]
        [RuleRequiredField]
        public string Name
        {
            get { return GetPropertyValue<string>(nameof(Name)); }
            set { SetPropertyValue(nameof(Name), value); }
        }

        [Size(-1)]
        public string 可用参数
        =>
             @"{contexts}:指相关的上下文
{text}:指要翻译的文本
{ignore}:指可以忽略的一些词
            ";


        [XafDisplayName("用户提示"), Size(-1)]
        public string UserPrompt
        {
            get { return GetPropertyValue<string>(nameof(UserPrompt)); }
            set { SetPropertyValue(nameof(UserPrompt), value); }
        }
        [XafDisplayName("系统提示"), Size(-1)]
        public string SystemPrompt
        {
            get { return GetPropertyValue<string>(nameof(SystemPrompt)); }
            set { SetPropertyValue(nameof(SystemPrompt), value); }
        }

        [XafDisplayName("语言模型")]
        [RuleRequiredField]
        public AIModel Model
        {
            get { return GetPropertyValue<AIModel>(nameof(Model)); }
            set { SetPropertyValue(nameof(Model), value); }
        }

        [XafDisplayName("温度")]
        public decimal Temperature
        {
            get { return GetPropertyValue<decimal>(nameof(Temperature)); }
            set { SetPropertyValue(nameof(Temperature), value); }
        }

        [XafDisplayName("参考数量")]
        [ToolTip("如果有参数内容是分条传入的,使用几条")]
        public int ContextCount
        {
            get { return GetPropertyValue<int>(nameof(ContextCount)); }
            set { SetPropertyValue(nameof(ContextCount), value); }
        }

        [XafDisplayName("参考长度")]
        [ToolTip("如果有参数内容是分条传入的,使用几个Token")]
        public int ContextTokenCount
        {
            get { return GetPropertyValue<int>(nameof(ContextTokenCount)); }
            set { SetPropertyValue(nameof(ContextTokenCount), value); }
        }

        [XafDisplayName("默认配置")]
        public bool IsDefault
        {
            get { return GetPropertyValue<bool>(nameof(IsDefault)); }
            set { SetPropertyValue(nameof(IsDefault), value); }
        }

    }

}
