// See https://aka.ms/new-console-template for more information
using DevExpress.ExpressApp.Xpo;
using DevExpress.ExpressApp;
using AI.Labs.Module.BusinessObjects.VideoTranslate;
using RuntimePlugin;
using IPlugins;
using AI.Labs.Module.BusinessObjects;
using System.Drawing;

Console.WriteLine("Hello, World!");

//FFmpegHelper.ConvertFPS(@"-i d:\videotest\source.mp4", 50, @"d:\videotest\resetFps.mp4");
//Console.WriteLine(FFmpegHelper.ShowKeyFrames(@"d:\videotest\resetFps.mp4"));
//FFmpegHelper.Mp32Wav(@"-i d:\videoinfo\3\audio\2.mp3", @"d:\videotest\wav.wav");
//FFmpegHelper.NAudioGetClip(@"d:\videotest\wav.wav", 10, 1031, @"d:\videotest\clip1.wav");
//Console.WriteLine(FFmpegHelper.GetDuration(@"d:\videotest\clip1.wav"));

var file = "d:\\videotest\\output.mp4";
FFmpegHelper.CreateEmptyVideo(file, 10000, color: Color.Black);

var testScript = new SimpleFFmpegScript();

for (int i = 1; i < 5; i++)
{
    testScript.InputVideo($"D:\\VideoInfo\\3\\VideoClip\\{i:00000}.mp4");
    testScript.InputAudio($"D:\\VideoInfo\\3\\Audio\\{i}.mp3");
}

var t1 = testScript.InputVideoCommands[0];//.Select(0, 1000);
var t2 = testScript.InputVideoCommands[1];//.Select(0, 1200);
var t3 = testScript.InputVideoCommands[2];//.Select(0, 2000);

var background = testScript.CreateEmptyVideo(Color.Black, 10000);
//var backgroundAudio = testScript.CreateEmptyAudio(10000);

var output = background
    .PutVideo(t1, 1000, 2000)
    .PutVideo(t2, 1900, 3100, 200, 200)
    .PutVideo(t3, 3000, 5000, 400, 400);

//var outputAudio = backgroundAudio
var audioList = new List<SimpleFFmpegCommand>(){
    testScript.InputAudioCommands[0].PutAudio(1000, 0, 2000),
    testScript.InputAudioCommands[1].PutAudio(1900, 0, 1200),
    testScript.InputAudioCommands[2].PutAudio(3000, 0, 2000)
    };

var outputAudio = audioList.AMix();



FFmpegHelper.ExecuteFFmpegCommand(
    inputFiles: testScript.Inputs.Select(t => $"-i {t.FileName}").Join(" "),
    filterComplex: testScript.GetComplexScript(),
    outputFiles: file,
    outputOptions: $"-c:v libx264 -crf 18 -y -map \"{output.OutputLable}\" -map \"{outputAudio.OutputLable}\" -c:a aac "
    );
Console.WriteLine($"时长:{FFmpegHelper.GetDuration(file)}");

return;
XpoTypesInfoHelper.GetXpoTypeInfoSource();
XafTypesInfo.Instance.RegisterEntity(typeof(VideoInfo));
XafTypesInfo.Instance.RegisterEntity(typeof(VideoScriptProject));

//笔记本
var dbPath = Path.Combine("D:\\AI.Labs\\AI.Labs.Win\\bin\\Debug\\net7.0-windows8.0", "ai.labs.s3db");//"D:\\dev\\AI.Labs\\AI.Labs.Win\\bin\\Debug\\net7.0-windows\\ai.labs.s3db"
//家里台式机
//dbPath = Path.Combine("D:\\dev\\AI.Labs\\AI.Labs.Win\\bin\\Debug\\net7.0-windows8.0", "ai.labs.s3db");//"D:\\dev\\AI.Labs\\AI.Labs.Win\\bin\\Debug\\net7.0-windows\\ai.labs.s3db"




//FFmpegHelper.TestClips("D:\\VideoInfo\\3\\KlM1UMTEFAE.mp4", 0.2);
//return;

var connectionString = DevExpress.Xpo.DB.SQLiteConnectionProvider.GetConnectionString(dbPath);

XPObjectSpaceProvider osProvider = new XPObjectSpaceProvider(connectionString, null);
IObjectSpace objectSpace = osProvider.CreateObjectSpace();


var vi = objectSpace.GetObjectsQuery<VideoInfo>().First(t => t.Oid == 3);

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