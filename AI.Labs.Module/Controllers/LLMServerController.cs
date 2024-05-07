using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using AI.Labs.Module.BusinessObjects.KnowledgeBase;

namespace AI.Labs.Module.Controllers
{

    public class LLMServerController : ObjectViewController<ObjectView, BusinessKnowledgeBase>
    {
        public LLMServerController()
        {
            var startLLMServer = new SimpleAction(this, "StartLLMServer", null);
            startLLMServer.Caption = "启动LLM服务";
            startLLMServer.Execute += StartLLMServer_Execute;
            var endLLMServer = new SimpleAction(this, "StopLLMServer", null);
            endLLMServer.Caption = "停止服务";
            endLLMServer.Execute += EndLLMServer_Execute;
        }

        private void EndLLMServer_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            SemanticKernelMemory.Stop();
        }

        private void StartLLMServer_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            SemanticKernelMemory.StartServer();
        }
    }

    //#pragma warning disable SKEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
    //    public sealed class LLamaSharpEmbeddingGeneration : ITextEmbeddingGenerationService, IAIService
    //#pragma warning restore SKEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
    //    {
    //        private LLamaEmbedder _embedder;

    //        private readonly Dictionary<string, string> _attributes = new Dictionary<string, string>();

    //        public IReadOnlyDictionary<string, string> Attributes => _attributes;

    //        IReadOnlyDictionary<string, object> IAIService.Attributes => throw new NotImplementedException();

    //        public LLamaSharpEmbeddingGeneration(LLamaEmbedder embedder)
    //        {
    //            _embedder = embedder;
    //        }

    //        public async Task<IList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(IList<string> data, CancellationToken cancellationToken = default(CancellationToken))
    //        {
    //            return await Task.FromResult(data.Select((string text) => new ReadOnlyMemory<float>(_embedder.GetEmbeddings(text))).ToList());
    //        }

    //        public Task<IList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(IList<string> data, Kernel kernel = null, CancellationToken cancellationToken = default)
    //        {
    //            IList<ReadOnlyMemory<float>> t = data.Select(
    //                    (string text) => new ReadOnlyMemory<float>(
    //                    _embedder.GetEmbeddings(text)
    //                    )
    //                ).ToList();
    //            return Task.FromResult(
    //                t
    //                );
    //        }
    //    }
}


#pragma warning restore SKEXP0028 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
