using AI.Labs.Module.BusinessObjects.ChatInfo;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;

namespace AI.Labs.Module.BusinessObjects.TTS
{
    /// <summary>
    /// 用户手动进行朗读的动作
    /// chat上的chatitems使用chat的设置
    /// </summary>
    public interface IReadText
    {
        public string Message { get; }
        ITTSSettingProvider TTSSettingProvider { get; }
    }

    public interface ITTSSettingProvider
    {
        public VoiceSolution VoiceSolution { get; }
        public bool ReadUseSystem { get; }
    }
    
    //public class ReadMessageViewController:ObjectViewController<ObjectView,IReadText>
    //{
    //    public ReadMessageViewController()
    //    {
    //        var read = new SimpleAction(this, "ReadMessage", null);
    //        read.TargetObjectsCriteria = "Message!=null && Message!=''";
    //        read.SelectionDependencyType = SelectionDependencyType.RequireSingleObject;
    //        read.Execute += Read_Execute;
    //    }

    //    private void Read_Execute(object sender, SimpleActionExecuteEventArgs e)
    //    {
    //        this.ViewCurrentObject.
    //        //TTSEngine.ReadText(this.ViewCurrentObject.Message)
    //        //ChatViewController.ReadText(this.ViewCurrentObject);
    //    }
    //}

}
