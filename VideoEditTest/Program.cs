// See https://aka.ms/new-console-template for more information
using DevExpress.ExpressApp.Xpo;
using DevExpress.ExpressApp;
using AI.Labs.Module.BusinessObjects.VideoTranslate;
using RuntimePlugin;
using IPlugins;
using AI.Labs.Module.BusinessObjects;
using System.Drawing;
using System.Diagnostics;
using AI.Labs.Module.BusinessObjects.FilterComplexScripts;
using DevExpress.ExpressApp.SystemModule;
using NAudio.Wave;

Console.WriteLine("Start!");

//var mp3 = new AudioFileReader(@"d:\videoinfo\4\audio\1.mp3");
//Console.WriteLine(mp3.TotalTime);

//return;
//笔记本
var dbPath = Path.Combine("D:\\AI.Labs\\AI.Labs.Win\\bin\\Debug\\net7.0-windows8.0", "ai.labs.s3db");//"D:\\dev\\AI.Labs\\AI.Labs.Win\\bin\\Debug\\net7.0-windows\\ai.labs.s3db"
//家里台式机
dbPath = Path.Combine("D:\\dev\\AI.Labs\\AI.Labs.Win\\bin\\Debug\\net7.0-windows8.0", "ai.labs.s3db");//"D:\\dev\\AI.Labs\\AI.Labs.Win\\bin\\Debug\\net7.0-windows\\ai.labs.s3db"

var connectionString = DevExpress.Xpo.DB.SQLiteConnectionProvider.GetConnectionString(dbPath);

XpoTypesInfoHelper.GetXpoTypeInfoSource();
XafTypesInfo.Instance.RegisterEntity(typeof(VideoInfo));
//XafTypesInfo.Instance.RegisterEntity(typeof(VideoScriptProject));

XPObjectSpaceProvider osProvider = new XPObjectSpaceProvider(connectionString, null);
IObjectSpace objectSpace = osProvider.CreateObjectSpace();
var vi = objectSpace.GetObjectsQuery<VideoInfo>().First(t => t.Oid == 13);

VideoInfoViewController.CreateVideoProduct(objectSpace, vi);
return;

//vi.CnAudioSolution.FixSubtitleTimes();
//vi.SaveFixedSRT();

//var file = "d:\\videotest\\output.mp4";

//var testScript = new FilterComplexScript(objectSpace);
//testScript.OutputFileName = file;

//var rootVideo = testScript.InputVideo($"D:\\VideoInfo\\3\\KlM1UMTEFAE.mp4");
//var sw = Stopwatch.StartNew();
//var topClips = 1000;
//var audios = vi.Audios.OrderBy(t => t.Index)
//    .Take(topClips)
//    .ToArray();

//#region 准备音频
////这里的音频成为了视频的最后标准。
//Parallel.ForEach(audios, item =>
//{
//    Console.WriteLine("预处理音频" + item.Index);
//    var p = new AudioParameter { Index = item.Index, FileName = item.OutputFileName, StartTimeMS = (int)item.Subtitle.FixedStartTime.TotalMilliseconds, EndTimeMS = (int)item.Subtitle.FixedEndTime.TotalMilliseconds };
//    testScript.ImportAudioClip(p);
//});

//sw.Stop();
//var pmsg = $"并行，预处理音频耗时:{sw.ElapsedMilliseconds}ms";
//Console.WriteLine($"{pmsg}");

//FFmpegHelper.PutAudiosToTimeLine(testScript.AudioParameters, file + ".wav");

//testScript.InputAudio(file + ".wav");
//#endregion


//foreach (var item in audios)
//{
//    var videoClip = rootVideo.Select(
//        (int)item.Subtitle.StartTime.TotalMilliseconds,
//        (int)item.Subtitle.StartTime.AddMilliseconds((item.Subtitle.FixedEndTime - item.Subtitle.FixedStartTime).TotalMilliseconds).TotalMilliseconds
//        );

//    testScript.VideoProductClips.Add(videoClip);

//    var y1 = 50 * (item.Index % 10);
//    if ((item.Subtitle.FixedEndTime - item.Subtitle.EndTime).TotalMilliseconds >= 1500)
//    {
//        testScript.DrawText(300, y1, $"{item.Index}-原片:{item.Subtitle.StartTime}-{item.Subtitle.EndTime} {y1}", 24, item.Subtitle.StartTime, item.Subtitle.EndTime);
//        testScript.DrawText(640, y1, $"{item.Index}-延长:{item.Subtitle.EndTime}-{item.Subtitle.FixedEndTime}", 24, item.Subtitle.EndTime, item.Subtitle.FixedEndTime);
//    }
//}

////testScript.AddSubtitle(@"D:\VideoInfo\3\cnsrt.fix.srt");
//testScript.DrawCurrentTime();

//testScript.Export();

//Console.WriteLine($"时长:{FFmpegHelper.GetDuration(file)}");



