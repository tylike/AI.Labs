// --------------------------------------------------------------------------
//
// InputDialog_Custom.js ver1.03b
//
// Copyright (c) kotonoha*（https://aokikotori.com/）
// 本软件以 MIT 许可证发布。
// http://opensource.org/licenses/mit-license.php
//
// 2023/04/27 ver1.0 发布
// 2023/04/28 ver1.01 规格添加
//             - 删除多余的插件参数
//             - 启用方向键移动
//             - 在帮助中添加有关等待的描述
// 2023/04/29 ver1.02 规格添加
//             - 添加可指定最大字符数的功能
//             - 删除多余代码
// 2023/04/29 ver1.03 规格添加
//             - 添加可指定字体文件的功能
// 2023/05/01 ver1.03b 规格修正
//             - 为避免字体重复加载，已注释相关代码
// 2023/05/05 ver1.04 规格修正
//             - 改善窗口输出时的等待处理
//
// --------------------------------------------------------------------------
/*:
 * @target MZ
 * @plugindesc 在游戏中放置文本输入窗口的插件
 * 可创建自定义的输入窗口。
 *
 * @param fontFileName
 * @text 字体文件名
 * @desc 指定要使用的字体文件名。
 * 包括扩展名。
 * @type string
 * @default 
 * 
 * @param formWidth
 * @text 窗口宽度
 * @desc 指定窗口的宽度。
 * @default 400px
 * 
 * @param formHeight
 * @text 窗口高度
 * @desc 指定窗口的高度。
 * @default auto
 *
 * @param formBackgroundColor
 * @text 窗口背景色
 * @desc 指定窗口的背景色。
 * @default rgba(0, 0, 0, 0.5)
 *
 * @param formBorder
 * @text 窗口边框信息
 * @desc 指定窗口的边框信息。
 * @default 3px solid #fff
 *
 * @param formBorderRadius
 * @text 窗口边框半径
 * @desc 指定窗口的边框半径。
 * @default 5px
 *
 * @param formPadding
 * @text 窗口内边距
 * @desc 指定窗口的内边距。
 * @default 30px
 *
 * @param labelTextColor
 * @text 文本颜色
 * @desc 指定文本的颜色。
 * @default #FFFFFF
 * 
 * @param labelTextFontSize
 * @text 文本字体大小
 * @desc 指定文本的字体大小。
 * @default 18px
 *
 * @param labelTextMarginBottom
 * @text 文本底部边距
 * @desc 指定文本的底部边距。
 * @default 10px
 *
 * @param inputWidth
 * @text 文本框宽度
 * @desc 指定文本框的宽度。
 * @default 100%
 *
 * @param inputHeight
 * @text 文本框高度
 * @desc 指定文本框的高度。
 * @default 80px
 * 
 * @param inputFontSize
 * @text 文本框字体大小
 * @desc 指定文本框的字体大小。
 * @default 18px
 * 
 * @param inputColor
 * @text 文本框颜色
 * @desc 指定文本框的颜色。
 * @default #FFFFFF
 *  
 * @param inputBackgroundColor
 * @text 文本框背景色
 * @desc 指定文本框的背景色。
 * @default rgba(0, 0, 0, 0.5)
 * 
 * @param inputBorder
 * @text 文本框边框信息
 * @desc 指定文本框的边框信息。
 * @default 1px solid #fff
 * 
 * @param inputBorderRadius
 * @text 文本框边框半径
 * @desc 指定文本框的边框半径。
 * @default 5px
 *
 * @param inputPadding
 * @text 文本框内边距
 * @desc 指定文本框的内边距。
 * @default 5px
 * 
 * @param inputFontSize
 * @text 文本框字体大小
 * @desc 指定文本框的字体大小。
 * @default 18px
 *
 * @param buttonContainerWidth
 * @text 按钮容器宽度
 * @desc 指定按钮容器的宽度。
 * @default 100%
 *
* @param buttonContainerMargin
* @text 按钮容器边距
* @desc 指定按钮容器的边距。
* @default 15px 0 0 0
* 
* @param okButtonWidth
* @text OK按钮宽度
* @desc 指定OK按钮的宽度。
* @default 120px
* 
* @param okButtonHeight
* @text OK按钮高度
* @desc 指定OK按钮的高度。
* @default 40px
* 
* @param okButtonFontSize
* @text OK按钮字体大小
* @desc 指定OK按钮的字体大小。
* @default 18px
* 
* @param okButtonColor
* @text OK按钮文字颜色
* @desc 指定OK按钮的文字颜色。
* @default #FFFFFF
* 
* @param okButtonBackgroundColor
* @text OK按钮背景色
* @desc 指定OK按钮的背景色。
* @default rgba(0, 0, 0, 0.5)
* 
* @param okButtonBorder
* @text OK按钮边框信息
* @desc 指定OK按钮的边框信息。
* @default 1px solid #fff
*
* @param okButtonBorderRadius
* @text OK按钮边框半径
* @desc 指定OK按钮的边框半径。
* @default 5px
*
* @param okButtonPadding
* @text OK按钮内边距
* @desc 指定OK按钮的内边距。
* @default 5px 10px
* 
* @param cancelButtonWidth
* @text 取消按钮宽度
* @desc 指定取消按钮的宽度。
* @default 120px
* 
* @param cancelButtonHeight
* @text 取消按钮高度
* @desc 指定取消按钮的高度。
* @default 40px
*
* @param cancelButtonFontSize
* @text 取消按钮字体大小
* 
* @desc 指定取消按钮的字体大小。
* @default 18px
* 
* @param cancelButtonColor
* @text 取消按钮文字颜色
* @desc 指定取消按钮的文字颜色。
* @default #FFFFFF
* 
* @param cancelButtonBackgroundColor
* @text 取消按钮背景色
* @desc 指定取消按钮的背景色。
* @default rgba(0, 0, 0, 0.5)
*
* @param cancelButtonBorder
* @text 取消按钮边框信息
* @desc 指定取消按钮的边框信息。
* @default 1px solid #fff
*
* @param cancelButtonBorderRadius
* @text 取消按钮边框半径
* @desc 指定取消按钮的边框半径。
* @default 5px
*
* @param cancelButtonPadding
* @text 取消按钮内边距
* @desc 指定取消按钮的内边距。
* @default 5px 10px
*
* @param cancelButtonMarginLeft
* @text 取消按钮左侧边距
* @desc 指定取消按钮的左侧边距。
* @default 10px
*
* @command openDialog
* @text 显示文本输入
* @desc 显示文本输入窗口。
*
* @arg varId
* @text 变量ID
* @desc 存储输入文本的变量ID
* @type variable
* @default
*
* @arg defaultText
* @text 消息
* @desc 指定提示输入文本的消息。
* @default 请输入您的文本。
*
* @arg defaultValue
* @text 默认值
* @desc 指定输入栏显示的初始值。
* @default
*
* @arg okButtonLavel
* @text OK按钮文字
* @desc 指定OK按钮的文字。
* @default OK
* 
* @arg cancelButtonLavel
* @text 取消按钮文字
* @desc 指定取消按钮的文字。
* @default 取消
*
* @arg maxLength
* @text 最大字符数
* @desc 指定可输入的最大字符数。
* @default 64
* 
* @help
* 在插件命令中选择“InputDialog_Custom”，并设置变量ID、消息和默认值。
* 游戏屏幕上将显示文本输入窗口，并且可以使用键盘输入文本。
* 
* 使用键盘确认输入时，请按Shift键+Enter键而不是Enter键。
* 方便起见，可以在某处显示此说明。
* 输入的文本将存储在指定的变量ID中。
* 如果输入为空或取消，将存储0
* 
* 【　！注意！　】
* 如果在插件命令之后立即执行其他插件或脚本，有可能会同时执行。
* 如果您希望按顺序执行，请在此插件命令和下一个事件之间插入几帧的等待。
* 
*/

