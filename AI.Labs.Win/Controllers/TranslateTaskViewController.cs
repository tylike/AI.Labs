using System.Text;
//using SubtitlesParser.Classes; // 引入SubtitlesParser的命名空间
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DiffPlex.DiffBuilder.Model;
using DiffPlex.DiffBuilder;
using DiffPlex;
using System.Diagnostics;
using DevExpress.XtraRichEdit;
using DevExpress.ExpressApp.Office.Win;
using System;
using System.Threading.Tasks;

//using SubtitlesParser.Classes.Parsers;
namespace AI.Labs.Module.BusinessObjects.VideoTranslate
{
    public class TranslateTaskViewController : ObjectViewController<ObjectView, TranslateTask>
    {
        public TranslateTaskViewController()
        {
            var runTranslate = new SimpleAction(this, "翻译", null);
            runTranslate.Execute += RunTranslate_Execute;
            //var runAddSymbol = new SimpleAction(this, "加标点", null);
            //runAddSymbol.Execute += RunAddSymbol_Execute;

            var compare = new SimpleAction(this, "对比", null);
            compare.Execute += Compare_Execute;

            var 后推未处理的 = new SimpleAction(this, "后推", null);
            后推未处理的.Execute += 后推未处理的_Execute;

            var matchSourceSubtitle = new SimpleAction(this, "匹配源字幕", null);
            matchSourceSubtitle.Execute += MatchSourceSubtitle_Execute;



        }

        private void MatchSourceSubtitle_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            foreach (TranslateTask item in e.SelectedObjects)
            {
                TextCompare.MatchSourceSubtitles(item);
            }
        }

        string RemoveSymbol(string input)
        {
            var removeSymbol = input.Replace(",", "");
            removeSymbol = removeSymbol.Replace(".", "");
            removeSymbol = removeSymbol.Replace("?", "");
            removeSymbol = removeSymbol.Replace("!", "");
            removeSymbol = removeSymbol.Replace(";", "");
            removeSymbol = removeSymbol.Replace(":", "");
            removeSymbol = removeSymbol.Replace("，", "");
            removeSymbol = removeSymbol.Replace("。", "");
            removeSymbol = removeSymbol.Replace("？", "");
            removeSymbol = removeSymbol.Replace("！", "");
            removeSymbol = removeSymbol.Replace("；", "");
            removeSymbol = removeSymbol.Replace("：", "");
            removeSymbol = removeSymbol.Replace("  ", " ");
            removeSymbol = removeSymbol.Trim().ToLower();
            return removeSymbol;
        }
        int FindLastSentenceIndex(string text)
        {
            char[] finds = new char[] { '.', '?', '!', ';', ':', '，', '。', '？', '！', '；', '：' };
            int lastIndex = -1;
            //如果是符号结尾的,可能是自己填加的,也可能是本来就结束的

            foreach (var item in finds)
            {
                var i = text.LastIndexOf(item);
                if (lastIndex < i)
                {
                    lastIndex = i;
                }
            }
            return lastIndex;
        }

        private void 后推未处理的_Execute(object sender, SimpleActionExecuteEventArgs e)
        {

            foreach (TranslateTask t in e.SelectedObjects)
            {
                var txt = t.Text.ToLower();
                var lastIndex = FindLastSentenceIndex(t.Result);
                if (lastIndex > -1)
                {
                    if (lastIndex + 1 < t.Result.Length)
                    {
                        lastIndex++;

                        var last = t.Result.Substring(lastIndex).Trim().ToLower();
                        last = RemoveSymbol(last);
                        var rst = -1;
                        string nextProcess = null;
                        for (int i = last.Length; i > 0; i--)
                        {
                            var ends = string.Join("", last.TakeLast(i));
                            rst = txt.LastIndexOf(ends);
                            if (rst > -1)
                            {
                                nextProcess = t.Text.Substring(rst);
                                break;
                            }
                            var starts = string.Join("", last.Take(i));
                            rst = txt.LastIndexOf(starts);
                            if (rst > -1)
                            {
                                nextProcess = t.Text.Substring(rst);
                                break;
                            }
                        }
                        Debug.WriteLine(nextProcess);
                    }
                }
            }
        }

