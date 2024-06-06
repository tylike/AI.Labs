using AutoGen.Core;
using System.Windows.Forms;

namespace AI.Labs.Module.BusinessObjects.AutoComplexTask
{
    public class Graph
    {
        private readonly List<Transition> transitions = new List<Transition>();

        public Graph(IEnumerable<Transition> transitions)
        {
            this.transitions.AddRange(transitions);
        }

        public void AddTransition(Transition transition)
        {
            transitions.Add(transition);
        }

        /// <summary>
        /// Get the transitions of the workflow.
        /// </summary>
        public IEnumerable<Transition> Transitions => transitions;

        /// <summary>
        /// Get the next available agents that the messages can be transit to.
        /// </summary>
        /// <param name="fromAgent">the from agent</param>
        /// <param name="messages">messages</param>
        /// <returns>A list of agents that the messages can be transit to</returns>
        public async Task<IEnumerable<IAgent>> TransitToNextAvailableAgentsAsync(IAgent fromAgent, IEnumerable<IMessage> messages)
        {
            var nextAgents = new List<IAgent>();
            var availableTransitions = transitions.FindAll(t => t.From == fromAgent) ?? Enumerable.Empty<Transition>();
            foreach (var transition in availableTransitions)
            {
                if (await transition.CanTransitionAsync(messages))
                {
                    nextAgents.Add(transition.To);
                }
            }

            return nextAgents;
        }
    }
    public class GroupChat : IGroupChat
    {
        private IAgent? admin;
        private List<IAgent> agents = new List<IAgent>();
        private IEnumerable<IMessage> initializeMessages = new List<IMessage>();
        private Graph? workflow = null;

        public IEnumerable<IMessage>? Messages { get; private set; }

        /// <summary>
        /// Create a group chat. The next speaker will be decided by a combination effort of the admin and the workflow.
        /// </summary>
        /// <param name="admin">admin agent. If provided, the admin will be invoked to decide the next speaker.</param>
        /// <param name="workflow">workflow of the group chat. If provided, the next speaker will be decided by the workflow.</param>
        /// <param name="members">group members.</param>
        /// <param name="initializeMessages"></param>
        public GroupChat(
            IEnumerable<IAgent> members,
            IAgent? admin = null,
            IEnumerable<IMessage>? initializeMessages = null,
            Graph? workflow = null)
        {
            this.admin = admin;
            this.agents = members.ToList();
            this.initializeMessages = initializeMessages ?? new List<IMessage>();
            this.workflow = workflow;

            this.Validation();
        }

        private void Validation()
        {
            // check if all agents has a name
            if (this.agents.Any(x => string.IsNullOrEmpty(x.Name)))
            {
                throw new Exception("All agents must have a name.");
            }

            // check if any agents has the same name
            var names = this.agents.Select(x => x.Name).ToList();
            if (names.Distinct().Count() != names.Count)
            {
                throw new Exception("All agents must have a unique name.");
            }

            // if there's a workflow
            // check if the agents in that workflow are in the group chat
            if (this.workflow != null)
            {
                var agentNamesInWorkflow = this.workflow.Transitions.Select(x => x.From.Name!).Concat(this.workflow.Transitions.Select(x => x.To.Name!)).Distinct();
                if (agentNamesInWorkflow.Any(x => !this.agents.Select(a => a.Name).Contains(x)))
                {
                    throw new Exception("All agents in the workflow must be in the group chat.");
                }
            }

            // must provide one of admin or workflow
            if (this.admin == null && this.workflow == null)
            {
                throw new Exception("Must provide one of admin or workflow.");
            }
        }

