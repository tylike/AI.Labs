using System.Text;
//using SubtitlesParser.Classes; // 引入SubtitlesParser的命名空间
using System.Text.RegularExpressions;
using DevExpress.XtraRichEdit.API.Native;
using DiffPlex.DiffBuilder.Model;
using DiffPlex.DiffBuilder;
using DiffPlex;
using DevExpress.Xpo;
using DevExpress.ExpressApp;
using DevExpress.Persistent.AuditTrail;
using com.sun.istack.@internal;
using Humanizer.DateTimeHumanizeStrategy;

//using SubtitlesParser.Classes.Parsers;
namespace AI.Labs.Module.BusinessObjects.VideoTranslate
{
    public class FixSubtitleTime
    {
        public string Subtitle { get; set; }
        public string FixedSubtitle { get; set; }

        public int SkipStartCount { get; set; }
        public int MatchCount { get; set; }
        public int SkipEndCount { get; set; }

        SideBySideDiffBuilder builder;
        public FixSubtitleTime(NSubtitleItem nSubtitleItem)
        {
            var sis = nSubtitleItem.SourceItems.OrderBy(t => t.Oid).ToArray();
            var queue = sis.Select(t => t.Lines);// (t => ClearString(t.Lines));
            var s1 = TextCompare.ClearString(string.Join(" ", queue));
            var s2 = TextCompare.ClearString(nSubtitleItem.EnglishText);

            //相同:
            if(s1 == s2)
            {
                nSubtitleItem.Start = sis.First().StartTime;
                nSubtitleItem.End = sis.Last().EndTime;
                return;
            }

            var diff = nSubtitleItem.Task.GetDiffBuilder().BuildDiffModel(s1, s2);
            //diff.new
            //在old中，被第一个匹配到的位置之前的，删除的，则说明这些是属于上一个字幕的。
            //在old中，被最后一个位置匹配到的位置之后的，删除的，则说明这些是属于下一个字幕的。
            var all = diff.OldText.Lines.SelectMany(t => t.SubPieces).Where(t => t.Text!=null && t.Text != " ").ToArray();
            
            
            var allMatch = all.Where(t => t.Type == ChangeType.Unchanged).ToArray();

            var first = allMatch.FirstOrDefault();
            var last = allMatch.LastOrDefault();
            if (first != null)
            {
                var pre = all.Where(t => t.Position < first.Position && t.Type == ChangeType.Deleted);
                var next = all.Where(t => t.Position > last.Position && t.Type == ChangeType.Deleted);

                var charCount = all.Sum(t => t.Text.Length);

                var p1 = sis.First();
                var p2 = sis.Last();

                var s10 = TextCompare.ClearString(p1.Lines.Replace(" ", ""));
                var perCharTime1 = (p1.EndTime - p1.StartTime).TotalMilliseconds / s10.Length;

                nSubtitleItem.Start = TimeSpan.FromMilliseconds(
                    Math.Round(
                    p1.StartTime.TotalMilliseconds + (pre.Sum(t => t.Text.Length) * perCharTime1), 3
                    )
                    );


                var s20 = TextCompare.ClearString(p2.Lines.Replace(" ", ""));
                var perCharTime2 = (p2.EndTime - p2.StartTime).TotalMilliseconds / s20.Length;
                var end = p2.EndTime.TotalMilliseconds - (next.Sum(t => t.Text.Length) * perCharTime2);
                nSubtitleItem.End = TimeSpan.FromMilliseconds(Math.Round(end - 100, 3));

                SkipStartCount = pre.Count();
                SkipEndCount = next.Count();
                MatchCount = allMatch.Count();

            }
        }

    }

