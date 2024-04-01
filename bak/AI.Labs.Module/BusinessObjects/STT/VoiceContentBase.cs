using DevExpress.Xpo;
using DevExpress.ExpressApp.DC;

namespace AI.Labs.Module.BusinessObjects.STT
{

    /// <summary>
    /// 可以进行语音识别的内容基类
    /// </summary>
    public class VoiceContentBase : SimpleXPObject,ICanRecord
    {
        public VoiceContentBase(Session s) : base(s)
        {

        }

        //[XafDisplayName("模型")]
        public STTModel Model
        {
            get { return GetPropertyValue<STTModel>(nameof(Model)); }
            set { SetPropertyValue(nameof(Model), value); }
        }

        /// <summary>
        /// 识别结果翻译为英文
        /// </summary>
        public bool TranslateToEnglish
        {
            get { return GetPropertyValue<bool>(nameof(TranslateToEnglish)); }
            set { SetPropertyValue(nameof(TranslateToEnglish), value); }
        }

        //[Size(-1)]
        //public byte[] FileContent
        //{
        //    get { return GetPropertyValue<byte[]>(nameof(FileContent)); }
        //    set { SetPropertyValue(nameof(FileContent), value); }
        //}

        //[Size(-1)]
        //public string Text
        //{
        //    get { return GetPropertyValue<string>(nameof(Text)); }
        //    set { SetPropertyValue(nameof(Text), value); }
        //}

        //[XafDisplayName("提示")]
        [Size(1000)]
        public string Prompt
        {
            get { return GetPropertyValue<string>(nameof(Prompt)); }
            set { SetPropertyValue(nameof(Prompt), value); }
        }

        ////[XafDisplayName("用时(毫秒)")]
        //public int DurationMilliSecond
        //{
        //    get { return GetPropertyValue<int>(nameof(DurationMilliSecond)); }
        //    set { SetPropertyValue(nameof(DurationMilliSecond), value); }
        //}

        //[Size(-1)]
        //public string FileName
        //{
        //    get { return GetPropertyValue<string>(nameof(FileName)); }
        //    set { SetPropertyValue(nameof(FileName), value); }
        //}
    }
}
