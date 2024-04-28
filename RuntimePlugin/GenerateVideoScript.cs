using AI.Labs.Module;
using AI.Labs.Module.BusinessObjects;
using AI.Labs.Module.BusinessObjects.VideoTranslate;
using DevExpress.CodeParser;
using DevExpress.ExpressApp;
using IPlugins;
using sun.security.provider;
using System.Diagnostics;
using System.Text;
using YoutubeExplode.Search;
using static edu.stanford.nlp.io.EncodingPrintWriter;
using System.Linq;
using DevExpress.Charts.Native;
using DevExpress.Pdf;
using YoutubeExplode.Videos;
using javax.swing.text;
using DevExpress.XtraSpreadsheet.Model;
using NAudio.Dmo;
using jdk.nashorn.@internal.ir;
using AI.Labs.Module.BusinessObjects.AudioBooks;

namespace RuntimePlugin;


public class GenerateVideoScript : IPlugin<VideoInfo>, IDisposable
{
    void GetScript()
    {
    }

    VideoInfo video;
    Controller controller;
    void IPlugin<VideoInfo>.Invoke(VideoInfo video, Controller controller)
    {
        //Debugger.Break();
        this.video = video;
        this.controller = controller;

        //Debugger.Break();
        //throw new NotImplementedException();
        Output("插件输出:你好102!" + Environment.NewLine);
    }

    void Output(string msg)
    {
        if (controller != null)
        {
            controller.Application.UIThreadInvoke(() =>
            {
                video.Output(msg);
            });
        }
        else
        {
            Console.WriteLine(msg);
        }
    }

    public void Dispose()
    {
        this.video = null;
        this.controller = null;
    }
}
