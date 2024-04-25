// See https://aka.ms/new-console-template for more information
using DevExpress.ExpressApp.Xpo;
using DevExpress.ExpressApp;
using AI.Labs.Module.BusinessObjects.VideoTranslate;
using RuntimePlugin;
using IPlugins;
using AI.Labs.Module.BusinessObjects;
using System.Drawing;
using System.Diagnostics;

Console.WriteLine("Hello, World!");

//FFmpegHelper.ConvertFPS(@"-i d:\videotest\source.mp4", 50, @"d:\videotest\resetFps.mp4");
//Console.WriteLine(FFmpegHelper.ShowKeyFrames(@"d:\videotest\resetFps.mp4"));
//FFmpegHelper.Mp32Wav(@"-i d:\videoinfo\3\audio\2.mp3", @"d:\videotest\wav.wav");
//FFmpegHelper.NAudioGetClip(@"d:\videotest\wav.wav", 10, 1031, @"d:\videotest\clip1.wav");

//FFmpegHelper.GetClip(@"-i d:\videoinfo\3\audio\2.mp3", 10, 1031, @"d:\videotest\clip1.mp3");
//Console.WriteLine($"期望:1021,wav duration:{FFmpegHelper.GetDuration(@"d:\videotest\clip1.wav")}");
//Console.WriteLine($"期望:1021,mp3 duration:{FFmpegHelper.GetDuration(@"d:\videotest\clip1.mp3")}");
//return;

//笔记本
var dbPath = Path.Combine("D:\\AI.Labs\\AI.Labs.Win\\bin\\Debug\\net7.0-windows8.0", "ai.labs.s3db");//"D:\\dev\\AI.Labs\\AI.Labs.Win\\bin\\Debug\\net7.0-windows\\ai.labs.s3db"
//家里台式机
//dbPath = Path.Combine("D:\\dev\\AI.Labs\\AI.Labs.Win\\bin\\Debug\\net7.0-windows8.0", "ai.labs.s3db");//"D:\\dev\\AI.Labs\\AI.Labs.Win\\bin\\Debug\\net7.0-windows\\ai.labs.s3db"
//FFmpegHelper.TestClips("D:\\VideoInfo\\3\\KlM1UMTEFAE.mp4", 0.2);
//return;

var connectionString = DevExpress.Xpo.DB.SQLiteConnectionProvider.GetConnectionString(dbPath);

XpoTypesInfoHelper.GetXpoTypeInfoSource();
XafTypesInfo.Instance.RegisterEntity(typeof(VideoInfo));
XafTypesInfo.Instance.RegisterEntity(typeof(VideoScriptProject));

XPObjectSpaceProvider osProvider = new XPObjectSpaceProvider(connectionString, null);
IObjectSpace objectSpace = osProvider.CreateObjectSpace();
var vi = objectSpace.GetObjectsQuery<VideoInfo>().First(t => t.Oid == 3);
vi.CnAudioSolution.FixSubtitleTimes();
vi.SaveFixedSRT();

var file = "d:\\videotest\\output.mp4";
//FFmpegHelper.CreateEmptyVideo(file, 10000, color: Color.Black);

var testScript = new SimpleFFmpegScript(objectSpace);
testScript.OutputFileName = file;

var rootVideo = testScript.InputVideo($"D:\\VideoInfo\\3\\KlM1UMTEFAE.mp4");
var sw = Stopwatch.StartNew();
var topClips = 10;
var audios = vi.Audios.OrderBy(t => t.Index).Take(topClips).ToArray();

#region 准备音频
//这里的音频成为了视频的最后标准。
Parallel.ForEach(audios, item =>
{
    Console.WriteLine("预处理音频" + item.Index);
    var p = new AudioParameter { Index = item.Index, FileName = item.OutputFileName, StartTimeMS = (int)item.Subtitle.FixedStartTime.TotalMilliseconds, EndTimeMS = (int)item.Subtitle.FixedEndTime.TotalMilliseconds };
    testScript.ImportAudioClip(p);
});
sw.Stop();
var pmsg = ($"并行，预处理音频耗时:{sw.ElapsedMilliseconds}ms");
Console.WriteLine($"{pmsg}");
testScript.AddSubtitle(@"D:\VideoInfo\3\cnsrt.fix.srt");
//testScript.ImportAudioClip(new AudioParameter { FileName = $"D:\\VideoInfo\\3\\Audio\\1.mp3", StartTimeMS = 1000, EndTimeMS = 2000,Speed = 1.1 });
//testScript.ImportAudioClip(new AudioParameter { FileName = $"D:\\VideoInfo\\3\\Audio\\2.mp3", StartTimeMS = 3000, EndTimeMS = 5000 });
//testScript.ImportAudioClip(new AudioParameter { FileName = $"D:\\VideoInfo\\3\\Audio\\3.mp3", StartTimeMS = 5000, EndTimeMS = 7000 });
//testScript.ImportAudioClip(new AudioParameter { FileName = null, StartTimeMS = 7000, EndTimeMS = 10000 });
FFmpegHelper.PutAudiosToTimeLine(testScript.AudioParameters, file + ".wav");
testScript.InputAudio(file + ".wav");
#endregion

