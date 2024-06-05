using AutoGen;
using AutoGen.Core;

namespace AI.Labs.Module.BusinessObjects.AutoComplexTask
{
    public class UserProxyAgent : ConversableAgent
    {
        public UserProxyAgent(
            string name,
            string systemMessage = "You are a helpful AI assistant",
            ConversableAgentConfig? llmConfig = null,
            Func<IEnumerable<IMessage>, CancellationToken, Task<bool>>? isTermination = null,
            HumanInputMode humanInputMode = HumanInputMode.ALWAYS,
            IDictionary<string, Func<string, Task<string>>>? functionMap = null,
            string? defaultReply = null)
            : base(name: name,
                  systemMessage: systemMessage,
                  llmConfig: llmConfig,
                  isTermination: isTermination,
                  humanInputMode: humanInputMode,
                  functionMap: functionMap,
                  defaultReply: defaultReply)
        {
        }
    }

}
