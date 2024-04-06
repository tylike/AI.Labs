//using SubtitlesParser.Classes; // 引入SubtitlesParser的命名空间
using AI.Labs.Module.BusinessObjects.KnowledgeBase;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Office.Win;
using DevExpress.XtraRichEdit;
using DevExpress.XtraRichEdit.API.Native;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Text;
using Browser;
using System.IO;
using DevExpress.ExpressApp.Xpo;
using static sun.font.LayoutPathImpl;
using org.omg.CosNaming.NamingContextPackage;
using DevExpress.XtraRichEdit.API.Layout;
using com.sun.org.apache.xml.@internal.res;
using YoutubeExplode.Demo.Cli;
using YoutubeExplode.Demo.Cli.Utils;
using YoutubeExplode.Videos;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using DevExpress.ExpressApp.SystemModule;
using IPlugins;
using System.Reflection;
//using SubtitlesParser.Classes.Parsers;
namespace AI.Labs.Module.BusinessObjects.VideoTranslate
{
    public class ProcessedItem
    {
        string Trim(string text)
        {
            if (text.EndsWith("..."))
            {
                return text[0..^3];
            }
            return text;
        }

        string _content;
        public string Content
        {
            get
            {
                if (string.IsNullOrEmpty(_content))
                {
                    _content = string.Join(" ", Source.Select(x => Trim(x.Lines)));
                }
                return _content;
            }
        }

        public List<SubtitleItem> Source { get; } = new List<SubtitleItem>(); // 来源条目的索引列表
    }

    public class WinVideoInfoViewController : ObjectViewController<ObjectView, VideoInfo>
    {
        public WinVideoInfoViewController()
        {
            var runVideoScript = new SimpleAction(this, "运行视频脚本", null);
            runVideoScript.Execute += RunVideoScript_Execute;
        }
        private async void RunVideoScript_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            Output("开始执行脚本!");
            await Task.Run(() =>
            {
                RunVideoScriptCore();
            });
        }

        private void RunVideoScriptCore()
        {
            var video = ViewCurrentObject;
            WeakReference weakReference;

            LoadRun(out weakReference);

            // 循环尝试强制GC，直到WeakReference不再存活或达到循环次数限制
            for (int i = 0; i < 10 && weakReference.IsAlive; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                Thread.Sleep(500); // 稍微等待一段时间，让系统有时间处理卸载

                if (!weakReference.IsAlive)
                {
                    Output("成功:插件执行完毕,卸载完成!");
                    break;
                }
            }

            if (weakReference.IsAlive)
            {
                Output("强制卸载之后,仍然没有成功卸载\n报错:上下文尚未完全卸载。可能需要进一步检查持有的引用。");
            }
        }
        void Output(string msg)
        {
            Application.UIThreadInvoke(() =>
            {
                ViewCurrentObject.Output(msg);
            });
        }

