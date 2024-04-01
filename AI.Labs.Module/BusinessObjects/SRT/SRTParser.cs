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
        public List<ISRT> ParseString(string srtString, Encoding encoding, bool autoIndex,Func<ISRT> createInstance)
        {
            return ParseStream<ISRT>(new MemoryStream(encoding.GetBytes(srtString)), encoding, autoIndex, createInstance);
        }

        public List<T> ParseStream<T>(Stream srtStream, Encoding encoding, bool autoIndex, Func<T> createInstance)
            where T:ISRT
        {
            if (!srtStream.CanRead || !srtStream.CanSeek)
            {
                string message = $"Stream must be seekable and readable in a subtitles parser. Operation interrupted; isSeekable: {srtStream.CanSeek} - isReadable: {srtStream.CanSeek}";
                throw new ArgumentException(message);
            }

            srtStream.Position = 0L;
            StreamReader reader = new StreamReader(srtStream, encoding, detectEncodingFromByteOrderMarks: true);
            var list = new List<T>();
            List<string> list2 = GetSrtSubTitleParts(reader).ToList();
            if (list2.Any())
            {
                foreach (string item in list2)
                {
                    List<string> list3 = (from s in item.Split(new string[1] { Environment.NewLine }, StringSplitOptions.None)
                                          select s.Trim() into l
                                          where !string.IsNullOrEmpty(l)
                                          select l).ToList();
                    var subtitleItem = createInstance();
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
                            subtitleItem.Text += item2;
                            //subtitleItem.PlainText += (Regex.Replace(item2, "\\{.*?\\}|<.*?>", string.Empty));
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

}
