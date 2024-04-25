namespace AI.Labs.Module.BusinessObjects
{
    public class SimpleFFmpegCommand
    {
        public SimpleFFmpegCommand(SimpleFFmpegScript script)
        {
            this.Script = script;
        }

        public int Index { get; set; }
        public string Command { get; set; }

        string outputLabel;
        public string OutputLable
        {
            get
            {
                if (outputLabel == null)
                {
                    outputLabel = $"[{SimpleMediaType.ToString().First()}{Index}]";
                }
                return outputLabel;
            }
            set
            {
                outputLabel = value;
            }
        }

        public SimpleMediaType SimpleMediaType { get; set; }

        public SimpleFFmpegScript Script { get; }

    }
}
