using AutoGen.Core;
using System.Windows.Forms;

namespace AI.Labs.Module.BusinessObjects.AutoComplexTask
{
    public static class GroupChatExtension
    {
        public const string TERMINATE = "[GROUPCHAT_TERMINATE]";
        public const string CLEAR_MESSAGES = "[GROUPCHAT_CLEAR_MESSAGES]";

        [Obsolete("please use SendIntroduction")]
        public static void AddInitializeMessage(this IAgent agent, string message, IGroupChat groupChat)
        {
            var msg = new TextMessage(Role.User, message)
            {
                From = agent.Name
            };

            groupChat.SendIntroduction(msg);
        }

        /// <summary>
        /// Send an instruction message to the group chat.
        /// </summary>
        public static void SendIntroduction(this IAgent agent, string message, IGroupChat groupChat)
        {
            var msg = new TextMessage(Role.User, message)
            {
                From = agent.Name
            };

            groupChat.SendIntroduction(msg);
        }

        /// <summary>
        /// 这段 C# 代码定义了一个名为 `MessageToKeep` 的静态扩展方法，用于过滤群组聊天消息，保留特定条件下的消息。
        ///**功能分解：**
        ///1. **输入：**
        ///    - `this IGroupChat _`: 当前群组聊天对象(未使用)。
        ///    - `IEnumerable<IMessage> messages`: 需要过滤的消息列表。
        ///2. **查找倒数第二个 "清除聊天" 消息：**
        ///    - `var lastCLRMessageIndex = messages.ToList().FindLastIndex(x => x.IsGroupChatClearMessage());` 
        ///        - 查找最后一个 "清除聊天" 消息的索引。
        ///    -  `if (messages.Count(m => m.IsGroupChatClearMessage()) > 1)` 
        ///        - 如果存在多个 "清除聊天" 消息，则找到倒数第二个 "清除聊天" 消息的索引：
        ///            -  `lastCLRMessageIndex = messages.ToList().FindLastIndex(lastCLRMessageIndex - 1, lastCLRMessageIndex - 1, x => x.IsGroupChatClearMessage());` 
        ///                - 从上一个找到的索引位置向前查找。
        ///        -  `messages = messages.Skip(lastCLRMessageIndex);` 
        ///                - 保留倒数第二个 "清除聊天" 消息之后的所有消息。
        ///3. **查找最后一个 "清除聊天" 消息：**
        ///    -  `lastCLRMessageIndex = messages.ToList().FindLastIndex(x => x.IsGroupChatClearMessage());` 
        ///        - 再次查找最后一个 "清除聊天" 消息的索引(因为 messages 可能已经被修改)。
        ///4. **保留特定消息：**
        ///    - `if (lastCLRMessageIndex != -1 && messages.Count() - lastCLRMessageIndex >= 2)` 
        ///        - 如果找到了最后一个 "清除聊天" 消息，并且该消息之后至少还有两条消息，则：
        ///            - `messages = messages.Skip(lastCLRMessageIndex);` 
        ///                - 保留最后一个 "清除聊天" 消息之后的所有消息。
        ///5. **输出：**
        ///    - 返回过滤后的消息列表 `messages`。
        ///**总结：**
        ///`MessageToKeep` 方法的主要功能是过滤群组聊天消息，保留以下两种情况下的消息：
        ///- 所有消息都在倒数第二个 "清除聊天" 消息之后。
        ///- 最后一个 "清除聊天" 消息之后至少还有两条消息。
        ///**注意：**
        ///- 代码中 `IsGroupChatClearMessage()` 方法的具体实现没有给出，它用于判断一条消息是否是 "清除聊天" 消息。
        ///- 该方法假设 "清除聊天" 消息的语义是清空之前的聊天记录。
        /// </summary>
        /// <param name="_"></param>
        /// <param name="messages"></param>
        /// <returns></returns>
        public static IEnumerable<IMessage> MessageToKeep(
            this IGroupChat _,
            IEnumerable<IMessage> messages)
        {
            var lastCLRMessageIndex = messages.ToList()
                    .FindLastIndex(x => x.IsGroupChatClearMessage());

            // if multiple clr messages, e.g [msg, clr, msg, clr, msg, clr, msg]
            // only keep the the messages after the second last clr message.
            if (messages.Count(m => m.IsGroupChatClearMessage()) > 1)
            {
                lastCLRMessageIndex = messages.ToList()
                    .FindLastIndex(lastCLRMessageIndex - 1, lastCLRMessageIndex - 1, x => x.IsGroupChatClearMessage());
                messages = messages.Skip(lastCLRMessageIndex);
            }

            lastCLRMessageIndex = messages.ToList()
                .FindLastIndex(x => x.IsGroupChatClearMessage());

            if (lastCLRMessageIndex != -1 && messages.Count() - lastCLRMessageIndex >= 2)
            {
                messages = messages.Skip(lastCLRMessageIndex);
            }

            return messages;
        }

