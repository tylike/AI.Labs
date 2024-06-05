using AllInAI.Sharp.API.Dto;
using AllInAI.Sharp.API.Req;
using AllInAI.Sharp.API.Res;
using AllInAI.Sharp.API.Service;
using DevExpress.XtraSpreadsheet.Model;
using OpenAI.Interfaces;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels.ResponseModels;

namespace AI.Labs.Module.BusinessObjects
{
    public class AliOnlineChatService : IChatCompletionService
    {
        public async Task<ChatCompletionCreateResponse> CreateCompletion(ChatCompletionCreateRequest chatCompletionCreate, string modelId = null, CancellationToken cancellationToken = default)
        {
            AuthOption authOption = new AuthOption() { Key = "sk-***", BaseUrl = "https://api.openai.com", AIType = AllInAI.Sharp.API.Enums.AITypeEnum.Ali };

            ChatService chatService = new ChatService(authOption);
            CompletionReq completionReq = new CompletionReq();
            List<MessageDto> messages = new List<MessageDto>();
            messages.Add(new MessageDto() { Role = "user", Content = "Hello!" });
            completionReq.Model = "gpt-3.5-turbo";
            completionReq.Messages = messages;
            CompletionRes completionRes = await chatService.Completion(completionReq);

        }

        public async IAsyncEnumerable<ChatCompletionCreateResponse> CreateCompletionAsStream(ChatCompletionCreateRequest chatCompletionCreate, string modelId = null, bool justDataMode = true, CancellationToken cancellationToken = default)
        {

            AuthOption authOption = new AuthOption() { Key = "sk-***", BaseUrl = chatCompletionCreate.Model, AIType = AllInAI.Sharp.API.Enums.AITypeEnum.Ali };

            ChatService chatService = new ChatService(authOption);
            CompletionReq completionReq = new CompletionReq();
            List<MessageDto> messages = new List<MessageDto>();

            messages.Add(new MessageDto() { Role = "user", Content = "Hello!" });

            completionReq.Model = chatCompletionCreate.Model;
            completionReq.Messages = messages;
            CompletionRes completionRes = await chatService.Completion(completionReq);
            await foreach (var x in chatService.CompletionStream(completionReq))
            {
                yield return new ChatCompletionCreateResponse() { Choices = x.Choices, Error = x.Error }
            }
        }
    }
}
