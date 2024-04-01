#r "D:\dev\AI.Labs\AI.Labs.Module\bin\Debug\net7.0-windows\AI.Labs.Module.dll"

using AI.Labs.Module.BusinessObjects.VideoTranslate;
using System.Diagnostics;
using System.IO;
using DevExpress.ExpressApp;

Debugger.Break();
VideoInfo video = null; //#

video.VideoScript.Output += "hello, world!";
Debug.WriteLine(video.VideoScript.Output);
