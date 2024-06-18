using AI.Labs.Module.BusinessObjects;
using AI.Labs.Module.BusinessObjects.AISpreadSheet;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Office.Win;
using DevExpress.XtraGauges.Presets.PresetManager;
using DevExpress.XtraSpreadsheet.Services.Implementation;
using RagServer.Module.BusinessObjects;
using System.Text;
using AI.Labs.Module.Controllers;
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
            for (int i = 0; i < 10; i++)
            {
                var act = new SimpleAction(this, $"{this.GetType().Name}_RuntimeExcelAction{i}", null);
                act.Active["visible"] = false;
                actions.Add(act);
            }
        }

        List<SimpleAction> actions = new List<SimpleAction>();
        ActionBase GetHideAction()
        {
            return actions.FirstOrDefault(t => t.Active["visible"] == false);
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
                await AIHelper.Ask(userPrompt, t =>
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
                },
                streamOut: false,
                aiModel: ViewCurrentObject.AIModel,
                role: role
                );
                Application.ShowViewStrategy.ShowMessage(action.Caption + ":执行完成了!输出结果在您选中的单元格。");
            }
            else if (role.OutputTo == 1)
            {
                var word = View.GetItems<RichTextPropertyEditor>().First();
                var doc = word.RichEditControl.Document;
                doc.AppendText("\n------------------------------------------------------------------------------\n");

                await AIHelper.Ask(userPrompt,
                t =>
                {
                    doc.AppendText(t.Content);
                }, aiModel: ViewCurrentObject.AIModel, role: role, streamOut: true);

                doc.AppendText("\n------------------------------------------------------------------------------");

                Application.ShowViewStrategy.ShowMessage(action.Caption + ":执行完成了!输出结果在信息窗口中。");
            }
            else
            {
                Application.ShowViewStrategy.ShowMessage("输出位置设置错误,目前只支持0:到sheet中的目标单元格,1:到excel界面的文字输出窗口", InformationType.Error);
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
        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            var infos = ObjectSpace.GetObjectsQuery<SpreadsheetAction>().ToArray();

            foreach (var item in infos)
            {
                var act = GetHideAction();
                if (act == null)
                {
                    break;
                }
                act.Caption = item.Caption;
                act.Tag = item;
                act.ToolTip = item.Tooltip;
                act.Executed += RuntimeExcel_Executed;
                act.Shortcut = item.ShortcutKey;
                if (!string.IsNullOrEmpty(item.ImageName))
                    act.ImageName = item.ImageName;
                act.Active["visible"] = true;
            }
        }
        

        private async void RuntimeExcel_Executed(object sender, ActionBaseEventArgs e)
        {
            var ranges = editor.SpreadsheetControl.GetSelectedRanges();
            if (!ranges.All(t => t.RowCount == 1 || t.ColumnCount == 1))
            {
                throw new UserFriendlyException("选中的区域必须是一行或一列!");
            }
            var action = (SimpleAction)sender;
            var actionInfo = (SpreadsheetAction)(action.Tag);

            //只选中一个时认为这是一列
            var isRow = !(ranges.First().ColumnCount == 1);

            var ui = SynchronizationContext.Current;
            foreach (var item in ranges)
            {
                foreach (var c in item.ExistingCells)
                {
                    if (c.Value.IsEmpty)
                        continue;

                    var userPrompt = actionInfo.UserPrompt;
                    userPrompt = userPrompt.Replace("{T}", c.Value.TextValue);
                    var refs = actionInfo.GetReferences();

                    if (refs != null)
                    {
                        userPrompt = userPrompt.Replace("{R}", refs);
                    }


                    var targetCell = isRow ?
                        editor.SpreadsheetControl.ActiveWorksheet[c.RowIndex + actionInfo.OutputOffset, c.ColumnIndex] :
                        editor.SpreadsheetControl.ActiveWorksheet[c.RowIndex, c.ColumnIndex + actionInfo.OutputOffset];
                    targetCell.SetValue("");

                    await AIHelper.Ask(userPrompt, t =>
                    {
                        var x = targetCell.Value.TextValue + (t.Content + "").Trim();
                        try
                        {
                            targetCell.SetValueFromText(x);
                        }
                        catch (Exception ex)
                        {
                            targetCell.SetValueFromText(ex.Message);
                        }
                    },
                    streamOut: true,
                    aiModel: actionInfo.Model,
                    temperature: actionInfo.Temperature,
                    n_ctx: actionInfo.MaxToken,
                    systemPrompt: actionInfo.SystemPrompt,
                    uiContext: ui
                    );

                }
            }

            var selectedCellFormula = editor.SpreadsheetControl.SelectedCell.Formula;
            var selectedCellText = editor.SpreadsheetControl.SelectedCell.Value.TextValue;

            //var userPrompt = "";
            ////如果有消息模板,则使用消息模板,否则可能在role中有自定义的指令,说明需要消息模板
            //if (!string.IsNullOrEmpty(role.ShortcutMessageTemplate))
            //{
            //    if (role.ShortcutMessageTemplate.Contains("{F}"))
            //    {
            //        if (!string.IsNullOrEmpty(selectedCellFormula))
            //        {
            //            userPrompt = role.ShortcutMessageTemplate.Replace("{F}", selectedCellFormula);
            //        }
            //        else
            //        {
            //            throw new UserFriendlyException($"您定义的消息模板是:[{role.ShortcutMessageTemplate}],其中包含了{{F}},含义是要使用公式,但并没有输入公式!");
            //        }
            //    }

            //    if (role.ShortcutMessageTemplate.Contains("{T}"))
            //    {
            //        userPrompt = role.ShortcutMessageTemplate.Replace("{T}", selectedCellText);
            //    }
            //}

            //if (role.OutputTo == 0)
            //{
            //    await AIHelper.Ask(userPrompt, t =>
            //    {
            //        var x = (t.Content + "").Trim();
            //        try
            //        {
            //            editor.SpreadsheetControl.SelectedCell.Formula = x;
            //        }
            //        catch
            //        {
            //            editor.SpreadsheetControl.SelectedCell.SetValueFromText(x);
            //        }
            //    },
            //    streamOut: false,
            //    aiModel: ViewCurrentObject.AIModel,
            //    role: role
            //    );
            //    Application.ShowViewStrategy.ShowMessage(action.Caption + ":执行完成了!输出结果在您选中的单元格。");
            //}
            //else if (role.OutputTo == 1)
            //{
            //    var word = View.GetItems<RichTextPropertyEditor>().First();
            //    var doc = word.RichEditControl.Document;
            //    doc.AppendText("\n------------------------------------------------------------------------------\n");

            //    await AIHelper.Ask(userPrompt,
            //    t =>
            //    {
            //        doc.AppendText(t.Content);
            //    }, aiModel: ViewCurrentObject.AIModel, role: role, streamOut: true);

            //    doc.AppendText("\n------------------------------------------------------------------------------");

            //    Application.ShowViewStrategy.ShowMessage(action.Caption + ":执行完成了!输出结果在信息窗口中。");
            //}
            //else
            //{
            //    Application.ShowViewStrategy.ShowMessage("输出位置设置错误,目前只支持0:到sheet中的目标单元格,1:到excel界面的文字输出窗口", InformationType.Error);
            //}

            //await AskAI((SimpleAction)sender, txt, ViewCurrentObject.AIModel, t =>
            //{
            //    pos = doc.InsertText(pos, t.Content).End;
            //});
            //doc.InsertText(pos, "\n------------------------------------------------------------------------------");
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
