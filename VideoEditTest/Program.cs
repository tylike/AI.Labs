// See https://aka.ms/new-console-template for more information
using DevExpress.ExpressApp.Xpo;
using DevExpress.ExpressApp;
using AI.Labs.Module.BusinessObjects.VideoTranslate;
using RuntimePlugin;
using IPlugins;
using AI.Labs.Module.BusinessObjects;

Console.WriteLine("Hello, World!");

XpoTypesInfoHelper.GetXpoTypeInfoSource();
XafTypesInfo.Instance.RegisterEntity(typeof(VideoInfo));
XafTypesInfo.Instance.RegisterEntity(typeof(VideoScriptProject));

//笔记本
var dbPath = Path.Combine("D:\\AI.Labs\\AI.Labs.Win\\bin\\Debug\\net7.0-windows8.0", "ai.labs.s3db");//"D:\\dev\\AI.Labs\\AI.Labs.Win\\bin\\Debug\\net7.0-windows\\ai.labs.s3db"
//家里台式机
//dbPath = Path.Combine("D:\\dev\\AI.Labs\\AI.Labs.Win\\bin\\Debug\\net7.0-windows8.0", "ai.labs.s3db");//"D:\\dev\\AI.Labs\\AI.Labs.Win\\bin\\Debug\\net7.0-windows\\ai.labs.s3db"

var connectionString = DevExpress.Xpo.DB.SQLiteConnectionProvider.GetConnectionString(dbPath);

XPObjectSpaceProvider osProvider = new XPObjectSpaceProvider(connectionString, null);
IObjectSpace objectSpace = osProvider.CreateObjectSpace();


var vi = objectSpace.GetObjectsQuery<VideoInfo>().First(t => t.Oid == 2);

var script = objectSpace.GetObjectsQuery<VideoScriptProject>().FirstOrDefault(t => t.Name == "test1x");

if (script == null)
{
    script = objectSpace.CreateObject<VideoScriptProject>();
    script.Name = "test1";
    script.VideoInfo = vi;
    script.CreateProject(objectSpace);
    
    var modifiedObjects = objectSpace.ModifiedObjects.OfType<SubtitleItem>().ToList();

    script.Export();
    vi.SaveSRTToFile(AI.Labs.Module.BusinessObjects.Helper.SrtLanguage.中文, "fixed", true);
    vi.SaveSRTToFile(AI.Labs.Module.BusinessObjects.Helper.SrtLanguage.英文, "fixed", true);

    modifiedObjects = objectSpace.ModifiedObjects.OfType<SubtitleItem>().ToList();

    objectSpace.CommitChanges();
}

//Console.WriteLine(vi.VideoURL);

//IPlugin<VideoInfo> engine = new GenerateVideoScript();
//engine.Invoke(vi, null);
//Console.Read();