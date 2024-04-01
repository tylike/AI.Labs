using System.Text;
//using SubtitlesParser.Classes; // 引入SubtitlesParser的命名空间
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DiffPlex.DiffBuilder.Model;
using DiffPlex.DiffBuilder;
using DiffPlex;
using System.Diagnostics;

//using SubtitlesParser.Classes.Parsers;
namespace AI.Labs.Module.BusinessObjects.VideoTranslate
{
    public class TranslateTaskViewController : ObjectViewController<ObjectView, TranslateTask>
    {
        static void Compare(string text1,string text2,RichEdit)
        {
            string text1 = " learn about Vector embeddings  which transform Rich data like words or images into numerical vectors that capture their Essence  this course from anikubo will help you understand the significance of text embeddings showcase  their diverse applications  guide you through generating your own with open Ai and even delve into integrating vectors with databases   by the end  you'll be equipped to build an AI assistant using these powerful representations so let's begin          hi everyone and welcome to this course all about vector embeddings  by the end of this course  you will be able to understand what Vector embeddings are  how they are generated  as well as understand why we even care about them in the first place   we are going to do this thanks to visual explainers as well as some hands-on experience by building out a project that uses Vector embeddings to submit your understanding of them by the end my name is Anya Kubo  and I'm a software developer and course creator on YouTube  as well as on codewithanyu.com and I'm going to be your guide to this hot but slightly complex topic so before we get going   let's just have a quick look at what this course will cover      so first off  we're going to learn what Vector embeddings are in the first place and what they're used for  after we understand that  I will show you what a real Vector embedding looks like and show you how to make one yourself and after that I will delve into why companies might want to store Vector embeddings in a database as well as show you how to store Vector embeddings in your own database just as a company focused on AI word next we will take a quick look at a popular package called Lang Chen that will help us with the next part of making an AI assistant in Python and if you don't know any python don't worry I'm going to talk you through it step by step okay so a lot to learn but by the end you should be an expert in this aspect of AI development so what are we waiting for let's do it what are vector embeddings in computer";
            string text2 = " Learn about Vector Embeddings, which transform Rich data like words or images into numerical vectors that capture their essence. This course from Anikubo will help you understand the significance of Text Embeddings, showcase their diverse applications, guide you through generating your own with Open AI, and even delve into integrating vectors with databases. By the end, you'll be equipped to build an AI assistant using these powerful representations. So let's begin!\r\n\r\nHi everyone and welcome to this course all about Vector Embeddings. By the end of this course, you will be able to understand what Vector Embeddings are, how they are generated, as well as understand why we even care about them in the first place. We are going to do this thanks to visual explainers, as well as some hands-on experience by building out a project that uses Vector Embeddings to submit your understanding of them.           My name is Anya Kubo, and I'm a software developer and course creator on YouTube, as well as on CodeWithAnyu.com.            I'll be your guide to this hot but slightly complex topic. So before we get going, let's just have a quick look at what this course will cover:\r\n\r\nFirst off, we're going to learn what Vector Embeddings are in the first place and what they're used for. After we understand that, I will show you what a real Vector Embedding looks like and show you how to make one yourself. Next,         I will delve into why companies might want to store Vector Embeddings in a database, as well as show you how to store Vector Embeddings in your own database. Just as a company focused on AI,          we'll take a quick look at a popular package called LangChain that will help us with the next part of making an AI Assistant in Python. And if you don't know any Python, don't worry; I'm going to talk you through it step by step.\r\n\r\nOkay, so a lot to learn, but by the end, you should be an expert in this aspect of AI development. So what are we waiting for? Let's do it!";

            // 创建一个 diff builder
            var diffBuilder = new SideBySideDiffBuilder(new Differ());

            // 生成差异
            var result = diffBuilder.BuildDiffModel(text1.ToLower(), text2.ToLower());

            // 输出原文和修改后的文本的差异
            Console.WriteLine("Old Text Diff:");
            DisplayDifferences(result.OldText);
            Console.WriteLine("\nNew Text Diff:");
            DisplayDifferences(result.NewText);
        }
        //需要比较英文与加标点的结果
        static void DisplayDifferences(DiffPaneModel diffPane)
        {
            foreach (var line in diffPane.Lines)
            {
                if (line.SubPieces == null)
                {
                    Console.Write(line.Text);
                }
                else
                {
                    foreach (var subPiece in line.SubPieces)
                    {
                        switch (subPiece.Type)
                        {
                            case ChangeType.Inserted:
                                Console.ForegroundColor = ConsoleColor.Green;
                                break;
                            case ChangeType.Deleted:
                                Console.ForegroundColor = ConsoleColor.Red;
                                break;
                            default:
                                Console.ForegroundColor = ConsoleColor.Gray;
                                break;
                        }

                        Console.Write(subPiece.Text);
                        Console.ResetColor();
                    }
                }

                Console.WriteLine();
            }
        }


        public TranslateTaskViewController()
        {
            var runTranslate = new SimpleAction(this, "翻译", null);
            runTranslate.Execute += RunTranslate_Execute;
            var runAddSymbol = new SimpleAction(this, "加标点", null);
            runAddSymbol.Execute += RunAddSymbol_Execute;

            var compare = new SimpleAction(this, "对比", null);
            compare.Execute += Compare_Execute;

            var 后推未处理的 = new SimpleAction(this, "后推", null);
            后推未处理的.Execute += 后推未处理的_Execute;

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
                var rst = CompareText(item.Text, item.Result);
                Debug.WriteLine(rst);
            }

        }

        private async void RunAddSymbol_Execute(object sender, SimpleActionExecuteEventArgs e)
        {

            var sa = sender as SimpleAction;
            var caption = sa.Caption;
            int i = 0;
            foreach (TranslateTask t in e.SelectedObjects)
            {
                //                
                //{t.Lines}
                //");
                sa.Caption = $"{caption}:{i++}/{e.SelectedObjects.Count}";

                var text = t.Source;// 
                if (string.IsNullOrEmpty(text))
                {
                    var texts = t.SubtitleItems.OrderBy(x => x.Index).Select(t => t.Lines);

                    text = string.Join("\n\n", texts);
                }
                t.Result = "";
                await AIHelper.Ask(t.Video.AddSymbolPrompt, text,
                    cm =>
                    {
                        t.Result += cm.Content;
                        Application.UIThreadDoEvents();
                    },
                    streamOut: true,
                    url: t.Video.Model.ApiUrlBase,
                    api_key: t.Video.Model.ApiKey,
                    modelName: t.Video.Model.Name
                    );
            }
            sa.Caption = caption;
        }

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
                var texts = t.SubtitleItems.OrderBy(x => x.Index).Select(t => t.Lines);
                var text = string.Join("\n\n", texts);
                await AIHelper.Ask(t.Video.TranslateTaskPrompt, text,
                    cm =>
                    {
                        t.Text += cm.Content;
                        Application.UIThreadDoEvents();
                    },
                    streamOut: true,
                    url: t.Video.Model.ApiUrlBase,
                    api_key: t.Video.Model.ApiKey,
                    modelName: t.Video.Model.Name
                    );
            }
        }
    }

}
