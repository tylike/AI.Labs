//using SubtitlesParser.Classes; 
// 引入SubtitlesParser的命名空间
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
//using SubtitlesParser.Classes.Parsers;

namespace AI.Labs.Module.BusinessObjects.VideoTranslate
{
    public class VideoScriptProjectViewController : ObjectViewController<ObjectView, VideoScriptProject>
    {
        public VideoScriptProjectViewController()
        {
            var createScriptProject = new SimpleAction(this, "创建角本项目", null);
            createScriptProject.Execute += CreateScriptProject_Execute;

            var createAndExport = new SimpleAction(this,"创建和导出",null);
            createAndExport.Execute += CreateAndExport_Execute;
        }

        private void CreateAndExport_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var objectSpace = this.ObjectSpace;

            var vi = objectSpace.GetObjectsQuery<VideoInfo>().First(t => t.Oid == 3);

            var script = objectSpace.GetObjectsQuery<VideoScriptProject>().FirstOrDefault(t => t.Name == "test10");

            if (script == null)
            {
                script = objectSpace.CreateObject<VideoScriptProject>();
                script.Name = "test10";
                script.VideoInfo = vi;
                script.CreateProject(objectSpace);

                script.Export();
                script.VideoInfo.SaveSRTToFile(AI.Labs.Module.BusinessObjects.Helper.SrtLanguage.中文, "fixed", true);
                script.VideoInfo.SaveSRTToFile(AI.Labs.Module.BusinessObjects.Helper.SrtLanguage.英文, "fixed", true);

                script.VideoInfo.SaveSRTToFile(AI.Labs.Module.BusinessObjects.Helper.SrtLanguage.中文, "", false);
                script.VideoInfo.SaveSRTToFile(AI.Labs.Module.BusinessObjects.Helper.SrtLanguage.英文, "", false);

                objectSpace.CommitChanges();
            }
        }

        private void CreateScriptProject_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var objectSpace = this.ObjectSpace;

            var vi = objectSpace.GetObjectsQuery<VideoInfo>().First(t => t.Oid == 3);

            var script = objectSpace.GetObjectsQuery<VideoScriptProject>().FirstOrDefault(t => t.Name == "test1x");

            if (script == null)
            {
                script = objectSpace.CreateObject<VideoScriptProject>();
                script.Name = "test1";
                script.VideoInfo = vi;
                script.CreateProject(objectSpace);
                objectSpace.CommitChanges();
            }
        }
    }
    public class MediaClipViewController:ObjectViewController<ObjectView, MediaClip>
    {
        public MediaClipViewController()
        {
            var changeAudioSpeed = new SimpleAction(this, "ChangeAudioSpeed", null)
            {
                Caption = "1.音频调速"                
            };
            changeAudioSpeed.Execute += SimpleAction_Execute;

            var changeVideoSpeed = new SimpleAction(this, "ChangeVideoSpeed", null)
            {
                Caption = "x.视频调速"
            };
            changeVideoSpeed.Execute += ChangeVideoSpeed_Execute;

            var delayVideo = new SimpleAction(this, "DelayVideo", null)
            {
                Caption = "2.视频延时"
            };

            delayVideo.Execute += DelayVideo_Execute;
            var delayAudio = new SimpleAction(this, "DelayAudio", null)
            {
                Caption = "3.音频延时"
            };
            delayAudio.Execute += DelayAudio_Execute;

            var readAudioFileInfo = new SimpleAction(this, "ReadAudioFileInfo", null)
            {
                Caption = "取音频信息"
            };
            readAudioFileInfo.Execute += ReadAudioFileInfo_Execute;
        }

        private void ReadAudioFileInfo_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            foreach (var item in e.SelectedObjects.OfType<MediaClip>())
            {
                item.AudioClip.FileDuration = FFmpegHelper.GetDuration(item.AudioClip.OutputFile);
                //item.AudioClip.取音频信息();
            }
        }


        private void DelayAudio_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            foreach (var item in e.SelectedObjects.OfType<MediaClip>())
            {
                item.AudioClip.计算延时();
            }
        }

        private void DelayVideo_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            foreach (var item in e.SelectedObjects.OfType<MediaClip>())
            {
                item.VideoClip.计算延时();
            }
        }

        private void ChangeVideoSpeed_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            foreach (var item in e.SelectedObjects.OfType<MediaClip>())
            {
                item.VideoClip.计算调速();
            }
        }

        private void SimpleAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            foreach (var item in e.SelectedObjects.OfType<MediaClip>())
            {
                item.AudioClip.计算调速();
            }
        }
    }
}
