using DevExpress.Persistent.Base;
using DevExpress.Xpo;

namespace AI.Labs.Module.BusinessObjects.STT
{
    /// <summary>
    /// 用于演示语音识别的内容
    /// 可以先进行录音
    /// 然后进行识别内容
    /// </summary>
    [NavigationItem("STT")]
    public class SpeechRecognition : VoiceContentBase
    {
        public SpeechRecognition(Session s):base(s)
        {
        }
    }
}
