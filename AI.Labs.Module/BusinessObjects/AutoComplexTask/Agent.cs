using AutoGen.Core;
using AutoGen.DotnetInteractive;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenAI.ObjectModels.RequestModels;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace AI.Labs.Module.BusinessObjects.AutoComplexTask
{
    public class AgentGroup : SimpleXPObject
    {
        public AgentGroup(Session s) : base(s)
        {

        }

        [XafDisplayName("小组名称")]
        public string GroupName
        {
            get { return GetPropertyValue<string>(nameof(GroupName)); }
            set { SetPropertyValue(nameof(GroupName), value); }
        }

        [XafDisplayName("小组任务")]
        public string GroupMemo
        {
            get { return GetPropertyValue<string>(nameof(GroupMemo)); }
            set { SetPropertyValue(nameof(GroupMemo), value); }
        }

        //[Association, DevExpress.Xpo.Aggregated]
        //public XPCollection<Agent> Agents
        //{
        //    get
        //    {
        //        return GetCollection<Agent>(nameof(Agents));
        //    }
        //}
    }
    public class CodeRunAgent : Agent
    {
        public CodeRunAgent(Session s) : base(s)
        {

        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            Name = "CodeRunner";
        }
        public async override Task<string> Send(string message)
        {
            var workDir = Path.Combine(Path.GetTempPath(), "InteractiveService");
            if (!Directory.Exists(workDir))
                Directory.CreateDirectory(workDir);

            using var service = new InteractiveService(workDir);
            var dotnetInteractiveFunctions = new DotnetInteractiveFunction(service);

            var resultFile = Path.Combine(workDir, "result.txt");
            if (File.Exists(resultFile))
                File.Delete(resultFile);

            await service.StartAsync(workDir, default);
            string codeBlockPrefix = "```csharp";
            string codeBlockSuffix = "```";
            int maximumOutputToKeep = 500;


            string[] array = message.Split(new string[1] { codeBlockPrefix }, StringSplitOptions.RemoveEmptyEntries);
            if (array.Length == 0)
            {
                throw new Exception("代码错?");
            }

            StringBuilder result = new StringBuilder();
            int i = 0;
            result.AppendLine("// [DOTNET_CODE_BLOCK_EXECUTION]");
            string[] array2 = array;
            foreach (string text in array2)
            {
                int num = text.IndexOf(codeBlockSuffix);
                if (num != -1)
                {
                    string text2 = text.Substring(0, num).Trim();
                    if (text2.Length != 0)
                    {
                        string text3 = await service.SubmitCSharpCodeAsync(text2, default);
                        if (text3 != null)
                        {
                            result.AppendLine($"### Executing result for code block {i++}");
                            result.AppendLine(text3);
                            result.AppendLine("### End of executing result ###");
                        }
                    }
                }
            }

            if (resultFile.Length <= maximumOutputToKeep)
            {
                maximumOutputToKeep = resultFile.Length;
            }

            return resultFile.ToString();

        }
    }

    [NavigationItem("自动任务")]
    public class Agent : SimpleXPObject
    {
        public Agent(Session s) : base(s)
        {

        }

        [Association]
        public ComplexTask Task
        {
            get { return GetPropertyValue<ComplexTask>(nameof(Task)); }
            set { SetPropertyValue(nameof(Task), value); }
        }

        //public AgentGroup AgentGroup
        //{
        //    get { return GetPropertyValue<AgentGroup>(nameof(AgentGroup)); }
        //    set { SetPropertyValue(nameof(AgentGroup), value); }
        //}

        [XafDisplayName("名称")]
        public string Name
        {
            get { return GetPropertyValue<string>(nameof(Name)); }
            set { SetPropertyValue(nameof(Name), value); }
        }

        [Size(-1)]
        [XafDisplayName("系统提示")]
        public string SystemPrompt
        {
            get { return GetPropertyValue<string>(nameof(SystemPrompt)); }
            set { SetPropertyValue(nameof(SystemPrompt), value); }
        }

        public AIModel Model
        {
            get { return GetPropertyValue<AIModel>(nameof(Model)); }
            set { SetPropertyValue(nameof(Model), value); }
        }

        public async virtual Task<string> Send(string message)
        {
            var modelName = "qwen-max";
            var request = new ChatCompletionCreateRequest
            {
                Messages = new List<ChatMessage>(),
                Model = modelName,
                Temperature = 0,
                MaxTokens = 1024
            };

            if (!string.IsNullOrEmpty(SystemPrompt))
            {
                request.Messages.Add(ChatMessage.FromSystem(SystemPrompt));
            }

            foreach (var x in Task.GetHistory())
            {
                request.Messages.Add(x);
            }

            request.Messages.Add(ChatMessage.FromUser(message));

            var rst = new StringBuilder();
            await AIHelper.AskCore(t =>
            {
                rst.Append(t.Content);
                Debug.WriteLine(t.Content);

            }, true, AIHelper.DefaultAIService, request, SynchronizationContext.Current);

            return rst.ToString();
        }

    }

    public class UserAgent : Agent
    {
        public UserAgent(Session s) : base(s)
        {

        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            Name = "User";
        }

    }

}
