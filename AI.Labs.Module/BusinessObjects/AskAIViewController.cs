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
            callBusinessManager.Execute += CallBusinessManager_Execute;

            var createBusinessManager = new SimpleAction(this, "招聘助理", null);
            createBusinessManager.ImageName = "BO_Person";
            createBusinessManager.Execute += CreateBusinessManager_Execute;
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
            var os = Application.CreateObjectSpace(typeof(Chat));
            var role = os.GetObjectsQuery<PredefinedRole>()
                .FirstOrDefault(x => x.Business == this.View.ObjectTypeInfo.Type.FullName);

            if(role == null)
            {
                Application.ShowViewStrategy.ShowMessage("没有找到业务助理，请先招聘助理!");
                return;
            }

            var chat = os.GetObjectsQuery<Chat>().FirstOrDefault(t=>t.Role.Oid == role.Oid);
            //chat.Role = role;
            chat.SourceView = this.View;
            chat.InitializeDataAction = (history,os) =>
            {
                if (typeof(IAIDialog).IsAssignableFrom(View.ObjectTypeInfo.Type) )
                {
                    var method = View.ObjectTypeInfo.Type.GetMethod("InitializeContextData");
                    method.Invoke(null, new object[] { history,os,chat,this });
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

            e.ShowViewParameters.CreatedView = Application.CreateDetailView(os,chat,true);
            e.ShowViewParameters.TargetWindow = TargetWindow.NewModalWindow;
            e.ShowViewParameters.Context = TemplateContext.PopupWindow;
            var dc = Application.CreateController<DialogController>();
            dc.SaveOnAccept = false;            
            e.ShowViewParameters.Controllers.Add(dc);
        }
    }
}
