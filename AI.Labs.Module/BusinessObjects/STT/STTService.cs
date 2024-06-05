using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Utils;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using System.ComponentModel;
using System.Diagnostics;
using TranscribeCS;

namespace AI.Labs.Module.BusinessObjects.STT
{
    /// <summary>
    /// 系统启动时自动加载
    /// 系统中应该只使用此服务做为接口，后续如果扩展接入其他语别服务时，只在这里扩展
    /// 这里的设计当前过于简单，由于没有其他识别服务的选项，先能跑起来再说吧!
    /// </summary>
    //[NavigationItem("STT")]
    public class STTService : StringKeyBase
    {
        public STTService(Session s) : base(s)
        {

        }

        /// <summary>
        /// 服务当前使用的模型
        /// </summary>
        public STTModel CurrentModel
        {
            get { return GetPropertyValue<STTModel>(nameof(CurrentModel)); }
            set { SetPropertyValue(nameof(CurrentModel), value); }
        }

        [XafDisplayName("自动启动")]
        public bool AutoStart
        {
            get { return GetPropertyValue<bool>(nameof(AutoStart)); }
            set { SetPropertyValue(nameof(AutoStart), value); }
        }


        //[Browsable(false)]
        //不要删，就是为了让模型生成messages的引用。

        /// <summary>
        /// 服务当前状态
        /// </summary>
        [ModelDefault("AllowEdit", "False")]
        public STTServiceState State
        {
            get { return GetPropertyValue<STTServiceState>(nameof(State)); }
            set { SetPropertyValue(nameof(State), value); }
        }

        IWhisperEngine _whisperEngine;

        public void Log(string log, DateTime? time = null)
        {
            var l = new STTServiceLog(Session);
            l.Time = time ?? DateTime.Now;
            l.LogMessage = log;
        }

        public static Func<STTService, IWhisperEngine> CreateWhisperEngine { get; set; }

        public string Start()
        {
            if (State == STTServiceState.Stopped)
            {

                if (_whisperEngine == null)
                {
                    if (CreateWhisperEngine == null)
                    {
                        throw new Exception("需要在主程序中设置如何创建Whisper Engine!");
                    }
                    var sw = Stopwatch.StartNew();
                    _whisperEngine = CreateWhisperEngine(this);
                    _whisperEngine.Setup();
                    State = STTServiceState.Running;
                    sw.Stop();
                    var msg = string.Format(Messages.STTServiceStarted.GetDisplayText(), sw.ElapsedMilliseconds);
                    Log(msg);
                    return msg;
                }
            }
            return string.Empty;

        }

        public string Stop()
        {
            if (State == STTServiceState.Running)
            {

                if (_whisperEngine != null)
                {
                    _whisperEngine.Dispose();
                    _whisperEngine = null;
                    State = STTServiceState.Stopped;
                    var msg = Messages.STTServiceStopped.GetDisplayText();
                    Log(msg);
                    return msg;
                }

            }
            return string.Empty;
        }

        public void Restart()
        {
            Stop();
            Start();
        }

        /// <summary>
        /// 语音识别
        /// </summary>
        /// <param name="waveData">输入的wav文件内容进行识别</param>
        /// <returns>识别出的文字</returns>
        public string SpeechRecognition(byte[] waveData)
        {
            return _whisperEngine.GetTextFromWavData(waveData);

        }
        public event EventHandler<NewSegmentArgs> NewSegment;

#warning 需要处理:实时语音识别时,进行单个识别是否会报错
        /// <summary>
        /// 开始实时语音识别
        /// 效果不太好
        /// </summary>
        public void StartSpeechRecognition()
        {

            _whisperEngine.NewSegments += (s, e) =>
            {
                if (NewSegment != null)
                {
                    this.NewSegment(s, e);
                }
            };
            _whisperEngine.StartRealTimeSpeechRecognition();

        }

        public static STTService Instance { get; internal set; }

        public List<STTServiceLog> Logs
        {
            get
            {
                return Session.Query<STTServiceLog>().ToList();
            }
        }
    }

    public class STTServiceLog : XPObject
    {
        public STTServiceLog(Session s) : base(s)
        {

        }

        [ModelDefault("DisplayFormat", "yyyy-MM-dd HH:mm:ss.fff")]
        public DateTime Time
        {
            get { return GetPropertyValue<DateTime>(nameof(Time)); }
            set { SetPropertyValue(nameof(Time), value); }
        }


        public string LogMessage
        {
            get { return GetPropertyValue<string>(nameof(LogMessage)); }
            set { SetPropertyValue(nameof(LogMessage), value); }
        }

    }

    public enum STTServiceState
    {
        /// <summary>
        /// 已停止
        /// </summary>
        Stopped,
        /// <summary>
        /// 运行中
        /// </summary>
        Running,

    }
}