        public static string CompareText(string text1, string text2)
        {
            // 创建一个 diff builder
            var diffBuilder = new InlineDiffBuilder(new Differ());

            // 生成差异
            var result = diffBuilder.BuildDiffModel(text1, text2);
            var sb = new StringBuilder();
            // 遍历差异结果并输出
            foreach (var line in result.Lines)
            {
                switch (line.Type)
                {
                    case ChangeType.Inserted:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("+ ");
                        sb.Append("+");
                        break;
                    case ChangeType.Deleted:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("- ");
                        sb.Append("-");
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.Write("  ");
                        sb.Append(" ");
                        break;
                }

                Console.WriteLine(line.Text);
                sb.AppendLine(line.Text);
            }

            // 重置控制台颜色
            Console.ResetColor();
            return sb.ToString();
        }

        private void Compare_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            foreach (TranslateTask item in e.SelectedObjects)
            {
                //var editor = View.GetItems<RichTextPropertyEditor>().First();
                item.GenerateSourceAndSymbolDiff(null);
            }
        }

        //private async void RunAddSymbol_Execute(object sender, SimpleActionExecuteEventArgs e)
        //{

        //    var sa = sender as SimpleAction;
        //    var caption = sa.Caption;
        //    int i = 0;
        //    foreach (TranslateTask t in e.SelectedObjects)
        //    {
        //        //                
        //        //{t.Lines}
        //        //");
        //        sa.Caption = $"{caption}:{i++}/{e.SelectedObjects.Count}";

        //        var text = t.Source;// 
        //        if (string.IsNullOrEmpty(text))
        //        {
        //            var texts = t.SubtitleItems.OrderBy(x => x.Index).Select(t => t.Lines);

        //            text = string.Join("\n\n", texts);
        //        }
        //        t.Result = "";
        //        await AIHelper.Ask(t.Video.AddSymbolPrompt, text,
        //            cm =>
        //            {
        //                t.Result += cm.Content;
        //                Application.UIThreadDoEvents();
        //            },
        //            streamOut: true,
        //            url: t.Video.Model.ApiUrlBase,
        //            api_key: t.Video.Model.ApiKey,
        //            modelName: t.Video.Model.Name
        //            );
        //    }
        //    sa.Caption = caption;
        //}

        //我想写一个英文字幕翻译为中文字幕的程序:
        //翻译功能使用大模型来帮我完成,这个已经完成。

        //1.在英文的srt文件中，并没有标点符号，我需要使用我自己的大模型加上标点符号。
        //1.1.英文字幕文件并没有按一句话来断句，我想为字幕加上标点符号，并按一句话或半句话来断句。（已完成）
        //1.2中英文需要时间上大致对齐。（如何实现？）

        //2.我需要在将加上标点符号的内容进行断句，根据句子长度来决定，具体是一句还是半句，我需要根实验才能知道。

        //3.断句后，我需要去srt中找到这句话所在的时间，并大致推算一下当前句子的时间，直到处理所有的句子。(如何实现？)

        //4.断句完成后，我再去进行翻译，仍然使用大模型，一句一句的翻译，但会把整段内容传给大模型，让他参考以便得到更多的上下文信息。（如何实现？）




        private async void RunTranslate_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            foreach (TranslateTask t in e.SelectedObjects)
            {
                t.Text = "";
                var texts = t.SubtitleItems.OrderBy(x => x.Index).Select(t => $"{t.Index}\n{t.Lines}");
                var context = string.Join("\n\n", texts);
                //await AIHelper.Ask(t.Video.TranslateTaskPrompt, context,
                //    cm =>
                //    {
                //        t.Text += cm.Content;
                //        Application.UIThreadDoEvents();
                //    },
                //    streamOut: true,
                //    url: t.Video.Model.ApiUrlBase,
                //    api_key: t.Video.Model.ApiKey,
                //    modelName: t.Video.Model.Name
                //    );

                foreach (var item in t.SubtitleItems)
                {
                    var text = item.PlainText; //string.Join("\n\n", item.PlainText);
                    item.CnText = "";

                    await AIHelper.Ask(t.Video.TranslateTaskPrompt + "\n上下文内容:" + context, "要翻译的句子:\n" + text,
                        cm =>
                        {
                            item.CnText += cm.Content;
                            Application.UIThreadDoEvents();
                        },
                        t.Video.Model,
                        streamOut: true                        
                        );
                }

            }
        }
    }

}
