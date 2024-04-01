
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp;
using System.Diagnostics;

using TranscribeCS;

namespace AI.Labs.Module.BusinessObjects.STT
{
    public class RealTimeSpeechRecognitionViewController : ObjectViewController<ObjectView, ICanRealTimeSpeechRecognition>
    {
        public RealTimeSpeechRecognitionViewController()
        {
            var startRealTimeSpeechRecognition = new SimpleAction(this, "StartRealTimeSpeechRecognition", null);
            startRealTimeSpeechRecognition.Execute += StartRealTimeSpeechRecognition_Execute;
            
            var action = new SimpleAction(this, "Start AzureMicrophone", null);
            action.Execute += StartAzureMicrophone_Execute;
        }

#warning 由于记用这个代码模型编辑器会出错，待处理
        //AzureASR asr = new AzureASR();

        private async void StartAzureMicrophone_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            //this.asr.SpeechRecognizer.Recognized += (s, e) =>
            //{
            //    Application.UIThreadInvoke(new Action(() =>
            //    {
            //        TimeSpan ts = TimeSpan.Zero;
            //        if (!string.IsNullOrEmpty(e.Result.Text))
            //        {
            //            ViewCurrentObject.AddSegment(TimeSpan.Zero, e.Result.Duration, e.Result.Text);
            //        }
            //    }));
            //};
            //await asr.Start();

        }

        private void StartRealTimeSpeechRecognition_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            STTService.Instance.NewSegment += Instance_NewSegment;
            STTService.Instance.StartSpeechRecognition();

        }
        string lastText;
        private void Instance_NewSegment(object sender, NewSegmentArgs e)
        {
            var msg = string.Join("\n+", e.Segments.Select(t => t.Text));

            Application.UIThreadInvoke(new Action(() =>
            {
                int s0 = e.Segments.Count - e.NewCount;
                for (int i = s0; i < e.Segments.Count; i++)
                {
                    var item = e.Segments[i];
                    Debug.WriteLine(item.Text);
                    if (item.Text != lastText)
                    {
                        ViewCurrentObject.AddSegment(item.StartTimeSpan, item.EndTimeSpan, "Segments.Count:" + e.Segments.Count + "/NewCount:" + e.NewCount + "    ," + item.Text);
                    }
                    lastText = item.Text;
                }
            }));

            //foreach (var item in e.Segments)
            //{
            //    Debug.WriteLine(item.text);
            //}
        }
    }
}
