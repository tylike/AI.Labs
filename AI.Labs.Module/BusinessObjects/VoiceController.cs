using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using System;
using System.Diagnostics;
using System.Linq;
using NAudio.Wave;
using AI.Labs.Module.BusinessObjects.TTS;

namespace PPTMaker.Module.Controllers
{
    public class VoiceController : ObjectViewController<ObjectView, ReadTextInfo>
    {
        public VoiceController()
        {
            var action = new SimpleAction(this, "生成语音", null);
            action.Execute += Action_Execute;

            var play = new SimpleAction(this, "播放", null);
            play.Execute += Play_Execute;
        }
        private void Play_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            foreach (ReadTextInfo item in e.SelectedObjects)
            {
                if (item.FileContent == null || item.FileContent.Length <= 0)
                {
                    item.FileContent = TTSEngine.GetTextToSpeechData((string)item.Text, (string)item.Solution.DisplayName);
                }
                TTSEngine.Play(item.FileContent);
            }
        }

        private void Action_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            if (e.SelectedObjects.Count > 0)
            {
                foreach (ReadTextInfo x in e.SelectedObjects)
                {
                    //x.GetAudioFile();
                    x.State = TTSState.生成中;
                    x.FileContent = TTSEngine.GetTextToSpeechData(x.Text, x.Solution.DisplayName);
                    x.State = TTSState.已生成;
                }
                Application.ShowViewStrategy.ShowMessage($"{e.SelectedObjects.Count}条语音生成完成,可以进行播放了!");
                ObjectSpace.CommitChanges();
            }
            else
            {
                Application.ShowViewStrategy.ShowMessage($"没有选中任何要生成语音的记录!");
            }
        }
    }
}