    public class TextCompare
    {
        public static string ClearString(string source)
        {
            if (source == null)
                return "";
            source = source.ToLower();
            source = source.Replace("\n", " ");
            source = source.Replace("\r", "");
            source = source.Replace(",", "");
            source = source.Replace(".", "");
            source = source.Replace("?", "");
            source = source.Replace("!", "");
            source = source.Replace("-"," ");
            source = source.Replace(";", "");
            return source;
        }
        static DiffPiece FindLast(IEnumerable<DiffPiece> pieces)
        {
            DiffPiece last = pieces.FirstOrDefault();

            foreach (var item in pieces)
            {
                if (item.Position > last.Position && item.Position < (last.Position + 10))
                {
                    last = item;
                }
            }
            return last;
        }

        /// <summary>
        /// 检查已匹配的结果
        /// </summary>
        public static void CheckMatchResult(NSubtitleItem nSubtitleItem)
        {
            var queue = nSubtitleItem.SourceItems.OrderBy(t => t.Oid).Select(t => t.Lines);// (t => ClearString(t.Lines));
            var s1 = string.Join(" ", queue);
            var s2 = nSubtitleItem.EnglishText;// ClearString(nSubtitleItem.EnglishText);
            var diff = nSubtitleItem.Task.GetDiffBuilder().BuildDiffModel(s1, s2);
            var rst = new StringBuilder();
            rst.AppendP("字幕:");
            rst.AppendP($"{s1}");
            rst.AppendP($"修正");
            rst.AppendP($"{s2}");

            rst.AppendP("以修正为准，比较结果：");
            GetDiffHtml(diff.NewText, rst);
            rst.AppendP("以原文为准，比较结果：");
            GetDiffHtml(diff.OldText, rst);

            s1 = ClearString(s1);
            s2 = ClearString(s2);
            diff = nSubtitleItem.Task.GetDiffBuilder().BuildDiffModel(s1, s2);

            rst.Append("<hr>");
            rst.AppendP("去除干扰字幕:");
            rst.AppendP($"{s1}");
            rst.AppendP($"去除干扰修正");
            rst.AppendP($"{s2}");

            rst.AppendP("以修正为准，比较结果：");
            GetDiffHtml(diff.NewText, rst);
            rst.AppendP("以原文为准，比较结果：");
            GetDiffHtml(diff.OldText, rst);




            nSubtitleItem.比较详情 = rst.ToString();
        }

