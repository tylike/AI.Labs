using DevExpress.Xpo;
using System.Text;
//using SubtitlesParser.Classes; // 引入SubtitlesParser的命名空间
using System.Text.RegularExpressions;
//using SubtitlesParser.Classes.Parsers;
namespace AI.Labs.Module.BusinessObjects.VideoTranslate
{
    public class SRTParser
    {
        private readonly string[] _delimiters = new string[3] { "-->", "- >", "->" };

        public List<SubtitleItem> ParseStringToXPOObject(string srtString, Encoding encoding, Session session, bool autoIndex)
        {
            return ParseStreamToXPOObject(new MemoryStream(encoding.GetBytes(srtString)), encoding, session, autoIndex);
        }

        public List<SubtitleItem> ParseStreamToXPOObject(Stream srtStream, Encoding encoding, Session session, bool autoIndex)
        {
            return ParseStream<SubtitleItem>(srtStream, encoding, autoIndex, () => new SubtitleItem(session));
        }
        public List<ISRT> ParseString(string srtString, Encoding encoding, bool autoIndex, Func<ISRT> createInstance)
        {
            return ParseStream<ISRT>(new MemoryStream(encoding.GetBytes(srtString)), encoding, autoIndex, createInstance);
        }

        public List<T> ParseStream<T>(Stream srtStream, Encoding encoding, bool autoIndex, Func<T> createInstance)
            where T : ISRT
        {
            if (!srtStream.CanRead || !srtStream.CanSeek)
            {
                string message = $"Stream must be seekable and readable in a subtitles parser. Operation interrupted; isSeekable: {srtStream.CanSeek} - isReadable: {srtStream.CanSeek}";
                throw new ArgumentException(message);
            }

            srtStream.Position = 0L;
            var reader = new StreamReader(srtStream, encoding, detectEncodingFromByteOrderMarks: true);
            var result = new List<T>();
            var textTemp = GetSrtSubTitleParts(reader).ToList();

            if (textTemp.Any())
            {
                T before = default;
                foreach (string item in textTemp)
                {
                    //一条字幕的三个部分
                    List<string> list3 = (from s in item.Split(new string[1] { Environment.NewLine }, StringSplitOptions.None)
                                          select s.Trim() into l
                                          where !string.IsNullOrEmpty(l)
                                          select l).ToList();

                    var subtitleItem = createInstance();
                    foreach (string item2 in list3)
                    {
                        //直接忽略序号
                        //还没有时间部分时，就开始解析，如果是时间部分，这是试出来的，能解析出来就是时间部分
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
                            //有了时间部分，就是内容部分
                            //subtitleItem.Lines.Add(item2);
                            subtitleItem.Text += item2;
                            //subtitleItem.PlainText += (Regex.Replace(item2, "\\{.*?\\}|<.*?>", string.Empty));
                        }
                    }
                    if (autoIndex)
                        subtitleItem.Index = result.Count;
                    else
                        subtitleItem.Index = int.Parse(list3.First());
                    //认为第一部分是序号，但是不是数字就会出错，所以不建议使用
                    #warning 有问题的代码

                    //&& subtitleItem.Lines.Any()
                    //有时间就算是有效的字幕
                    if ((subtitleItem.StartTime.TotalMilliseconds != 0 || subtitleItem.EndTime.TotalMilliseconds != 0))
                    {
                        if (before != null)
                        {
                            subtitleItem.Before = before;
                            subtitleItem.BeforeGap = subtitleItem.StartTime - before.EndTime;
                            before.Next = subtitleItem;
                        }
                        result.Add(subtitleItem);
                        before = subtitleItem;
                    }
                }

                if (result.Any())
                {
                    return result;
                }

                throw new ArgumentException("Stream is not in a valid Srt format");
            }

            throw new FormatException("Parsing as srt returned no srt part.");
        }

        /// <summary>
        /// 返回所有字幕内容,但是是一条为一行，
        /// 并且包含了序号、时间、内容
        /// </summary>                                                                                                                                       
        /// <param name="reader"></param>
        /// <returns></returns>
        private IEnumerable<string> GetSrtSubTitleParts(TextReader reader)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                string text;
                //读取一行
                string line = (text = reader.ReadLine());
                if (text == null)
                {
                    break;
                }
                //有空行则返回内容，说明上一条字幕结束
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

}
