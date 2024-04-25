namespace AI.Labs.Module.BusinessObjects
{
    public class SimpleFFmpegSubtitleOption
    {

        public string SrtFileName { get; set; }

        public string GetScript()
        {
            return $"subtitles='{DrawTextClip.FixText(SrtFileName)}':force_style='Fontsize=20,PrimaryColour=&H00ffff00&,MarginV=200'";
        }
    }
}
