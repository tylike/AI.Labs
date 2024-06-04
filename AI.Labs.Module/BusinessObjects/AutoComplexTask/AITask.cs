﻿using AutoGen;
using AutoGen.Core;
using AutoGen.DotnetInteractive;
using AutoGen.LMStudio;
using AutoGen.OpenAI;
using Azure.AI.OpenAI;
using Azure.Core.Pipeline;
using DevExpress.XtraSpreadsheet.Model;
using System.Diagnostics;

namespace AI.Labs.Module.BusinessObjects.AutoComplexTask
{
    public class AgentHelper 
    {
        private readonly GPTAgent innerAgent;
        public static string url = "https://na9yqj2l5a5r.share.zrok.io";
        public static GPTAgent CreateAgent(            
            Uri host = null,
            string modelName = "",
            string api_key = "",
            string name = "helper",
            string systemMessage = "You are a helpful AI assistant",
            float temperature = 0.7f,
            int maxTokens = 1024,
            IEnumerable<FunctionDefinition>? functions = null,
            IDictionary<string, Func<string, Task<string>>>? functionMap = null)
        {
            if (host == null)
                host = new Uri(url);

            var client = ConfigOpenAIClientForLMStudio(host,api_key);
            return new GPTAgent(
                name: name,
                systemMessage: systemMessage,
                openAIClient: client,
                modelName: modelName, // model name doesn't matter for LM Studio
                temperature: temperature,
                maxTokens: maxTokens,
                functions: functions,
                functionMap: functionMap);
        }

        public string Name => innerAgent.Name;

        public Task<IMessage> GenerateReplyAsync(
            IEnumerable<IMessage> messages,
            GenerateReplyOptions? options = null,
            System.Threading.CancellationToken cancellationToken = default)
        {
            return innerAgent.GenerateReplyAsync(messages, options, cancellationToken);
        }

        private static OpenAIClient ConfigOpenAIClientForLMStudio(Uri host,string api_key)
        {
            // create uri from host and port
            
            var handler = new CustomHttpClientHandler(host);
            var httpClient = new HttpClient(handler);
            var option = new OpenAIClientOptions(OpenAIClientOptions.ServiceVersion.V2022_12_01)
            {
                Transport = new HttpClientTransport(httpClient),
                
            };
            return new OpenAIClient(api_key, option);
        }

        private sealed class CustomHttpClientHandler : HttpClientHandler
        {
            private Uri _modelServiceUrl;

            public CustomHttpClientHandler(Uri modelServiceUrl)
            {
                _modelServiceUrl = modelServiceUrl;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                // request.RequestUri = new Uri($"{_modelServiceUrl}{request.RequestUri.PathAndQuery}");
                var uriBuilder = new UriBuilder(_modelServiceUrl);
                uriBuilder.Path = request.RequestUri.PathAndQuery;
                request.RequestUri = uriBuilder.Uri;
                return base.SendAsync(request, cancellationToken);
            }
        }
    }

    internal static class LLMConfiguration
    {
        public static OpenAIConfig GetQWen()
        {
            var openAIKey = "";// Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? throw new Exception("Please set OPENAI_API_KEY environment variable.");
            var modelId = "gpt-3.5-turbo";
            
            return new OpenAIConfig(openAIKey, modelId);
        }

        //public static OpenAIConfig GetOpenAIGPT3_5_Turbo()
        //{
        //    var openAIKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? throw new Exception("Please set OPENAI_API_KEY environment variable.");
        //    var modelId = "gpt-3.5-turbo";
        //    return new OpenAIConfig(openAIKey, modelId);
        //}

        //public static OpenAIConfig GetOpenAIGPT4()
        //{
        //    var openAIKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? throw new Exception("Please set OPENAI_API_KEY environment variable.");
        //    var modelId = "gpt-4";

        //    return new OpenAIConfig(openAIKey, modelId);
        //}

        //public static AzureOpenAIConfig GetAzureOpenAIGPT3_5_Turbo(string deployName = "gpt-35-turbo-16k")
        //{
        //    var azureOpenAIKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? throw new Exception("Please set AZURE_OPENAI_API_KEY environment variable.");
        //    var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new Exception("Please set AZURE_OPENAI_ENDPOINT environment variable.");

        //    return new AzureOpenAIConfig(endpoint, deployName, azureOpenAIKey);
        //}

