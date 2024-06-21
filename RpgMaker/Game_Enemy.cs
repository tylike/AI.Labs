
// Partial class for Game_Enemy
public partial class Game_Enemy : Game_Battler
{
    private int _enemyId;
    private string _letter;
    private bool _plural;
    private int _screenX;
    private int _screenY;

    public Game_Enemy(int enemyId, int x, int y)
    {
        Initialize(enemyId, x, y);
    }

    public override void Initialize(int enemyId, int x, int y)
    {
        base.Initialize();
        Setup(enemyId, x, y);
    }

    public override void InitMembers()
    {
        base.InitMembers();
        _enemyId = 0;
        _letter = "";
        _plural = false;
        _screenX = 0;
        _screenY = 0;
    }

    public void Setup(int enemyId, int x, int y)
    {
        _enemyId = enemyId;
        _screenX = x;
        _screenY = y;
        RecoverAll();
    }

    public bool IsEnemy() => true;

    public Game_Troop FriendsUnit() => GameTroop.Instance;

    public Game_Party OpponentsUnit() => GameParty.Instance;

    public int Index() => GameTroop.Instance.Members().IndexOf(this);

    public bool IsBattleMember() => Index() >= 0;

    public int EnemyId() => _enemyId;

    public Data_Enemy Enemy() => DataEnemies.Get(_enemyId);

    public override List<object> TraitObjects() => base.TraitObjects().Concat(new List<object> { Enemy() }).ToList();

    public int ParamBase(int paramId) => Enemy().Params[paramId];

    public int Exp() => Enemy().Exp;

    public int Gold() => Enemy().Gold;

    public List<object> MakeDropItems()
    {
        var rate = DropItemRate();
        return Enemy().DropItems.Where(di => di.Kind > 0 && new Random().Next(di.Denominator) < rate)
            .Select(di => ItemObject(di.Kind, di.DataId)).ToList();
    }

    public int DropItemRate() => GameParty.Instance.HasDropItemDouble() ? 2 : 1;

    public object ItemObject(int kind, int dataId)
    {
        switch (kind)
        {
            case 1: return DataItems.Get(dataId);
            case 2: return DataWeapons.Get(dataId);
            case 3: return DataArmors.Get(dataId);
            default: return null;
        }
    }

    public bool IsSpriteVisible() => true;

    public int ScreenX() => _screenX;

    public int ScreenY() => _screenY;

    public string BattlerName() => Enemy().BattlerName;

    public int BattlerHue() => Enemy().BattlerHue;

    public string OriginalName() => Enemy().Name;

    public string Name() => OriginalName() + (_plural ? _letter : "");

    public bool IsLetterEmpty() => _letter == "";

    public void SetLetter(string letter) => _letter = letter;

    public void SetPlural(bool plural) => _plural = plural;

    public override void PerformActionStart(Game_Action action)
    {
        base.PerformActionStart(action);
        RequestEffect("whiten");
    }

    public override void PerformAction(Game_Action action) => base.PerformAction(action);

    public override void PerformActionEnd() => base.PerformActionEnd();

    public override void PerformDamage()
    {
        base.PerformDamage();
        SoundManager.PlayEnemyDamage();
        RequestEffect("blink");
    }

    public override void PerformCollapse()
    {
        base.PerformCollapse();
        switch (CollapseType())
        {
            case 0:
                RequestEffect("collapse");
                SoundManager.PlayEnemyCollapse();
                break;
            case 1:
                RequestEffect("bossCollapse");
                SoundManager.PlayBossCollapse1();
                break;
            case 2:
                RequestEffect("instantCollapse");
                break;
        }
    }

    public void Transform(int enemyId)
    {
        var name = OriginalName();
        _enemyId = enemyId;
        if (OriginalName() != name)
        {
            _letter = "";
            _plural = false;
        }
        Refresh();
        if (NumActions() > 0)
        {
            MakeActions();
        }
    }

    public bool MeetsCondition(Game_Action action)
    {
        var param1 = action.ConditionParam1;
        var param2 = action.ConditionParam2;
        switch (action.ConditionType)
        {
            case 1: return MeetsTurnCondition(param1, param2);
            case 2: return MeetsHpCondition(param1, param2);
            case 3: return MeetsMpCondition(param1, param2);
            case 4: return MeetsStateCondition(param1);
            case 5: return MeetsPartyLevelCondition(param1);
            case 6: return MeetsSwitchCondition(param1);
            default: return true;
        }
    }

    public bool MeetsTurnCondition(int param1, int param2)
    {
        var n = TurnCount();
        if (param2 == 0)
        {
            return n == param1;
        }
        else
        {
            return n > 0 && n >= param1 && n % param2 == param1 % param2;
        }
    }

    public bool MeetsHpCondition(int param1, int param2) => HpRate() >= param1 && HpRate() <= param2;

    public bool MeetsMpCondition(int param1, int param2) => MpRate() >= param1 && MpRate() <= param2;

    public bool MeetsStateCondition(int param) => IsStateAffected(param);

    public bool MeetsPartyLevelCondition(int param) => GameParty.Instance.HighestLevel() >= param;

    public bool MeetsSwitchCondition(int param) => GameSwitches.GetValue(param);

    public bool IsActionValid(Game_Action action) => MeetsCondition(action) && CanUse(DataSkills.Get(action.SkillId));

    public Game_Action SelectAction(List<Game_Action> actionList, int ratingZero)
    {
        var sum = actionList.Sum(a => a.Rating - ratingZero);
        if (sum > 0)
        {
            var value = new Random().Next(sum);
            foreach (var action in actionList)
            {
                value -= action.Rating - ratingZero;
                if (value < 0)
                {
                    return action;
                }
            }
        }
        return null;
    }

    public void SelectAllActions(List<Game_Action> actionList)
    {
        var ratingMax = actionList.Max(a => a.Rating);
        var ratingZero = ratingMax - 3;
        actionList = actionList.Where(a => a.Rating > ratingZero).ToList();
        for (int i = 0; i < NumActions(); i++)
        {
            Action(i).SetEnemyAction(SelectAction(actionList, ratingZero));
        }
    }

    public override void MakeActions()
    {
        base.MakeActions();
        if (NumActions() > 0)
        {
            var actionList = Enemy().Actions.Where(a => IsActionValid(a)).ToList();
            if (actionList.Count > 0)
            {
                SelectAllActions(actionList);
            }
        }
        SetActionState("waiting");
    }
}