        /// <summary>
        /// Select the next speaker based on the conversation history.
        /// The next speaker will be decided by a combination effort of the admin and the workflow.
        /// Firstly, a group of candidates will be selected by the workflow. If there's only one candidate, then that candidate will be the next speaker.
        /// Otherwise, the admin will be invoked to decide the next speaker using role-play prompt.
        /// </summary>
        /// <param name="currentSpeaker">current speaker</param>
        /// <param name="conversationHistory">conversation history</param>
        /// <returns>next speaker.</returns>
        public async Task<IAgent> SelectNextSpeakerAsync(IAgent currentSpeaker, IEnumerable<IMessage> conversationHistory)
        {
            var agentNames = this.agents.Select(x => x.Name).ToList();
            if (this.workflow != null)
            {
                var nextAvailableAgents = await this.workflow.TransitToNextAvailableAgentsAsync(currentSpeaker, conversationHistory);
                agentNames = nextAvailableAgents.Select(x => x.Name).ToList();
                if (agentNames.Count() == 0)
                {
                    throw new Exception("No next available agents found in the current workflow");
                }

                if (agentNames.Count() == 1)
                {
                    return this.agents.First(x => x.Name == agentNames.First());
                }
            }

            if (this.admin == null)
            {
                throw new Exception("No admin is provided.");
            }

            var systemMessage = new TextMessage(Role.System,
                content: $@"You are in a role play game. Carefully read the conversation history and carry on the conversation.
The available roles are:
{string.Join(",", agentNames)}

Each message will start with 'From name:', e.g:
From {agentNames.First()}:
//your message//.");

            var conv = this.ProcessConversationsForRolePlay(this.initializeMessages, conversationHistory);

            var messages = new IMessage[] { systemMessage }.Concat(conv);
            var response = await this.admin.GenerateReplyAsync(
                messages: messages,
                options: new GenerateReplyOptions
                {
                    Temperature = 0,
                    MaxToken = 128,
                    StopSequence = new[] { ":" },
                    Functions = new FunctionContract[] { },
                });

            var name = response?.GetContent() ?? throw new Exception("No name is returned.");

            // remove From
            name = name!.Substring(5);
            var t = this.agents.FirstOrDefault(x => x.Name!.ToLower() == name.ToLower());
            if (t == null)
            {
                throw new Exception("LLM 太傻了!");
            }
            return t;
        }


        //这段 C# 代码定义了一个名为 SelectNextSpeakerAsync 的异步方法，用于在多角色对话场景中，根据对话历史和预设规则选择下一个发言者。
        //功能分解：
        //方法目标：
        //根据对话历史选择下一位发言者。
        //下一位发言者的决定结合了管理员和工作流程的共同作用。
        //候选人筛选(可选)：
        //if (this.workflow != null): 如果定义了工作流程，则使用工作流程筛选候选人。
        //var nextAvailableAgents = await this.workflow.TransitToNextAvailableAgentsAsync(currentSpeaker, conversationHistory);
        // 调用工作流程的 TransitToNextAvailableAgentsAsync 方法获取下一组可用的代理。

        //如果没有找到候选人，则抛出异常。
        //如果只有一个候选人，则直接返回该候选人。

        //管理员决策(如果有多个候选人)：

        //if (this.admin == null): 如果未提供管理员，则抛出异常。

        //构建系统消息：
        //向管理员解释当前场景：角色扮演游戏，需要根据对话历史选择下一个发言者。
        //列出可用的角色：string.Join(",", agentNames)。
        //说明每条消息的格式：From name: //your message//。
        //var conv = this.ProcessConversationsForRolePlay(this.initializeMessages, conversationHistory); : 处理对话历史，准备发送给管理员。
        //var messages = new IMessage[] { systemMessage }.Concat(conv);: 将系统消息和处理后的对话历史合并。
        //var response = await this.admin.GenerateReplyAsync(messages: messages, options: ...);: 调用管理员的 GenerateReplyAsync 方法获取回复，该方法可能是一个语言模型。
        //从管理员的回复中提取下一个发言者的名字。
        //返回结果：

        //根据提取的名字找到对应的发言者对象并返回。
        //如果找不到，则抛出异常。
        //总结：

        //SelectNextSpeakerAsync 方法实现了在多角色对话场景中选择下一个发言者的逻辑。它首先根据预设的工作流程筛选候选人，如果有多个候选人，则将决策权交给管理员，由管理员根据对话历史决定下一个发言者。

        //注意：

        //代码中 IAgent, IMessage, TextMessage, Role, GenerateReplyOptions, ProcessConversationsForRolePlay 等类型需要根据具体的应用场景进行定义。
        //this.admin.GenerateReplyAsync 方法的具体实现取决于所使用的管理员类型，它可能是一个语言模型或其他类型的决策系统。
        //代码中的中文注释 "LLM 太傻了!" 表明了开发者对语言模型在某些情况下表现不佳的无奈。

        /// <inheritdoc />
        public void AddInitializeMessage(IMessage message)
        {
            this.SendIntroduction(message);
        }

        public async Task<IEnumerable<IMessage>> CallAsync(
            IEnumerable<IMessage>? conversationWithName = null,
            int maxRound = 10,
            CancellationToken ct = default)
        {
            var conversationHistory = new List<IMessage>();
            if (conversationWithName != null)
            {
                conversationHistory.AddRange(conversationWithName);
            }

            var lastSpeaker = conversationHistory.LastOrDefault()?.From switch
            {
                null => this.agents.First(),
                _ => this.agents.FirstOrDefault(x => x.Name == conversationHistory.Last().From) ?? throw new Exception("The agent is not in the group chat"),
            };
            var round = 0;
            while (round < maxRound)
            {
                var currentSpeaker = await this.SelectNextSpeakerAsync(lastSpeaker, conversationHistory);
                var processedConversation = this.ProcessConversationForAgent(this.initializeMessages, conversationHistory);
                var result = await currentSpeaker.GenerateReplyAsync(processedConversation) ?? throw new Exception("No result is returned.");
                conversationHistory.Add(result);

                // if message is terminate message, then terminate the conversation
                if (result?.IsGroupChatTerminateMessage() ?? false)
                {
                    break;
                }

                lastSpeaker = currentSpeaker;
                round++;
            }

            return conversationHistory;
        }

        public void SendIntroduction(IMessage message)
        {
            this.initializeMessages = this.initializeMessages.Append(message);
        }
    }

}
