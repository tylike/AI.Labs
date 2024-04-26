namespace AI.Labs.Module.BusinessObjects.FilterComplexScripts
{
    public class FilterComplexCommand
    {
        public FilterComplexCommand()
        {

        }

        [Obsolete("使用Script.CreateCommand", false)]
        public FilterComplexCommand(FilterComplexScript script)
        {
            Script = script;
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

        public FilterComplexScript Script { get; internal set; }

    }
}