        private static void GetDiffHtml(DiffPaneModel diff, StringBuilder rst)
        {
            foreach (var line in diff.Lines)
            {

                if (line.SubPieces == null)
                {
                    //rst.Append("本行内容");
                    //rst.Append(line.Text);
                    //rst.Append("\n");
                }
                else
                {
                    rst.AppendP("子级:");

                    foreach (var subPiece in line.SubPieces)
                    {
                        switch (subPiece.Type)
                        {
                            case ChangeType.Unchanged:
                                rst.Append($"<b>{subPiece.Text}</b>");
                                break;
                            case ChangeType.Deleted:
                                rst.Append($"<font color='red'>{subPiece.Text}</font>");
                                break;
                            case ChangeType.Inserted:
                                rst.Append($"<font color='darkgreen'>{subPiece.Text}</font>");
                                break;
                            //case ChangeType.Imaginary:
                            //    rst.Append($"<font style='color:blue'>{subPiece.Text}</font>");
                            //    break;
                            case ChangeType.Modified:
                                rst.Append($"<b>{subPiece.Text}</b>");
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        public static void MatchSourceSubtitles(NSubtitleItem newSubtitle)
        {
            var task = newSubtitle.Task;
            var diffBuilder = task.GetDiffBuilder();
            //之前已经匹配到了这里
            var beforeMax = task
                .NSubtitleItems
                .Where(t => t.Oid < newSubtitle.Oid)
                .OrderByDescending(t => t.Oid)
                .FirstOrDefault()?

                .SourceItemOids?.Split(',')
                .Select(t => int.Parse(t))
                .Max() ?? 0;

            var 未匹配的字幕 = task.SubtitleItems.Where(t => t.Oid >= beforeMax).OrderBy(t => t.Index).ToList();

            var queue = 未匹配的字幕.ToArray();
            var sentence = newSubtitle.EnglishText;

            //foreach (var item in newSubtitle.SourceItems.ToArray())
            //{
            //    newSubtitle.SourceItems.Remove(item);
            //}

            var s1 = string.Join(" ", queue.Select(t => $"[{t.Oid}] {ClearString(t.Lines)} [{t.Oid}]"));
            var s2 = ClearString(sentence);
            var diff = diffBuilder.BuildDiffModel(s1, s2);

            var pieces = diff.OldText.Lines.SelectMany(t => t.SubPieces).Where(t => t.Text != null && t.Text != " " && t.Type == ChangeType.Unchanged).ToArray();
            var first = pieces.FirstOrDefault();
            var last = FindLast(pieces);
            if (last != null)
            {
                var oids = diff.OldText.Lines.SelectMany(t => t.SubPieces).Where(t => t.Position >= first.Position && t.Position <= last.Position && t.Text!=null && Regex.IsMatch(t.Text, @"^\[\d+\]")).Distinct().Select(t => t.Text.TrimStart('[').TrimEnd(']'));
                //oids为空，可能是因为 被匹配到的字幕正好是中间位置，找不到oid如:
                //加过标点的字幕是: hello,world
                //源字幕是:[oid]abc hello world cde[oid]
                //这时候匹配到的是hello world,但是找不到oid
                //这时候需要找到最近的一个oid
                if (oids.Count() == 0)
                {
                    var lastOid = diff.OldText.Lines.SelectMany(t => t.SubPieces).FirstOrDefault(t => t.Text!=null && Regex.IsMatch(t.Text, @"^\[\d+\]") && t.Position >= last.Position);
                    if (lastOid != null)
                    {
                        oids = new string[] { lastOid.Text.TrimStart('[').TrimEnd(']') };
                    }
                }
                newSubtitle.SourceItemOids = string.Join(",", oids);

                new FixSubtitleTime(newSubtitle);

            }
#warning 如果没有匹配到，则报错
        }



        public static void MatchSourceSubtitles(TranslateTask task)
        {
            if (string.IsNullOrEmpty(task.Result))
            {
                throw new UserFriendlyException("TranslateTask.Result为空!请先加上标点符号!");
            }
            var sentencesText = task.Result;
            string[] sentences = SplitIntoSentences(sentencesText);

            // 生成差异
            var session = task.Session;

            int index = 0;
            foreach (var sentence in sentences)
            {
                var subtitle = new NSubtitleItem(session);
                subtitle.EnglishText = sentence;
                subtitle.Index = index++;
                task.NSubtitleItems.Add(subtitle);
                //处理每个句子

                //var 未匹配的字幕 = task.SubtitleItems.OrderBy(t => t.Index).ToList();
                ////之前已经匹配到了这里

                //                var beforeMax = task
                //                    .NSubtitleItems
                //                    .OrderByDescending(t => t.Index)
                //                    .FirstOrDefault()?

                //                    .SourceItemOids?.Split(',')
                //                    .Select(t => int.Parse(t))
                //                    .Max() ?? 0;
#warning 由于oid是后生成的,所以一定要保证是存储到库里后才有内容,后面可以全部按Index来处理
                //                var 未匹配的字幕 = task.SubtitleItems.Where(t => t.Oid >= beforeMax).OrderBy(t => t.Index).ToList();
                MatchSourceSubtitles(subtitle);
            }






            //TextCompare.MatchSourceSubtitles(item, 未匹配的字幕, diffBuilder);

        }

        // 将文本分割为句子数组
        static string[] SplitIntoSentences(string text)
        {
            // 使用正则表达式分割文本为句子
            return Regex.Split(text, @"(?<=[.!?])\s+");
        }

        // 计算Levenshtein距离
        static int LevenshteinDistance(string s1, string s2)
        {
            int[,] distance = new int[s1.Length + 1, s2.Length + 1];

            for (int i = 0; i <= s1.Length; i++)
                distance[i, 0] = i;

            for (int j = 0; j <= s2.Length; j++)
                distance[0, j] = j;

            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                    distance[i, j] = Math.Min(Math.Min(
                        distance[i - 1, j] + 1,
                        distance[i, j - 1] + 1),
                        distance[i - 1, j - 1] + cost);
                }
            }

            return distance[s1.Length, s2.Length];
        }

        // 找出句子中包含的s1中的行
        static List<int> FindMatchingLines(string sentence, string s1)
        {
            List<int> matchingLines = new List<int>();

            // 将s1分割为行数组
            string[] s1Lines = s1.Split('\n');

            // 遍历每一行，检查是否包含在句子中
            for (int i = 0; i < s1Lines.Length; i++)
            {
                var count = (decimal)s1Lines[i].Split(' ').Length;
                // 使用Levenshtein距离进行模糊匹配
                var rst = Compare(s1Lines[i], sentence);
                var grst = rst.NewText.Lines.SelectMany(t => t.SubPieces).GroupBy(t => t.Type);
                var equ = rst.NewText.Lines.SelectMany(t => t.SubPieces.Where(v => v.Text != " " && v.Type == ChangeType.Unchanged));
                if (equ.Count() > (count / 2))
                {
                    matchingLines.Add(i); // 添加匹配的行号（从1开始）
                }
                //if (LevenshteinDistance(sentence, s1Lines[i]) <= Math.Max(sentence.Length, s1Lines[i].Length) / 3)
                //{
                //    
                //}
            }

            return matchingLines;
        }
        /// <summary>
        /// 比较两行文本，返回差异
        /// </summary>
        /// <param name="text1"></param>
        /// <param name="text2"></param>
        /// <returns></returns>
        public static SideBySideDiffModel Compare(string text1, string text2)
        {
            var diffBuilder = new SideBySideDiffBuilder(new Differ());
            // 生成差异
            return diffBuilder.BuildDiffModel(text1.ToLower(), text2.ToLower());
        }

        public static void Compare(string text1, string text2, Document output)
        {
            // 创建一个 diff builder
            var diffBuilder = new SideBySideDiffBuilder(new Differ());
            text1 = text1.Replace("\n", " ").Replace("\r", "");
            text2 = text2.Replace("\n", " ").Replace("\r", "");
            // 生成差异
            var result = diffBuilder.BuildDiffModel(text1.ToLower(), text2.ToLower());

            // 输出原文和修改后的文本的差异
            output.AppendText("Old Text Diff:");
            output.AppendHtmlText(
            DisplayDifferences(result.OldText));

            output.AppendText("\nNew Text Diff:");

            output.AppendHtmlText(DisplayDifferences(result.NewText));
        }
        //需要比较英文与加标点的结果
        static string DisplayDifferences(DiffPaneModel diffPane)
        {
            var rst = new StringBuilder();

            foreach (var line in diffPane.Lines)
            {
                rst.Append("本行内容");
                rst.Append(line.Text);
                rst.Append("\n");
                if (line.SubPieces == null)
                {

                }
                else
                {
                    rst.Append("子级:");

                    foreach (var subPiece in line.SubPieces)
                    {
                        switch (subPiece.Type)
                        {
                            case ChangeType.Unchanged:
                                rst.Append(subPiece.Text);
                                break;
                            case ChangeType.Deleted:
                                rst.Append($"<span style='color:red'>{subPiece.Text}</span>");
                                break;
                            case ChangeType.Inserted:
                                rst.Append($"<span style='color:green'>{subPiece.Text}</span>");
                                break;
                            case ChangeType.Imaginary:
                                rst.Append($"<span style='color:blue'>{subPiece.Text}</span>");
                                break;
                            case ChangeType.Modified:
                                rst.Append($"<b>{subPiece.Text}</b>");
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            return rst.ToString();
        }
    }

}
