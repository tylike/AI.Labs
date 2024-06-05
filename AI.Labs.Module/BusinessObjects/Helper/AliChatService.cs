using AllInAI.Sharp.API.Extensions;
using AllInAI.Sharp.API.Req;
using AllInAI.Sharp.API.Res;
using AllInAI.Sharp.API.Utils;
using System.Text.Json;

namespace AI.Labs.Module.BusinessObjects
{
    public class AliChatService : IChatService
    {
        public async Task<CompletionRes> Completion(HttpClient _httpClient, CompletionReq req, string? accesstoken = null, CancellationToken cancellationToken = default)
        {
            AliCompletionReq aliReq = new AliCompletionReq();
            aliReq.Model = req.Model;
            aliReq.Input.Messages = req.Messages;
            aliReq.Parameters.TopP = req.TopP;
            aliReq.Parameters.MaxTokens = req.MaxTokens;
            string url = "/api/v1/services/aigc/text-generation/generation";
            AliCompletionRes completionRes = await _httpClient.PostAndReadAsAsync<AliCompletionRes>(url, aliReq, cancellationToken);
            CompletionRes res = GetCompletion(completionRes);
            res.Model = req.Model;
            return res;
        }

        public async IAsyncEnumerable<CompletionRes> CompletionStream(HttpClient _httpClient, CompletionReq req, string? accesstoken = null, CancellationToken cancellationToken = default)
        {
            AliCompletionReq aliReq = new AliCompletionReq();
            aliReq.Model = req.Model;
            aliReq.Input.Messages = req.Messages;
            aliReq.Parameters.TopP = req.TopP;
            aliReq.Parameters.MaxTokens = req.MaxTokens;
            aliReq.Parameters.IncrementalOutput = true;
            //aliReq.Parameters.ResultFormat = "text";
            string url = "/api/v1/services/aigc/text-generation/generation";
            using var response = _httpClient.PostAsStreamAsync(url, aliReq, cancellationToken);
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);
            while (!reader.EndOfStream)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var line = await reader.ReadLineAsync();
                // Skip empty lines
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }
                if (line.StartsWith("id:") || line.StartsWith("event:result") || line.StartsWith(":HTTP_STATUS/200"))
                {
                    continue;
                }
                line = line.RemoveIfStartWith("data:");

                CompletionRes res;
                try
                {
                    res = new CompletionRes();
                    // When the response is good, each line is a serializable CompletionCreateRequest
                    var completionRes = JsonSerializer.Deserialize<AliCompletionRes>(line);
                    // Exit the loop if the stream is done
                    // if (completionRes.Output.FinishReason == "stop" || completionRes.Output.Choices[0].FinishReason == "stop") {
                    //     break;
                    // }
                    res = GetCompletion(completionRes);
                    res.Model = req.Model;
                }
                catch (Exception)
                {
                    // When the API returns an error, it does not come back as a block, it returns a single character of text ("{").
                    // In this instance, read through the rest of the response, which should be a complete object to parse.
                    line += await reader.ReadToEndAsync();
                    res = JsonSerializer.Deserialize<CompletionRes>(line);
                }
                if (null != res)
                {
                    yield return res;
                }
            }
        }

        public Task<EmbeddingRes> Embedding(HttpClient _httpClient, EmbeddingReq req, string? accesstoken = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        private CompletionRes GetCompletion(AliCompletionRes completionRes)
        {
            CompletionRes res = new CompletionRes();
            if (completionRes != null)
            {
                res.Error = completionRes.Error;
                if (completionRes.Error != null) { return res; }
                if (string.IsNullOrEmpty(completionRes.Output.Text))
                {
                    res.Choices = completionRes.Output.Choices;
                    if (completionRes.Output.Choices != null)
                        res.Result = completionRes.Output.Choices[0].Message.Content;
                }
                else
                {
                    res.Result = completionRes.Output.Text;
                }
                res.Usage = new UsageRes();
                res.Usage.PromptTokens = completionRes.Usage.InputTokens;
                res.Usage.CompletionTokens = completionRes.Usage.OutputTokens;
            }
            return res;
        }
    }
}
