using AI.Labs.Module.BusinessObjects;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Office.Win;

namespace AI.Labs.Win.Controllers
{
    public class ExcelViewController : ObjectViewController<DetailView, SpreadSheetDocument>
    {
        public ExcelViewController()
        {
            var functionHelp = new SimpleAction(this, "FunctionHelp", null);
            functionHelp.Execute += FunctionHelp_Execute;
        }
        SpreadsheetPropertyEditor editor;
        protected override void OnActivated()
        {
            base.OnActivated();
            editor = View.GetItems<SpreadsheetPropertyEditor>().First();
        }

        private async void FunctionHelp_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var f = editor.SpreadsheetControl.SelectedCell.Formula;
            var rst = await AIHelper.Ask("#你的任务是解释excel中的公式.", $"#请解释这个公式的作用:{f}", "http://127.0.0.1:8000");
            if (!rst.IsError)
            {
                MessageBox.Show(rst.Message);
            }
        }
    }
    public class WordDocumentViewController : ObjectViewController<DetailView, IWordDocument>
    {
        public WordDocumentViewController()
        {
            var continueWrite = new SimpleAction(this, "ContinueWrite", null);
            continueWrite.Execute += ContinueWrite_Execute;

            var summarize = new SimpleAction(this, "Summarize", null);
            summarize.Execute += Summarize_Execute;

            var translateToEnglish = new SimpleAction(this, "TranslateToEnglish", null);
            translateToEnglish.Execute += TranslateToEnglish_Execute;

            var translateToJapanese = new SimpleAction(this, "TranslateToJapanese", null);
            translateToJapanese.Execute += TranslateToJapanese_Execute;

            var translateToKorean = new SimpleAction(this, "TranslateToKorean", null);
            translateToKorean.Execute += TranslateToKorean_Execute;

        }

        private async void TranslateToKorean_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            await TranslateTo("韩语");
        }

        private async void TranslateToJapanese_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            await TranslateTo("日语");
        }

        private async void TranslateToEnglish_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            await TranslateTo("英文");
        }

        private async Task TranslateTo(string targetLanguage)
        {
            var doc = editor.RichEditControl.Document;
            var text = doc.GetText(doc.Selection);
            var rst = await AIHelper.Ask("#内容:\n" + text, $"#将给出的文字翻译为:{targetLanguage}", "http://127.0.0.1:8000");
            if (!rst.IsError)
            {
                doc.InsertText(doc.Selection.End, "\n---------------------------\n" + rst.Message + "\n---------------------------\n");
            }
        }

        private async void Summarize_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var doc = editor.RichEditControl.Document;
            var text = doc.GetText(doc.Selection);
            var rst = await AIHelper.Ask("#内容:\n" + text, "#简要总结选中文字要表达的内容:", "http://127.0.0.1:8000");
            if (!rst.IsError)
            {
                doc.InsertText(doc.Selection.End, "\n---------------------------\n" + rst.Message + "\n---------------------------\n");
            }
        }

        RichTextPropertyEditor editor;
        protected override void OnActivated()
        {
            base.OnActivated();
            editor = View.GetItems<RichTextPropertyEditor>().First();
            editor.ValueStoring += Editor_ValueStoring;
        }
        protected override void OnDeactivated()
        {
            if (editor != null)
                editor.ValueStoring -= Editor_ValueStoring;
            base.OnDeactivated();
        }

        private void Editor_ValueStoring(object sender, ValueStoringEventArgs e)
        {
            ViewCurrentObject.Content = editor.RichEditControl.Document.Text;
        }

        private async void ContinueWrite_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var doc = editor.RichEditControl.Document;
            var text = doc.GetText(doc.Selection);
            var rst = await AIHelper.Ask("#内容:\n" + text, "#使用相同的文风，续写这段话:", "http://127.0.0.1:8000");
            if (!rst.IsError)
            {
                doc.InsertText(doc.Selection.End, "\n---------------------------\n" + rst.Message + "\n---------------------------\n");
            }
        }
    }
}