(() => {

    const pluginName = 'InputDialog_Custom';
    const parameters = PluginManager.parameters("InputDialog_Custom");
    const fontFileName = parameters['fontFileName'] || '';

    const stopPropagation = (event) => {
        event.stopPropagation();
    };

    const style = document.createElement('style');

    if (fontFileName && fontFileName.trim() !== '') {

        const _Scene_Boot_loadGameFonts = Scene_Boot.prototype.loadGameFonts;
        Scene_Boot.prototype.loadGameFonts = function () {
            _Scene_Boot_loadGameFonts.call(this);
            FontManager.load('customFont', fontFileName);
        };

        const font = new FontFace('customFont', 'url("./fonts/' + fontFileName + '")');
        document.fonts.add(font);
        //font.load().then(() => { // 当出现显示延迟时，取消这行代码的注释
        style.textContent = `form, input, button {font-family: 'customFont';}`;
        document.head.appendChild(style);
        //}); 如果您遇到显示上的延迟，请取消这里的注释


    }

    const _Game_Interpreter_updateWaitMode = Game_Interpreter.prototype.updateWaitMode;
    Game_Interpreter.prototype.updateWaitMode = function () {
        if (this._waitMode === "waitInputForm") {
            const form = document.getElementById("inputForm");
            if (!form) {
                this.setWaitMode("");
                return false;
            }
            return true;
        }
        return _Game_Interpreter_updateWaitMode.call(this);
    };

    PluginManager.registerCommand(pluginName, 'openDialog', function (args) {

        $gameMap._interpreter.setWaitMode('waitInputForm');

        const varId = Number(args.varId);
        const defaultText = args.defaultText;
        const defaultValue = args.defaultValue;
        const okButtonLavel = args.okButtonLavel;
        const cancelButtonLavel = args.cancelButtonLavel;
        const maxLength = args.maxLength;

        // 画布的布局
        const form = createHtmlForm(varId, defaultValue, defaultText, okButtonLavel, cancelButtonLavel, maxLength);

        // スマホ用おまじない // 手机用的魔法
        form.addEventListener('touchstart', stopPropagation);

        const canvas = document.getElementById("gameCanvas");
        const rect = canvas.getBoundingClientRect();
        form.style.left = "50%";
        form.style.top = "50%";
        form.style.transform = "translate(-50%, -50%)";

        document.body.appendChild(form);

        // 将焦点置于文本输入框
        form.elements["textInput"].focus();

        // 窗口大小调整时将窗口置于中央
        const onResize = () => {
            form.style.left = "50%";
            form.style.top = "50%";
            form.style.transform = "translate(-50%, -50%)";
        };

        window.addEventListener("resize", onResize);

        // 关闭窗口时的处理
        form.addEventListener("close", () => {
            document.body.removeChild(form);
            document.head.removeChild(style);
        });

    });

    // 创建画布
    function createHtmlForm(varId, defaultValue, defaultText, okButtonLavel, cancelButtonLavel, maxLength) {

        const form = document.createElement("form");
        const labelText = document.createElement("div");
        const input = document.createElement("input");
        const style = document.createElement("style");
        const buttonContainer = document.createElement("div");
        const okButton = document.createElement("button");
        const cancelButton = document.createElement("button");

        form.id = "inputForm";
        form.autocomplete = "off";
        input.id = "textInput";
        input.type = "text";
        input.inputmode = "text";
        input.maxLength = maxLength;
        okButton.type = "button";
        cancelButton.type = "button";

        // 创建表单的样式
        form.style.position = "absolute";
        form.style.width = parameters["formWidth"];
        form.style.height = parameters["formHeight"];
        form.style.backgroundColor = parameters["formBackgroundColor"];
        form.style.border = parameters["formBorder"];
        form.style.borderRadius = parameters["formBorderRadius"];
        form.style.padding = parameters["formPadding"];
        form.style.display = "flex";
        form.style.flexDirection = "column";
        form.style.justifyContent = "space-between";
        form.style.alignItems = "center";
        form.style.zIndex = "1000";
        form.style.boxSizing = "border-box";

        // 提示标签
        labelText.innerText = defaultText;
        labelText.style.color = parameters["labelTextColor"];
        labelText.style.fontSize = parameters["labelTextFontSize"];
        labelText.style.marginBottom = parameters["labelTextMarginBottom"];

        // 创建文本输入框的样式
        input.value = defaultValue;
        input.style.width = parameters["inputWidth"];
        input.style.color = parameters["inputColor"];
        input.style.backgroundColor = parameters["inputBackgroundColor"];
        input.style.height = parameters["inputHeight"];
        input.style.border = parameters["inputBorder"];
        input.style.fontSize = parameters["inputFontSize"];
        input.style.padding = parameters["inputPadding"];
        input.style.borderRadius = parameters["inputBorderRadius"];
        input.style.boxSizing = "border-box";

        // 创建按钮边角的样式
        buttonContainer.style.display = "flex";
        buttonContainer.style.justifyContent = "space-around";
        buttonContainer.style.width = parameters["buttonContainerWidth"];
        buttonContainer.style.margin = parameters["buttonContainerMargin"];

        // 创建确定按钮的样式
        okButton.innerText = okButtonLavel;
        okButton.style.backgroundColor = parameters["okButtonBackgroundColor"];
        okButton.style.color = parameters["okButtonColor"];
        okButton.style.width = parameters["okButtonWidth"];
        okButton.style.height = parameters["okButtonHeight"]
        okButton.style.border = parameters["okButtonBorder"];
        okButton.style.fontSize = parameters["okButtonFontSize"];
        okButton.style.borderRadius = parameters["okButtonBorderRadius"];
        okButton.style.padding = parameters["okButtonPadding"];
        okButton.style.cursor = parameters["cursorStyle"];
        okButton.style.cursor = "pointer";

        // 创建取消按钮的样式
        cancelButton.innerText = cancelButtonLavel;
        cancelButton.style.backgroundColor = parameters["cancelButtonBackgroundColor"];
        cancelButton.style.color = parameters["cancelButtonColor"];
        cancelButton.style.width = parameters["cancelButtonWidth"];
        cancelButton.style.height = parameters["cancelButtonHeight"]
        cancelButton.style.border = parameters["cancelButtonBorder"];
        cancelButton.style.fontSize = parameters["cancelButtonFontSize"];
        cancelButton.style.borderRadius = parameters["cancelButtonBorderRadius"];
        cancelButton.style.padding = parameters["cancelButtonPadding"];
        cancelButton.style.marginLeft = parameters["cancelButtonMarginLeft"];
        cancelButton.style.cursor = "pointer";

        // 输入栏的焦点
        style.innerHTML = `input:focus {outline: 0px solid #fff !important;}`;
        document.head.appendChild(style);

        // 取消发送处理
        form.addEventListener("contextmenu", (event) => {
            event.preventDefault();
        });
        form.addEventListener("submit", (event) => {
            event.preventDefault();
        });

        // 检查是否正在转换中
        let isComposing = false;
        input.addEventListener("compositionstart", () => {
            isComposing = true;
        });
        input.addEventListener("compositionend", () => {
            isComposing = false;
        });

        // 键盘输入时的处理
        input.addEventListener("keydown", (event) => {

            // 启用Backspace键的功能
            if (event.key === "Backspace" && !isComposing) {
                event.preventDefault();
                const startPos = input.selectionStart;
                const endPos = input.selectionEnd;
                if (startPos !== null && endPos !== null && startPos !== endPos) {
                    const value = input.value;
                    input.value = value.slice(0, startPos) + value.slice(endPos);
                    input.selectionStart = input.selectionEnd = startPos;
                } else if (startPos !== null && startPos > 0) {
                    const value = input.value;
                    input.value = value.slice(0, startPos - 1) + value.slice(startPos);
                    input.selectionStart = input.selectionEnd = startPos - 1;
                }
            } else if (event.key === "ArrowRight" || event.key === "ArrowLeft") {
                // 启用光标键的功能
                event.preventDefault();
                const direction = event.key === "ArrowRight" ? 1 : -1;
                const startPos = input.selectionStart;
                const endPos = input.selectionEnd;
                if (startPos !== null && endPos !== null) {
                    const newPosition = Math.max(Math.min(startPos + direction, input.value.length), 0);
                    input.selectionStart = input.selectionEnd = newPosition;
                }
            } else if (event.key === "Enter") {
                if (event.shiftKey) {
                    // 使用Enter+Shift进行确认操作
                    event.preventDefault();
                    form.dispatchEvent(new Event("submit"));
                    okButton.onclick();
                } else {
                    // 如果仅按下Enter键，取消事件
                    event.preventDefault();
                }
            }
        });

        // 禁用BackSpace键
        const _Input_onKeyDown = Input._onKeyDown;
        Input._onKeyDown = function (event) {
            if (event.key === "Backspace") {
                return;
            }
            _Input_onKeyDown.call(this, event);
        };

        // 确定按钮的处理

        okButton.onclick = async () => {
            const inputValue = document.getElementById("textInput").value;
            $gameVariables.setValue(varId, inputValue ? inputValue : '');
            removeHtmlForm();
        };

        // 取消按钮的处理
        cancelButton.onclick = () => {
            removeHtmlForm();
        };

        buttonContainer.appendChild(okButton);
        buttonContainer.appendChild(cancelButton);
        form.appendChild(labelText);
        form.appendChild(input);
        form.appendChild(buttonContainer);
        return form;

    }

    // 移除输入表单
    function removeHtmlForm() {
        const form = document.getElementById("inputForm");
        if (form) {
            form.removeEventListener('touchstart', stopPropagation);
            document.body.removeChild(form);
            $gameMap._interpreter.setWaitMode("");
        }
    }

})();