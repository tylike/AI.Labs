
// 使用C#语法重构给定的JavaScript代码

using System;
using System.Collections.Generic;

// 假设InputManager类已存在，用于处理输入数据
public partial class InputManager
{
    // 静态类，用于键盘和游戏手柄输入处理
    public static class Input
    {
        private static Dictionary<string, bool> _currentState = new Dictionary<string, bool>();
        private static Dictionary<string, bool> _previousState = new Dictionary<string, bool>();
        private static List<GamepadState> _gamepadStates = new List<GamepadState>();
        private static string? _latestButton;
        private static int _pressedTime;
        private static int _dir4;
        private static int _dir8;
        private static string? _preferredAxis;
        private static DateTime _date;
        private static VirtualKey? _virtualButton;

        // 初始化输入系统
        public static void Initialize()
        {
            Clear();
            SetupEventHandlers();
        }

        // 键盘重复间隔时间（帧）
        public static int KeyRepeatWait = 24;
        // 键盘重复间隔（帧）
        public static int KeyRepeatInterval = 6;

        // 键盘映射表
        private static readonly Dictionary<int, string> KeyMapper = new Dictionary<int, string>
        {
            [9] = "Tab", // tab
            [13] = "Ok", // enter
            [16] = "Shift", // shift
            [17] = "Control", // control
            [18] = "Alt", // alt
            [27] = "Escape", // escape
            [32] = "Ok", // space
            [33] = "PageUp", // pageup
            [34] = "PageDown", // pagedown
            [37] = "Left", // left arrow
            [38] = "Up", // up arrow
            [39] = "Right", // right arrow
            [40] = "Down", // down arrow
            [45] = "Escape", // insert
            [81] = "PageUp", // Q
            [87] = "PageDown", // W
            [88] = "Escape", // X
            [90] = "Ok", // Z
            [96] = "Escape", // numpad 0
            [98] = "Down", // numpad 2
            [100] = "Left", // numpad 4
            [102] = "Right", // numpad 6
            [104] = "Up", // numpad 8
            [120] = "Debug" // F9
        };

        // 游戏手柄映射表
        private static readonly Dictionary<int, string> GamepadMapper = new Dictionary<int, string>
        {
            [0] = "Ok", // A
            [1] = "Cancel", // B
            [2] = "Shift", // X
            [3] = "Menu", // Y
            [4] = "PageUp", // LB
            [5] = "PageDown", // RB
            [12] = "Up", // D-pad up
            [13] = "Down", // D-pad down
            [14] = "Left", // D-pad left
            [15] = "Right" // D-pad right
        };

        // 清空所有输入数据
        public static void Clear()
        {
            _currentState.Clear();
            _previousState.Clear();
            _gamepadStates.Clear();
            _latestButton = null;
            _pressedTime = 0;
            _dir4 = 0;
            _dir8 = 0;
            _preferredAxis = null;
            _date = DateTime.Now;
            _virtualButton = null;
        }

        // 更新输入数据
        public static void Update()
        {
            PollGamepads();
            if (_currentState.ContainsKey(_latestButton))
            {
                _pressedTime++;
            }
            else
            {
                _latestButton = null;
            }
            foreach (var keyValue in _currentState)
            {
                if (_currentState[keyValue.Key] && !_previousState.ContainsKey(keyValue.Key))
                {
                    _latestButton = keyValue.Key;
                    _pressedTime = 0;
                    _date = DateTime.Now;
                }
                _previousState[keyValue.Key] = _currentState[keyValue.Key];
            }
            if (_virtualButton != null)
            {
                _latestButton = _virtualButton.Value;
                _pressedTime = 0;
                _virtualButton = null;
            }
            UpdateDirection();
        }

        // 检查键是否按下
        public static bool IsPressed(string keyName)
        {
            if (_isEscapeCompatible(keyName) && IsPressed("Escape"))
            {
                return true;
            }
            return _currentState.ContainsKey(keyName);
        }

