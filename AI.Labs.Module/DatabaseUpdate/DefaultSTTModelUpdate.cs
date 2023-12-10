using DevExpress.ExpressApp;
using AI.Labs.Module.BusinessObjects.STT;
using AI.Labs.Module.BusinessObjects;

namespace AI.Labs.Module.DatabaseUpdate;


// 只是创建了记录，实际文件还需要自己去下载或放置到对应的目录d:\ai.stt
// 如果不想放到这个目录，在程序运行起来后去修改路径也是可以的，只是个配置，对应上了就能读。
// Only the record is created, the actual file needs to be downloaded or placed in the corresponding directory (d:\ai.stt).
// If you don't want to put it in this directory, you can modify the path after the program is running
// it's just a configuration，as long as it matches, it can be read.

public class DefaultSTTModelUpdate
{
    IObjectSpace os;

    public DefaultSTTModelUpdate(IObjectSpace os)
    {
        this.os = os;
        this.sttModels = os.GetObjectsQuery<STTModel>().ToList();
        Create();
        CreateService();
    }
    void CreateService()
    {
        var s = os.GetObjectsQuery<STTService>().FirstOrDefault();
        if (s == null)
        {
            s = os.CreateObject<STTService>();
            s.CurrentModel = sttModels.FirstOrDefault(t => t.Name == "Whisper Small V2");
            s.Oid = "Speech To Text Service";
        }
    }
    void Create()
    {
        CreateDefaultSTTModelRecord("Whisper Tiny V2", "d:\\ai.stt\\ggml-tiny.bin", "");
        CreateDefaultSTTModelRecord("Whisper Small V2", "d:\\ai.stt\\ggml-small.bin", "");
        CreateDefaultSTTModelRecord("Whisper Medium V2", "d:\\ai.stt\\ggml-medium.bin", "");
        CreateDefaultSTTModelRecord("Whisper Large V2", "d:\\ai.stt\\ggml-large.bin", "");
    }

    List<STTModel> sttModels;
    void CreateDefaultSTTModelRecord(string name, string filePath, string description)
    {
        if (!sttModels.Any(t => t.Name == name))
        {
            var stt = os.CreateObject<STTModel>();
            stt.Name = name;
            stt.ModelFilePath = filePath;
            stt.Description = description;
            sttModels.Add(stt);
        }
    }
}


public class DefaultLLMModelUpdate
{
    IObjectSpace os;

    public DefaultLLMModelUpdate(IObjectSpace os)
    {
        this.os = os;
        this.llmModels = os.GetObjectsQuery<AIModel>().ToList();
        Create();

    }

    void Create()
    {
        CreateDefaultLLMModelRecord("gpt-3.5-turbo-1106;", "https://api.openai-proxy.org", "sk-f4squMoKg6ljSfw1GSSGb0kgLJLh6aU13UE1fzt3HIYaXDu5@24111", "都需要填写正确");
        CreateDefaultLLMModelRecord("gpt-3.5-turbo", "http://localhost:1234", "sk-f4squMoKg6ljSfw1GSSGb0kgLJLh6aU13UE1fzt3HIYaXDu5@24111", "只有网址有用");
    }

    List<AIModel> llmModels;
    void CreateDefaultLLMModelRecord(string name, string apiUrlBase,string apiKey, string description)
    {
        if (!llmModels.Any(t => t.Name == name))
        {
            var llm = os.CreateObject<AIModel>();
            llm.Name = name;
            llm.ApiUrlBase = apiUrlBase;
            llm.Description = description;
            llm.ApiKey = apiKey;
            llmModels.Add(llm);
        }
    }
}
