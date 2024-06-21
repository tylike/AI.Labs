using AI.Labs.Module.Controllers;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Xpo;
using DevExpress.XtraReports.Design.View;
using Microsoft.CodeAnalysis.Differencing;
using System.Diagnostics;

namespace AI.Labs.Module.BusinessObjects.AISpreadSheet
{


    public class AICodeViewController : CustomizeDialogViewController<ObjectView, CodeTask>
    {
        public AICodeViewController()
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

            var createRmmzConvertTask = new SimpleAction(this, "创建任务", null);
            createRmmzConvertTask.ToolTip = "批量的加载所有分割完成的文件,创建任务";
            createRmmzConvertTask.Execute += CreateRmmzConvertTask_Execute;
        }

        private void CreateRmmzConvertTask_Execute(object sender, SimpleActionExecuteEventArgs e)
        {

            const string inputDirectory = @"D:\WalkMan\RPG\RPG.Client\split.js";
            const string outputDirectory = @"d:\RpgMakerSharp";

            string directoryPath = inputDirectory;
            Directory.CreateDirectory(outputDirectory);

            string[] jsFiles = Directory.GetFiles(directoryPath, "*.js");
            foreach (var item in jsFiles)
            {
                var t = ObjectSpace.CreateObject<CodeTask>();
                t.InputFile = item;
                t.OutputFile = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(item) + ".cs");
            }
            ObjectSpace.CommitChanges();
        }

        List<SimpleAction> actions = new List<SimpleAction>();
        SimpleAction GetHideAction()
        {
            return actions.FirstOrDefault(t => t.Active["visible"] == false);
        }


        //private async void WriteFunction_Execute(object sender, SimpleActionExecuteEventArgs e)
        //{

        //}
        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            var infos = ObjectSpace.GetObjectsQuery<AICoderAction>().ToArray();

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

                act.Execute += Act_Execute;
                act.Shortcut = item.ShortcutKey;
                if (!string.IsNullOrEmpty(item.ImageName))
                    act.ImageName = item.ImageName;
                act.Active["visible"] = true;
            }
        }

        private async void Act_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var action = (SimpleAction)sender;
            var actionInfo = (AICoderAction)(action.Tag);

            var ranges = e.SelectedObjects.OfType<CodeTask>();

            var ui = SynchronizationContext.Current;
            foreach (var item in ranges)
            {
                var userPrompt = actionInfo.UserPrompt;
                userPrompt = userPrompt.Replace("{T}", item.InputContent);
                var refs = actionInfo.GetReferences();

                if (refs != null)
                {
                    userPrompt = userPrompt.Replace("{R}", refs);
                }
                item.SubmitTime = DateTime.Now;
                Debug.WriteLine(item.InputFile);
                await AIHelper.Ask(userPrompt, t =>
                {
                    item.OutputContent += t.Content;
                    Debug.Write(t.Content);
                },
                streamOut: true,
                aiModel: actionInfo.Model,
                temperature: actionInfo.Temperature,
                n_ctx: actionInfo.MaxToken,
                systemPrompt: actionInfo.SystemPrompt,
                uiContext: ui
                );
                if (!string.IsNullOrEmpty(item.OutputContent))
                {
                    item.OutputContent = item.OutputContent.Replace("\n", Environment.NewLine);
                }
                item.CompleteTime = DateTime.Now;
            }
        }

    }

}
