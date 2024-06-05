using AutoGen;
using AutoGen.Core;
using AutoGen.DotnetInteractive;
using DevExpress.CodeParser;
using DevExpress.ExpressApp.Utils;
using System.Diagnostics;
using System.Text;

namespace AI.Labs.Module.BusinessObjects.AutoComplexTask
{

    public class AITask
    {
        public static async Task RunAsync()
        {
            #region 环境准备
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

            #endregion


            //var llm = new LLMConfig()
            var gptConfig = new OpenAILikeConfig(new Uri("https://na9yqj2l5a5r.share.zrok.io"));

            #region 助手-暂时没发现有用
            var helperAgent = AgentHelper.CreateAgent(
        name: "helper",
        systemMessage: "You are a helpful AI assistant",
        temperature: 0f
        );
            #endregion

            #region 组管理员
            var groupAdmin = AgentHelper.CreateAgent(
        name: "groupAdmin",
        systemMessage: "You are the admin of the group chat",
        temperature: 0f
        )
        .RegisterPrintMessage();
            #endregion

            #region 用户
            var userProxy = new UserProxyAgent(name: "user", defaultReply: GroupChatExtension.TERMINATE, humanInputMode: HumanInputMode.NEVER)
        .RegisterPrintMessage();
            #endregion

            #region 管理员
            // Create admin agent

            #region old prompt
            //You are a manager who takes coding problem from user and resolve problem by splitting them into small tasks and assign each task to the most appropriate agent.
            //Here's available agents who you can assign task to:
            //- coder: write dotnet code to resolve task
            //-runner: run dotnet code from coder

            //The workflow is as follows:
            //-You take the coding problem from user
            //-You break the problem into small tasks.For each tasks you first ask coder to write code to resolve the task. Once the code is written, you ask runner to run the code.
            //- Once a small task is resolved, you summarize the completed steps and create the next step.
            //-You repeat the above steps until the coding problem is resolved.
            //你的返回内容必须是下列json之一,不要多余的说明.
            //You can use the following json format to assign task to agents:
            //```task
            //{
            //    "to": "{agent_name}",
            //    "task": "{a short description of the task}",
            //    "context": "{previous context from scratchpad}"
            //}
            //```

            //If you need to ask user for extra information, you can use the following format:
            //```ask
            //{
            //    "question": "{question}"
            //}
            //```

            //Once the coding problem is resolved, summarize each steps and results and send the summary to the user using the following format:
            //```summary
            //{
            //    "problem": "{coding problem}",
            //    "steps": [
            //        {
            //        "step": "{step}",
            //            "result": "{result}"
            //        }
            //    ]
            //}
            //```

            //Your reply must contain one of[task | ask | summary] to indicate the type of your message. 
            #endregion

            var admin = new AssistantAgent(
                name: "admin",
                systemMessage: """
            你是一位经理，负责从用户那里获取编程问题，并将问题分解成小任务，然后将每个任务分配给最合适的代理。
            你的返回内容必须是下列json之一,不要多余的说明.
            以下是你可以分配任务的可用代理：

            coder：编写 .NET 代码来解决任务
            runner：运行 coder 编写的 .NET 代码

            工作流程如下：

            你从用户那里获取编程问题
            你将问题分解成小任务。对于每个任务，你首先要求 coder 编写代码来解决任务。代码编写完成后，你要求 runner 运行代码。
            一旦一个小任务得到解决，你就总结已完成的步骤并创建下一步。
            你重复上述步骤，直到编程问题得到解决。
            你可以使用以下 JSON 格式将任务分配给代理：
            ```task
            {
                "to": "{agent_name}",
                "task": "{任务的简短描述}",
                "context": "{暂存器中的先前上下文}"
            }
            ```
            如果你需要向用户询问更多信息，你可以使用以下格式：
            ```ask
            {
                "question": "{问题}"
            }
            ```
            一旦编程问题得到解决，总结每个步骤和结果，并使用以下格式将摘要发送给用户：
            ```summary
            {
                "problem": "{编程问题}",
                "steps": [
                    {
                        "step": "{步骤}",
                        "result": "{结果}"
                    }
                ]
            }
            ```
            你的回复必须包含 [task|ask|summary] 中的一个，以指示你的消息类型。
            """,
                llmConfig: new ConversableAgentConfig
                {
                    Temperature = 0,
                    ConfigList = new ILLMConfig[] { gptConfig },
                })
                .RegisterPrintMessage();
            #endregion

            #region 码农
            // create coder agent
            // The coder agent is a composite agent that contains dotnet coder, code reviewer and nuget agent.
            // The dotnet coder write dotnet code to resolve the task.
            // The code reviewer review the code block from coder's reply.
            // The nuget agent install nuget packages if there's any.
            // You act as dotnet coder, you write dotnet code to resolve task. Once you finish writing code, ask runner to run the code for you.

            //Here're some rules to follow on writing dotnet code:
            //- put code between ```csharp and ```
            //- When creating http client, use `var httpClient = new HttpClient()`. Don't use `using var httpClient = new HttpClient()` because it will cause error when running the code.
            //- Try to use `var` instead of explicit type.
            //- Try avoid using external library, use .NET Core library instead.
            //- Use top level statement to write code.
            //- Always print out the result to console. Don't write code that doesn't print out anything.

            //If you need to install nuget packages, put nuget packages in the following format:
            //```nuget
            //nuget_package_name
            //```

            //If your code is incorrect, Fix the error and send the code again.

            //Here's some externel information
            //- The link to mlnet repo is: https://github.com/dotnet/machinelearning. you don't need a token to use github pr api. Make sure to include a User-Agent header, otherwise github will reject it.
            var coderAgent = AgentHelper.CreateAgent(
                name: "coder",
                systemMessage: @"
你是一个 .NET 程序员的，根据提供的任务编写 .NET 代码。代码编写完成后，你会要求运行器帮你运行代码。
以下是编写 .NET 代码时需要遵循的一些规则：

- 使用 ```csharp 和 ``` 包裹代码。
- 创建 HTTP 客户端时，使用 `var httpClient = new HttpClient()`。不要使用 `using var httpClient = new HttpClient()`，因为这会在运行代码时导致错误。
- 尽量使用 `var` 而不是显式类型。
- 尽量避免使用外部库，而是使用 .NET Core 库。
- 使用顶级语句编写代码。
- 始终将结果打印到控制台。不要编写不打印任何内容的代码。

如果需要安装 NuGet 包，请使用以下格式：
```nuget
nuget_package_name
```

如果代码不正确，请修复错误并重新发送代码。

以下是一些外部信息：
- mlnet 存储库的链接为：https://github.com/dotnet/machinelearning。 你不需要令牌即可使用 GitHub API。 请确保包含 User-Agent 标头，否则 GitHub 将拒绝该请求。
",
                //config: gptConfig,
                temperature: 0.4f)
                .RegisterPrintMessage();
            #endregion

            #region 代码检查
            // code reviewer agent will review if code block from coder's reply satisfy the following conditions:
            // - There's only one code block
            // - The code block is csharp code block
            // - The code block is top level statement
            // - The code block is not using declaration
            // You are a code reviewer who reviews code from coder. You need to check if the code satisfy the following conditions:
            //- The reply from coder contains at least one code block, e.g ```csharp and ```
            //- There's only one code block and it's csharp code block
            //- The code block is not inside a main function. a.k.a top level statement
            //- The code block is not using declaration when creating http client

            //You don't check the code style, only check if the code satisfy the above conditions.

            //Put your comment between ```review and ```, if the code satisfies all conditions, put APPROVED in review.result field. Otherwise, put REJECTED along with comments. make sure your comment is clear and easy to understand.

            //## Example 1 ##
            //```review
            //comment: The code satisfies all conditions.
            //result: APPROVED
            //```

            //## Example 2 ##
            //```review
            //comment: The code is inside main function. Please rewrite the code in top level statement.
            //result: REJECTED
            //```

            var codeReviewAgent = AgentHelper.CreateAgent(
                name: "reviewer",
                systemMessage: """
            你是一名代码审查员，负责审查程序员提交的代码。你需要检查代码是否满足以下条件：

            - 程序员的回复中包含至少一个代码块，例如 ```csharp 和 ```
            - 只有一个代码块，并且它是 C# 代码块
            - 代码块不在 main 函数内部，也称为顶级语句
            - 代码块在创建 HTTP 客户端时未使用 using 声明

            你不需要检查代码风格，只需检查代码是否满足上述条件。

            请将你的评论放在 ```review 和 ``` 之间，如果代码满足所有条件，请在 review.result 字段中填写 APPROVED。 否则，填写 REJECTED 并附带评论。 确保你的评论清晰易懂。

            ## 例 1 ##
            ```review
            comment: 代码满足所有条件。
            result: APPROVED
            ```

            ## 例 2 ##
            ```review
            comment: 代码位于 main 函数内部。请使用顶级语句重写代码。
            result: REJECTED
            ```
            """,
                //config: gptConfig,
                temperature: 0f)
                .RegisterPrintMessage();
            #endregion

            #region 运行测试
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
            #endregion

            #region 流程
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
                new Transition[] {adminToCoderTransition,
                    coderToReviewerTransition,
                    reviewerToAdminTransition,
                    adminToRunnerTransition,
                    runnerToAdminTransition,
                    adminToUserTransition,
                    userToAdminTransition, });
            #endregion

            #region 群聊
            // create group chat
            var groupChat = new GroupChat(
                admin: groupAdmin,
                members: new IAgent[] { admin, coderAgent, runner, codeReviewAgent, userProxy },
                workflow: workflow);

            // task 1: retrieve the most recent pr from mlnet and save it in result.txt
            var groupChatManager = new GroupChatManager(groupChat);
            #endregion


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
