// --------------------------------------------------------------------------------------
//
// ChatGPT_APIMZ.js v1.2
//
// 版权所有 (c) kotonoha*（https://aokikotori.com/）
// 本软件在MIT许可下发布。
// 许可证详情请见：http://opensource.org/licenses/mit-license.php
//
// 2023/04/13 ver1.0β 插件公开
// 2023/04/28 ver1.07 功能添加、修正
//				－添加了支持问题和支持回答。
//				－将助手角色的生成调整到回答时进行。
//				－当窗口的滚动条在顶部时，Enter键可以起作用。
//				－可以通过光标键操作窗口的滚动条。
// 2023/04/29 ver1.08 功能添加、修正
//				－除了memory_talk的回答之外，也反映支持问题和支持回答。
//				－添加并修正了帮助内容。
// 2023/05/01 ver1.09 功能修正
//				－提前了消息窗口的显示时机。
//				－修复了从保存数据重新开始时，memory_talk的第一次对话无法响应的故障。
// 2023/05/05 ver1.10 功能修正
//				－修复了当超过memory_talk的设置次数时，system角色被删除的故障。
// 2023/05/06 ver1.11 功能修正
//				－修复了不使用memory_talk的回答无法进入变量的故障。
//				－调整了等待回答的处理。
// 2023/05/09 ver1.2 功能添加
//				－可以在插件参数中设置自定义字体。
//				－可以在插件命令中设置角色名、脸图以及索引。
//
// --------------------------------------------------------------------------------------
/*:
 * @target MZ
 * @plugindesc 与ChatGPT API通信，让AI为角色生成对话的插件
 * @author kotonoha*
 * @url https://github.com/kotonoha0109/kotonoha_tkoolMZ_Plugins/blob/main/plugins/ChatGPT_APIMZ.js
 *
 * @param ChatGPT_Model
 * @type string
 * @default gpt-3.5-turbo
 * @desc ChatGPT的AI模型
 *
 * @param ChatGPT_URL
 * @type string
 * @default https://api.openai.com/v1/chat/completions
 * @desc ChatGPT的URL
 * 如果使用服务器端，则为该文件的URL
 *
 * @param ChatGPT_APIkey
 * @type string
 * @default sk-
 * @desc ChatGPT的API密钥（纯数字为变量ID，字符串则为API密钥）
 * ※可以将API密钥存储在变量中。
 *
 * @param UserMessageVarId
 * @type variable
 * @default 1
 * @desc 存储玩家问题的变量ID
 *
 * @param AnswerMessageVarId
 * @type variable
 * @default 2
 * @desc 存储AI答案的变量ID
 *
 * @param MemoryMessageVarId
 * @type variable
 * @default 3
 * @desc 存储对话历史的变量ID
 *
 * @param VisibleSwitchID
 * @type switch
 * @default 
 * @desc 隐藏答案的开关ID
 * 当只想记录答案而不显示时使用。
 *
 * @param BrStr
 * @type boolean
 * @default true
 * @desc 自动换行
 * 如果回答中包含换行码，则执行换行处理。
 *
 * @param ReplaceStr
 * @type string
 * @default 
 * @desc 过滤字符
 * 逐个字符判断。例如写入"\"会隐藏引号。
 *
 * @param SystemMessage
 * @type multiline_string
 * @default 请用日语回答。
 * @desc 对AI的通用指令（如“用日语书写”或“简洁地总结在120字内”）
 *
 * @param FontFileName
 * @desc 指定使用的字体文件名。
 * 包括扩展名。
 * @type string
 * @default 
 * 
 * @command chat
 * @text 发送聊天消息
 * @desc 向API发送查询的命令
 *
 * @arg system
 * @type multiline_string
 * @default 
 * @desc 对该事件的指示
 *
 * @arg message
 * @type multiline_string
 * @default 
 * @desc 该事件的问题内容
 * 当CustomQuestionMessageVarId为0或变量为空时，使用此问题。
 *
 * @arg message_before
 * @type multiline_string
 * @default 
 * @desc 在问题之前添加的内容
 * 用于补充说明。
 * 
 * @arg message_after
 * @type multiline_string
 * @default
 * @desc 在问题之后添加的内容
 * 用于补充说明。
 * 
 * @arg displayHeader
 * @type string
 * @default
 * @desc 在回答前显示的内容
 * 写入"userMessage"会替换为问题(message)内容。
 * 
 * @arg temperature
 * @type Number
 * @default 1
 * @desc 采样温度（0～1）
 * 值越低相关性越高，越高则产生更多样化的词汇
 *
 * @arg top_p
 * @type Number
 * @default 0.9
 * @desc 文本多样性（0～1）
 * 值越低一致性越高，越高则文本越多样
 *
 * @arg max_tokens
 * @type Number
 * @default 512
 * @desc AI回答的最大令牌数（gpt-3.5-turbo最高4096）
 * 日语每字符约等于2～3个令牌
 *
 * @arg memory_talk
 * @type Number
 * @default 10
 * @desc 对话历史保存量
 * 记忆的对话轮数（一次提问+回答计为1）
 *
 * @arg CuatomQuestionMessageVarId
 * @type variable
 * @default
 * @desc 用于存储对该事件问题的变量ID
 * 空则使用插件参数设置。
 *
 * @arg CustomAnswerMessageVarId
 * @type variable
 * @default
 * @desc 用于存储该事件答案的变量ID
 * 空则使用插件参数设置。
 *
 * @arg CustomMemoryMessageVarId
 * @type variable
 * @default
 * @desc 用于存储该事件对话历史的变量ID
 *
 * @arg support_message
 * @type multiline_string
 * @default
 * @desc 支持问题
 * 创建对事件问题的示例。
 * 
 * @arg support_answer
 * @type multiline_string
 * @default
 * @desc 支持答案
 * 对支持问题的示例回答。
 * 
 * @arg characterName
 * @type string
 * @default
 * @desc 角色名称
 * 在消息窗口上显示。
 * 
 * @arg faceImage
 * @type file
 * @default
 * @desc 角色面部图形
 * 不显示时留空。
 * @dir img/faces/
 * 
 * @arg faceIndex
 * @type number
 * @default
 * @desc 面部图形索引
 * 按照MZ的规则，左上为0～3，右下为4～7。
 * 
 * @help 该插件通过与ChatGPT API通信，让AI为游戏中的角色生成对话。
 * 需要自行获取并设置专属的API密钥。
 *
 * 【注意事项】
 * 请确保使用游戏玩家自己的API密钥！
 * 直接在作品中公开API密钥会导致密钥泄露！
 * 因API密钥泄露或使用费用产生的任何问题，责任自负！
 * 
 * 【基本使用方法】
 * (1) 将从OpenAI获取的API密钥设置到ChatGPT_APIkey中。
 *
 * (2) 至少需要3个空闲变量。
 *   - 临时存放玩家问题的变量ID，
 *     请设置到UserMessageVarId。
 *   - 临时存放AI答案的变量ID，
 *     请设置到AnswerMessageVarId。
 *   - 临时存放对话历史的变量ID，
 *     请设置到MemoryMessageVarId。
 *
 * (3) 在想要AI生成台词的事件中，使用插件命令选择
 * "ChatGPT_APIMZ"并进行角色设置。
 * 
 * # system
 * 对事件的具体指示，作为插件参数SystemMessage的补充。
 *
 * 例如，如果参数已设为“用日语回答”，则在此可添加如“但请使用敬语”的补充说明。
 * 
 *
 * # message
 * 询问AI的问题内容。
 * 如果使用CuatomQuestionMessageVarId存储问题，则此栏应为空。
 * 
 * 
 * # message_before, message_after
 * 当使用变量输入问题时，message会被变量值取代。
 * system中的内容在GPT-3模型中可能被忽略，因此，
 * 如果AI未按预期回答，可尝试在这两个字段中加入前后文。
 * 在问题前后添加的文字内容，AI将据此生成回答。
 * 例如，若变量值为“你好”，message_before设为“请问”，
 * message_after设为“吗？”，那么AI面对的问题将是“请问你好吗？”。
 * 
 * 
 * # displayHeader
 * 消息窗口中显示的标题。
 * 要显示变量ID1的值，输入\V[1]即可。
 * 或者，输入"userMessage"将仅显示去除message_before和message_after后的质问。
 * 
 * # temperature, top_p
 * 控制AI回答多样性的数值。
 * 请设置0到1之间的数值。
 * 
 * # max_tokens
 * 设置最大令牌数（日语每字符大约2～3令牌）。
 * 设定文字长度上限，但如果响应字符数超过最大令牌数，则回答会被截断。
 *
 * # memory_talk
 * 对话历史保存的数量。
 * 设置的数值代表保存的交互轮数。
 * 数值越大，越能进行连贯对话，但也会增加API调用成本。
 * 无需保存历史时，设置为0。
 *
 * # CuatomQuestionMessageVarId
 * 存储事件问题的变量ID。
 * 如果问题已通过如姓名输入窗口或聊天窗口输入并存储在变量中，请指定该变量ID。
 * ※当同时设置此变量和message时，message优先。
 * ※这与插件参数UserMessageVarId不同。
 *
 * # CustomAnswerMessageVarId
 * 存储该事件答案的变量ID。
 * 答案会保存至插件参数AnswerMessageVarId中，但若希望为每个事件单独记录答案，
 * 则指定此变量ID。
 *
 * # CustomMemoryMessageVarId
 * 存储该事件对话历史的变量ID。
 * 作为API通信所需的数组被记录，无法直接调用。
 * 若要手动清除历史，请清空此变量ID对应的变量。
 * 
 * # support_message, support_answer
 * 创建对话示例。
 * AI在回答时会参考这些示例。
 * 如：support_message设为“自我介绍”，
 * support_answer设为“我是暹罗猫！五岁喵~”。
 * 这样设置后，后续对话中AI更倾向于以第一人称“我”和结尾词“喵~”来回答。
 * 
 * # characterName, faceName, faceIndex
 * 设置角色名、面部图形文件名及其显示索引。
 * 面部图形文件位于img/faces/目录中，输入文件名。
 * 不显示面部图形则留空。
 * 索引从左到右，第一行0～3，第二行4～7。
 * 
 * 【关于网页版的运行】
 * 本插件生成的消息窗口使用了HTML。
 * 在网页浏览器中游玩时，消息窗口可能会超出游戏区域显示。
 * 此时，请另外准备一个包含了iframe并加载了由Maker生成的index.html的HTML文件。
 * 
 * 【消息窗口的自定义】
 * 若要自定义消息窗口的宽度、高度、位置、背景色等，请修改
 * function createStreamingTextElement() 的内容。
 * 请使用窗口调整工具。
 * ▼窗口调整工具
 * https://aokikotori.com/chatgpt_apimz_window/
 * 
 * 【与服务器端的联动】
 * 可以在服务器上放置PHP、Python等文件，并将API密钥等请求头信息保密。
 * ▼PHP示例见：
 * https://github.com/kotonoha0109/kotonoha_tkoolMZ_Plugins/blob/main/plugins/php/request.php
 * 
 * 将API密钥配置于PHP文件中，上传至服务器后，将
 * 插件参数中的ChatGPT_URL设置为PHP文件的URL。
 * 插件参数中的ChatGPT_APIkey不再需要，请务必删除。
 *
 */
