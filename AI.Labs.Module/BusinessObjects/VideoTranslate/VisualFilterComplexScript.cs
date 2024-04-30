using DevExpress.Xpo;
//using SubtitlesParser.Classes; // 引入SubtitlesParser的命名空间
using DevExpress.ExpressApp.Model;
using System.Drawing;
using DevExpress.Persistent.Base;
using System.Management;
//using SubtitlesParser.Classes.Parsers;
namespace AI.Labs.Module.BusinessObjects.VideoTranslate
{
    [NavigationItem("视频翻译")]
    public class VisualFilterComplexScript : SimpleXPObject
    {
        public VisualFilterComplexScript(Session s) : base(s)
        {

        }


        public VideoInfo Video
        {
            get { return GetPropertyValue<VideoInfo>(nameof(Video)); }
            set { SetPropertyValue(nameof(Video), value); }
        }

        [Size(-1)]
        [ModelDefault("RowCount", "0")]
        public string InputOptions
        {
            get { return GetPropertyValue<string>(nameof(InputOptions)); }
            set { SetPropertyValue(nameof(InputOptions), value); }
        }

        [Size(-1)]
        [ModelDefault("RowCount", "0")]
        public string InputFiles
        {
            get { return GetPropertyValue<string>(nameof(InputFiles)); }
            set { SetPropertyValue(nameof(InputFiles), value); }
        }

        [Size(-1)]
        [ModelDefault("RowCount","0")]
        public string StartCommand
        {
            get { return GetPropertyValue<string>(nameof(StartCommand)); }
            set { SetPropertyValue(nameof(StartCommand), value); }
        }


        public bool ShowWindow
        {
            get { return GetPropertyValue<bool>(nameof(ShowWindow)); }
            set { SetPropertyValue(nameof(ShowWindow), value); }
        }

        public bool UseShell
        {
            get { return GetPropertyValue<bool>(nameof(UseShell)); }
            set { SetPropertyValue(nameof(UseShell), value); }
        }


        [Size(-1)]
        public string FilterComplexText
        {
            get { return GetPropertyValue<string>(nameof(FilterComplexText)); }
            set { SetPropertyValue(nameof(FilterComplexText), value); }
        }

        public string OutputOptions
        {
            get { return GetPropertyValue<string>(nameof(OutputOptions)); }
            set { SetPropertyValue(nameof(OutputOptions), value); }
        }

        [Size(-1),ModelDefault("RowCount","0")]
        public string OutputFiles
        {
            get { return GetPropertyValue<string>(nameof(OutputFiles)); }
            set { SetPropertyValue(nameof(OutputFiles), value); }
        }


        public bool WriteDebugBatchFile
        {
            get { return GetPropertyValue<bool>(nameof(WriteDebugBatchFile)); }
            set { SetPropertyValue(nameof(WriteDebugBatchFile), value); }
        }



        [Size(-1)]
        public string Output
        {
            get { return GetPropertyValue<string>(nameof(Output)); }
            set { SetPropertyValue(nameof(Output), value); }
        }

        [Action(Caption ="执行")]
        public void Execute()
        {
            var filterComplexArg = "";
            if (!string.IsNullOrEmpty(FilterComplexText))
            {
                var path = Path.GetDirectoryName(OutputFiles);
                var filterComplexFile = $"{path}\\filter_complex.txt";
                File.WriteAllText(filterComplexFile, FilterComplexText);
                filterComplexArg = $"-/filter_complex {filterComplexFile}";
            }
            var cmd = $"{InputOptions} {InputFiles} {filterComplexArg} {OutputOptions} {OutputFiles}";
            if (!string.IsNullOrEmpty(OutputFiles) && WriteDebugBatchFile)
            {
                File.WriteAllText(OutputFiles + ".bat", $"{FFmpegHelper.ffmpegFile} {cmd}");
            }
            FFmpegHelper.ExecuteFFmpegCommand(cmd, ShowWindow);
        }
    }

}