        private void LoadRun(out WeakReference weakReference)
        {
            string pluginPath = @"D:\dev\AI.Labs\RuntimePlugin\bin\Debug\net7.0-windows\RuntimePlugin.dll";

            try
            {
                var video = ViewCurrentObject;
                var IPlugin = typeof(IPlugin);
                var pluginLoadContext = new PluginLoadContext(pluginPath);
                var assembly = pluginLoadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginPath)));

                var pluginType = assembly.GetTypes().FirstOrDefault(t => IPlugin.IsAssignableFrom(t));

                if (pluginType == null)
                {
                    Output("插件没有实现IPlugin接口或是抽象的!");
                }

                var pluginInstance = (IPlugin<VideoInfo>)Activator.CreateInstance(pluginType);
                // 传入所需的参数
                try
                {
                    pluginInstance.Invoke(this.ViewCurrentObject, this);
                    Output("插件执行完成,等待卸载!");
                }
                catch (Exception ex)
                {
                    Output("执行报错:");
                    Output(ex.Message);
                }
                finally
                {
                    pluginInstance.Dispose();
                    pluginInstance = null;
                }


                weakReference = new WeakReference(pluginLoadContext, true);

                pluginLoadContext.Unload();
                pluginLoadContext = null;

            }
            catch (Exception ex)
            {
                // 处理加载插件或执行时的异常
                //Console.WriteLine($"An error occurred: {ex.Message}");
                throw ex;
            }
        }




    }

    //    public class VideoViewController : ObjectViewController<DetailView, VideoInfo>
    //    {
    //        public VideoViewController()
    //        {
    //            var loadText = new SimpleAction(this, "加载字幕到编辑器", null);
    //            loadText.ToolTip = "加载字幕到Excel Sheet编辑器中";
    //            loadText.Execute += LoadText_Execute;

    //            //var addSymbol = new SimpleAction(this, "修复标点", null);
    //            //addSymbol.Execute += AddSymbol_Execute;

    //            var showChatGPT = new SimpleAction(this, "ShowChatGPT", null);
    //            showChatGPT.Execute += ShowChatGPT_Execute;

    //            var saveFixedSRT = new SimpleAction(this, "保存修复的字幕", null);
    //            saveFixedSRT.Execute += SaveFixedSRT_Execute;

    //            //var splitBySentence = new SimpleAction(this, "按句子拆分", null);
    //            //splitBySentence.Execute += SplitBySentence_Execute;

    //            var loadFixedSRTFromFile = new SimpleAction(this, "加载标点字幕", null);
    //            loadFixedSRTFromFile.ToolTip = "从文件加载修复过标点符号的字幕文件";
    //            loadFixedSRTFromFile.Execute += LoadFixedSRTFromFile_Execute;


    //        }


    //        private void LoadFixedSRTFromFile_Execute(object sender, SimpleActionExecuteEventArgs e)
    //        {
    //            var ofd = new OpenFileDialog();
    //            ofd.Filter = "SRT字幕文件|*.srt;*.txt";
    //            ofd.InitialDirectory = ViewCurrentObject.ProjectPath;
    //            if (ofd.ShowDialog() == DialogResult.OK)
    //            {
    //                LoadFixedSRT(File.ReadAllText(ofd.FileName));
    //            }
    //        }

    //        bool HasSentence(string text)
    //        {
    //            if (text.EndsWith("..."))
    //            {
    //                text = text.Substring(0, text.Length - "...".Length);
    //            }
    //            return text.Contains(". ")
    //                ||
    //                text.Contains("? ")
    //                ||
    //                text.Contains("! ");
    //        }

    //        string[] SplitBySentence(string text)
    //        {
    //            if (text.EndsWith("..."))
    //            {
    //                text = text.Substring(0, text.Length - "...".Length);
    //            }
    //            text = text.Replace(". ", ". ||");
    //            text = text.Replace("! ", "! ||");
    //            text = text.Replace("? ", "? ||");
    //            return text.Split("||", StringSplitOptions.RemoveEmptyEntries);
    //        }

    //        public List<SubtitleItem> SplitBySentence(SubtitleItem item)
    //        {
    //            var startTime = item.StartTime;
    //            var endTime = item.EndTime;

    //            string str = item.Lines; //"这是一个测试.请根据这个测试来拆分字符串."; // 字符串
    //            bool hasContinue = str.EndsWith("...");

    //            string[] segments = SplitBySentence(str); // 根据"."拆分字符串
    //            double totalMilliseconds = (endTime - startTime).TotalMilliseconds; // 计算总时间
    //            double timePerChar = totalMilliseconds / str.Length; // 计算每个字符的时间

    //            TimeSpan currentStartTime = startTime;

    //            List<SubtitleItem> items = new List<SubtitleItem>();

    //            foreach (string segment in segments)
    //            {
    //                double segmentTime = segment.Length * timePerChar; // 计算每段的时间
    //                TimeSpan segmentEndTime = currentStartTime.Add(TimeSpan.FromMilliseconds(segmentTime)); // 计算每段的结束时间
    //                Console.WriteLine($"开始时间：{currentStartTime}, 结束时间：{segmentEndTime}, 内容：\"{segment}\"");
    //                var newSubitem = new SubtitleItem(item.Session) { StartTime = currentStartTime, EndTime = segmentEndTime, Lines = segment };
    //                newSubitem.Index = item.Index;
    //                items.Add(newSubitem);

    //                currentStartTime = segmentEndTime; // 更新开始时间为当前段的结束时间
    //            }

    //            if (items.Any() && hasContinue)
    //            {
    //                items.Last().Lines += "...";
    //            }
    //            return items;
    //        }

    //        List<SubtitleItem> SplitsBySentence(List<SubtitleItem> items)
    //        {
    //            var processedItems = new List<SubtitleItem>();
    //            foreach (var item in items)
    //            {
    //                if (HasSentence(item.Lines))
    //                {
    //                    var splits = SplitBySentence(item);
    //                    item.Splits.AddRange(splits);
    //                    processedItems.AddRange(splits);
    //                }
    //                else
    //                {
    //                    processedItems.Add(item);
    //                }
    //            }
    //            return processedItems;
    //        }

    //        List<ProcessedItem> MergeItems(List<SubtitleItem> items)
    //        {
    //            var processedItems = new List<ProcessedItem>();
    //            var pi = new ProcessedItem();
    //            processedItems.Add(pi);

    //            foreach (var item in items)
    //            {
    //                pi.Source.Add(item);

    //                if (!item.Lines.EndsWith("..."))
    //                {
    //                    pi = new ProcessedItem();
    //                    processedItems.Add(pi);
    //                }

    //            }

    //            return processedItems;
    //        }


    //        //private void SplitBySentence_Execute(object sender, SimpleActionExecuteEventArgs e)
    //        //{
    //        //    var sheet = View.GetItems<SpreadsheetPropertyEditor>().First(t => t.Id == nameof(VideoInfo.SplitMerge));

    //        //    var doc = sheet.SpreadsheetControl.Document.Worksheets.FirstOrDefault();
    //        //    var srt = ViewCurrentObject.FixSRTPrompt;
    //        //    #region fix and check
    //        //    srt = SrtFixer.AutoFixSrtFormat(srt);
    //        //    var rst = SrtChecker.CheckSrt(srt);
    //        //    if (!string.IsNullOrEmpty(rst))
    //        //    {
    //        //        throw new UserFriendlyException(rst);
    //        //    }
    //        //    #endregion

    //        //    sheet.SpreadsheetControl.Document.BeginUpdate();

    //        //    var session = (Application.CreateObjectSpace(typeof(SubtitleItem)) as XPObjectSpace).Session;
    //        //    var fixedTitles = SRTHelper.ParseSrtStringContentToObject(srt, session, false);
    //        //    var splits = SplitsBySentence(fixedTitles.ToList());

    //        //    //拆分的:最小
    //        //    //foreach (var item in splits)
    //        //    //{
    //        //    //    doc.Cells[item.Index, 4].Value = item.Lines;
    //        //    //    doc.Cells[item.Index, 5].Value = TextCompare.ClearString(item.Lines);
    //        //    //    doc.Cells[item.Index, 6].Value = TextCompare.ClearString(doc.Cells[item.Index, 3].Value.TextValue);
    //        //    //}

    //        //    //合并的.
    //        //    var merges = MergeItems(splits);

    //        //    int k = 1;
    //        //    //foreach (var item in fixedTitles)
    //        //    //{
    //        //    //    doc.Cells[k, 4].Value = item.Lines;
    //        //    //    doc.Cells[k, 5].Value = TextCompare.ClearString(item.Lines);
    //        //    //    doc.Cells[k, 6].Value = TextCompare.ClearString(doc.Cells[k, 3].Value.TextValue);
    //        //    //    k++;
    //        //    //}

    //        //    //最终的:可能是句子最少的
    //        //    k = 1;
    //        //    var nsubtitles = new List<SubtitleItem>();
    //        //    foreach (var item in merges)
    //        //    {
    //        //        //k = item.Source.First().Index + 1;
    //        //        var ni = new SubtitleItem(session)
    //        //        {
    //        //            StartTime = item.Source.First().StartTime,
    //        //            EndTime = item.Source.Last().EndTime,
    //        //            Lines = item.Content,
    //        //            Index = k
    //        //        };
    //        //        nsubtitles.Add(ni);

    //        //        doc.Cells[k, 0].Value = ni.StartTime.ToString("hh\\:mm\\:ss\\,fff");
    //        //        doc.Cells[k, 1].Value = ni.EndTime.ToString("hh\\:mm\\:ss\\,fff");
    //        //        doc.Cells[k, 2].Value = (ni.EndTime - ni.StartTime).TotalSeconds;

    //        //        doc.Cells[k, 3].Value = ni.Lines;
    //        //        //int x = 0;
    //        //        doc.MergeCells(doc.Range.FromLTRB(0, k, 0, k + item.Source.Count - 1));
    //        //        doc.MergeCells(doc.Range.FromLTRB(1, k, 1, k + item.Source.Count - 1));
    //        //        doc.MergeCells(doc.Range.FromLTRB(2, k, 2, k + item.Source.Count - 1));
    //        //        doc.MergeCells(doc.Range.FromLTRB(3, k, 3, k + item.Source.Count - 1));


    //        //        foreach (var s in item.Source)
    //        //        {
    //        //            doc.Cells[k, 4].Value = s.Lines;
    //        //            doc.Cells[k, 5].Value = s.StartTime.ToString("hh\\:mm\\:ss\\,fff");
    //        //            doc.Cells[k, 6].Value = s.EndTime.ToString("hh\\:mm\\:ss\\,fff");
    //        //            k++;

    //        //            //source index是来源的索引

    //        //        }

    //        //        //k += item.Source.Count;
    //        //    }

    //        //    k = 1;
    //        //    foreach (var s in fixedTitles)
    //        //    {
    //        //        doc.Cells[k, 7].Value = s.Lines;
    //        //        doc.Cells[k, 8].Value = s.StartTime.ToString("hh\\:mm\\:ss\\,fff");
    //        //        doc.Cells[k, 9].Value = s.EndTime.ToString("hh\\:mm\\:ss\\,fff");

    //        //        if (s.Splits.Count > 1)
    //        //        {
    //        //            doc.MergeCells(doc.Range.FromLTRB(7, k, 7, k + s.Splits.Count - 1));
    //        //            doc.MergeCells(doc.Range.FromLTRB(8, k, 8, k + s.Splits.Count - 1));
    //        //            doc.MergeCells(doc.Range.FromLTRB(9, k, 9, k + s.Splits.Count - 1));
    //        //        }


    //        //        k += (s.Splits.Count == 0 ? 1 : s.Splits.Count);

    //        //    }

    //        //    SRTHelper.SaveToSrtFile(nsubtitles, Path.Combine(ViewCurrentObject.ProjectPath, "按句分英文.srt"), SrtLanguage.英文);
    //        //    sheet.SpreadsheetControl.Document.EndUpdate();
    //        //}

    //        private void SaveFixedSRT_Execute(object sender, SimpleActionExecuteEventArgs e)
    //        {
    //            //ViewCurrentObject.Subtitles.Count = 0;

    //            var doc = sheet.SpreadsheetControl.Document.Worksheets.FirstOrDefault();
    //            var srt = ViewCurrentObject.FixSRTPrompt;
    //            if (string.IsNullOrEmpty(srt))
    //            {
    //                var sb = new StringBuilder();
    //                for (var i = 1; i <= ViewCurrentObject.Subtitles.Count + 1; i++)
    //                {
    //                    var txt = doc.Cells[16, i].Value.TextValue;
    //                    if (!string.IsNullOrEmpty(txt))
    //                    {
    //                        if (sb.Length > 0)
    //                        {
    //                            sb.AppendLine();
    //                        }
    //                        sb.Append(txt);
    //                    }
    //                }
    //                srt = sb.ToString();
    //                ViewCurrentObject.FixSRTPrompt = sb.ToString();
    //            }

    //            #region fix and check
    //            srt = SrtFixer.AutoFixSrtFormat(srt);
    //            var rst = SrtChecker.CheckSrt(srt);
    //            if (!string.IsNullOrEmpty(rst))
    //            {
    //                throw new UserFriendlyException(rst);
    //            }
    //            #endregion

    //            sheet.SpreadsheetControl.Document.BeginUpdate();

    //            var session = (Application.CreateObjectSpace(typeof(SubtitleItem)) as XPObjectSpace).Session;
    //            var fixedTitles = SRTHelper.ParseSrtStringContentToObject(srt, session, false);
    //            //先分
    //            var splits = SplitsBySentence(fixedTitles.ToList());
    //            //合并.
    //            var merges = MergeItems(splits);

    //            int k = 1;
    //            foreach (var item in fixedTitles)
    //            {
    //                doc.Cells[k, 4].Value = item.Lines;
    //                doc.Cells[k, 5].Value = TextCompare.ClearString(item.Lines);
    //                doc.Cells[k, 6].Value = TextCompare.ClearString(doc.Cells[k, 3].Value.TextValue);
    //                k++;
    //            }
    //            k = 1;
    //            var nsubtitles = new List<SubtitleItem>();
    //            foreach (var item in merges)
    //            {
    //                //k = item.Source.First().Index + 1;
    //                doc.Cells[k, 8].Value = item.Content;
    //                nsubtitles.Add(new SubtitleItem(session)
    //                {
    //                    StartTime = item.Source.First().StartTime,
    //                    EndTime = item.Source.Last().EndTime,
    //                    Lines = item.Content,
    //                    Index = k
    //                });
    //                k++;
    //            }

    //            SRTHelper.SaveToSrtFile(nsubtitles, Path.Combine(ViewCurrentObject.ProjectPath, "按句分英文.srt"), SrtLanguage.英文);
    //            sheet.SpreadsheetControl.Document.EndUpdate();



    //        }

    //        FrmChatGPT gpt;
    //        private void ShowChatGPT_Execute(object sender, SimpleActionExecuteEventArgs e)
    //        {
    //            gpt = new FrmChatGPT();
    //            gpt.Show();
    //        }
    //        bool debugPrompt = false;
    //        //(int leftColumn, int rightColumn) LastRange;

    ////        private async void AddSymbol_Execute(object sender, SimpleActionExecuteEventArgs e)
    ////        {
    ////            if (!debugPrompt)
    ////            {
    ////                if (gpt == null)
    ////                {
    ////                    throw new UserFriendlyException("还没有启动GPT对话窗口!");
    ////                }
    ////                if (!gpt.IsStartMonit)
    ////                {
    ////                    await gpt.StartMonit();
    ////                }
    ////            }
    ////            //var text = ViewCurrentObject.TextLines;
    ////            //var sentences = WordProcesser.GetSentences(text);
    ////            //return;
    ////            //将选中的内容发送给AI，然后将返回的内容插入到文档中
    ////            var doc = sheet.SpreadsheetControl.Document.Worksheets.FirstOrDefault();

    ////            //var rng = doc.GetSelectedRanges().First();

    ////            //LastRange = (rng.LeftColumnIndex, rng.RightColumnIndex);

    ////            //var oids = new List<int>();

    ////            //for (int i = ViewCurrentObject.FixSubtitleStartIndex; i < ViewCurrentObject.FixSubtitleStartIndex + ViewCurrentObject.FixSubtitleBatchCount; i++)
    ////            //{
    ////            //    oids.Add((int)doc.Cells[15, i].Value.NumericValue);
    ////            //}


    ////            var lines = ViewCurrentObject.Subtitles.OrderBy(t => t.Index).Skip(ViewCurrentObject.FixSubtitleStartIndex).Take(ViewCurrentObject.FixSubtitleBatchCount);//.Where(x => oids.Contains(x.Oid)).ToList();
    ////            var selection = new StringBuilder();

    ////            foreach (var x in lines)
    ////            {
    ////                //selection.AppendLine(x.Index.ToString());
    ////                //selection.AppendLine($"{x.StartTime.ToString("hh\\:mm\\:ss\\,fff")} --> {x.EndTime.ToString("hh\\:mm\\:ss\\,fff")}");                
    ////                selection.AppendLine($@"{x.Index}
    ////{x.StartTime.ToString("hh\\:mm\\:ss\\,fff")} --> {x.EndTime.ToString("hh\\:mm\\:ss\\,fff")}
    ////{x.Lines}
    ////");
    ////            }
    ////            Debug.WriteLine(selection.ToString());
    ////            if (!debugPrompt)
    ////            {
    ////                gpt.MessageRecived += Gpt_MessageRecived;
    ////                await gpt.Send(ViewCurrentObject.AddSymbolPrompt + ":\n" + selection.ToString(), true);
    ////            }
    ////        }

    //        private void Gpt_MessageRecived(object sender, MessageInfo e)
    //        {
    //            //var doc = sheet.SpreadsheetControl.Document.Worksheets.FirstOrDefault();
    //            //var newMsg = e.History.LastOrDefault();
    //            //if (newMsg != null)
    //            //{
    //            //    //doc.Cells[16, ViewCurrentObject.FixSubtitleStartIndex].Value += newMsg.Text;
    //            //    //保存到文件,备查备用。
    //            //    var file = Path.Combine(ViewCurrentObject.ProjectPath, $"{this.ViewCurrentObject.Oid}_{ViewCurrentObject.FixSubtitleStartIndex}_{ViewCurrentObject.FixSubtitleBatchCount}_{DateTime.Now.ToString("yyMMddHHmmssfff")}.txt");
    //            //    File.WriteAllText(file, newMsg.Text);

    //            //    try
    //            //    {
    //            //        LoadFixedSRT(newMsg.Text);
    //            //        ////先分
    //            //        //var splits = SplitsBySentence(fixedTitles.ToList());
    //            //        ////合并.
    //            //        //var merges = MergeItems(splits);
    //            //    }
    //            //    catch (Exception ex)
    //            //    {
    //            //        Debugger.Break();
    //            //        Debug.WriteLine(ex.Message);
    //            //    }

    //            //}
    //            //ViewCurrentObject.FixSubtitleStartIndex += 30;
    //            //gpt.MessageRecived -= Gpt_MessageRecived;
    //        }

    //        private void LoadFixedSRT(string srtText)
    //        {
    //            //立即解析出结果
    //            #region fix and check

    //            var srt = srtText;
    //            srt = SrtFixer.AutoFixSrtFormat(srt);
    //            var rst = SrtChecker.CheckSrt(srt);
    //            if (!string.IsNullOrEmpty(rst))
    //            {
    //                throw new UserFriendlyException(rst);
    //            }
    //            #endregion
    //            //sheet.SpreadsheetControl.Document.BeginUpdate();
    //            var session = (Application.CreateObjectSpace(typeof(SubtitleItem)) as XPObjectSpace).Session;
    //            var fixedTitles = SRTHelper.ParseSrtStringContentToObject(srt, session, false);
    //            var doc = sheet.SpreadsheetControl.Document.Worksheets.First();
    //            sheet.SpreadsheetControl.Document.BeginUpdate();
    //            foreach (var item in fixedTitles)
    //            {
    //                doc.Cells[item.Index+1, 4].Value = item.Lines;
    //            }
    //            sheet.SpreadsheetControl.Document.EndUpdate();
    //        }

    //        RichTextPropertyEditor editor;
    //        SpreadsheetPropertyEditor sheet;
    //        protected override void OnActivated()
    //        {
    //            base.OnActivated();
    //            //editor = View.GetItems<RichTextPropertyEditor>().First();
    //            //sheet = View.GetItems<SpreadsheetPropertyEditor>().First(t => t.Id == nameof(VideoInfo.SubtitleSheetEditor));
    //        }
    //        protected override void OnViewControlsCreated()
    //        {
    //            base.OnViewControlsCreated();

    //            //editor.RichEditControl.SelectionChanged += richEditControl1_SelectionChanged;
    //            var f = Application.MainWindow.Template as Form;
    //            f.KeyPreview = true;
    //            //editor.RichEditControl.KeyUp += RichEditControl_KeyUp;
    //            //editor.RichEditControl.KeyDown += RichEditControl_KeyDown;
    //        }
    //        private void RichEditControl_KeyDown(object sender, KeyEventArgs e)
    //        {
    //            if (e.KeyCode == Keys.F12)
    //            {
    //                var richEditControl1 = sender as RichEditControl;
    //                var doc = richEditControl1.Document;
    //                var all = doc.BeginUpdateCharacters(doc.Range);
    //                all.BackColor = Color.White;
    //                doc.EndUpdateCharacters(all);
    //                var txt = doc.GetText(doc.Selection);
    //                if (!string.IsNullOrEmpty(txt))
    //                {
    //                    DocumentRange[] ranges = richEditControl1.Document.FindAll(txt, SearchOptions.None);

    //                    foreach (var item in ranges)
    //                    {
    //                        CharacterProperties cp = richEditControl1.Document.BeginUpdateCharacters(item);
    //                        cp.BackColor = Color.Yellow;
    //                        richEditControl1.Document.EndUpdateCharacters(cp);
    //                    }
    //                }
    //                e.SuppressKeyPress = true;
    //            }
    //        }

    //        private void RichEditControl_KeyUp(object sender, KeyEventArgs e)
    //        {
    //            if (e.KeyCode == Keys.Control)
    //            {
    //            }
    //        }

    //        private void richEditControl1_SelectionChanged(object sender, EventArgs e)
    //        {

    //        }

    //        #region step.1 load text to spreadsheet editor
    //        private void LoadText_Execute(object sender, SimpleActionExecuteEventArgs e)
    //        {
    //            var doc = sheet.SpreadsheetControl.Document.Worksheets.FirstOrDefault();

    //            sheet.SpreadsheetControl.Document.BeginUpdate();
    //            sheet.SpreadsheetControl.Document.Unit = DevExpress.Office.DocumentUnit.Inch;
    //            doc.Cells[0, 0].Value = "序号";
    //            doc.Cells[0, 1].Value = "开始时间";
    //            doc.Cells[0, 2].Value = "结束时间";
    //            doc.Cells[0, 3].Value = "字幕内容";
    //            doc.Cells[0, 4].Value = "修复字幕";


    //            doc.Cells[0, 5].Value = "清理原版";
    //            doc.Cells[0, 6].Value = "清理修复";



    //            doc.Cells[0, 13].Value = "时长";
    //            doc.Cells[0, 14].Value = "秒数";
    //            doc.Cells[0, 15].Value = "Oid";
    //            doc.Cells[0, 16].Value = "批量加标点后";
    //            //doc.Cells[i, 0].ColumnWidth = time / 1000;
    //            var cntRng = doc.Range.FromLTRB(0, 0, 20, ViewCurrentObject.Subtitles.Count);
    //            cntRng.Borders.SetAllBorders(Color.Black, DevExpress.Spreadsheet.BorderLineStyle.Thin);

    //            int i = 1;
    //            foreach (var x in ViewCurrentObject.Subtitles.OrderBy(t => t.Index))
    //            {
    //                doc.Cells[i, 0].Value = x.Index;
    //                doc.Cells[i, 1].Value = x.StartTime.ToString("hh\\:mm\\:ss\\,fff");
    //                doc.Cells[i, 2].Value = x.EndTime.ToString("hh\\:mm\\:ss\\,fff");
    //                doc.Cells[i, 3].Value = x.Lines;

    //                var time = (x.EndTime - x.StartTime).TotalMilliseconds;



    //                doc.Cells[i, 13].Value = time;
    //                doc.Cells[i, 14].Value = (x.EndTime - x.StartTime).TotalSeconds;
    //                doc.Cells[i, 15].Value = x.Oid;
    //                i++;
    //            }
    //            sheet.SpreadsheetControl.Document.EndUpdate();
    //            //editor.RichEditControl.Document.AppendText(ViewCurrentObject.TextLines);
    //            //var editor = 
    //        }
    //        #endregion
    //    }

}
