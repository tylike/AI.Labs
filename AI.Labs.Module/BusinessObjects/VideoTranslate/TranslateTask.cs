using DevExpress.ExpressApp.DC;
using DevExpress.Xpo;
//using SubtitlesParser.Classes; // 引入SubtitlesParser的命名空间
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.Editors;
using System.Windows.Forms;
using DevExpress.XtraRichEdit;
using DevExpress.XtraRichEdit.API.Native;
using System.Drawing;
using DiffPlex.DiffBuilder;
using DiffPlex;

//using SubtitlesParser.Classes.Parsers;
namespace AI.Labs.Module.BusinessObjects.VideoTranslate
{
    public class TranslateTask : SimpleXPObject
    {
        public TranslateTask(Session s) : base(s)
        {
        }



        [Association]
        public XPCollection<NSubtitleItem> NSubtitleItems => GetCollection<NSubtitleItem>(nameof(NSubtitleItems));

        //[Association]
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


        public string Title
        {
            get { return GetPropertyValue<string>(nameof(Title)); }
            set { SetPropertyValue(nameof(Title), value); }
        }


        //[Association]
        public XPCollection<SubtitleItem> SubtitleItems => GetCollection<SubtitleItem>(nameof(SubtitleItems));

        [Size(-1)]
        [XafDisplayName("中文")]
        public string Text
        {
            get { return GetPropertyValue<string>(nameof(Text)); }
            set { SetPropertyValue(nameof(Text), value); }
        }

        [Action]
        public void 生成原文()
        {
            Source = string.Join(Environment.NewLine, SubtitleItems.Select(t => t.Lines));
        }

        //[Action]
        //public void 生成外站加标点提示()
        //{
        //    try
        //    {
        //        Clipboard.SetText(this.Video.AddSymbolPrompt + "\n" + this.Source, TextDataFormat.Text);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        [XafDisplayName("标点差异")]
        [ToolTip("原文与带标点符号的版本之间的差异")]
        [EditorAlias(EditorAliases.RichTextPropertyEditor)]
        [Size(-1)]
        public string SourceAndSymbolDiffText
        {
            get { return GetPropertyValue<string>(nameof(SourceAndSymbolDiffText)); }
            set { SetPropertyValue(nameof(SourceAndSymbolDiffText), value); }
        }

        //public void SaveSourceAndSymbolDiff(DevExpress.XtraRichEdit.API.Native.Document doc)
        //{
        //    this.SourceAndSymbolDiff = doc.DocBytes;
        //}

        public DevExpress.XtraRichEdit.API.Native.Document GetSourceAndSymbolDiff()
        {
            var wordProcessor = new RichEditDocumentServer();
            //wordProcessor.DocBytes = SourceAndSymbolDiff;
            return wordProcessor.Document;
            //Document document = wordProcessor.Document;
            //DocumentPosition pos1 = document.CreatePosition(2);
            //document.InsertText(pos1, "The Word Processing Document API is a non-visual .NET library.It allows you to automate frequent word processing tasks.\n");
            //}
        }

        //[Action(Caption ="差异比较",ToolTip ="将原文与带标点符号的版本之间的差异比较")]        
        public void GenerateSourceAndSymbolDiff(Document doc)
        {
            //WordProcesser.
            if (doc == null)
                doc = GetSourceAndSymbolDiff();

            TextCompare.Compare(Source, Result, doc);
            //this.SourceAndSymbolDiff = doc.DocBytes;
            SourceAndSymbolDiffText = doc.RtfText;
            doc.SaveDocument($"d:\\temp\\diff{this.Oid}.docx", DocumentFormat.OpenXml);
        }


        [Action]
        public void 生成外站翻译字幕提示()
        {
            try
            {
                var texts = SubtitleItems.OrderBy(x => x.Index).Select(t => @$"{t.StartTime.ToString(@"hh\:mm\:ss\,fff")} -> {t.StartTime.ToString(@"hh\:mm\:ss\,fff")}
{t.Lines}");
                var text = string.Join("\n\n", texts);

                Clipboard.SetText(this.Video.TranslateTaskPrompt + "\n" + text
                    , TextDataFormat.Text);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        SideBySideDiffBuilder sideBySideDiffBuilder;
        /// <summary>
        /// 为了让builder公用不重复声明new
        /// </summary>
        /// <returns></returns>
        public SideBySideDiffBuilder GetDiffBuilder()
        {
            if (sideBySideDiffBuilder == null)
                sideBySideDiffBuilder = new SideBySideDiffBuilder(new Differ());
            return sideBySideDiffBuilder;
        }

        [XafDisplayName("原文")]
        [Size(-1)]
        [Persistent("SourceText")]
        public string Source
        {
            get
            {
                var t = GetPropertyValue<string>(nameof(Source));

                return t;
            }
            set { SetPropertyValue(nameof(Source), value); }
        }

        [Size(-1)]
        [XafDisplayName("英文")]
        [ToolTip("带标点符号的")]
        public string Result
        {
            get { return GetPropertyValue<string>(nameof(Result)); }
            set { SetPropertyValue(nameof(Result), value); }
        }

        decimal? _标点正确率;
        public decimal 标点正确率
        {
            get
            {
                if (!_标点正确率.HasValue)
                {
                    var sourceWords = (Source + "").Split('.', '!', '?', ' ').Select(t => t.ToLower()).Distinct();
                    var resultWords = (Result + "").Split('.', '!', '?', ' ').Select(t => t.ToLower()).Distinct();
                    var correctWords = sourceWords.Intersect(resultWords);
                    return (decimal)correctWords.Count() / sourceWords.Count();
                }
                return _标点正确率.Value;
            }
        }


    }

}
