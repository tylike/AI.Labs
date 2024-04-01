using AI.Labs.Module.BusinessObjects.VideoTranslate;
using IPlugins;
using System.Diagnostics;

namespace RuntimePlugin
{
    public class GenerateVideoScript : IPlugin<VideoInfo>
    {
        void IPlugin<VideoInfo>.Invoke(VideoInfo video)
        {
            Debugger.Break();
            //throw new NotImplementedException();
            video.VideoScript.Output += Environment.NewLine + "插件输出:你好102!";
        }
    }
}
