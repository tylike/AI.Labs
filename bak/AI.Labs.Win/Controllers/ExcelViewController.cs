using AI.Labs.Module.BusinessObjects;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Office.Win;

namespace AI.Labs.Win.Controllers
{
    public class ExcelViewController : CustomizeDialogViewController<DetailView, SpreadSheetDocument>
    {
        public ExcelViewController()
        {
            //var functionHelp = new SimpleAction(this, "FunctionHelp", null);
            //functionHelp.Execute += FunctionHelp_Execute;

            //var writeFunction = new SimpleAction(this, "WriteFunction", null);
            //writeFunction.Execute += WriteFunction_Execute;
        }

        protected async override void RuntimeAction_Executed(object sender, ActionBaseEventArgs e)
        {
            var action = (SimpleAction)sender;
            var role = (PredefinedRole)(action.Tag);
            var selectedCellFormula = editor.SpreadsheetControl.SelectedCell.Formula;
            var selectedCellText = editor.SpreadsheetControl.SelectedCell.Value.TextValue;
            
            var userPrompt = "";
            //如果有消息模板,则使用消息模板,否则可能在role中有自定义的指令,说明需要消息模板
            if (!string.IsNullOrEmpty(role.ShortcutMessageTemplate))
            {
                if (role.ShortcutMessageTemplate.Contains("{F}"))
                {
                    if (!string.IsNullOrEmpty(selectedCellFormula))
                    {
                        userPrompt = role.ShortcutMessageTemplate.Replace("{F}", selectedCellFormula);
                    }
                    else
                    {
                        throw new UserFriendlyException($"您定义的消息模板是:[{role.ShortcutMessageTemplate}],其中包含了{{F}},含义是要使用公式,但并没有输入公式!");
                    }
                }

                if (role.ShortcutMessageTemplate.Contains("{T}"))
                {
                    userPrompt = role.ShortcutMessageTemplate.Replace("{T}", selectedCellText);
                }
            }

            if (role.OutputTo == 0)
            {
                await AIHelper.Ask(role,userPrompt, ViewCurrentObject.AIModel, t =>
                {
                    var x = (t.Content + "").Trim();
                    try
                    {
                        editor.SpreadsheetControl.SelectedCell.Formula = x;
                    }
                    catch
                    {
                        editor.SpreadsheetControl.SelectedCell.SetValueFromText(x);
                    }
                }, streamOut: false);
                Application.ShowViewStrategy.ShowMessage(action.Caption +":执行完成了!输出结果在您选中的单元格。");
            }
            else if (role.OutputTo == 1)
            {
                var word = View.GetItems<RichTextPropertyEditor>().First();
                var doc = word.RichEditControl.Document;
                doc.AppendText("\n------------------------------------------------------------------------------\n");
                
                await AIHelper.Ask(role,userPrompt, ViewCurrentObject.AIModel,
                t =>
                {
                    doc.AppendText(t.Content);
                }, streamOut: true);

                doc.AppendText("\n------------------------------------------------------------------------------");

                Application.ShowViewStrategy.ShowMessage(action.Caption + ":执行完成了!输出结果在信息窗口中。");
            }
            else
            {
                Application.ShowViewStrategy.ShowMessage("输出位置设置错误,目前只支持0:到sheet中的目标单元格,1:到excel界面的文字输出窗口",InformationType.Error);
            }           

            //await AskAI((SimpleAction)sender, txt, ViewCurrentObject.AIModel, t =>
            //{
            //    pos = doc.InsertText(pos, t.Content).End;
            //});
            //doc.InsertText(pos, "\n------------------------------------------------------------------------------");

        }

        //private async void WriteFunction_Execute(object sender, SimpleActionExecuteEventArgs e)
        //{

        //}

        SpreadsheetPropertyEditor editor;
        protected override void OnActivated()
        {
            base.OnActivated();
            editor = View.GetItems<SpreadsheetPropertyEditor>().First();
            editor.ControlCreated += Editor_ControlCreated;
            
        }

        private void Editor_ControlCreated(object sender, EventArgs e)
        {
            editor.SpreadsheetControl.Options.Behavior.FunctionNameCulture = DevExpress.XtraSpreadsheet.FunctionNameCulture.English;
        }

        //private async void FunctionHelp_Execute(object sender, SimpleActionExecuteEventArgs e)
        //{
        //    var f = editor.SpreadsheetControl.SelectedCell.Formula;
        //    await AskAI("#你的任务是解释excel中的公式.使用中文对话。", $"#请解释这个公式的作用:{f}");
        //}


    }
}