        /// <summary>
        /// Return true if <see cref="IMessage"/> contains <see cref="TERMINATE"/>, otherwise false.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool IsGroupChatTerminateMessage(this IMessage message)
        {
            return message.GetContent()?.Contains(TERMINATE) ?? false;
        }

        public static bool IsGroupChatClearMessage(this IMessage message)
        {
            return message.GetContent()?.Contains(CLEAR_MESSAGES) ?? false;
        }

        public static IEnumerable<IMessage> ProcessConversationForAgent(
            this IGroupChat groupChat,
            IEnumerable<IMessage> initialMessages,
            IEnumerable<IMessage> messages)
        {
            messages = groupChat.MessageToKeep(messages);
            return initialMessages.Concat(messages);
        }

        /// <summary>
        /// 这段 C# 代码定义了一个名为 `ProcessConversationsForRolePlay` 的静态扩展方法，用于处理群组聊天消息，使其适用于角色扮演场景。
        ///**功能分解：**
        ///1. **输入：**
        ///    - `this IGroupChat groupChat`:  当前群组聊天对象。
        ///    - `IEnumerable<IMessage> initialMessages`:  初始消息列表。
        ///    - `IEnumerable<IMessage> messages`:  需要处理的消息列表。
        ///2. **消息过滤：**
        ///    - `messages = groupChat.MessageToKeep(messages);`  调用 `MessageToKeep` 方法过滤需要保留的消息。
        ///3. **消息合并：**
        ///    - `var messagesToKeep = initialMessages.Concat(messages);` 将初始消息和过滤后的消息合并成一个新的列表。
        ///4. **消息格式化：**
        ///    - 使用 `Select` 方法遍历所有消息，并对每条消息进行格式化：
        ///        -  `From {x.From
        ///    }:\n{x.GetContent()
        ///}\n < eof_msg >\nround #\n{i}` 
        ///            - 添加发送者信息(`From { x.From}:`)。
        ///            -添加消息内容(`{ x.GetContent()}`) 。
        ///            -添加 `< eof_msg >` 标记消息结束。
        ///            -  添加回合信息 (`round #\n{i}`) 。
        ///5. **创建新消息对象：**
        ///    -  `new TextMessage(Role.User, content: msg)`  使用格式化后的消息内容创建新的 `TextMessage` 对象，并将角色设置为 `User`。
        ///6. **输出：**
        ///    - 返回一个包含格式化后的 `TextMessage` 对象的 `IEnumerable<IMessage>` 列表。
        ///**总结：**
        ///`ProcessConversationsForRolePlay` 方法的主要功能是将群组聊天消息进行格式化处理，添加发送者、回合等信息，并将其封装成适用于角色扮演场景的 `TextMessage` 对象。 
        ///**注意：**
        ///-  代码中 `MessageToKeep` 方法的具体实现没有给出，它可能用于根据特定规则过滤消息。
        ///- `IGroupChat`, `IMessage`, `TextMessage`, `Role` 等类型需要根据具体的应用场景进行定义。
        /// </summary>
        /// <param name="groupChat"></param>
        /// <param name="initialMessages"></param>
        /// <param name="messages"></param>
        /// <returns></returns>
        internal static IEnumerable<IMessage> ProcessConversationsForRolePlay(
                this IGroupChat groupChat,
                IEnumerable<IMessage> initialMessages,
                IEnumerable<IMessage> messages)
        {
            messages = groupChat.MessageToKeep(messages);
            var messagesToKeep = initialMessages.Concat(messages);

            return messagesToKeep.Select((x, i) =>
            {
                var msg = @$"From {x.From}:
{x.GetContent()}
<eof_msg>
round # 
                {i}";

                return new TextMessage(Role.User, content: msg);
            });
        }
    }

}
