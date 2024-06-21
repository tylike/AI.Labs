
using System;
using System.Drawing;
using System.Drawing.Imaging;

// 等效的C#代码

public partial class Sprite_Gauge : Sprite
{
    // 初始化方法
    public Sprite_Gauge(params object[] args)
    {
        Initialize(args);
        InitMembers();
        CreateBitmap();
    }

    // 遗传父类构造函数
    protected override void Initialize(params object[] args)
    {
        base.Initialize(args);
    }

    // 成员初始化
    private void InitMembers()
    {
        Battler = null;
        StatusType = "";
        Value = double.NaN;
        MaxValue = double.NaN;
        TargetValue = double.NaN;
        TargetMaxValue = double.NaN;
        Duration = 0;
        FlashingCount = 0;
    }

    // 销毁对象
    public override void Destroy(DestroyOptions options = DestroyOptions.None)
    {
        Bitmap?.Destroy();
        base.Destroy(options);
    }

    // 创建位图
    private void CreateBitmap()
    {
        int width = BitmapWidth();
        int height = BitmapHeight();
        Bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
    }

    // 获取位图宽度
    public int BitmapWidth => 128;

    // 获取位图高度
    public int BitmapHeight => 24;

    // 获取仪表高度
    public int GaugeHeight => 12;

    // 获取仪表X位置
    public int GaugeX => StatusType == "time" ? 0 : 30;

    // 获取标签Y位置
    public int LabelY => 3;

    // 获取标签字体
    public Font LabelFontFace => GameSystem.MainFontFace();

    // 获取标签字号
    public int LabelFontSize => GameSystem.MainFontSize() - 2;

    // 获取数值字体
    public Font ValueFontFace => GameSystem.NumberFontFace();

    // 获取数值字号
    public int ValueFontSize => GameSystem.MainFontSize() - 6;

    // 设置状态
    public void Setup(Battler battler, string statusType)
    {
        Battler = battler;
        StatusType = statusType;
        Value = GetCurrentValue();
        MaxValue = GetCurrentMaxValue();
        UpdateBitmap();
    }

    // 更新对象
    public override void Update()
    {
        base.Update();
        UpdateBitmap();
    }

    // 更新位图
    private void UpdateBitmap()
    {
        double value = GetCurrentValue();
        double maxValue = GetCurrentMaxValue();
        if (value != TargetValue || maxValue != TargetMaxValue)
        {
            UpdateTargetValue(value, maxValue);
        }
        UpdateGaugeAnimation();
        UpdateFlashing();
    }

    // 更新目标值
    private void UpdateTargetValue(double value, double maxValue)
    {
        TargetValue = value;
        TargetMaxValue = maxValue;
        if (double.IsNaN(Value))
        {
            Value = value;
            MaxValue = maxValue;
            Redraw();
        }
        else
        {
            Duration = Smoothness();
        }
    }

    // 平滑度
    private int Smoothness() => StatusType == "time" ? 5 : 20;

    // 更新仪表动画
    private void UpdateGaugeAnimation()
    {
        if (Duration > 0)
        {
            double d = Duration;
            Value = (Value * (d - 1) + TargetValue) / d;
            MaxValue = (MaxValue * (d - 1) + TargetMaxValue) / d;
            Duration--;
            Redraw();
        }
    }

    // 更新闪烁
    private void UpdateFlashing()
    {
        if (StatusType == "time")
        {
            FlashingCount++;
            if (Battler.IsInputting())
            {
                if (FlashingCount % 30 < 15)
                {
                    SetBlendColor(FlashingColor1());
                }
                else
                {
                    SetBlendColor(FlashingColor2());
                }
            }
            else
            {
                SetBlendColor(Color.Transparent);
            }
        }
    }

    // 闪烁颜色1
    private Color FlashingColor1() => Color.FromArgb(255, 255, 255, 64);

    // 闪烁颜色2
    private Color FlashingColor2() => Color.FromArgb(0, 0, 255, 48);

    // 是否有效
    public bool IsValid() => Battler != null
        && (StatusType == "tp" && !Battler.IsPreserveTp() ? GameParty.InBattle() : true);

