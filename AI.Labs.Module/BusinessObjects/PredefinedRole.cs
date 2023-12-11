using DevExpress.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp;
using AI.Labs.Module.BusinessObjects.TTS;
using AI.Labs.Module.BusinessObjects.ChatInfo;

namespace AI.Labs.Module.BusinessObjects
{
    [XafDisplayName("虚拟员工")]
    [NavigationItem("设置")]
    public class PredefinedRole : XPObject
    {
        public PredefinedRole(Session s) : base(s)
        {

        }

        [XafDisplayName("业务范围")]
        public string Business
        {
            get { return GetPropertyValue<string>(nameof(Business)); }
            set { SetPropertyValue(nameof(Business), value); }
        }

        [XafDisplayName("欢迎用语")]
        public string WelcomeText
        {
            get { return GetPropertyValue<string>(nameof(WelcomeText)); }
            set { SetPropertyValue(nameof(WelcomeText), value); }
        }

        [XafDisplayName("语音方案")]
        public VoiceSolution Voice
        {
            get { return GetPropertyValue<VoiceSolution>(nameof(Voice)); }
            set { SetPropertyValue(nameof(Voice), value); }
        }
        [Action(Caption = "试听欢迎", ImageName = "Play")]
        public void PlayWelcome()
        {
            //TTSEngine.
            var cacheFileName = $"welcome_audio_{this.Oid}_{LastUpdateTime:yyyy_MM_dd_HH_mm_ss_fff}.mp3";
            TTSEngine.PlayTTSWithCache(WelcomeText, cacheFileName, Voice?.DisplayName);
        }

        [XafDisplayName("名称")]
        public string Name
        {
            get { return GetPropertyValue<string>(nameof(Name)); }
            set { SetPropertyValue(nameof(Name), value); }
        }

        [XafDisplayName("最后更新时间")]
        public DateTime LastUpdateTime
        {
            get { return GetPropertyValue<DateTime>(nameof(LastUpdateTime)); }
            set { SetPropertyValue(nameof(LastUpdateTime), value); }
        }

        [Association, DevExpress.Xpo.Aggregated]
        [XafDisplayName("预制内容")]
        public XPCollection<Prompt> Prompts
        {
            get
            {
                return GetCollection<Prompt>(nameof(Prompts));
            }
        }
        protected override void OnSaving()
        {
            base.OnSaving();
            LastUpdateTime = DateTime.Now;
        }
    }

    public class PredefinedRoleViewController : ObjectViewController<ObjectView, PredefinedRole>
    {
        public PredefinedRoleViewController()
        {
            //var play = new SimpleAction(this, "PlayPreDefinedRoleWelcomeVoice", null);
            //play.Execute += Play_Execute;
            //play.Caption = "播放欢迎";
        }

        private void Play_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            //TTSEngine.GetTextToSpeechMp3File(ViewCurrentObject.WelcomeText, play: true);
        }
    }
}
