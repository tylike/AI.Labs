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
using System.ComponentModel;
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

    public class SubtitlesProcessor
    {
        public static IEnumerable<string> AddPunctuation(IEnumerable<string> lines)
        {
            foreach (var line in lines)
            {
                yield return AddPunctuationToLine(line);
            }
        }

        private static string AddPunctuationToLine(string line)
        {
            // Add punctuation to the line using your own punctuation model
            // ...

            return line;
        }

        public static IEnumerable<string> SplitSentences(IEnumerable<string> lines)
        {
            foreach (var line in lines)
            {
                foreach (var sentence in SplitLineIntoSentences(line))
                {
                    yield return sentence;
                }
            }
        }

        private static IEnumerable<string> SplitLineIntoSentences(string line)
        {
            // Split the line into sentences based on sentence length
            // ...

            return new List<string> { line }; // Placeholder, replace with actual implementation
        }

        public static IEnumerable<(string, TimeSpan)> AlignTime(IEnumerable<(string, TimeSpan)> sentences, IEnumerable<(TimeSpan, TimeSpan)> timeRanges)
        {
            var alignedSentences = new List<(string, TimeSpan)>();

            foreach (var (sentence, time) in sentences)
            {
                var alignedTime = FindAlignedTime(time, timeRanges);
                alignedSentences.Add((sentence, alignedTime));
            }

            return alignedSentences;
        }

        private static TimeSpan FindAlignedTime(TimeSpan time, IEnumerable<(TimeSpan, TimeSpan)> timeRanges)
        {
            // Find the aligned time based on the time ranges
            // ...

            return time; // Placeholder, replace with actual implementation
        }

        public static IEnumerable<string> TranslateSentences(IEnumerable<string> sentences)
        {
            foreach (var sentence in sentences)
            {
                yield return TranslateSentence(sentence);
            }
        }

        private static string TranslateSentence(string sentence)
        {
            // Translate the sentence using your translation model
            // ...

            return sentence;
        }

        public static IEnumerable<SubtitleItem> CreateSubtitlesFile(string inputFile, Session session,bool autoIndex)
        {
            using (var fileStream = File.OpenRead(inputFile))
            {
                var parser = new NSrtParser();
                var subtitleItems = parser.ParseStream(fileStream, Encoding.UTF8, session,autoIndex);
                return subtitleItems;
            }
        }
        public static IEnumerable<SubtitleItem> CreateSubtitles(string content, Session session,bool autoIndex)
        {
            var parser = new NSrtParser();
            var subtitleItems = parser.ParseString(content, Encoding.UTF8, session, autoIndex);
            return subtitleItems;
        }


        /// <summary>
        /// 使用大模型加上标点符号
        /// </summary>
        /// <param name="text"></param>
        public async static Task AddSymbol(string text, int limitLength, AIModel ai)
        {
            var sb = new StringBuilder();
            foreach (var item in text.SplitIntoGroups(limitLength))
            {
                await AIHelper.Ask("将用户输入的英文内容加上标点符号", item,
                    cm =>
                    {
                        //t.Text += cm.Content;
                        sb.Append(cm.Content);
                    },
                    streamOut: true,
                    url: ai.ApiUrlBase,
                    api_key: ai.ApiKey,
                    modelName: ai.Name
                    );
            }
        }

        /// <summary>
        /// 实现对字幕内容的标点符号的添加、并与原字幕内容进行时间上的对齐。
        /// 对“SRT字幕列表”和“有标点符号的文本”进行合并，得到开始和结束时间
        /// </summary>
        /// <param name="SRT字幕列表">现有List<SubtitleItem>字幕列表,每个字幕有开始时间和结束时间,以及字幕内容.但字幕内容中不包含标点符号、换行符。</param>
        /// <param name="有标点符号的文本">现有string contents,是字幕内容的所有文字,这个内容中包含标点符号，换行符。</param>
        /// <returns>合并后的结果</returns>
        public List<NewSubtitleItem> Match(List<SubtitleItem> SRT字幕列表, string 有标点符号的文本)
        {
            // 将有标点符号的文本按照句子进行分割：对于分割后的一个string，其中可能包含了多种情况,以下是各种情况.
            //srt内容:

            //00:00:00,000 --> 00:00:02,000
            //AAAA BBBB CCCC DDDD EEEE FFFF

            //00:00:02,000 --> 00:00:04,000
            //GGGG HHHH IIII JJJJ KKKK LLLL

            //00:00:04,000 --> 00:00:06,000
            //MMMM NNNN OOOO PPPP QQQQ RRRR

            //有标点符号的文本 的内容
            //AAAA BBBB CCCC DDDD.EEEE FFFF GGGG HHHH IIII JJJJ KKKK LLLL MMMM.NNNN OOOO PPPP QQQQ RRRR

            //可能出现的情况:
            //已知O,代表的是SubtitleItem的一个实例.

            //0.  O[
            //1.  O1=N1
            //2.  O1+O2 = N1
            //2.1 O1 + ... +  O[t+1] = N1
            //3. O1.X = N1
            //4. O1+O2.X = N1

            //可能被分割成为多个句子,这些句子可能分别在不同的字幕中,也可能在同一个字幕中.

            var sentences = SplitSentences(有标点符号的文本);
            var mergedSubtitles = new List<NewSubtitleItem>();
            // 遍历SRT字幕列表
            foreach (var s in sentences)
            {
                var nsi = new NewSubtitleItem();
                //跳过已处理的内容，取得第一个匹配的内容，应该是a包含b或b包含a均可。这个后续研究。
                //srt.skip(已处理过的).where(t=>t.plaintext.contains(s))
                var items = SRT字幕列表.Where(t => t.PlainText.Contains(s)).ToList();
                nsi.Subtitles.AddRange(items);
            }
            return mergedSubtitles;
        }

        private IEnumerable<string> SplitSentences(string text)
        {
            // 使用适当的方法将文本分割成句子
            // ...
            return new List<string>(); // 替换为实际的实现
        }

        private IEnumerable<string> AlignSentences(string subtitleText, IEnumerable<string> sentences)
        {
            // 根据字幕内容和句子列表，进行时间上的对齐
            // ...

            return new List<string>(); // 替换为实际的实现
        }



        public static void SaveToSrtFile(List<SubtitleItem> subtitleItems, string newFile)
        {
            // 移除所有内容为空的字幕项
            //subtitleItems = subtitleItems.Where(s => s.Lines.Any(line => !string.IsNullOrWhiteSpace(line))).ToList();

            // 保存到新的SRT文件中
            using (var fileStream = new FileStream(newFile, FileMode.Create))
            {
                using (var writer = new StreamWriter(fileStream, Encoding.UTF8))
                {
                    int l = 0;
                    for (int i = 0; i < subtitleItems.Count; i++)
                    {

                        var item = subtitleItems[i];
                        if (item.Lines.Length > 0)
                        {
                            var endTime = item.EndTime;
                            if (i + 1 < subtitleItems.Count)
                            {
                                var next = subtitleItems[i + 1];
                                endTime = next.StartTime;
                            }
                            writer.WriteLine(l++);
                            writer.WriteLine($"{item.StartTime.ToString(@"hh\:mm\:ss\,fff")} --> {endTime.ToString(@"hh\:mm\:ss\,fff")}");
                            //foreach (var line in item.Lines)
                            //{
                            //    writer.WriteLine(line);
                            //}
                            writer.WriteLine(item.Lines);
                            writer.WriteLine();
                        }
                    }
                }
            }
        }
    }
    public class NewSubtitleItem
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Lines { get; set; }

        /// <summary>
        /// 是指原来没有标点符号的字幕内容
        /// </summary>
        public List<SubtitleItem> Subtitles { get; set; }
    }
    public class NSrtParser
    {
        private readonly string[] _delimiters = new string[3] { "-->", "- >", "->" };

        public List<SubtitleItem> ParseString(string srtString, Encoding encoding, Session session, bool autoIndex)
        {
            return ParseStream(new MemoryStream(encoding.GetBytes(srtString)), encoding, session, autoIndex);
        }

        public List<SubtitleItem> ParseStream(Stream srtStream, Encoding encoding, Session session, bool autoIndex)
        {

            if (!srtStream.CanRead || !srtStream.CanSeek)
            {
                string message = $"Stream must be seekable and readable in a subtitles parser. Operation interrupted; isSeekable: {srtStream.CanSeek} - isReadable: {srtStream.CanSeek}";
                throw new ArgumentException(message);
            }

            srtStream.Position = 0L;
            StreamReader reader = new StreamReader(srtStream, encoding, detectEncodingFromByteOrderMarks: true);
            List<SubtitleItem> list = new List<SubtitleItem>();
            List<string> list2 = GetSrtSubTitleParts(reader).ToList();
            if (list2.Any())
            {
                foreach (string item in list2)
                {
                    List<string> list3 = (from s in item.Split(new string[1] { Environment.NewLine }, StringSplitOptions.None)
                                          select s.Trim() into l
                                          where !string.IsNullOrEmpty(l)
                                          select l).ToList();
                    SubtitleItem subtitleItem = new SubtitleItem(session);
                    foreach (string item2 in list3)
                    {
                        if (subtitleItem.StartTime.TotalMilliseconds == 0 && subtitleItem.EndTime.TotalMilliseconds == 0)
                        {
                            if (TryParseTimecodeLine(item2, out var startTc, out var endTc))
                            {
                                subtitleItem.StartTime = TimeSpan.FromMilliseconds(startTc);
                                subtitleItem.EndTime = TimeSpan.FromMilliseconds(endTc);
                            }
                        }
                        else
                        {
                            //subtitleItem.Lines.Add(item2);
                            subtitleItem.Lines += item2;
                            subtitleItem.PlainText += (Regex.Replace(item2, "\\{.*?\\}|<.*?>", string.Empty));
                        }
                    }
                    if (autoIndex)
                        subtitleItem.Index = list.Count;
                    else
                        subtitleItem.Index = int.Parse(list3.First());

                    //&& subtitleItem.Lines.Any()
                    if ((subtitleItem.StartTime.TotalMilliseconds != 0 || subtitleItem.EndTime.TotalMilliseconds != 0))
                    {
                        list.Add(subtitleItem);
                    }
                }

                if (list.Any())
                {
                    return list;
                }

                throw new ArgumentException("Stream is not in a valid Srt format");
            }

            throw new FormatException("Parsing as srt returned no srt part.");
        }

        private IEnumerable<string> GetSrtSubTitleParts(TextReader reader)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                string text;
                string line = (text = reader.ReadLine());
                if (text == null)
                {
                    break;
                }

                if (string.IsNullOrEmpty(line.Trim()))
                {
                    string res = sb.ToString().TrimEnd();
                    if (!string.IsNullOrEmpty(res))
                    {
                        yield return res;
                    }

                    sb = new StringBuilder();
                }
                else
                {
                    sb.AppendLine(line);
                }
            }

            if (sb.Length > 0)
            {
                yield return sb.ToString();
            }
        }

        private bool TryParseTimecodeLine(string line, out int startTc, out int endTc)
        {
            string[] array = line.Split(_delimiters, StringSplitOptions.None);
            if (array.Length != 2)
            {
                startTc = -1;
                endTc = -1;
                return false;
            }

            startTc = ParseSrtTimecode(array[0]);
            endTc = ParseSrtTimecode(array[1]);
            return true;
        }

        private static int ParseSrtTimecode(string s)
        {
            var match = Regex.Match(s, "[0-9]+:[0-9]+:[0-9]+([,\\.][0-9]+)?");
            if (match.Success)
            {
                s = match.Value;
                if (TimeSpan.TryParse(s.Replace(',', '.'), out var result))
                {
                    return (int)result.TotalMilliseconds;
                }
            }

            return -1;
        }
    }


    [XafDisplayName("视频")]
    [NavigationItem("视频翻译")]
    public class VideoInfo : SimpleXPObject
    {
        public VideoInfo(Session s) : base(s)
        {

        }



        [EditorAlias(EditorAliases.SpreadsheetPropertyEditor)]
        public byte[] SubtitleSheetEditor
        {
            get { return GetPropertyValue<byte[]>(nameof(SubtitleSheetEditor)); }
            set { SetPropertyValue(nameof(SubtitleSheetEditor), value); }
        }

        [XafDisplayName("标题")]
        public string Title
        {
            get { return GetPropertyValue<string>(nameof(Title)); }
            set { SetPropertyValue(nameof(Title), value); }
        }

        [Size(-1)]
        [ModelDefault("RowCount", "0")]
        public string VideoURL
        {
            get { return GetPropertyValue<string>(nameof(VideoURL)); }
            set { SetPropertyValue(nameof(VideoURL), value); }
        }

        //[EditorAlias(EditorAliases.RichTextPropertyEditor)]
        [Size(-1)]
        public string AIFixedSubtitle
        {
            get { return GetPropertyValue<string>(nameof(AIFixedSubtitle)); }
            set { SetPropertyValue(nameof(AIFixedSubtitle), value); }
        }

        [XafDisplayName("修复开始")]
        public int FixSubtitleStartIndex
        {
            get { return GetPropertyValue<int>(nameof(FixSubtitleStartIndex)); }
            set { SetPropertyValue(nameof(FixSubtitleStartIndex), value); }
        }

        [XafDisplayName("修复条数")]
        [RuleRange(1, 1000)]
        public int FixSubtitleBatchCount
        {
            get { return GetPropertyValue<int>(nameof(FixSubtitleBatchCount)); }
            set { SetPropertyValue(nameof(FixSubtitleBatchCount), value); }
        }


        [XafDisplayName("拆分合并")]
        [EditorAlias(EditorAliases.SpreadsheetPropertyEditor)]
        public byte[] SplitMerge
        {
            get { return GetPropertyValue<byte[]>(nameof(SplitMerge)); }
            set { SetPropertyValue(nameof(SplitMerge), value); }
        }


        public AIModel Model
        {
            get { return GetPropertyValue<AIModel>(nameof(Model)); }
            set { SetPropertyValue(nameof(Model), value); }
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            TranslateTaskPrompt = @"
1.请将以下SRT英文字幕翻译成为中文
2.可以适当合并短句,合并后不要太长.对断句做合适的处理,使用意思尽量完整。
3.注意:翻译完成后应该保证仍是可用的SRT格式。
4.合并多段时需要保证与原音频时间上的对齐。
";
            AddSymbolPrompt = @"将以下内容加上标点符号,保持原有格式，保留换行，不要合并多条，不要移动单词位置，一段字幕没有表达完整时末尾加上省略号，不要输出除字幕以外的解释，结果是纯文本格式:";
            FixSubtitleBatchCount = 30;
        }

        [XafDisplayName("翻译要求")]
        [Size(-1)]
        public string TranslateTaskPrompt
        {
            get { return GetPropertyValue<string>(nameof(TranslateTaskPrompt)); }
            set { SetPropertyValue(nameof(TranslateTaskPrompt), value); }
        }

        [XafDisplayName("标点要求")]
        [Size(-1)]
        [RuleRequiredField]
        public string AddSymbolPrompt
        {
            get { return GetPropertyValue<string>(nameof(AddSymbolPrompt)); }
            set { SetPropertyValue(nameof(AddSymbolPrompt), value); }
        }

        [Action(Caption = "加载字幕", ToolTip = "从SRT字幕文件加载字幕")]
        public void Load()
        {
            if (string.IsNullOrEmpty(VideoDefaultSRT))
            {
                throw new UserFriendlyException("请先填写SRT字幕文件路径!");
            }
            var t = SubtitlesProcessor.CreateSubtitlesFile(VideoDefaultSRT, Session, true).Where(t => !string.IsNullOrEmpty(t.Lines) && t.Lines.Trim().Length > 0);
            int idx = 0;
            foreach (var item in t)
            {
                item.Index = idx++;
                Subtitles.Add(item);
            }
        }

        [Action(Caption = "生成翻译任务")]
        public void GenerateTranslateTask()
        {
            if (PerTaskTranslateCount < 0)
            {
                throw new UserFriendlyException("错误:没有设置任务翻译的数量!");
            }

            int pageSize = PerTaskTranslateCount;
            var source = Subtitles.OrderBy(t => t.Index).ToArray();
            int totalRecords = source.Count();
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            for (int page = 1; page <= totalPages; page++)
            {
                var pagedData = source.Skip((page - 1) * pageSize).Take(pageSize);
                // 对数据进行相应的处理
                var task = new TranslateTask(Session);
                task.Index = page;
                task.SubtitleItems.AddRange(pagedData);
                this.TranslateTasks.Add(task);
            }
        }

        [Action(Caption = "生成加标点任务")]
        public void CreateAddSymbolTask()
        {
            int index = 0;
            var task = new TranslateTask(Session);
            task.Index = index;
            index++;
            this.TranslateTasks.Add(task);
            var limitLength = 2000;
            foreach (var item in Subtitles)
            {
                if ((task.Text + "").Length + item.Lines.Length < limitLength)
                {
                    task.Text += " " + item.Lines;
                    task.SubtitleItems.Add(item);
                }
                else
                {
                    task = new TranslateTask(Session);
                    task.Index = index;
                    this.TranslateTasks.Add(task);
                    index++;
                }
            }
        }

        [XafDisplayName("任务翻译")]
        [ToolTip("生成的每个任务的翻译字幕的条数")]
        public int PerTaskTranslateCount
        {
            get { return GetPropertyValue<int>(nameof(PerTaskTranslateCount)); }
            set { SetPropertyValue(nameof(PerTaskTranslateCount), value); }
        }


        [XafDisplayName("项目路径")]
        [Size(-1)]
        [ModelDefault("RowCount", "1")]
        [RuleRequiredField]
        public string ProjectPath
        {
            get { return GetPropertyValue<string>(nameof(ProjectPath)); }
            set { SetPropertyValue(nameof(ProjectPath), value); }
        }

        [XafDisplayName("字幕文件")]
        [Size(-1)]
        [ModelDefault("RowCount", "1")]
        [RuleRequiredField]
        public string VideoDefaultSRT
        {
            get { return GetPropertyValue<string>(nameof(VideoDefaultSRT)); }
            set { SetPropertyValue(nameof(VideoDefaultSRT), value); }
        }
        [Action]
        public void SaveToFile()
        {
            File.WriteAllText(VideoDefaultSRT + ".onlytext.txt", TextLines);
        }
        [ModelDefault("RowCount", "10")]

        public string TextLines
        {
            get => string.Join(" ", Subtitles.Select(t => t.Lines));
        }

        [Association]
        public XPCollection<SubtitleItem> Subtitles
        {
            get { return GetCollection<SubtitleItem>(nameof(Subtitles)); }
        }

        [Association]
        public XPCollection<TranslateTask> TranslateTasks
        {
            get => GetCollection<TranslateTask>(nameof(TranslateTasks));

        }

    }

    public class SubtitleItem : SimpleXPObject
    {
        public SubtitleItem(Session s) : base(s)
        {

        }

        /// <summary>
        /// 仅在拆分时辅助使用
        /// </summary>
        [Browsable(false)]
        public List<SubtitleItem> Splits
        {
            get;
        } = new List<SubtitleItem>();

        //[Association]
        //public NSubtitleItem NSubtitle
        //{
        //    get { return GetPropertyValue<NSubtitleItem>(nameof(NSubtitle)); }
        //    set { SetPropertyValue(nameof(NSubtitle), value); }
        //}

        [Association]
        public VideoInfo Video
        {
            get { return GetPropertyValue<VideoInfo>(nameof(Video)); }
            set { SetPropertyValue(nameof(Video), value); }
        }

        public int Index
        {
            get { return GetPropertyValue<int>(nameof(Index)); }
            set { SetPropertyValue(nameof(Index), value); }
        }
        [ModelDefault("DisplayFormat", "{0:HH:mm:ss.fff}")]
        public TimeSpan StartTime
        {
            get { return GetPropertyValue<TimeSpan>(nameof(StartTime)); }
            set { SetPropertyValue(nameof(StartTime), value); }
        }
        [ModelDefault("DisplayFormat", "{0:HH:mm:ss.fff}")]
        public TimeSpan EndTime
        {
            get { return GetPropertyValue<TimeSpan>(nameof(EndTime)); }
            set { SetPropertyValue(nameof(EndTime), value); }
        }
        [Size(-1)]
        public string Lines
        {
            get { return GetPropertyValue<string>(nameof(Lines)); }
            set { SetPropertyValue(nameof(Lines), value); }
        }
        string[] words;
        public string[] Words
        {
            get
            {
                if (words == null)
                {
                    words = (Lines + "").Replace("\n", " ").Replace("\r", "").Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(t => !string.IsNullOrEmpty(t)).ToArray();
                }
                return words;
            }
        }

        [Size(-1)]
        public string PlainText
        {
            get { return GetPropertyValue<string>(nameof(PlainText)); }
            set { SetPropertyValue(nameof(PlainText), value); }
        }

        [Association]
        public TranslateTask TranslateTask
        {
            get { return GetPropertyValue<TranslateTask>(nameof(TranslateTask)); }
            set { SetPropertyValue(nameof(TranslateTask), value); }
        }


    }

}
