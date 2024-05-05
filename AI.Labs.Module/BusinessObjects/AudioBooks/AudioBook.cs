using DevExpress.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using System.Diagnostics;
using System.Text.RegularExpressions;
using AI.Labs.Module.BusinessObjects.TTS;
using DevExpress.ExpressApp.Editors;
using NAudio.Wave;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using AI.Labs.Module.BusinessObjects.STT;
using DevExpress.Utils.Filtering.Internal;
using System.Xml;
using sun.tools.tree;

namespace AI.Labs.Module.BusinessObjects.AudioBooks
{
    [NavigationItem("应用场景")]
    [XafDisplayName("有声书籍")]
    public class AudioBook : XPObject, IWordDocument
    {
        [XafDisplayName("服务提供")]
        public TTSProvider SSMLProvider
        {
            get { return GetPropertyValue<TTSProvider>(nameof(SSMLProvider)); }
            set { SetPropertyValue(nameof(SSMLProvider), value); }
        }

        //[Action(Caption ="生成音频",ToolTip ="使用ssml生成音频")]
        public async Task SaySSML()
        {
            if (string.IsNullOrEmpty(this.SSML))
            {
                throw new UserFriendlyException("还没有生成ssml!");
            }
            if (SSMLProvider == null || SSMLProvider.Engine != VoiceEngine.AzureTTS || string.IsNullOrEmpty(SSMLProvider.ApiKey) || string.IsNullOrEmpty(SSMLProvider.BaseUrl))
            {
                throw new UserFriendlyException("1.必须选择AzureTTS类型的服务提供商\n2.必须配置APIKey和网址. \n说明:目前微软是免费的,需要自行申请,申请时需要有外币支付功能的信用卡，但不会扣费。");
            }

            var path = this.CheckOutputPath();
            path = Path.Combine(path, "ssml.mp3");

            await AzureTTSEngine.PlasySSML(this.SSML, SSMLProvider.ApiKey, SSMLProvider.BaseUrl, path);
        }

        public void DefaultSSMLPrompt()
        {
            string getStyles(IEnumerable<VoiceStyle> styles)
            {
                return string.Join(";", styles.Select(t => $"{t.Name}:{t.Caption}"));
            }
            var rw = string.Join("\r\n\n", this.Roles.Select(t => $"{t.Name}的声音名称是:{t.VoiceSolution.DisplayName},说话风格可选:{getStyles(t.VoiceSolution.Styles)}"));

            SSMLPrompt = @$"#要求：
你精通ssml，请根以下小品对话，分析人物性格、说话情绪，生成对应的ssml，考虑使用的声音名称（都是中文角色）、角色扮演(role)、说话风格(style)等参数，人名不需要读出只输出说话内容即可
{rw}
输出示例:
<speak xmlns=""http://www.w3.org/2001/10/synthesis"" xmlns:mstts=""http://www.w3.org/2001/mstts"" xmlns:emo=""http://www.w3.org/2009/10/emotionml"" version=""1.0"" xml:lang=""zh-CN"">
<voice name=""{Roles.First().VoiceSolution.DisplayName}"" style=""narration-relaxed"">清晨起床，拥抱太阳，我要当大侠，我有梦想。</voice>
<voice name=""{Roles.Last().VoiceSolution.DisplayName}"" style=""newscast"">出去。</voice>
<!--把其他对话也生成同样格式的ssml-->
</speak>
#对话内容是:";
        }

        public void PlaySSML()
        {
            var path = this.CheckOutputPath();
            path = Path.Combine(path, "ssml.mp3");
            AudioPlayer.WindowsMediaPlayerPlay(path);
        }

        [Size(2000), ModelDefault("RowCount", "0")]
        public string SSMLAudioFile
        {
            get { return GetPropertyValue<string>(nameof(SSMLAudioFile)); }
            set { SetPropertyValue(nameof(SSMLAudioFile), value); }
        }


        [XafDisplayName("生成要求"), Size(-1)]
        public string SSMLPrompt
        {
            get { return GetPropertyValue<string>(nameof(SSMLPrompt)); }
            set { SetPropertyValue(nameof(SSMLPrompt), value); }
        }

        [XafDisplayName("流式输出")]
        public bool SSMLStream
        {
            get { return GetPropertyValue<bool>(nameof(SSMLStream)); }
            set { SetPropertyValue(nameof(SSMLStream), value); }
        }

