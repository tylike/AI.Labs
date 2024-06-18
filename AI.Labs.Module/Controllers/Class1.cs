using AI.Labs.Module.BusinessObjects;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI.Labs.Module.Controllers
{
    public abstract class CustomizeDialogViewController<TView, TBusinessObject> : ObjectViewController<TView, TBusinessObject>
        where TView : ObjectView
    {
        public CustomizeDialogViewController()
        {
            for (int i = 0; i < 10; i++)
            {
                var act = new SimpleAction(this, $"{this.GetType().Name}_RuntimeAction{i}", null);
                act.Active["visible"] = false;
                actions.Add(act);
            }
        }

        List<ActionBase> actions = new List<ActionBase>();
        ActionBase GetHideAction()
        {
            return actions.FirstOrDefault(t => t.Active["visible"] == false);
        }

        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            var infos = ObjectSpace.GetObjectsQuery<PredefinedRole>().Where(t => t.Business == this.View.Model.ModelClass.Name).ToArray();

            foreach (var item in infos)
            {
                var act = GetHideAction();
                if (act == null)
                {
                    break;
                }
                act.Caption = item.ShortcutCaption;
                act.Tag = item;
                act.ToolTip = item.ShortcutTooltip;
                act.Executed += RuntimeAction_Executed;
                if (!string.IsNullOrEmpty(item.ShortcutImageName))
                    act.ImageName = item.ShortcutImageName;
                act.Active["visible"] = true;
            }
        }
        protected virtual async void RuntimeAction_Executed(object sender, ActionBaseEventArgs e)
        {
            await Task.CompletedTask;
        }
        public static async Task AskAI(SimpleAction action, string userMessage, AIModel aiModel, Action<OpenAI.ObjectModels.RequestModels.ChatMessage> processAction)
        {
            var role = action.Tag as PredefinedRole;
            if (role == null)
            {
                throw new UserFriendlyException("错误,按钮不是一个快捷可执行问AI类型!");
            }
            await AIHelper.Ask(userMessage, processAction, aiModel: aiModel, role: role, streamOut: true);
        }
    }

}
