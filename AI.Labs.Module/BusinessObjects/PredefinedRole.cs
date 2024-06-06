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
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            this.ShortcutImageName = "ModelEditor_GenerateContent";
            this.ShortcutMessageTemplate = "内容:{T}";
        }

        [XafDisplayName("快捷方式")]
        public bool Shortcut
        {
            get { return GetPropertyValue<bool>(nameof(Shortcut)); }
            set { SetPropertyValue(nameof(Shortcut), value); }
        }

        [XafDisplayName("快捷方式标题")]
        public string ShortcutCaption
        {
            get { return GetPropertyValue<string>(nameof(ShortcutCaption)); }
            set { SetPropertyValue(nameof(ShortcutCaption), value); }
        }

        [XafDisplayName("快捷方式帮助")]
        public string ShortcutTooltip
        {
            get { return GetPropertyValue<string>(nameof(ShortcutTooltip)); }
            set { SetPropertyValue(nameof(ShortcutTooltip), value); }
        }

        [XafDisplayName("快捷方式图标")]
        public string ShortcutImageName
        {
            get { return GetPropertyValue<string>(nameof(ShortcutImageName)); }
            set { SetPropertyValue(nameof(ShortcutImageName), value); }
        }

        [XafDisplayName("输出位置")]
        public int OutputTo
        {
            get { return GetPropertyValue<int>(nameof(OutputTo)); }
            set { SetPropertyValue(nameof(OutputTo), value); }
        }


        [XafDisplayName("消息模板")]
        [Size(-1)]
        [ToolTip("例如:“内容:{T}”，程序在执行时在word文档中将{T}替换成用户选中的内容。在excel中，{T}是选中单元格的内容,{F}将被替换成用户选中的公式.")]
        public string ShortcutMessageTemplate
        {
            get { return GetPropertyValue<string>(nameof(ShortcutMessageTemplate)); }
            set { SetPropertyValue(nameof(ShortcutMessageTemplate), value); }
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
            Voice.Read(WelcomeText);
#warning 需要加入缓存文件的功能
            //TTSEngine.PlayTTSWithCache(WelcomeText, cacheFileName, Voice?.DisplayName);
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

        //[XafDisplayName("系统提示")]
        //public string Prompts
        //{
        //    get { return GetPropertyValue<string>(nameof(Prompts)); }
        //    set { SetPropertyValue(nameof(Prompts), value); }
        //}
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
