using DevExpress.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp;
using OpenAI.ObjectModels.RequestModels;
using AI.Labs.Module.BusinessObjects.Contexts;
using OpenAI.Utilities.FunctionCalling;

namespace AI.Labs.Module.BusinessObjects
{
    [XafDisplayName("数据库交互")]
    [NavigationItem("应用场景")]
    public class DataBaseCommander : XPObject, IAIDialog<DataBaseCommander>
    {
        public DataBaseCommander(Session s) : base(s)
        {

        }

        [XafDisplayName("标题")]
        public string Title
        {
            get { return GetPropertyValue<string>(nameof(Title)); }
            set { SetPropertyValue(nameof(Title), value); }
        }

        public static void InitializeContextData(ChatCompletionCreateRequest history, IObjectSpace os, Chat chat, ViewController c)
        {
            history.Tools = FunctionCallingHelper.GetFunctionDefinitions<SQLHelper>().Select(t=>ToolDefinition.DefineFunction(t)).ToList();
            if (c.View.CurrentObject is DataBaseCommander dbc && dbc != null)
            {
                //if (dbc.IsChatGLM)
                //{
                //    //history.ReturnFunctionCall = true;
                //    var msg = history.Messages.FirstOrDefault();
                //    if (msg != null)
                //    {
                //        //msg.FunctionCall
                //        msg.Functions = history.Functions;
                //    }
                //}
            }
        }

        public static void ChatResponse(Chat chat, ChatItem item)
        {

        }
    }
}
