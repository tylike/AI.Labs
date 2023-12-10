using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp;
using System.Diagnostics;
using System.Runtime.Versioning;

//using TranscribeCS;

namespace AI.Labs.Module.BusinessObjects.STT
{
    public interface ICanRecord
    {

    }
    public interface ICanRealTimeSpeechRecognition
    {
        void AddSegment(TimeSpan begin,TimeSpan end, string text);
    }

    [SupportedOSPlatform("windows")]
    public class RecordController : ObjectViewController<ObjectView, ICanRecord>
    {
        SimpleAction record;

        public RecordController()
        {
            record = new SimpleAction(this, "RecordAudio", null);
            record.Caption = "Record";
            record.Execute += Record_Execute;
            recorder = new Recorder();
        }

        Recorder recorder;
        private void Record_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            if (recorder.IsStarted)
            {
                record.Caption = "录音";
                recorder.StopRecord();
                //var os = this.ObjectSpace;
                //VoiceContentBase uv;
                //if (View is ListView)
                //{
                //    uv = os.CreateObject<VoiceContentBase>();
                //}
                //else
                //{
                //    uv = ViewCurrentObject;
                //}

                //uv.FileContent = File.ReadAllBytes(recorder.SaveFileName);
                //uv.FileName = recorder.SaveFileName;
                //os.CommitChanges();
            }
            else if (!recorder.IsStarted)
            {
                recorder.SaveFileName = $"d:\\audio\\UserVoice_{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss")}.wav";
                recorder.StartRecord();
                record.Caption = "停止";
            }
        }
    }
    public static class UIHelper
    {
        public static Action<Action> UIInvoke { get; set; }
        public static Action UIDoEvents { get; set; }
        public static void UIThreadInvoke(this XafApplication app, Action action)
        {
            if (UIInvoke != null)
            {
                UIInvoke(action);
            }
        }
        public static void UIThreadDoEvents(this XafApplication app)
        {
            if(UIDoEvents != null)
            {
                UIDoEvents();
            }
        }
    }

    [SupportedOSPlatform("windows")]
    public class UserVoiceViewController : ObjectViewController<ObjectView, VoiceContentBase>
    {
        public UserVoiceViewController()
        {
            var speechRecognition = new SimpleAction(this, "SpeechRecognition", null);
            speechRecognition.Caption = "Speech Recognition";
            speechRecognition.Execute += SpeechRecognition_Execute;

            //var mp32wav = new SimpleAction(this, "Mp3 To Wav", null);
            //mp32wav.Execute += Mp32wav_Execute;

            //recorder = new Recorder();

            //windowsRecorder = new SimpleAction(this, "Windows Recorder 录音", null);

            //var unloadModel = new SimpleAction(this, "Unload Model", null);
            //unloadModel.Execute += UnloadModel_Execute;

            var play = new SimpleAction(this, "PlayWav", null);
            play.Execute += Play_Execute;
            play.Caption = "Play";
        }

        private void Play_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            //TTSEngine.Play(ViewCurrentObject.FileContent, true);
        }

        private void UnloadModel_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            //if (stt != null)
            //{
            //    stt.Dispose();
            //    stt = null;
            //}
        }

        private void Mp32wav_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            //AI.MP32WAV(ViewCurrentObject.FileName);
        }

        //AudioParser AudioParser = new AudioParser();

        //WhisperEngine stt;
        private void SpeechRecognition_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            //if (stt == null)
            //{
            //    stt = new WhisperEngine(ViewCurrentObject.Model.ModelFilePath);
            //    stt.Setup();
            //}

            //var sw = Stopwatch.StartNew();
            //var userVoiceToText = stt.GetTextFromWavData(ViewCurrentObject.FileContent, prompts: ViewCurrentObject.Prompt);
            //sw.Stop();
            ////ViewCurrentObject.DurationMilliSecond = (int)sw.ElapsedMilliseconds;

            ////AudioParser.Start(ViewCurrentObject.FileContent); 
            ////AI.LocalGetTextFromAudio(ViewCurrentObject.FileName);

            //ViewCurrentObject.Text = userVoiceToText;
            //ObjectSpace.CommitChanges();
            //Application.ShowViewStrategy.ShowMessage($"{userVoiceToText}");

        }


    }
}
