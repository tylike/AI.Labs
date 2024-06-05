﻿using AutoGen;
using AutoGen.Core;
using AutoGen.LMStudio;
using AutoGen.OpenAI;

namespace AI.Labs.Module.BusinessObjects.AutoComplexTask
{
    public class ConversableAgent : IAgent
    {
        private readonly IAgent? innerAgent;
        private readonly string? defaultReply;
        private readonly HumanInputMode humanInputMode;
        private readonly IDictionary<string, Func<string, Task<string>>>? functionMap;
        private readonly string systemMessage;
        private readonly IEnumerable<FunctionContract>? functions;

        public ConversableAgent(
            string name,
            string systemMessage = "You are a helpful AI assistant",
            IAgent? innerAgent = null,
            string? defaultAutoReply = null,
            HumanInputMode humanInputMode = HumanInputMode.NEVER,
            Func<IEnumerable<IMessage>, CancellationToken, Task<bool>>? isTermination = null,
            IDictionary<string, Func<string, Task<string>>>? functionMap = null)
        {
            this.Name = name;
            this.defaultReply = defaultAutoReply;
            this.functionMap = functionMap;
            this.humanInputMode = humanInputMode;
            this.innerAgent = innerAgent;
            this.IsTermination = isTermination;
            this.systemMessage = systemMessage;
        }

        public ConversableAgent(
            string name,
            string systemMessage = "You are a helpful AI assistant",
            ConversableAgentConfig? llmConfig = null,
            Func<IEnumerable<IMessage>, CancellationToken, Task<bool>>? isTermination = null,
            HumanInputMode humanInputMode = HumanInputMode.AUTO,
            IDictionary<string, Func<string, Task<string>>>? functionMap = null,
            string? defaultReply = null)
        {
            this.Name = name;
            this.defaultReply = defaultReply;
            this.functionMap = functionMap;
            this.humanInputMode = humanInputMode;
            this.IsTermination = isTermination;
            this.systemMessage = systemMessage;
            this.innerAgent = llmConfig?.ConfigList != null ? this.CreateInnerAgentFromConfigList(llmConfig) : null;
            this.functions = llmConfig?.FunctionContracts;
        }

        /// <summary>
        /// For test purpose only.
        /// </summary>
        internal IAgent? InnerAgent => this.innerAgent;

        private IAgent? CreateInnerAgentFromConfigList(ConversableAgentConfig config)
        {
            IAgent? agent = null;
            foreach (var llmConfig in config.ConfigList ?? Enumerable.Empty<ILLMConfig>())
            {
                IAgent nextAgent = llmConfig switch
                {
                    AzureOpenAIConfig azureConfig => new GPTAgent(this.Name!, this.systemMessage, azureConfig, temperature: config.Temperature ?? 0),
                    OpenAIConfig openAIConfig => AgentHelper.CreateAgent(name: this.Name, systemMessage: this.systemMessage, temperature: config.Temperature ?? 0), //new GPTAgent(this.Name!, this.systemMessage, openAIConfig, temperature: config.Temperature ?? 0),
                    LMStudioConfig lmStudioConfig => new LMStudioAgent(
                        name: this.Name,
                        config: lmStudioConfig,
                        systemMessage: this.systemMessage,
                        temperature: config.Temperature ?? 0),
                    OpenAILikeConfig openAILikeConfig => AgentHelper.CreateAgent(name: this.Name, systemMessage: this.systemMessage, temperature: config.Temperature ?? 0),
                    _ => throw new ArgumentException($"Unsupported config type {llmConfig.GetType()}"),
                };

                if (agent == null)
                {
                    agent = nextAgent;
                }
                else
                {
                    agent = agent.RegisterMiddleware(async (messages, option, agent, cancellationToken) =>
                    {
                        var agentResponse = await nextAgent.GenerateReplyAsync(messages, option, cancellationToken: cancellationToken);

                        if (agentResponse is null)
                        {
                            return await agent.GenerateReplyAsync(messages, option, cancellationToken);
                        }
                        else
                        {
                            return agentResponse;
                        }
                    });
                }
            }

            return agent;
        }

        public string Name { get; }

        public Func<IEnumerable<IMessage>, CancellationToken, Task<bool>>? IsTermination { get; }

        public async Task<IMessage> GenerateReplyAsync(
            IEnumerable<IMessage> messages,
            GenerateReplyOptions? overrideOptions = null,
            CancellationToken cancellationToken = default)
        {
            // if there's no system message, add system message to the first of chat history
            if (!messages.Any(m => m.IsSystemMessage()))
            {
                var systemMessage = new TextMessage(Role.System, this.systemMessage, from: this.Name);
                messages = new[] { systemMessage }.Concat(messages);
            }

            // process order: function_call -> human_input -> inner_agent -> default_reply -> self_execute
            // first in, last out

            // process default reply
            MiddlewareAgent agent;
            if (this.innerAgent != null)
            {
                agent = innerAgent.RegisterMiddleware(async (msgs, option, agent, ct) =>
                {
                    var updatedMessages = msgs.Select(m =>
                    {
                        if (m.From == this.Name)
                        {
                            m.From = this.innerAgent.Name;
                            return m;
                        }
                        else
                        {
                            return m;
                        }
                    });

                    return await agent.GenerateReplyAsync(updatedMessages, option, ct);
                });
            }
            else
            {
                agent = new MiddlewareAgent<DefaultReplyAgent>(new DefaultReplyAgent(this.Name!, this.defaultReply ?? "Default reply is not set. Please pass a default reply to assistant agent"));
            }

            // process human input
            var humanInputMiddleware = new HumanInputMiddleware(mode: this.humanInputMode, isTermination: this.IsTermination);
            agent.Use(humanInputMiddleware);

            // process function call
            var functionCallMiddleware = new FunctionCallMiddleware(functions: this.functions, functionMap: this.functionMap);
            agent.Use(functionCallMiddleware);

            return await agent.GenerateReplyAsync(messages, overrideOptions, cancellationToken);
        }
    }

}
