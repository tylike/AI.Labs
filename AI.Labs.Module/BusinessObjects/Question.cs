using DevExpress.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.DC;
using DevExpress.XtraRichEdit.SpellChecker;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using AI.Labs.Module.BusinessObjects.Contexts;
using DevExpress.ExpressApp.Xpo;

namespace AI.Labs.Module.BusinessObjects
{
    [XafDisplayName("问题")]
    [NavigationItem]
    public class Question : XPObject
    {
        public Question(Session s) : base(s)
        {

        }

        [XafDisplayName("要求")]
        [Size(-1)]
        public string Requirement
        {
            get { return GetPropertyValue<string>(nameof(Requirement)); }
            set { SetPropertyValue(nameof(Requirement), value); }
        }

        [XafDisplayName("问题")]
        [Size(-1)]
        public string Content
        {
            get { return GetPropertyValue<string>(nameof(Content)); }
            set { SetPropertyValue(nameof(Content), value); }
        }
        [XafDisplayName("朗读回复")]
        public bool TTSAnswer
        {
            get { return GetPropertyValue<bool>(nameof(TTSAnswer)); }
            set { SetPropertyValue(nameof(TTSAnswer), value); }
        }
    }

    public class QuestionViewController1 : ObjectViewController<ObjectView, Question>
    {
        public QuestionViewController1()
        {
            var fcsCall = new SimpleAction(this, "计算方法", null);
            fcsCall.Execute += FcsCall_Execute;
        }

        private void FcsCall_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            //SQLHelper.Execute("select * from Book");

            //FCS.Calc();
        }
    }
}