    // 当前值
    private double GetCurrentValue() => Battler != null
        ? switch (StatusType)
        {
            case "hp" => Battler.Hp;
            case "mp" => Battler.Mp;
            case "tp" => Battler.Tp;
            case "time" => Battler.TpbChargeTime();
            default => double.NaN;
        }
        : double.NaN;

// 当前最大值
private double GetCurrentMaxValue() => Battler != null
    ? switch (StatusType)
{
    case "hp" => Battler.Mhp;
    case "mp" => Battler.Mmp;
    case "tp" => Battler.MaxTp();
    case "time" => 1;
    default => double.NaN;
}
        : double.NaN;

// 标签
private string Label => switch (StatusType)
{
    case "hp" => TextManager.HpA;
    case "mp" => TextManager.MpA;
    case "tp" => TextManager.TpA;
    default => "";
};

// 仪表背景色
private Color GaugeBackColor => ColorManager.GaugeBackColor();

// 仪表颜色1
private Color GaugeColor1() => switch (StatusType)
{
    case "hp" => ColorManager.HpGaugeColor1();
    case "mp" => ColorManager.MpGaugeColor1();
    case "tp" => ColorManager.TpGaugeColor1();
    case "time" => ColorManager.CtGaugeColor1();
    default => ColorManager.NormalColor();
};

// 仪表颜色2
private Color GaugeColor2() => switch (StatusType)
{
    case "hp" => ColorManager.HpGaugeColor2();
    case "mp" => ColorManager.MpGaugeColor2();
    case "tp" => ColorManager.TpGaugeColor2();
    case "time" => ColorManager.CtGaugeColor2();
    default => ColorManager.NormalColor();
};

// 标签颜色
private Color LabelColor => ColorManager.SystemColor();

// 标签描边颜色
private Color LabelOutlineColor => ColorManager.OutlineColor();

// 标签描边宽度
private int LabelOutlineWidth => 3;

// 数值颜色
private Color ValueColor() => switch (StatusType)
{
    case "hp" => ColorManager.HpColor(Battler);
    case "mp" => ColorManager.MpColor(Battler);
    case "tp" => ColorManager.TpColor(Battler);
    default => ColorManager.NormalColor();
};

// 数值描边颜色
private Color ValueOutlineColor => "rgba(0, 0, 0, 1)";

// 数值描边宽度
private int ValueOutlineWidth => 2;

// 重绘
private void Redraw()
{
    Bitmap.Clear();
    double currentValue = GetCurrentValue();
    if (!double.IsNaN(currentValue))
    {
        DrawGauge();
        if (StatusType != "time")
        {
            DrawLabel();
            if (IsValid())
            {
                DrawValue();
            }
        }
    }
}

// 画仪表
private void DrawGauge()
{
    int gaugeX = GaugeX;
    int gaugeY = BitmapHeight - GaugeHeight;
    int gaugeWidth = BitmapWidth - gaugeX;
    int gaugeHeight = GaugeHeight;
    DrawGaugeRect(gaugeX, gaugeY, gaugeWidth, gaugeHeight);
}

// 画仪表矩形
private void DrawGaugeRect(int x, int y, int width, int height)
{
    float rate = GaugeRate();
    int fillW = (int)((width - 2) * rate);
    int fillH = height - 2;
    Color color0 = GaugeBackColor;
    Color color1 = GaugeColor1();
    Color color2 = GaugeColor2();
    Bitmap.FillRectangle(x, y, width, height, color0);
    Bitmap.GradientFillRectangle(x + 1, y + 1, fillW, fillH, color1, color2);
}

// 仪表比例
private float GaugeRate() => IsValid() ? GetCurrentValue() / GetCurrentMaxValue() : 0;

// 画标签
private void DrawLabel()
{
    string label = Label;
    int x = LabelOutlineWidth / 2;
    int y = LabelY;
    int width = BitmapWidth;
    int height = BitmapHeight;
    SetupLabelFont();
    Bitmap.PaintOpacity = LabelOpacity();
    Bitmap.DrawString(label, x, y, width, height, HorizontalAlignment.Left);
    Bitmap.PaintOpacity = 255;
}

// 设置标签字体
private void SetupLabel