var background = testScript.CreateEmptyVideo(Color.Black, (int)audios.Last().Subtitle.FixedEndTime.TotalMilliseconds);
var output = background;
//
foreach (var item in audios)
{
    var loop = 1;

    if (item.Subtitle.Duration < item.Subtitle.FixedDuration)
    {
        loop = (int)Math.Ceiling(item.Subtitle.FixedDuration / (double)item.Subtitle.Duration);
    }

    var videoClip = rootVideo.Select((int)item.Subtitle.StartTime.TotalMilliseconds, (int)item.Subtitle.EndTime.TotalMilliseconds,loop);
    output = output.PutVideo(videoClip, (int)item.Subtitle.FixedStartTime.TotalMilliseconds, (int)item.Subtitle.FixedEndTime.TotalMilliseconds);
}

//var t1 = testScript.InputVideoCommands[0];//.Select(0, 1000);
//var t2 = testScript.InputVideoCommands[1];//.Select(0, 1200);
//var t3 = testScript.InputVideoCommands[2];//.Select(0, 2000);

//var backgroundAudio = testScript.CreateEmptyAudio(10000);

//var output = background
//    .PutVideo(t1, 1000, 2000)
//    .PutVideo(t2, 1900, 3100, 200, 200)
//    .PutVideo(t3, 3000, 5000, 400, 400);

testScript.DrawCurrentTime();
testScript.DrawText(200, 300, "测试", 24, TimeSpan.FromSeconds(0.1), TimeSpan.FromSeconds(10));
testScript.Export(output);

Console.WriteLine($"时长:{FFmpegHelper.GetDuration(file)}");

return;




var script = objectSpace.GetObjectsQuery<VideoScriptProject>().FirstOrDefault(t => t.Name == "test10");
if (Directory.Exists("D:\\VideoInfo\\3\\Audio_ChangeSpeed"))
    Directory.Delete("D:\\VideoInfo\\3\\Audio_ChangeSpeed", true);
if (Directory.Exists("D:\\VideoInfo\\3\\Video_Delay"))
    Directory.Delete("D:\\VideoInfo\\3\\Video_Delay", true);
if (Directory.Exists("D:\\VideoInfo\\3\\Audio_ChangeSpeed"))
    Directory.Delete("D:\\VideoInfo\\3\\Audio_ChangeSpeed", true);
if (Directory.Exists("D:\\VideoInfo\\3\\Audio_Delay"))
    Directory.Delete("D:\\VideoInfo\\3\\Audio_Delay", true);



if (script == null)
{
    script = objectSpace.CreateObject<VideoScriptProject>();
    script.Name = "test1x";
    script.VideoInfo = vi;
    script.CreateProject(objectSpace);

    script.Export();
    //script.VideoInfo.SaveSRTToFile(AI.Labs.Module.BusinessObjects.Helper.SrtLanguage.中文, "fixed", true);
    //script.VideoInfo.SaveSRTToFile(AI.Labs.Module.BusinessObjects.Helper.SrtLanguage.英文, "fixed", true);

    //script.VideoInfo.SaveSRTToFile(AI.Labs.Module.BusinessObjects.Helper.SrtLanguage.中文, "", false);
    //script.VideoInfo.SaveSRTToFile(AI.Labs.Module.BusinessObjects.Helper.SrtLanguage.英文, "", false);

    objectSpace.CommitChanges();
}


//var modifiedObjects = objectSpace.ModifiedObjects.OfType<SubtitleItem>().ToList();
//script.Export();
//vi.SaveSRTToFile(AI.Labs.Module.BusinessObjects.Helper.SrtLanguage.中文, "fixed", true);
//vi.SaveSRTToFile(AI.Labs.Module.BusinessObjects.Helper.SrtLanguage.英文, "fixed", true);

//modifiedObjects = objectSpace.ModifiedObjects.OfType<SubtitleItem>().ToList();


//Console.WriteLine(vi.VideoURL);

//IPlugin<VideoInfo> engine = new GenerateVideoScript();
//engine.Invoke(vi, null);
//Console.Read();