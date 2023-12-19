using AI.Labs.Module.BusinessObjects.ChatInfo;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.SystemModule;

namespace AI.Labs.Module.BusinessObjects
{
    public class AskAIViewController : ObjectViewController<ObjectView, IAIDialog>
    {
        public AskAIViewController()
        {
            var callBusinessManager = new SimpleAction(this, "呼叫助理", null);
            callBusinessManager.ImageName = "NewComment";
            callBusinessManager.Caption = "呼叫助理(对话框)";
            callBusinessManager.Execute += CallBusinessManager_Execute;

            var callBusinessManagerMW = new SimpleAction(this, "呼叫助理主窗", null);
            callBusinessManagerMW.ImageName = "NewComment";
            callBusinessManagerMW.Execute += CallBusinessManagerMW_Execute;

            var createBusinessManager = new SimpleAction(this, "招聘助理", null);
            createBusinessManager.ImageName = "BO_Person";
            createBusinessManager.Execute += CreateBusinessManager_Execute;
        }

        private void CallBusinessManagerMW_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            CallBusinessManager(e, TargetWindow.Default, TemplateContext.View);

        }

        private void CreateBusinessManager_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var os = Application.CreateObjectSpace(typeof(PredefinedRole));
            var role = os.CreateObject<PredefinedRole>();
            role.Business = this.View.ObjectTypeInfo.Type.FullName;
            e.ShowViewParameters.CreatedView = Application.CreateDetailView(os, role, true);
            e.ShowViewParameters.TargetWindow = TargetWindow.NewWindow;
        }

        private void CallBusinessManager_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            CallBusinessManager(e, TargetWindow.NewModalWindow, TemplateContext.PopupWindow);
        }

        private void CallBusinessManager(SimpleActionExecuteEventArgs e,TargetWindow targetWindow,TemplateContext templateContext)
        {
            var os = Application.CreateObjectSpace(typeof(Chat));
            var role = os.GetObjectsQuery<PredefinedRole>()
                .FirstOrDefault(x => x.Business == this.View.ObjectTypeInfo.Type.FullName);

            if (role == null)
            {
                Application.ShowViewStrategy.ShowMessage("没有找到业务助理，请先招聘助理!");
                return;
            }

            var chat = os.GetObjectsQuery<Chat>().FirstOrDefault(t => t.Role.Oid == role.Oid);
            if (chat == null)
            {
                chat = os.CreateObject<Chat>();
            }

            //chat.Role = role;
            chat.SourceView = this.View;
#warning 如果不是从这里打开的chat视图时，则初始化数据不会被调用，这里需要处理
            chat.InitializeDataAction = (history, os) =>
            {
                if (typeof(IAIDialog).IsAssignableFrom(View.ObjectTypeInfo.Type))
                {
                    var method = View.ObjectTypeInfo.Type.GetMethod("InitializeContextData");
                    method.Invoke(null, new object[] { history, os, chat, this });
                }
            };
            chat.ProcessResponseAction = (item) =>
            {
                if (typeof(IAIDialog).IsAssignableFrom(View.ObjectTypeInfo.Type))
                {
                    var method = View.ObjectTypeInfo.Type.GetMethod("ChatResponse");
                    if (method != null)
                    {
                        method.Invoke(null, new object[] { chat, item });
                    }
                }
            };

            e.ShowViewParameters.CreatedView = Application.CreateDetailView(os, chat, true);
            e.ShowViewParameters.TargetWindow = targetWindow;
            e.ShowViewParameters.Context = templateContext;
            var dc = Application.CreateController<DialogController>();
            dc.SaveOnAccept = false;
            e.ShowViewParameters.Controllers.Add(dc);
        }
    }
}