        [Size(-1)]
        public string SSML
        {
            get { return GetPropertyValue<string>(nameof(SSML)); }
            set { SetPropertyValue(nameof(SSML), value); }
        }
        [XafDisplayName("流式输出"), ToolTip("为了测试流式输出性能是否会快一些,即收到N个结果后才刷新一次界面,实际测下来差不多")]
        public int StreamCountOutput
        {
            get { return GetPropertyValue<int>(nameof(StreamCountOutput)); }
            set { SetPropertyValue(nameof(StreamCountOutput), value); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <param name="reGenerate">对于一个段落,如果已经生成了,是否强制生新生成</param>
        /// <exception cref="UserFriendlyException"></exception>
        public async static Task GenerateAudioBook(IEnumerable<AudioBookTextAudioItem> items,bool reGenerate)
        {
            if (!items.Any())
            {
                throw new UserFriendlyException("传入的参数中没有任何条目!");
            }

            var book = items.First().AudioBook;
            var outputPath = book.CheckOutputPath();

            foreach (var item in items)
            {
                var rst = await AudioBookTextAudioItem.GenerateAudioFile(reGenerate, item);
                if (rst.Item2 != -1)
                {
                    item.Duration = rst.Item2;
                    item.OutputFileName = rst.FileName;
                }
                if(item.Duration == -1)
                {
                    item.Duration = (int)FFmpegHelper.GetDuration(item.OutputFileName).Value;
                }
            }

            //List<Task<(AudioBookTextAudioItem,int)>> tasks = new ();

            //Parallel.ForEach(items, new ParallelOptions { MaxDegreeOfParallelism = 3 }, item =>
            //{
            //    try
            //    {
            //        tasks.Add(AudioBookTextAudioItem.GenerateAudioFile(reGenerate, item));
            //    }
            //    catch (Exception ex)
            //    {
            //        Debug.WriteLine(ex.Message);
            //    }
            //    Task.Delay(200).Wait();
            //});

            //Task.WaitAll(tasks.ToArray());

            //foreach (var item in tasks)
            //{
            //    item.Result.Item1.Duration = item.Result.Item2;
            //}

            await Task.CompletedTask;
        }
        public string CheckOutputPath()
        {
            if (string.IsNullOrEmpty(OutputPath))
            {
                throw new UserFriendlyException("请先在书籍上设置输出路径!");
            }
            var di = new DirectoryInfo(OutputPath);
            if (!di.Exists)
            {
                try
                {
                    di.Create();
                }
                catch (Exception ex)
                {
                    throw new UserFriendlyException($"文件夹:{di.FullName}不存在,在创建时报错:" + ex.Message);
                }
            }
            return di.FullName;
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            this.OutputPath = "d:\\AudioBook\\";
        }

        #region Properties
        [Size(1000)]
        [EditorAlias(EditorAliases.StringPropertyEditor)]
        [ModelDefault("RowCount", "1")]
        [XafDisplayName("输出路径")]
        public string OutputPath
        {
            get { return GetPropertyValue<string>(nameof(OutputPath)); }
            set { SetPropertyValue(nameof(OutputPath), value); }
        }


        [ToolTip("简单模式:说话前的内容,与说话内容都使用一种声音播放")]
        [XafDisplayName("简单模式")]
        public bool SimpleMode
        {
            get { return GetPropertyValue<bool>(nameof(SimpleMode)); }
            set { SetPropertyValue(nameof(SimpleMode), value); }
        }

        public AudioBook(Session s) : base(s)
        {
        }

        /// <summary>
        /// 用于解析说话人的AI模型
        /// </summary>
        [XafDisplayName("AI模型")]
        public AIModel AIModel
        {
            get { return GetPropertyValue<AIModel>(nameof(AIModel)); }
            set { SetPropertyValue(nameof(AIModel), value); }
        }

        [XafDisplayName("名称")]
        public string Name
        {
            get { return GetPropertyValue<string>(nameof(Name)); }
            set { SetPropertyValue(nameof(Name), value); }
        }

        [Size(-1)]
        [XafDisplayName("文本内容")]
        [ModelDefault("AllowEdit", "False")]
        public string Content
        {
            get { return GetPropertyValue<string>(nameof(Content)); }
            set { SetPropertyValue(nameof(Content), value); }
        }

        [XafDisplayName("内容")]
        [EditorAlias(EditorAliases.RichTextPropertyEditor)]
        public byte[] WordDocument
        {
            get { return GetPropertyValue<byte[]>(nameof(WordDocument)); }
            set { SetPropertyValue(nameof(WordDocument), value); }
        }




        [Size(-1)]
        [XafDisplayName("备注")]
        public string Description
        {
            get { return GetPropertyValue<string>(nameof(Description)); }
            set { SetPropertyValue(nameof(Description), value); }
        }

        #endregion

        #region 1.Paragraphs

        [Action(Caption = "拆分段落", ToolTip = "按换行符拆分为多个段落")]
        public void ParseParagerahs()
        {
            var lines = Content.Split("\n", StringSplitOptions.RemoveEmptyEntries);
            foreach (var t in lines)
            {
                var item = (t + "").Trim();
                if (!string.IsNullOrEmpty(item))
                {
                    var p = new AudioBookTextAudioItem(Session) { ArticleText = item, Index = AudioItems.Count + 1 };
                    AudioItems.Add(p);
                }
            }
        }

        //段落
        //[Association, DevExpress.Xpo.Aggregated]
        //public XPCollection<AudioBookParagraphItem> Paragraphs { get => GetCollection<AudioBookParagraphItem>(nameof(Paragraphs)); }
        #endregion

        #region 2.声音角色
        //[Action(ToolTip = "为每个条目创建出角色")]
        //public async void GenerateRoles()
        //{

        //    var roleNames = SpreakItems.Select(t => t.Spreaker).GroupBy(t => t).Distinct();
        //    foreach (var role in roleNames)
        //    {
        //        await CreateOrFindAudioRole(role.Key);
        //    }
        //    await Task.CompletedTask;
        //}

        public const string SystemPrompt = @"#任务标题
将小说内容解析为结构化json数据.
重要:不要包含开头的```json和结尾的```字符。不要markdown格式。
返回的结果是程序可以直接解析的json,json示例如下:
{
    Text:'张三说:“这不好吧!”笑了一下',
    Spreaker:'张三',  //识别给出的小说内容,返回说话人(有可能为“旁白”即无说话人或作者的表达)、如果是多个人在说话则返回例如:'张三,李四'，
    SpreakBefore:'张三说',
    SpreakContent:'这不好吧!', //说明:说话内容中不包含开始和结束的引号、双引号。
    SpreakAfter:'笑了一下',
    Emotion:'尴尬',
    Volume:80
}
";

        //[Action(ToolTip = "解析每个条目的说话内容,即说话人、情绪、音量等")]
        //public async void ParseSpreakItems()
        //{
        //    //Session.Delete(this.SpreakItems);
        //    //int idx = 0;
        //    foreach (var item in SpreakItems.OrderBy(t => t.Index))
        //    {
        //        await item.ParseArticleText();
        //    }
        //}
        //说话项目、旁白
        //[Association, DevExpress.Xpo.Aggregated, ModelDefault("AllowEdit", "True")]
        //public XPCollection<AudioBookSpreakItem> SpreakItems { get => GetCollection<AudioBookSpreakItem>(nameof(SpreakItems)); }

        //说话角色

        [Association, DevExpress.Xpo.Aggregated, XafDisplayName("角色"), ToolTip("是指文本中希望按不同的声音来区分人物,每个角色可以设置指定的声音,并在每个段落中指定说话角色")]
        public XPCollection<AudioBookRole> Roles { get => GetCollection<AudioBookRole>(nameof(Roles)); }
        #endregion

        #region AudioItems

        /// <summary>
        /// 本书被分成了N段音频内容
        /// </summary>
        [Association, DevExpress.Xpo.Aggregated, XafDisplayName("段落")]
        public XPCollection<AudioBookTextAudioItem> AudioItems
        {
            get => GetCollection<AudioBookTextAudioItem>(nameof(AudioItems));
        }

        #endregion

        /// <summary>
        /// 查找角色,没有则按角色的名称创建角色,等待为角色分配一个声音
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public async Task<AudioBookRole> CreateOrFindAudioRole(string roleName)
        {
            var find = Roles.FirstOrDefault(t => t.Name == roleName);
            if (find == null)
            {
                find = new AudioBookRole(Session);
                find.Name = roleName;
                Roles.Add(find);
            }
            await Task.CompletedTask;
            return find;
        }

        #region 3.创建书籍
        /// <summary>
        /// 一键生成书籍,如果条目没有生成则去生成.
        /// </summary>
        //[Action(Caption = "生成书籍", ToolTip = "把每个条目生成的音频合并为一个音频,生成的文件放到了书籍指定的“输出路径”中")]
        public async Task CreateAudioBook(bool reGenerate)
        {
            var path = CheckOutputPath();
            await GenerateAudioBook(this.AudioItems,reGenerate);
            //Mp3FileUtils.Combine(this.AudioItems.Select(t => t.OutputFileName).ToArray(), Path.Combine(path, "audiobook.mp3"));
            Debug.WriteLine("处理完成!");
            await Task.CompletedTask;
        }

        #endregion

        #region 4.播放书籍
        //[Action(Caption = "播放书籍", ToolTip = "播放合成的完整书籍")]
        public async void PlayAudioBook()
        {
            var path = CheckOutputPath();
            var fp = Path.Combine(path, "audiobook.mp3");
            AudioPlayer.WindowsMediaPlayerPlay(fp);
            await Task.CompletedTask;
        }

        public void FixSubtitleTimes()
        {
            AudioBookTextAudioItem pre = null;
            foreach (var item in this.AudioItems.OrderBy(t => t.Index))
            {
                item.Subtitle.FixedStartTime = pre?.Subtitle.FixedEndTime ?? TimeSpan.Zero;
                item.Subtitle.FixedEndTime = item.Subtitle.FixedStartTime.AddMilliseconds(Math.Max(item.Duration, item.Subtitle.Duration));
                pre = item;
            }           
        }


        #endregion
    }

    public class WavFileUtils
    {
        public static void Combine(string outputFile, IEnumerable<string> inputFiles)
        {
            // 定义waveFileWriter在外部，因为我们需要在循环内部对其实例化
            WaveFileWriter waveFileWriter = null;

            try
            {
                foreach (string sourceFile in inputFiles)
                {
                    // 使用WaveFileReader读取源文件
                    using (WaveFileReader reader = new WaveFileReader(sourceFile))
                    {
                        if (waveFileWriter == null)
                        {
                            // 第一个文件确定输出格式
                            waveFileWriter = new WaveFileWriter(outputFile, reader.WaveFormat);
                        }
                        else
                        {
                            if (!reader.WaveFormat.Equals(waveFileWriter.WaveFormat))
                            {
                                throw new InvalidOperationException("WAV文件格式不匹配");
                            }
                        }

                        // 读取数据并写入输出文件
                        byte[] buffer = new byte[reader.Length];
                        int bytesRead;
                        while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            waveFileWriter.Write(buffer, 0, bytesRead);
                        }
                    }
                }
            }
            finally
            {
                if (waveFileWriter != null)
                {
                    // 正确关闭waveFileWriter并释放资源
                    waveFileWriter.Dispose();
                }
            }
        }
    }

