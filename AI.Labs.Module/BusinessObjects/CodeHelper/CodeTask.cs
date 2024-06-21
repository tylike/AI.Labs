using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using OpenAI.Tokenizer.GPT3;

namespace AI.Labs.Module.BusinessObjects.AISpreadSheet
{
    [XafDisplayName("代码任务")]
    [NavigationItem("Excel")]
    public class CodeTask : SimpleXPObject
    {
        public CodeTask(Session s) : base(s)
        {

        }

        [Size(-1)]
        [ModelDefault("RowCount", "1")]
        public string InputFile
        {
            get { return GetPropertyValue<string>(nameof(InputFile)); }
            set { SetPropertyValue(nameof(InputFile), value); }
        }

        public int InputLength => InputContent.Length;
        public int InputTokenCount => TokenizerGpt3.TokenCount(InputContent);

        string inputContent;

        [Size(-1)]
        public string InputContent
        {
            get
            {
                if (File.Exists(InputFile))
                {
                    if (string.IsNullOrEmpty(inputContent))
                    {
                        inputContent = File.ReadAllText(InputFile);
                    }
                    return inputContent;
                }
                return "";
            }
        }

        [Size(-1)]
        [ModelDefault("RowCount", "1")]
        public string OutputFile
        {
            get { return GetPropertyValue<string>(nameof(OutputFile)); }
            set { SetPropertyValue(nameof(OutputFile), value); }
        }

        [Size(-1)]
        public string OutputContent
        {
            get { return GetPropertyValue<string>(nameof(OutputContent)); }
            set { SetPropertyValue(nameof(OutputContent), value); }
        }

        public DateTime SubmitTime
        {
            get { return GetPropertyValue<DateTime>(nameof(SubmitTime)); }
            set { SetPropertyValue(nameof(SubmitTime), value); }
        }

        public DateTime CompleteTime
        {
            get { return GetPropertyValue<DateTime>(nameof(CompleteTime)); }
            set { SetPropertyValue(nameof(CompleteTime), value); }
        }
        protected override void OnSaving()
        {
            base.OnSaving();
            if (!string.IsNullOrEmpty(OutputFile))
            {
                File.WriteAllText(OutputFile, OutputContent);
            }
        }


    }

}
