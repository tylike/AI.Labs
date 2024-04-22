using AI.Labs.Module.BusinessObjects.VideoTranslate;
using DevExpress.ExpressApp;
using DevExpress.Xpo;
using System.Text;
//using SubtitlesParser.Classes; // 引入SubtitlesParser的命名空间
//using SubtitlesParser.Classes.Parsers;
namespace AI.Labs.Module.BusinessObjects.Helper
{

    public static class SRTHelper
    {

        #region old
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

        ///// <summary>
        ///// 使用大模型加上标点符号
        ///// </summary>
        ///// <param name="text"></param>
        //public async static Task AddSymbol(string text, int limitLength, AIModel ai)
        //{
        //    var sb = new StringBuilder();
        //    foreach (var item in text.SplitIntoGroups(limitLength))
        //    {
        //        await AIHelper.Ask("将用户输入的英文内容加上标点符号", item,
        //            cm =>
        //            {
        //                //t.Text += cm.Content;
        //                sb.Append(cm.Content);
        //            },
        //            streamOut: true,
        //            url: ai.ApiUrlBase,
        //            api_key: ai.ApiKey,
        //            modelName: ai.Name
        //            );
        //    }
        //}

        ///// <summary>
        ///// 实现对字幕内容的标点符号的添加、并与原字幕内容进行时间上的对齐。
        ///// 对“SRT字幕列表”和“有标点符号的文本”进行合并，得到开始和结束时间
        ///// </summary>
        ///// <param name="SRT字幕列表">现有List<SubtitleItem>字幕列表,每个字幕有开始时间和结束时间,以及字幕内容.但字幕内容中不包含标点符号、换行符。</param>
        ///// <param name="有标点符号的文本">现有string contents,是字幕内容的所有文字,这个内容中包含标点符号，换行符。</param>
        ///// <returns>合并后的结果</returns>
        //public List<VideoTranslate.SRT> Match(List<SubtitleItem> SRT字幕列表, string 有标点符号的文本)
        //{
        //    // 将有标点符号的文本按照句子进行分割：对于分割后的一个string，其中可能包含了多种情况,以下是各种情况.
        //    //srt内容:

        //    //00:00:00,000 --> 00:00:02,000
        //    //AAAA BBBB CCCC DDDD EEEE FFFF

        //    //00:00:02,000 --> 00:00:04,000
        //    //GGGG HHHH IIII JJJJ KKKK LLLL

        //    //00:00:04,000 --> 00:00:06,000
        //    //MMMM NNNN OOOO PPPP QQQQ RRRR

        //    //有标点符号的文本 的内容
        //    //AAAA BBBB CCCC DDDD.EEEE FFFF GGGG HHHH IIII JJJJ KKKK LLLL MMMM.NNNN OOOO PPPP QQQQ RRRR

        //    //可能出现的情况:
        //    //已知O,代表的是SubtitleItem的一个实例.

        //    //0.  O[
        //    //1.  O1=N1
        //    //2.  O1+O2 = N1
        //    //2.1 O1 + ... +  O[t+1] = N1
        //    //3. O1.X = N1
        //    //4. O1+O2.X = N1

        //    //可能被分割成为多个句子,这些句子可能分别在不同的字幕中,也可能在同一个字幕中.

        //    var sentences = SplitSentences(有标点符号的文本);
        //    var mergedSubtitles = new List<VideoTranslate.SRT>();
        //    // 遍历SRT字幕列表
        //    foreach (var s in sentences)
        //    {
        //        var nsi = new VideoTranslate.SRT();
        //        //跳过已处理的内容，取得第一个匹配的内容，应该是a包含b或b包含a均可。这个后续研究。
        //        //srt.skip(已处理过的).where(t=>t.plaintext.contains(s))
        //        var items = SRT字幕列表.Where(t => t.PlainText.Contains(s)).ToList();
        //        nsi.Subtitles.AddRange(items);
        //    }
        //    return mergedSubtitles;
        //}

        //private IEnumerable<string> SplitSentences(string text)
        //{
        //    // 使用适当的方法将文本分割成句子
        //    // ...
        //    return new List<string>(); // 替换为实际的实现
        //}

        //private IEnumerable<string> AlignSentences(string subtitleText, IEnumerable<string> sentences)
        //{
        //    // 根据字幕内容和句子列表，进行时间上的对齐
        //    // ...

        //    return new List<string>(); // 替换为实际的实现
        //}
        #endregion

        /// <summary>
        /// 从srt文件解释出字幕对象列表
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="session"></param>
        /// <param name="autoIndex"></param>
        /// <returns></returns>
        public static IEnumerable<SubtitleItem> ParseSrtFileToObject(string inputFile, Session session, bool autoIndex)
        {
            using (var fileStream = File.OpenRead(inputFile))
            {
                var parser = new SRTParser();
                var subtitleItems = parser.ParseStreamToXPOObject(fileStream, Encoding.UTF8, session, autoIndex);
                return subtitleItems;
            }
        }

        public static IEnumerable<SubtitleItem> ParseSrtStringContentToObject(string content, Session session, bool autoIndex)
        {
            var parser = new SRTParser();
            var subtitleItems = parser.ParseStringToXPOObject(content, Encoding.UTF8, session, autoIndex);
            return subtitleItems;
        }

        public static string ToSrtTimeString(this TimeSpan time)
        {
            return time.ToString(@"hh\:mm\:ss\,fff");
        } 
        public static void WriteTime(this StreamWriter writer, TimeSpan startTime,TimeSpan endTime)
        {
            writer.WriteLine($"{startTime.ToSrtTimeString()} --> {endTime.ToSrtTimeString()}");
        }

        public static void WriteSubtitleItem(this StreamWriter writer,string text,TimeSpan startTime,TimeSpan endTime,int? index = null)
        {
            if (index.HasValue)
            {
                writer.WriteLine(index.Value);
            }
            writer.WriteTime(startTime, endTime);
            writer.WriteLine(text);
            writer.WriteLine();
        }

        public static void CreateSubtitleStream(string newFile, Action<StreamWriter> core)
        {
            using var fileStream = new FileStream(newFile, FileMode.Create);
            using var writer = new StreamWriter(fileStream, Encoding.UTF8);
            core(writer);
            writer.Flush();
            fileStream.Flush();
        }

        /// <summary>
        /// 保存字幕对象列表到srt文件
        /// </summary>
        /// <param name="subtitleItems"></param>
        /// <param name="newFile"></param>
        public static void SaveToSrtFile(IList<SubtitleItem> subtitleItems, string newFile, SrtLanguage language,bool saveFixed)
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
                        var text = (language == SrtLanguage.英文 ? item.PlainText : item.CnText) ?? string.Empty;

                        if (text.Length > 0)
                        {
                            var endTime = saveFixed ? item.FixedEndTime: item.EndTime;
                            if (i + 1 < subtitleItems.Count)
                            {
                                var next = subtitleItems[i + 1];
                                endTime = saveFixed ? next.FixedStartTime : next.StartTime;
                            }
                            var startTime = saveFixed ? item.FixedStartTime : item.StartTime;
                            
                            writer.WriteSubtitleItem(text, startTime, endTime, l++);

                            writer.WriteLine();
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 字幕文件的语言
    /// 在保存时可以指定字幕文件的语言
    /// </summary>
    public enum SrtLanguage
    {
        英文,
        中文
    }

}
