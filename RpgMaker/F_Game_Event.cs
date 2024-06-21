
// 等效的C#代码
using System;

public partial class Game_Event : Game_Character
{
    public Game_Event(params object[] args)
    {
        Initialize(args);
    }

    // 初始化方法
    public override void Initialize(params object[] args)
    {
        base.Initialize(args);
        _mapId = Convert.ToInt32(args[0]);
        _eventId = Convert.ToInt32(args[1]);
        Locate(EventPosition.X, EventPosition.Y);
        Refresh();
    }

    // 成员初始化
    protected override void InitMembers()
    {
        base.InitMembers();
        _moveType = 0;
        _trigger = 0;
        _starting = false;
        _erased = false;
        _pageIndex = -2;
        _originalPattern = 1;
        _originalDirection = 2;
        _prelockDirection = 0;
        _locked = false;
    }

    // 获取事件ID
    public int eventId => _eventId;

    // 获取事件对象
    public EventData MapEvent => $dataMap.Events[_eventId];

    // 获取当前页面
    public EventPage page => MapEvent.Pages[_pageIndex];

    // 获取事件列表
    public List<GameObject> List => page.List;

    // 检查与角色碰撞
    public bool IsCollidedWithCharacters(int x, int y) =>
        base.IsCollidedWithCharacters(x, y) || IsCollidedWithPlayerCharacters(x, y);

    // 检查与事件碰撞
    public bool IsCollidedWithEvents(int x, int y)
    {
        var events = $gameMap.EventsXYNT(x, y);
        return events.Count > 0;
    }

    // 检查与玩家角色碰撞
    public bool IsCollidedWithPlayerCharacters(int x, int y) =>
        IsNormalPriority && $gamePlayer.IsCollided(x, y);

    // 锁定事件
    public void Lock() => _locked = !_locked ? true : false;

    // 解锁事件
    public void Unlock() => _locked = false;

    // 更新停止逻辑
    protected override void UpdateStop()
    {
        if (_locked)
        {
            ResetStopCount();
        }
        base.UpdateStop();
        if (!IsMoveRouteForcing())
        {
            UpdateSelfMovement();
        }
    }

    // 自我移动更新
    protected virtual void UpdateSelfMovement()
    {
        if (!_locked && IsNearTheScreen() && CheckStop(StopCountThreshold()))
        {
            switch (_moveType)
            {
                case 1:
                    MoveTypeRandom();
                    break;
                case 2:
                    MoveTypeTowardPlayer();
                    break;
                case 3:
                    MoveTypeCustom();
                    break;
            }
        }
    }

    // 停止计数阈值
    protected int StopCountThreshold() => 30 * (5 - MoveFrequency());

    // 随机移动类型
    protected virtual void MoveTypeRandom()
    {
        Random random = new Random();
        switch (random.Next(6))
        {
            case 0:
            case 1:
                MoveRandom();
                break;
            case 2:
            case 3:
            case 4:
                MoveForward();
                break;
            case 5:
                ResetStopCount();
                break;
        }
    }

    // 向玩家移动类型
    protected virtual void MoveTypeTowardPlayer()
    {
        if (IsNearThePlayer())
        {
            Random random = new Random();
            switch (random.Next(6))
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    MoveTowardPlayer();
                    break;
                case 4:
                    MoveRandom();
                    break;
                case 5:
                    MoveForward();
                    break;
            }
        }
        else
        {
            MoveRandom();
        }
    }

    // 是否靠近玩家
    protected bool IsNearThePlayer() => Math.Abs(DeltaXFrom($gamePlayer.X)) + Math.Abs(DeltaYFrom($gamePlayer.Y)) < 20;

    // 定制移动类型
    protected virtual void MoveTypeCustom() => UpdateRoutineMove();

    // 判断是否启动中
    public bool IsStarting => _starting;

    // 清除启动标志
    public void ClearStartingFlag() => _starting = false;

    // 判断触发条件
    public bool IsTriggerIn(params int[] triggers) => Array.IndexOf(triggers, _trigger) != -1;

    // 启动事件
    public void Start()
    {
        var list = List;
        if (list?.Length > 1)
        {
            _starting = true;
            if (_trigger.In(0, 1, 2))
            {
                Lock();
            }
        }
    }

    // 删除事件
    public void Erase() => _erased = true;

    // 刷新事件
    public void Refresh()
    {
        int newPageIndex = _erased ? -1 : FindProperPageIndex();
        if (_pageIndex != newPageIndex)
        {
            _pageIndex = newPageIndex;
            SetupPage();
        }
    }

    // 查找合适的页面索引
    protected int FindProperPageIndex()
    {
        var pages = MapEvent.Pages;
        for (int i = pages.Length - 1; i >= 0; i--)
        {
            var page = pages[i];
            if (MeetsConditions(page))
            {
                return i;
            }
        }
        return -1;
    }

    // 判断条件是否满足
    protected bool MeetsConditions(EventPage page)
    {
        // ... (根据C#语法替换条件判断)
    }

    // 设置页面设置
    protected virtual void SetupPage()
    {
        if (_pageIndex >= 0)
        {
            SetupPageSettings();
        }
        else
        {
            ClearPageSettings();
        }
        RefreshBushDepth();
        ClearStartingFlag();
        CheckEventTriggerAuto();
    }

    // 清除页面设置
    protected virtual void ClearPageSettings()
    {
        // ... (根据C#语法替换清除设置)
    }

    // 设置页面设置
    protected virtual void SetupPageSettings()
    {
        // ... (根据C#语法替换设置)
    }

    // 判断是否为原始模式
    public bool IsOriginalPattern() => Pattern() == _originalPattern;

    // 重置模式
    public void ResetPattern() => SetPattern(_originalPattern);

    // 检查触发表触发
    public virtual void CheckEventTriggerTouch(int x, int y)
    {
        if (!$gameMap.IsEventRunning())
        {
            if (_trigger == 2 && $gamePlayer.Pos(x, y))
            {
                if (!IsJumping() && IsNormalPriority())
                {
                    Start();
                }
            }
        }
    }

    // 自动触发检查
    public virtual void CheckEventTriggerAuto()
    {
        if (_trigger == 3)
        {
            Start();
        }
    }

    // 更新事件
    public override void Update()
    {
        base.Update();
        CheckEventTriggerAuto();
        UpdateParallel();
    }

    // 并行处理更新
    protected virtual void UpdateParallel()
    {
        if (_interpreter != null && !_interpreter.IsRunning())
        {
            _interpreter.Setup(List, _eventId);
        }
        _interpreter?.Update();
    }

    // 设置位置
    public override void Locate(int x, int y) => base.Locate(x, y);

    // 强制移动路线
    public override void ForceMoveRoute(MoveRoute moveRoute) => base.ForceMoveRoute(moveRoute);
}

// 注意：C#中没有`$dataMap`, `$gameMap`, `$gamePlayer`, `$gameActors`, `$gameSelfSwitches`, `$dataItems`等全局变量，它们在实际项目中通常通过依赖注入或注入游戏对象来访问。这里假设这些对象已经存在且可以在适当的地方使用。

//这个C#版本的代码实现了JavaScript代码中的大部分功能，包括事件对象的初始化、成员操作、事件逻辑等。由于C#和JavaScript的语法差异，一些细节可能需要调整，如数组操作、条件判断等。在实际项目中，这些全局变量应该被注入或者通过依赖注入的方式来使用。