        //public static AzureOpenAIConfig GetAzureOpenAIGPT4(string deployName = "gpt-4")
        //{
        //    var azureOpenAIKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? throw new Exception("Please set AZURE_OPENAI_API_KEY environment variable.");
        //    var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new Exception("Please set AZURE_OPENAI_ENDPOINT environment variable.");

        //    return new AzureOpenAIConfig(endpoint, deployName, azureOpenAIKey);
        //}
    }
    public class AITask
    {
        public static async Task RunAsync()
        {
            //var instance = new Example04_Dynamic_GroupChat_Coding_Task();

            // setup dotnet interactive
            var workDir = Path.Combine(Path.GetTempPath(), "InteractiveService");
            if (!Directory.Exists(workDir))
                Directory.CreateDirectory(workDir);

            using var service = new InteractiveService(workDir);
            var dotnetInteractiveFunctions = new DotnetInteractiveFunction(service);

            var result = Path.Combine(workDir, "result.txt");
            if (File.Exists(result))
                File.Delete(result);


            await service.StartAsync(workDir, default);
            //var llm = new LLMConfig()
            //var gptConfig = LLMConfiguration.GetAzureOpenAIGPT3_5_Turbo();

            var helperAgent = AgentHelper.CreateAgent(                
                name: "helper",
                systemMessage: "You are a helpful AI assistant",
                temperature: 0f                
                );

            var groupAdmin = AgentHelper.CreateAgent(
                name: "groupAdmin",
                systemMessage: "You are the admin of the group chat",
                temperature: 0f
                )
                .RegisterPrintMessage();

            var userProxy = new UserProxyAgent(name: "user", defaultReply: GroupChatExtension.TERMINATE, humanInputMode: HumanInputMode.NEVER)
                .RegisterPrintMessage();

            // Create admin agent
            var admin = new AssistantAgent(
                name: "admin",
                systemMessage: """
            You are a manager who takes coding problem from user and resolve problem by splitting them into small tasks and assign each task to the most appropriate agent.
            Here's available agents who you can assign task to:
            - coder: write dotnet code to resolve task
            - runner: run dotnet code from coder

            The workflow is as follows:
            - You take the coding problem from user
            - You break the problem into small tasks. For each tasks you first ask coder to write code to resolve the task. Once the code is written, you ask runner to run the code.
            - Once a small task is resolved, you summarize the completed steps and create the next step.
            - You repeat the above steps until the coding problem is resolved.

            You can use the following json format to assign task to agents:
            ```task
            {
                "to": "{agent_name}",
                "task": "{a short description of the task}",
                "context": "{previous context from scratchpad}"
            }
            ```

            If you need to ask user for extra information, you can use the following format:
            ```ask
            {
                "question": "{question}"
            }
            ```

            Once the coding problem is resolved, summarize each steps and results and send the summary to the user using the following format:
            ```summary
            {
                "problem": "{coding problem}",
                "steps": [
                    {
                        "step": "{step}",
                        "result": "{result}"
                    }
                ]
            }
            ```

            Your reply must contain one of [task|ask|summary] to indicate the type of your message.
            """,
                llmConfig: new ConversableAgentConfig
                {
                    Temperature = 0,
                    ConfigList = [gptConfig],
                })
                .RegisterPrintMessage();

            // create coder agent
            // The coder agent is a composite agent that contains dotnet coder, code reviewer and nuget agent.
            // The dotnet coder write dotnet code to resolve the task.
            // The code reviewer review the code block from coder's reply.
            // The nuget agent install nuget packages if there's any.
            var coderAgent = new GPTAgent(
                name: "coder",
                systemMessage: @"You act as dotnet coder, you write dotnet code to resolve task. Once you finish writing code, ask runner to run the code for you.

Here're some rules to follow on writing dotnet code:
- put code between ```csharp and ```
- When creating http client, use `var httpClient = new HttpClient()`. Don't use `using var httpClient = new HttpClient()` because it will cause error when running the code.
- Try to use `var` instead of explicit type.
- Try avoid using external library, use .NET Core library instead.
- Use top level statement to write code.
- Always print out the result to console. Don't write code that doesn't print out anything.

If you need to install nuget packages, put nuget packages in the following format:
```nuget
nuget_package_name
```

If your code is incorrect, Fix the error and send the code again.

Here's some externel information
- The link to mlnet repo is: https://github.com/dotnet/machinelearning. you don't need a token to use github pr api. Make sure to include a User-Agent header, otherwise github will reject it.
",
                config: gptConfig,
                temperature: 0.4f)
                .RegisterPrintMessage();

            // code reviewer agent will review if code block from coder's reply satisfy the following conditions:
            // - There's only one code block
            // - The code block is csharp code block
            // - The code block is top level statement
            // - The code block is not using declaration
            var codeReviewAgent = new GPTAgent(
                name: "reviewer",
                systemMessage: """
            You are a code reviewer who reviews code from coder. You need to check if the code satisfy the following conditions:
            - The reply from coder contains at least one code block, e.g ```csharp and ```
            - There's only one code block and it's csharp code block
            - The code block is not inside a main function. a.k.a top level statement
            - The code block is not using declaration when creating http client

            You don't check the code style, only check if the code satisfy the above conditions.

            Put your comment between ```review and ```, if the code satisfies all conditions, put APPROVED in review.result field. Otherwise, put REJECTED along with comments. make sure your comment is clear and easy to understand.
            
            ## Example 1 ##
            ```review
            comment: The code satisfies all conditions.
            result: APPROVED
            ```

            ## Example 2 ##
            ```review
            comment: The code is inside main function. Please rewrite the code in top level statement.
            result: REJECTED
            ```

            """,
                config: gptConfig,
                temperature: 0f)
                .RegisterPrintMessage();

            // create runner agent
            // The runner agent will run the code block from coder's reply.
            // It runs dotnet code using dotnet interactive service hook.
            // It also truncate the output if the output is too long.
            var runner = new AssistantAgent(
                name: "runner",
                defaultReply: "No code available, coder, write code please")
                .RegisterDotnetCodeBlockExectionHook(interactiveService: service)
                .RegisterMiddleware(async (msgs, option, agent, ct) =>
                {
                    var mostRecentCoderMessage = msgs.LastOrDefault(x => x.From == "coder") ?? throw new Exception("No coder message found");
                    return await agent.GenerateReplyAsync(new[] { mostRecentCoderMessage }, option, ct);
                })
                .RegisterPrintMessage();

            var adminToCoderTransition = Transition.Create(admin, coderAgent, async (from, to, messages) =>
            {
                // the last message should be from admin
                var lastMessage = messages.Last();
                if (lastMessage.From != admin.Name)
                {
                    return false;
                }

                return true;
            });
            var coderToReviewerTransition = Transition.Create(coderAgent, codeReviewAgent);
            var adminToRunnerTransition = Transition.Create(admin, runner, async (from, to, messages) =>
            {
                // the last message should be from admin
                var lastMessage = messages.Last();
                if (lastMessage.From != admin.Name)
                {
                    return false;
                }

                // the previous messages should contain a message from coder
                var coderMessage = messages.FirstOrDefault(x => x.From == coderAgent.Name);
                if (coderMessage is null)
                {
                    return false;
                }

                return true;
            });

            var runnerToAdminTransition = Transition.Create(runner, admin);

            var reviewerToAdminTransition = Transition.Create(codeReviewAgent, admin);

            var adminToUserTransition = Transition.Create(admin, userProxy, async (from, to, messages) =>
            {
                // the last message should be from admin
                var lastMessage = messages.Last();
                if (lastMessage.From != admin.Name)
                {
                    return false;
                }

                return true;
            });

            var userToAdminTransition = Transition.Create(userProxy, admin);

            var workflow = new Graph(
                [
                    adminToCoderTransition,
                    coderToReviewerTransition,
                    reviewerToAdminTransition,
                    adminToRunnerTransition,
                    runnerToAdminTransition,
                    adminToUserTransition,
                    userToAdminTransition,
                ]);

            // create group chat
            var groupChat = new GroupChat(
                admin: groupAdmin,
                members: [admin, coderAgent, runner, codeReviewAgent, userProxy],
                workflow: workflow);

            // task 1: retrieve the most recent pr from mlnet and save it in result.txt
            var groupChatManager = new GroupChatManager(groupChat);
            await userProxy.SendAsync(groupChatManager, "Retrieve the most recent pr from mlnet and save it in result.txt", maxRound: 30);
            Debug.Assert(File.Exists(result));//.Should().BeTrue();

            // task 2: calculate the 39th fibonacci number
            var answer = 63245986;
            // clear the result file
            File.Delete(result);

            var conversationHistory = await userProxy.InitiateChatAsync(groupChatManager, "What's the 39th of fibonacci number? Save the result in result.txt", maxRound: 10);
            Debug.Assert(File.Exists(result));//.Should().BeTrue();
            var resultContent = File.ReadAllText(result);
            Debug.Assert(resultContent.Contains(answer.ToString()));//.Should().Contain(answer.ToString());
        }
    }

}
