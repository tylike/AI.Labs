using AI.Labs.Module.BusinessObjects.TTS;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;

namespace AI.Labs.Module.BusinessObjects.AudioBooks
{
    [XafDisplayName("朗读角色")]
    public class AudioBookRole : XPObject
    {
        public AudioBookRole(Session s) : base(s)
        {

        }
        [XafDisplayName("角色名称"),ToolTip("为了区分不同角色而给角色取的一个名字")]
        public string Name
        {
            get { return GetPropertyValue<string>(nameof(Name)); }
            set { SetPropertyValue(nameof(Name), value); }
        }
        [XafDisplayName("朗读声音")]
        public VoiceSolution VoiceSolution
        {
            get { return GetPropertyValue<VoiceSolution>(nameof(VoiceSolution)); }
            set { SetPropertyValue(nameof(VoiceSolution), value); }
        }
        [XafDisplayName("所属书籍")]
        [Association]
        public AudioBook AudioBook
        {
            get { return GetPropertyValue<AudioBook>(nameof(AudioBook)); }
            set { SetPropertyValue(nameof(AudioBook), value); }
        }

        [XafDisplayName("朗读段落"),ToolTip("此角色朗读了这些段落")]
        [Association]
        public XPCollection<AudioBookTextAudioItem> SpreakItems
        {
            get => GetCollection<AudioBookTextAudioItem>(nameof(SpreakItems));
        }

        [XafDisplayName("朗读数量")]
        public int SpreakItemsCount { get => SpreakItems.Count; }

        [XafDisplayName("试听内容")]
        public string TryReadingText
        {
            get { return GetPropertyValue<string>(nameof(TryReadingText)); }
            set { SetPropertyValue(nameof(TryReadingText), value); }
        }

        [Action(Caption ="试听")]
        public async void TryReading()
        {
            if (this.VoiceSolution != null)
                await TTSEngine.ReadText(TryReadingText, this.VoiceSolution.DisplayName);
        }

    }
}
