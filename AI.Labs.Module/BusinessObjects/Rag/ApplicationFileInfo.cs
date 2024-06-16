using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.Diagnostics;

namespace RagServer.Module.BusinessObjects
{
    public enum ModelFileType
    {
        TextGenerate,
        Embedding,
        AudioRecognition,
    }

    [RuleCriteria(nameof(FileExist), CustomMessageTemplate = "文件不存在!", UsedProperties = nameof(FilePath))]
    [NonPersistent]
    public abstract class ISingleFile : XPObject
    {
        public ISingleFile(Session s) : base(s)
        {

        }
        protected virtual bool ValidateFileExist { get => true; }
        public bool FileExist
        {
            get
            {
                if (ValidateFileExist)
                    return File.Exists(FilePath);
                return true;
            }
        }


        public string Name
        {
            get { return GetPropertyValue<string>(nameof(Name)); }
            set { SetPropertyValue(nameof(Name), value); }
        }


        public string Memo
        {
            get { return GetPropertyValue<string>(nameof(Memo)); }
            set { SetPropertyValue(nameof(Memo), value); }
        }


        [Size(-1)]
        [ToolTip(@"如d:\llama.cpp\server.exe或d:\model\qwen.gguf等")]
        [ModelDefault("RowCount", "0")]
        public string FilePath
        {
            get { return GetPropertyValue<string>(nameof(FilePath)); }
            set { SetPropertyValue(nameof(FilePath), value); }
        }

    }

    [NavigationItem]
    [XafDisplayName("模型文件")]
    public class ModelFileInfo : ISingleFile
    {
        public ModelFileInfo(Session s) : base(s)
        {

        }
        public ModelFileType FileType
        {
            get { return GetPropertyValue<ModelFileType>(nameof(FileType)); }
            set { SetPropertyValue(nameof(FileType), value); }
        }
    }

    /// <summary>
    /// 用于记录llama.cpp或ollama的可执行程序
    /// </summary>
    [NavigationItem]
    [XafDisplayName("应用程序")]
    public class ApplicationFileInfo : ISingleFile
    {
        public ApplicationFileInfo(Session s) : base(s)
        {

        }
        //[RuleFromBoolProperty(CustomMessageTemplate ="文件不存在",TargetPropertyName ="FilePath")]

        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }
        protected override void OnChanged(string propertyName, object oldValue, object newValue)
        {
            base.OnChanged(propertyName, oldValue, newValue);
            if (!IsLoading)
            {
                if (propertyName == nameof(FilePath))
                {
                    GetHelp();
                }
            }
        }
        public void GetHelp()
        {
            var startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.FileName = FilePath;
            startInfo.Arguments = "--help";
            var process = new Process();
            process.StartInfo = startInfo;
            process.Start();
            Usage = process.StandardOutput.ReadToEnd();

            process.WaitForExit();
        }

        [Size(-1)]
        public string Usage
        {
            get { return GetPropertyValue<string>(nameof(Usage)); }
            set { SetPropertyValue(nameof(Usage), value); }
        }
    }


}