        // 检查键是否触发
        public static bool IsTriggered(string keyName)
        {
            if (_isEscapeCompatible(keyName) && IsTriggered("Escape"))
            {
                return true;
            }
            return _latestButton == keyName && _pressedTime == 0;
        }

        // 检查键是否重复
        public static bool IsRepeated(string keyName)
        {
            if (_isEscapeCompatible(keyName) && IsRepeated("Escape"))
            {
                return true;
            }
            return (
                _latestButton == keyName &&
                (_pressedTime == 0 ||
                 (_pressedTime >= KeyRepeatWait &&
                  _pressedTime % KeyRepeatInterval == 0))
            );
        }

        // 检查键是否长按
        public static bool IsLongPressed(string keyName)
        {
            if (_isEscapeCompatible(keyName) && IsLongPressed("Escape"))
            {
                return true;
            }
            return _latestButton == keyName && _pressedTime >= KeyRepeatWait;
        }

        // 获取方向值（数字键盘方向，0为中立）
        public static int Dir4 => _dir4;

        // 获取方向值（8向方向，数字键盘方向，0为中立）
        public static int Dir8 => _dir8;

        // 获取上次输入的时间（毫秒）
        public static DateTime Date => _date;

        public static void VirtualClick(string buttonName)
        {
            _virtualButton = buttonName;
        }

        private static void SetupEventHandlers()
        {
            Keyboard.OnKeyDown += OnKeyDown;
            Keyboard.OnKeyUp += OnKeyUp;
            Window.OnLostFocus += OnLostFocus;
        }

        private static void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (ShouldPreventDefault(e.KeyCode))
            {
                e.Handled = true;
            }
            if (e.KeyCode == 144) // NumLock
            {
                Clear();
            }
            string buttonName = KeyMapper[e.KeyCode];
            if (buttonName != null)
            {
                _currentState[buttonName] = true;
            }
        }

        private static bool ShouldPreventDefault(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.Backspace:
                case KeyCode.Tab:
                case KeyCode.PageUp:
                case KeyCode.PageDown:
                case KeyCode.LeftArrow:
                case KeyCode.UpArrow:
                case KeyCode.RightArrow:
                case KeyCode.DownArrow:
                    return true;
            }
            return false;
        }

        private static void OnKeyUp(object sender, KeyEventArgs e)
        {
            string buttonName = KeyMapper[e.KeyCode];
            if (buttonName != null)
            {
                _currentState[buttonName] = false;
            }
        }

        private static void OnLostFocus(object sender, EventArgs e)
        {
            Clear();
        }

        private static void PollGamepads()
        {
            if (GamepadManager.IsConnected)
            {
                var gamepads = GamepadManager.GetDevices();
                if (gamepads.Length > 0)
                {
                    foreach (var gamepad in gamepads)
                    {
                        if (gamepad.IsConnected)
                        {
                            UpdateGamepadState(gamepad);
                        }
                    }
                }
            }
        }

        private static void UpdateGamepadState(Gamepad gamepad)
        {
            var lastState = _gamepadStates.Count > gamepad.Index ? _gamepadStates[gamepad.Index] : new List<bool>();
            var newState = new List<bool>(gamepad.Buttons.Length);
            var buttons = gamepad.Buttons;
            var axes = gamepadAxes;
            var threshold = 0.5f;
            newState[12] = false;
            newState[13] = false;
            newState[14] = false;
            newState[15] = false;
            for (int i = 0; i < buttons.Length; i++)
            {
                newState[i] = buttons[i].IsPressed;
            }
            if (axes[1] < -threshold)
            {
                newState[12] = true; // up
            }
            else if (axes[1] > threshold)
            {
                newState[13] = true; // down
            }
            if (axes[0] < -threshold)
            {
                newState[14] = true; // left
            }
            else if (axes[0] >