    public class Mp3FileUtils
    {
        public static void Combine(string[] mp3Files, string mp3OuputFile)
        {
            using (var w = new BinaryWriter(File.Create(mp3OuputFile)))
            {
                new List<string>(mp3Files).ForEach(f => w.Write(File.ReadAllBytes(f)));
            }
        }

        public static void CombineMp3Files(string outputFilePath, IEnumerable<string> inputFilePaths)
        {
            // 列表用于存储所有的读取器
            List<Mp3FileReader> readerList = new List<Mp3FileReader>();

            try
            {
                // 遍历所有输入文件，创建Mp3FileReader实例
                foreach (string inputFilePath in inputFilePaths)
                {
                    Mp3FileReader reader = new Mp3FileReader(inputFilePath);
                    readerList.Add(reader);
                }

                // 创建合并的波形提供器
                WaveFormat commonFormat = readerList[0].WaveFormat;
                var waveProvider = new WaveMixerStream32(readerList.ConvertAll<WaveStream>(r => r), true);

                // 创建WaveFileWriter用于写入文件
                using (WaveFileWriter writer = new WaveFileWriter(outputFilePath, commonFormat))
                {
                    var buffer = new byte[waveProvider.WaveFormat.SampleRate * 4];
                    int bytesRead;

                    while ((bytesRead = waveProvider.Read(buffer, 0, buffer.Length)) > 0)  //waveProvider.Read 第一个参数要求是byte[] 怎么办?
                    {
                        writer.Write(buffer, 0, bytesRead);
                    }
                    writer.Flush();
                }
            }
            finally
            {
                // 确保所有的读取器都被正确关闭和释放资源
                foreach (var reader in readerList)
                {
                    reader.Dispose();
                }

            }
        }
    }

