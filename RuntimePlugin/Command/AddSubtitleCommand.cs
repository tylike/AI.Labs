using System.Drawing;

namespace RuntimePlugin;
public class AddSubtitleCommand : TextOptionBase
{
    public string SubtitleFile { get; set; }

    public AddSubtitleCommand(string subtitleFile)
    {
        SubtitleFile = subtitleFile;
    }
    public override string GetCommand(int ident)
    {
        var command = $"{ident.GetIdent()}subtitles={SubtitleFile}";
        command += $":box={BoxStyle}:boxborderw={BoxBorderWidth}:color={BoxBorderColor}:boxborderh={BoxBorderHeight}:boxbordera={BoxBorderAlpha}";
        command += $",fontfile={FontPath}:fontsize={FontSize}:fontcolor={FontColor}";
        return command;
    }
}