(() => {

    const pluginParameters = PluginManager.parameters('ChatGPT_APIMZ');
    const userMessageVarId = Number(pluginParameters['UserMessageVarId']) || 1;
    const answerMessageVarId = Number(pluginParameters['AnswerMessageVarId']) || 2;
    const memoryMessageVarId = Number(pluginParameters['MemoryMessageVarId']) || 3;
    const visibleSwitchID = Number(pluginParameters['VisibleSwitchID']) || null;
    const replacestr = String(pluginParameters['ReplaceStr']) || "";
    const brstr = pluginParameters['BrStr'] === 'true' || pluginParameters['BrStr'] === true;
    const systemMessage = String(pluginParameters['SystemMessage']) || "Please answer in Japanese.";
    const fontFileName = pluginParameters['FontFileName'] || '';

    let previousMessage = null;
    let isDoneReceived = false;
    let isFontLoaded = false;

    // 自定义字体的设置
    if (fontFileName && fontFileName.trim() !== '') {
        const _Scene_Boot_loadGameFonts = Scene_Boot.prototype.loadGameFonts;
        Scene_Boot.prototype.loadGameFonts = function () {
            _Scene_Boot_loadGameFonts.call(this);
            FontManager.load('customFont', fontFileName);
        };
        const font = new FontFace('customFont', 'url("./fonts/' + fontFileName + '")');
        document.fonts.add(font);
        font.load().then(() => {
            addCustomFontStyle();
        }).catch((error) => {
            console.error('フォントをロード出来ません：', error);
        });
    }

    PluginManager.registerCommand("ChatGPT_APIMZ", "chat", async (args) => {

        // 初始化窗口
        updateStreamingTextElement();
        isDoneReceived = false;

        const temperature = Number(args.temperature) || 1;
        const top_p = Number(args.top_p) || 0.9;
        const max_tokens = Number(args.max_tokens) || 512;
        const customQuestionMessageVarId = Number(args.CuatomQuestionMessageVarId) || null;
        const customAnswerMessageVarId = Number(args.CustomAnswerMessageVarId) || null;

        let targetVarId = customQuestionMessageVarId !== null ? customQuestionMessageVarId : 0;
        let variableValue = $gameVariables.value(targetVarId);
        let userMessage;
        let displayHeader;
        let support_message;
        let support_answer;
        let faceImage = args.faceImage !== undefined ? String(args.faceImage) : null;
        let faceIndex = Number(args.faceIndex) || 0;
        let characterName = String(args.characterName) || '';

        // 如果变量ID未定义，则将message反映到问题中
        if (targetVarId !== 0 && !variableValue) {
            if (!args.message || args.message === '') { return; }
            if (!args.message_before) { args.message_before = ''; }
            if (!args.message_after) { args.message_after = ''; }
            userMessage = args.message_before + args.message + args.message_after;
            userMessage_input = args.message;
        } else if (targetVarId === 0 && (!args.message || args.message === '')) {
            // 如果变量和message都是空的，则退出处理
            return;
        } else {
            // 否则，将变量customQuestionMessageVarId反映到问题中

            if (!args.message_before) { args.message_before = ''; }
            if (!args.message_after) { args.message_after = ''; }
            userMessage = variableValue ? args.message_before + variableValue + args.message_after : args.message_before + args.message + args.message_after;
            userMessage_input = variableValue ? variableValue : args.message;
        }

        // 处理控制字符
        userMessage = processControlCharacters(userMessage);
        $gameVariables.setValue(targetVarId, userMessage);

        if (userMessageVarId !== null) {
            $gameVariables.setValue(userMessageVarId, userMessage);
        }

        const customMemoryMessageVarId = Number(args.CustomMemoryMessageVarId) || memoryMessageVarId;
        let customMemoryMessage = $gameVariables.value(customMemoryMessageVarId);

        // 不进行记忆相关的处理
        if (Number(args.CustomMemoryMessageVarId) === 0 || !args.memory_talk) {
            $gameVariables.setValue(memoryMessageVarId, []);
            previousMessage = "";
            customMemoryMessage = [];
            customMemoryMessage.push({ role: 'system', content: processControlCharacters(systemMessage) });
            // 添加命令侧的system角色
            if (args.system) {
                customMemoryMessage.push({ role: 'system', content: (processControlCharacters(args.system) || "") });
            }
            // 推送支持问题和支持答案
            if (args.support_message && args.support_answer) {
                customMemoryMessage.push({ role: 'user', content: (processControlCharacters(args.support_message) || "") });
                customMemoryMessage.push({ role: 'assistant', content: (processControlCharacters(args.support_answer) || "") });
            }
            customMemoryMessage.push({ role: 'user', content: userMessage });
            $gameVariables.setValue(memoryMessageVarId, customMemoryMessage);

        } else {
            customMemoryMessage = $gameVariables.value(customMemoryMessageVarId);

            if (!Array.isArray(customMemoryMessage)) {
                customMemoryMessage = [];
                previousMessage = "";
                customMemoryMessage.push({ role: 'system', content: processControlCharacters(systemMessage) });
                // 添加命令侧的system角色

                if (args.system) {
                    customMemoryMessage.push({ role: 'system', content: (processControlCharacters(args.system) || "") });
                }
                // 推送支持问题和支持答案

                if (args.support_message && args.support_answer) {
                    customMemoryMessage.push({ role: 'user', content: (processControlCharacters(args.support_message) || "") });
                    customMemoryMessage.push({ role: 'assistant', content: (processControlCharacters(args.support_answer) || "") });
                }
                customMemoryMessage.push({ role: 'user', content: userMessage });

            } else {

                // 执行记忆对话的处理

                const memoryTalk = Number(args.memory_talk) * 2 || 1;
                customMemoryMessage.push({ role: 'user', content: userMessage });

                while (true) {
                    let userCount = customMemoryMessage.filter(item => item.role === 'user').length;
                    let assistantCount = customMemoryMessage.filter(item => item.role === 'assistant').length;

                    if (userCount + assistantCount > memoryTalk) {
                        let userIndex = customMemoryMessage.findIndex(item => item.role === 'user');
                        let assistantIndex = customMemoryMessage.findIndex(item => item.role === 'assistant');

                        if (userIndex >= 0 && assistantIndex >= 0) {
                            customMemoryMessage.splice(Math.min(userIndex, assistantIndex), 2);
                        } else {
                            break;
                        }
                    } else {
                        break;
                    }
                }

            }
            $gameVariables.setValue(customMemoryMessageVarId, customMemoryMessage);
        }

        const streamingTextElement = document.getElementById('streamingText');
        addCustomFontStyle();
        if ($gameSwitches.value(visibleSwitchID) !== true) {
            streamingTextElement.style.display = 'block';
        }

        streamingTextElement.innerHTML = '';
        //console.log(customMemoryMessage);

        (async () => {

            const ChatGPT_Model = String(pluginParameters['ChatGPT_Model']) || 'gpt-3.5-turbo';
            const ChatGPT_URL = String(pluginParameters['ChatGPT_URL']) || 'https://api.openai.com/v1/chat/completions';

            // 当非输出开关为OFF时，事件将停止
            if ($gameSwitches.value(visibleSwitchID) !== true) {
                $gameMap._interpreter.setWaitMode('wait_ChatGPT');
                // 在流媒体播放中停止事件的动作
                const event = $gameMap.event($gameMap._interpreter.eventId());
                currentEvent = event;
                event.setDirectionFix(true);
                event._originalMoveType = event._moveType;
                event._moveType = 0;
            }

            // 与ChatGPT API的通信
            const url = ChatGPT_URL;

            try {

                const response = await fetch(url, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'Authorization': 'Bearer ' + getChatGPT_APIkey(),
                    },
                    body: JSON.stringify({
                        model: ChatGPT_Model,
                        temperature: temperature,
                        top_p: top_p,
                        max_tokens: max_tokens,
                        stream: true,
                        messages: customMemoryMessage,
                    }),
                });
                //console.log(customMemoryMessage);

                if (!response.ok) {
                    const errorText = await response.text();
                    const errorJson = JSON.parse(errorText);
                    let errorMessage = String(errorJson.error.message).slice(0, 30);
                    // 输出API的消息
                    console.error('Error:', errorMessage);
                    $gameMessage.add(errorMessage);
                    isDoneReceived = true;
                    unlockControlsIfNeeded();
                    return;
                }

                // 执行事件
                const reader = response.body.getReader();
                const textDecoder = new TextDecoder();
                let buffer = '';
                let streamBuffer = '';
                let textArray = [];

                if (!args.displayHeader) args.displayHeader = "";
                let preMessage = processControlCharacters(args.displayHeader);
                preMessage = preMessage.replace(/userMessage/g, userMessage_input);

                // 设置脸图
                if (faceImage !== null && faceImage !== "") {
                    if (!faceIndex) { faceIndex = 0; }
                    const faceWidth = 144;
                    const faceHeight = 144;
                    const facesPerRow = 4;
                    const facesPerCol = 2;
                    const faceX = faceWidth * (faceIndex % facesPerRow);
                    const faceY = faceHeight * Math.floor(faceIndex / facesPerRow);
                    const faceImageUrl = '<img src="img/faces/' + faceImage + '.png" style="object-fit: none; object-position: -' + faceX + 'px -' + faceY + 'px; width: ' + faceWidth + 'px; height: ' + faceHeight + 'px; float: left; margin-right: 20px;">';
                    textArray = [preMessage, faceImageUrl];
                } else {
                    textArray = [preMessage];
                }

                // 设置角色名
                if (args.characterName) {
                    textArray.push(processControlCharacters(args.characterName) + "<br>");
                }

                // 与ChatGPT的通信
                while (true) {

                    const { value, done } = await reader.read();
                    if (done) { break; }
                    buffer += textDecoder.decode(value, { stream: true });

                    let newlineIndex;

                    do {

                        newlineIndex = buffer.indexOf('\n');
                        if (newlineIndex === -1) { break; }
                        const line = buffer.slice(0, newlineIndex);
                        buffer = buffer.slice(newlineIndex + 1);

                        if (line.startsWith('data:')) {

                            // 当达到流媒体文本的末端时，重新开始事件
                            if (line.includes('[DONE]')) {
                                previousMessage = streamBuffer;
                                // 将回答赋值给变量ID
                                let targetAnswerVarId = customAnswerMessageVarId !== null ? customAnswerMessageVarId : answerMessageVarId;
                                // 将回答赋值给assistant角色
                                customMemoryMessage.push({ role: 'assistant', content: previousMessage });
                                $gameVariables.setValue(targetAnswerVarId, previousMessage);
                                // 重新开始事件
                                isDoneReceived = true;
                                return;
                            }

                            const jsonData = JSON.parse(line.slice(5));

                            // 显示流媒体文本
                            if (jsonData.choices && jsonData.choices[0].delta && jsonData.choices[0].delta.content) {

                                let assistantMessage = jsonData.choices[0].delta.content;

                                // 另外保存为流缓冲区，专用于assistant角色
                                streamBuffer += assistantMessage;

                                // 将换行转换为<br>
                                if (brstr === true) { assistantMessage = assistantMessage.replace(/\n/g, "<br>"); }
                                assistantMessage = removeChars(assistantMessage, replacestr);

                                // 输出
                                textArray.push(assistantMessage);
                                const combinedText = textArray.join('');
                                const processedText = processControlCharacters(combinedText);
                                streamingTextElement.innerHTML = processedText;
                                //console.log(textArray);

                                // 根据输出进行滚动
                                setTimeout(() => {
                                    streamingTextElement.scrollTop = streamingTextElement.scrollHeight;
                                }, 0);

                            }
                        }
                    } while (newlineIndex !== -1);
                }

            } catch (error) {
                console.error('Error:', error);
                let errorMessage = error;
                $gameMessage.add(errorMessage);
                isDoneReceived = true;
                unlockControlsIfNeeded();
                return;
            }

        })();

    });

    // 赋值API密钥
    function getChatGPT_APIkey() {
        const APIKey = String(pluginParameters['ChatGPT_APIkey']) || 'sk-';
        const apiKeyVarId = parseInt(APIKey, 10);
        if (Number.isInteger(apiKeyVarId) && $gameVariables && $gameVariables.value(apiKeyVarId)) {
            return $gameVariables.value(apiKeyVarId);
        } else {
            return APIKey;
        }
    }

    // 去除NG文字
    const removeChars = (str, chars) => {
        const escapeRegExp = (str) => {
            return str.replace(/[.*+\-?^${}()|[\]\\]/g, '\\$&');
        };
        const escapedChars = escapeRegExp(chars);
        const regex = new RegExp(`[${escapedChars}]`, 'g');
        return str.replace(regex, '');
    }

    // 处理控制字符
    function processControlCharacters(str) {
        return str.replace(/\\([VNPI])\[(\d+)\]|\\G/g, function (matchedString, type, id) {
            if (matchedString === '\\G') {
                return TextManager.currencyUnit;
            }
            const numId = Number(id);
            switch (type) {
                case 'V':
                    return String($gameVariables.value(numId));
                case 'N':
                    return String($gameActors.actor(numId).name());
                case 'P':
                    return String($gameParty.members()[numId - 1].name());
                default:
                    return '';
            }
        });
    }

    // 自定义字体的设置
    function addCustomFontStyle() {
        if (!isFontLoaded) {
            const style = document.createElement('style');
            style.textContent = `#streamingText {font-family: 'customFont';}`;
            document.head.appendChild(style);
            isFontLoaded = true;
        }
    }

    // 实现等待模式
    const _Game_Interpreter_updateWaitMode = Game_Interpreter.prototype.updateWaitMode;
    Game_Interpreter.prototype.updateWaitMode = function () {
        if (this._waitMode === "waitChatGPT") {
            const streamingTextElement = document.getElementById("streamingText");
            if (!streamingTextElement) {
                this.setWaitMode("");
                return false;
            }
            return true;
        }
        return _Game_Interpreter_updateWaitMode.call(this);
    };
    // 控制窗口的滚动和开闭
    const scrollSpeed = 30; // 用于调整滚动速度的常数
    const _Scene_Map_update = Scene_Map.prototype.update;
    Scene_Map.prototype.update = function () {
        _Scene_Map_update.call(this);
        if (streamingTextElement && streamingTextElement.style.display !== "none") {
            if (Input.isPressed("up")) {
                streamingTextElement.scrollTop -= scrollSpeed;
            } else if (Input.isPressed("down")) {
                streamingTextElement.scrollTop += scrollSpeed;
            }
            if ((Input.isTriggered("ok") || Input.isTriggered("cancel") || TouchInput.isCancelled()) && isScrollAtEnd(streamingTextElement)) {
                unlockControlsIfNeeded();
            } else {
                if (Input.isTriggered("ok") || Input.isTriggered("cancel") || TouchInput.isCancelled()) {
                    streamingTextElement.scrollTop = streamingTextElement.scrollHeight;
                }
            }
        }
    };

    const _Game_Map_isEventRunning = Game_Map.prototype.isEventRunning;
    Game_Map.prototype.isEventRunning = function () {
        const isElementVisible = streamingTextElement && streamingTextElement.style.display !== "none";
        return _Game_Map_isEventRunning.call(this) || isElementVisible;
    };

    function isScrollAtEnd(element) {
        return element.scrollTop + element.clientHeight >= element.scrollHeight;
    }

    // 事件重新开始处理
    function unlockControlsIfNeeded() {
        if (isDoneReceived && streamingTextElement.scrollHeight - streamingTextElement.clientHeight <= streamingTextElement.scrollTop + 1) {
            streamingTextElement.style.display = 'none';
            streamingTextElement.innerHTML = '';
            if (typeof currentEvent !== 'undefined' && currentEvent) {
                currentEvent.setDirectionFix(false);
                currentEvent._moveType = currentEvent._originalMoveType;
                currentEvent = null;
            }
            $gameMap._interpreter.setWaitMode('');
        }
    }

    // 生成流媒体窗口
    // 当需要自定义窗口时，请更改此函数
    function createStreamingTextElement() {
        const windowHeight = window.innerHeight;
        const streamingTextHeight = 200;
        streamingTextElement = document.createElement('div');
        streamingTextElement.id = 'streamingText';
        streamingTextElement.style.display = 'none';
        streamingTextElement.style.position = 'fixed';
        streamingTextElement.style.zIndex = 100;
        streamingTextElement.style.left = '0';
        streamingTextElement.style.width = '800px';
        streamingTextElement.style.top = `${windowHeight - streamingTextHeight - 16}px`;
        streamingTextElement.style.boxSizing = 'border-box';
        streamingTextElement.style.height = '200px';
        streamingTextElement.style.color = 'white';
        streamingTextElement.style.fontSize = '22px';
        streamingTextElement.style.padding = '16px';
        streamingTextElement.style.background = 'linear-gradient(to bottom, rgba(15,28,69,0.8), rgba(8,59,112,0.8))';
        streamingTextElement.style.margin = '0 8px';
        streamingTextElement.style.borderWidth = '2px';
        streamingTextElement.style.borderStyle = 'solid';
        streamingTextElement.style.borderColor = 'white';
        streamingTextElement.style.borderRadius = '5px';
        streamingTextElement.style.overflowY = 'auto';
        document.body.appendChild(streamingTextElement);
    }
    createStreamingTextElement();

    // 调整屏幕尺寸变化时的消息窗口
    function updateStreamingTextElement() {

        // 获取当前的 Tsukuru（可能是指游戏制作软件如RPG Maker）画面尺寸
        //和浏览器屏幕尺寸
        const canvasWidth = Graphics.width;
        const canvasHeight = Graphics.height;
        const windowWidth = window.innerWidth;
        const windowHeight = window.innerHeight;
        const scaleX = windowWidth / canvasWidth;
        const scaleY = windowHeight / canvasHeight;
        const scale = Math.min(scaleX, scaleY);
        const adjustedWidth = canvasWidth * scale;
        const adjustedHeight = canvasHeight * scale;

        // 根据屏幕尺寸调整消息窗口的宽度和高度
        let streamingTextHeight = Math.min(200 * scale, 250);
        streamingTextElement.style.width = `${adjustedWidth - 16}px`;
        streamingTextElement.style.height = `${streamingTextHeight}px`;

        // 根据屏幕尺寸调整字体大小

        let limitedFontSize = Math.min(Math.max(22 * scale, 16), 28);
        streamingTextElement.style.fontSize = `${limitedFontSize}px`;

        // 根据屏幕尺寸调整消息窗口的位置
        const topPosition = (windowHeight - adjustedHeight) / 2 + adjustedHeight - streamingTextHeight - 16 * scaleY;
        streamingTextElement.style.top = `${topPosition}px`;
        streamingTextElement.style.left = `${(windowWidth - adjustedWidth) / 2}px`;

    }

    // 检查调整大小
    window.addEventListener('resize', () => {
        updateStreamingTextElement();
    });

})();