    public class SSMLGeneratorViewController : ObjectViewController<ObjectView, AudioBook>
    {
        public SSMLGeneratorViewController()
        {
            var generateSSML = new SimpleAction(this, "GenerateSSML", "SSMLActions");
            generateSSML.SelectionDependencyType = SelectionDependencyType.RequireSingleObject;
            generateSSML.Caption = "生成SSML";
            generateSSML.Execute += GenerateSSML_Execute;

            var generateSSMLAudio = new SimpleAction(this, "GenerateSSMLAudio", "SSMLActions");
            generateSSMLAudio.SelectionDependencyType = SelectionDependencyType.RequireSingleObject;
            generateSSMLAudio.Caption = "生成音频";
            generateSSMLAudio.Execute += GenerateSSMLAudio_Execute;

            var playSSMLAudio = new SimpleAction(this, "PlaySSMLAudio", "SSMLActions");
            playSSMLAudio.SelectionDependencyType = SelectionDependencyType.RequireSingleObject;
            playSSMLAudio.Caption = "播放音频";
            playSSMLAudio.Execute += PlaySSMLAudio_Execute;

            var defaultRequire = new SimpleAction(this, "RequireSSMLRequire", "SSMLActions");
            defaultRequire.Execute += DefaultRequire_Execute;
            defaultRequire.Caption = "生成默认要求";
        }

