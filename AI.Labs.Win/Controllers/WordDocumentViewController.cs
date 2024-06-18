using AI.Labs.Module.BusinessObjects;
using AI.Labs.Module.BusinessObjects.ChatInfo;
using AI.Labs.Module.Translate;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Office.Win;
using DevExpress.ExpressApp.Templates;
using DevExpress.XtraReports.Design.CodeCompletion;
using DevExpress.XtraRichEdit.API.Native;
using NAudio.CoreAudioApi;
using System.Security.Cryptography;
using System.Windows.Interop;
using AI.Labs.Module.Controllers;

namespace AI.Labs.Win.Controllers
{
    public class CreateAssistantViewController : ObjectViewController<ObjectView, IAssistant>
    {
        public CreateAssistantViewController()
        {
            var create = new SimpleAction(this, "CreateAssistant", null);
            create.Execute += Create_Execute;
        }

        private void Create_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var os = Application.CreateObjectSpace(typeof(PredefinedRole));
            var chat = os.CreateObject<PredefinedRole>();
            chat.Shortcut = true;
            chat.ShortcutCaption = "请输入名称";
            chat.Business = this.View.Model.ModelClass.Name;
            e.ShowViewParameters.CreatedView = Application.CreateDetailView(os, chat);
            e.ShowViewParameters.TargetWindow = TargetWindow.NewModalWindow;
            e.ShowViewParameters.Context = TemplateContext.View;

            //var assistant = ObjectSpace.CreateObject<BusinessKnowledgeBase>();
            //assistant.Title = ViewCurrentObject.Title;
            //assistant.Summary = ViewCurrentObject.Summary;
            //assistant.Keyword = ViewCurrentObject.Keyword;
            //assistant.Response = ViewCurrentObject.Response;
            //assistant.Text = ViewCurrentObject.Text;
            //assistant.Save();
            //ObjectSpace.CommitChanges();
            //ViewCurrentObject.Assistant = assistant;
        }
    }

    public class WordDocumentViewController : CustomizeDialogViewController<DetailView, IWordDocument>
    {
        public WordDocumentViewController()
        {


            //var continueWrite = new SimpleAction(this, "ContinueWrite", null);
            //continueWrite.Execute += ContinueWrite_Execute;

            //var summarize = new SimpleAction(this, "Summarize", null);
            //summarize.Execute += Summarize_Execute;

            //var translateToEnglish = new SimpleAction(this, "TranslateToEnglish", null);
            //translateToEnglish.Execute += TranslateToEnglish_Execute;

            //var translateToJapanese = new SimpleAction(this, "TranslateToJapanese", null);
            //translateToJapanese.Execute += TranslateToJapanese_Execute;

            //var translateToKorean = new SimpleAction(this, "TranslateToKorean", null);
            //translateToKorean.Execute += TranslateToKorean_Execute;

            //var translateToChinese = new SimpleAction(this, "TranslateToChinese", null);
            //translateToChinese.Execute += TranslateToChinese_Execute;

            //var translateToEnglishBaidu = new SimpleAction(this, "TranslateToEnglishBaidu", null);
            //translateToEnglishBaidu.Execute += TranslateToEnglishBaidu_Execute;
        }

        private string GetSelectionText(out Document doc)
        {
            doc = editor.RichEditControl.Document;
            return doc.GetText(doc.Selection);
        }
        private string GetSelectionText()
        {
            var doc = editor.RichEditControl.Document;
            return doc.GetText(doc.Selection);
        }
        protected override async void RuntimeAction_Executed(object sender, ActionBaseEventArgs e)
        {

            Document doc;
            string userSelectedDocumentText = GetSelectionText(out doc);
            var pos = doc.InsertText(doc.Selection.End, "\n------------------------------------------------------------------------------\n").End;
            var txt = "";
            var role = (PredefinedRole)((SimpleAction)sender).Tag;
            if (!string.IsNullOrEmpty(role.ShortcutMessageTemplate))
            {
                if (role.ShortcutMessageTemplate.Contains("{T}"))
                    txt = role.ShortcutMessageTemplate.Replace("{T}", userSelectedDocumentText);
                else
                    txt = role.ShortcutMessageTemplate;
            }

            await AskAI((SimpleAction)sender, txt, ViewCurrentObject.AIModel, t =>
            {
                pos = doc.InsertText(pos, t.Content).End;
                Application.UIThreadDoEvents();
            });

            doc.InsertText(pos, "\n------------------------------------------------------------------------------");
        }


        #region old version
        //private async void TranslateToEnglishBaidu_Execute(object sender, SimpleActionExecuteEventArgs e)
        //{
        //    editor.RichEditControl.Document.AppendText(await MicrosoftTranslate.Main(GetSelectionText(), false));
        //}

        //private async void TranslateToChinese_Execute(object sender, SimpleActionExecuteEventArgs e)
        //{
        //    await TranslateTo("中文");
        //}

        //private async void TranslateToKorean_Execute(object sender, SimpleActionExecuteEventArgs e)
        //{
        //    await TranslateTo("韩语");
        //}

        //private async void TranslateToJapanese_Execute(object sender, SimpleActionExecuteEventArgs e)
        //{
        //    await TranslateTo("日语");
        //}

        //private async void TranslateToEnglish_Execute(object sender, SimpleActionExecuteEventArgs e)
        //{
        //    await TranslateTo("英文");
        //}

        //private async Task TranslateTo(string targetLanguage)
        //{
        //    await AskAI($"#将给出的文字翻译为:{targetLanguage}");
        //}

        //private async Task AskAI(string userPrompt)
        //{
        //    Document doc;
        //    string text = GetSelectionText(out doc);
        //    var pos = doc.InsertText(doc.Selection.End, "\n------------------------------------------------------------------------------\n").End;
        //    var txt = userPrompt + "\n#内容:\n" + text;
        //    await Application.Ask("你是一个实用写作助手.", txt, ViewCurrentObject.AIModel,
        //    t => { },
        //    t =>
        //    {
        //        pos = doc.InsertText(pos, t).End;
        //    }, streamOut: true);

        //    doc.InsertText(pos, "\n------------------------------------------------------------------------------");
        //}



        //private async void Summarize_Execute(object sender, SimpleActionExecuteEventArgs e)
        //{
        //    await AskAI("#简要总结选中文字要表达的内容:");
        //} 
        //private async void ContinueWrite_Execute(object sender, SimpleActionExecuteEventArgs e)
        //{
        //    await AskAI("#使用相同的文风，续写这段话:");
        //}
        #endregion

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


    }
}
