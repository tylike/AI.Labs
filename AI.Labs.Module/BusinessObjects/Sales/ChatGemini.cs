using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Security.Policy;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;
using OpenAI.Extensions;
using System.Runtime.CompilerServices;
using System.Net.Http.Json;
namespace AI.Labs.Module.BusinessObjects.Sales
{


    #region output
    public class SafetyRating
    {
        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("probability")]
        public string Probability { get; set; }
    }

    public class ContentPart
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }

    public class Content
    {
        [JsonPropertyName("parts")]
        public List<ContentPart> Parts { get; set; } = new List<ContentPart>();

        [JsonPropertyName("role")]
        public string Role { get; set; }
    }

    public class Candidate
    {
        [JsonPropertyName("content")]
        public Content Content { get; set; }

        [JsonPropertyName("finishReason")]
        public string FinishReason { get; set; }

        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("safetyRatings")]
        public List<SafetyRating> SafetyRatings { get; set; }
    }

    public class PromptFeedback
    {
        [JsonPropertyName("safetyRatings")]
        public List<SafetyRating> SafetyRatings { get; set; }
    }

    public class GeminiChatResponse
    {
        [JsonPropertyName("candidates")]
        public List<Candidate> Candidates { get; set; }

        [JsonPropertyName("promptFeedback")]
        public PromptFeedback PromptFeedback { get; set; }

        [JsonPropertyName("error")]
        public Error Error { get; set; }
    }

    public class Error
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }
    }

    //"error": {
    //"code": 400,
    //"message": "User location is not supported for the API use.",
    //"status": "FAILED_PRECONDITION"
    //}

#endregion

#region input
public class GenerationConfig
    {
        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        [JsonPropertyName("topK")]
        public int TopK { get; set; }

        [JsonPropertyName("topP")]
        public int TopP { get; set; }

        [JsonPropertyName("maxOutputTokens")]
        public int MaxOutputTokens { get; set; }

        [JsonPropertyName("stopSequences")]
        public List<string> StopSequences { get; set; }
    }
    public class SafetySetting
    {
        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("threshold")]
        public string Threshold { get; set; }
    }

    public class GeminiChatRequest
    {
        [JsonPropertyName("contents")]
        public List<Content> Contents { get; set; } = new List<Content>();

        [JsonPropertyName("generationConfig")]
        public GenerationConfig GenerationConfig { get; set; }

        [JsonPropertyName("safetySettings")]
        public List<SafetySetting> SafetySettings { get; set; }
    }
    #endregion


    public class ChatGemini
    {
        /// <inheritdoc />
        public static async IAsyncEnumerable<string> CreateCompletionAsStream(GeminiChatRequest chatCompletionCreateRequest, string? modelId = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var _httpClient = new HttpClient();
            var uri = "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:streamGenerateContent?key=AIzaSyC_iV94Pg-1LvU1T2Dwxumh2BAogtv4gfc";

            // Helper data in case we need to reassemble a multi-packet response
            //ReassemblyContext ctx = new();

            // Mark the request as streaming
            //chatCompletionCreateRequest.Stream = true;

            // Send the request to the CompletionCreate endpoint
            //chatCompletionCreateRequest.ProcessModelId(modelId, _defaultModelId);

            using var response = _httpClient.PostAsStreamAsync(uri, chatCompletionCreateRequest, cancellationToken);
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);

            // Continuously read the stream until the end of it
            while (!reader.EndOfStream)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var line = await reader.ReadLineAsync();
                // Skip empty lines
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }
                yield return line;

                line = line.RemoveIfStartWith("data: ");

                // Exit the loop if the stream is done
                if (line.StartsWith("[DONE]"))
                {
                    break;
                }
                //ChatCompletionCreateResponse? block;
                //try
                //{
                //    // When the response is good, each line is a serializable CompletionCreateRequest
                //    block = JsonSerializer.Deserialize<ChatCompletionCreateResponse>(line);
                //}
                //catch (Exception)
                //{
                //    // When the API returns an error, it does not come back as a block, it returns a single character of text ("{").
                //    // In this instance, read through the rest of the response, which should be a complete object to parse.
                //    line += await reader.ReadToEndAsync();
                //    block = JsonSerializer.Deserialize<ChatCompletionCreateResponse>(line);
                //}


                //if (null != block)
                //{
                //    ctx.Process(block);

                //    if (!ctx.IsFnAssemblyActive)
                //    {
                //        yield return block;
                //    }
                //}
            }
            yield return null;
        }

        public static async Task<GeminiChatResponse> Send(GeminiChatRequest request)
        {

            //%% bash
            //curl https://generativelanguage.googleapis.com/v1beta/models/gemini-pro-vision:generateContent?key=$API_KEY \
            //-H 'Content-Type: application/json' \
            //-X POST \
            //-d '{
            //"contents": [{
            //    "parts":[{
            //        "text": "Write a story about a magic backpack."}]}]}' 2> /dev/null

            var client = new HttpClient();
            var uri = "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key=AIzaSyC_iV94Pg-1LvU1T2Dwxumh2BAogtv4gfc";
            //var request = new HttpRequestMessage
            //{
            //    Method = HttpMethod.Post,
            //    RequestUri = new Uri(),
            //    Content = new StringContent("{\"contents\": [{\"parts\":[{\"text\": \"" + userPrompty + "\"}]}]}")
            //    {
            //        Headers =
            //        {
            //            ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json")
            //        }
            //    }
            //};

            //var request = new GeminiChatRequest();
            //var response = await client.PostAsJsonAsync(uri,request);
            //var responseBody = await response.Content.ReadAsStringAsync();
            //return JsonSerializer.Deserialize<GeminiChatResponse>(responseBody);
            return await client.PostAndReadAsAsync<GeminiChatResponse>(uri, request);

            //Console.WriteLine(responseBody);
            
        }
    }

}
