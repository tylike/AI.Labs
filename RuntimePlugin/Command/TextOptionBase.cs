using System.Drawing;

namespace RuntimePlugin;

public class TextOptionBase : FilterCommand
{
    public string FontPath { get; set; } = FFmpegGlobalSettings.DefaultFont;
    public Color BoxBorderColor { get; set; } = Color.White;
    public Color FontColor { get; set; } = Color.White;
    public SubtitleBorderStyle BoxStyle { get; set; } = SubtitleBorderStyle.Box;
    public int BoxBorderWidth { get; set; } = 5;
    public int BoxBorderHeight { get; set; } = 1;
    public double BoxBorderAlpha { get; set; } = 1.0;
    public int FontSize { get; set; } = 24;
}
