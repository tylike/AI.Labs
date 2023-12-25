using DevExpress.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using NAudio.CoreAudioApi;
using DevExpress.PivotGrid.Criteria;
using DevExpress.Utils.Serializing;
using DevExpress.DashboardCommon.DataProcessing;

namespace AI.Labs.Module.BusinessObjects.TTS
{
    [NonPersistent]
    public abstract class TTSBase : XPObject
    {
        public TTSBase(Session s) : base(s)
        {

        }

        [XafDisplayName("文字")]
        [Size(-1)]
        public string Text
        {
            get { return GetPropertyValue<string>(nameof(Text)); }
            set { SetPropertyValue(nameof(Text), value); }
        }

        [XafDisplayName("音频内容")]
        [Size(-1)]
        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        public byte[] FileContent
        {
            get { return GetPropertyValue<byte[]>(nameof(FileContent)); }
            set { SetPropertyValue(nameof(FileContent), value); }
        }

        [XafDisplayName("状态")]
        [ModelDefault("AllowEdit", "False")]
        public TTSState State
        {
            get { return GetPropertyValue<TTSState>(nameof(State)); }
            set { SetPropertyValue(nameof(State), value); }
        }

        [XafDisplayName("音频生成用时(毫秒)")]
        [ModelDefault("AllowEdit", "False")]
        public int ElapsedMilliseconds
        {
            get { return GetPropertyValue<int>(nameof(ElapsedMilliseconds)); }
            set { SetPropertyValue(nameof(ElapsedMilliseconds), value); }
        }

        [XafDisplayName("声音方案")]
        [ToolTip("是指从文本生成语音时对TTS Engine使用的配置参数的集合")]
        public VoiceSolution Solution
        {
            get { return GetPropertyValue<VoiceSolution>(nameof(Solution)); }
            set { SetPropertyValue(nameof(Solution), value); }
        }
    }

    [NavigationItem("应用场景")]
    [XafDisplayName("文本阅读")]
    public class ReadTextInfo : TTSBase
    {
        public ReadTextInfo(Session s) : base(s)
        {

        }
    }
}