        private void DefaultRequire_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            ViewCurrentObject.DefaultSSMLPrompt();
        }
        static string XmlFormat(string xmlString)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlString);

            // 设置格式化选项
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,          // 启用缩进
                IndentChars = "  ",     // 缩进使用的字符（两个空格）
                NewLineChars = "\r\n",  // 设置换行符
                NewLineHandling = NewLineHandling.Replace // 替换换行符
            };

            // 将格式化后的 XML 输出到字符串
            using (var stringWriter = new System.IO.StringWriter())
            using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
            {
                doc.WriteTo(xmlWriter);
                xmlWriter.Flush();
                return (stringWriter.GetStringBuilder().ToString());
            }
        }

        private async void GenerateSSML_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            ViewCurrentObject.SSML = "";

            await AIHelper.Ask(
                ViewCurrentObject.SSMLPrompt, ViewCurrentObject.Content,
                t =>
                {
                    this.ViewCurrentObject.SSML = t.Content;
                },
                ViewCurrentObject.AIModel,
                ViewCurrentObject.SSMLStream
                );

            //var sw = Stopwatch.StartNew();
            //ViewCurrentObject.SSML = "";
            //if (ViewCurrentObject.SSMLStream)
            //{                
            //    var t = AIHelper.AskStream(ViewCurrentObject.SSMLPrompt, ViewCurrentObject.Content, ViewCurrentObject.AIModel);
            //    var i = 0;
            //    await foreach (var item in t)
            //    {
            //        if (!string.IsNullOrEmpty(item))
            //        {

            //            i++;
            //            if (i % ViewCurrentObject.StreamCountOutput == 0)
            //                Application.UIThreadDoEvents();
            //        }
            //        Debug.WriteLine(item);
            //    }
            //    Application.UIThreadDoEvents();
            //}
            //else
            //{
            //    var t =await AIHelper.Ask(ViewCurrentObject.SSMLPrompt, ViewCurrentObject.Content, ViewCurrentObject.AIModel);
            //    if (!t.IsError)
            //    {
            //        Application.UIThreadInvoke(() =>
            //        {

            //        });
            //        Application.UIThreadDoEvents();
            //    }
            //    else
            //    {
            //        throw new UserFriendlyException(t.Message);
            //    }
            //}
            //sw.Stop();
            this.ViewCurrentObject.SSML = XmlFormat(System.Net.WebUtility.HtmlDecode(ViewCurrentObject.SSML));
        }

        private void PlaySSMLAudio_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            ViewCurrentObject.PlaySSML();
        }

        private async void GenerateSSMLAudio_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            await ViewCurrentObject.SaySSML();
        }
    }

}
