using AI.Labs.Module.BusinessObjects;
using AI.Labs.Module.BusinessObjects.VideoTranslate;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Spreadsheet;
using DevExpress.Xpo;
using DevExpress.XtraRichEdit.Layout.Engine;
using System.Drawing;
using System.Text;


namespace AI.Labs.Module.BusinessObjects.FilterComplexScripts
{
    public class FilterComplexScript
    {
        #region db
        IObjectSpace os;
        public Session Session => (os as XPObjectSpace).Session;
        #endregion

        public FilterComplexScript(IObjectSpace os, VideoInfo video)
        {
            this.os = os;
            this.video = video;
            // new TextOption(Session) { FontSize = fontSize, HasBoxBorder = true },
        }
        public string OutputFileName { get; set; }

        #region helper
        public int GetNewIndex()
        {
            return Commands.Count + Audios.Count;
        }
        #endregion

        #region commands
        public FilterComplexCommand CreateCommand(string command, SimpleMediaType simpleMediaType, bool addOutputLabel = false, string outputLabel = null)
        {
            var cmd = new FilterComplexCommand() { Script = this, Index = GetNewIndex(), Command = command, SimpleMediaType = simpleMediaType };
            if (outputLabel != null)
            {
                cmd.OutputLable = outputLabel;
            }

            if (addOutputLabel)
            {
                cmd.Command += cmd.OutputLable;
            }
            Commands.Add(cmd);
            return cmd;
        }

        public List<FilterComplexCommand> Commands { get; set; } = new List<FilterComplexCommand>();

        #endregion

        public FilterComplexCommand CreateEmptyVideo(Color color, int durationMS, int w = 1280, int h = 720)
        {
            var filterComplex = $"color=c={color.Name.ToString().ToLower()}:s={w}x{h}:d={durationMS / 1000d:0.0000},format=yuv420p";

            return CreateCommand(filterComplex, SimpleMediaType.Video, true);
        }

        //public SimpleFFmpegCommand CreateEmptyAudio(int durationMS)
        //{
        //    var cmd = new SimpleFFmpegCommand(this) { Index = GetNewIndex(), SimpleMediaType = SimpleMediaType.Audio };
        //    var filterComplex = $"anullsrc=r=44100:cl=stereo,atrim=duration={durationMS / 1000d}{cmd.OutputLable}";
        //    cmd.Command = filterComplex;
        //    Commands.Add(cmd);
        //    return cmd;
        //    //ffmpeg -i input_video.mp4 -f lavfi -i anullsrc=channel_layout=stereo:sample_rate=44100 -filter_complex "[1:a]atrim=duration=10[audiosilence];[0:a][audiosilence]amerge[audio]" -map 0:v -map "[audio]" -c:v copy -c:a aac output.mp4
        //}

        #region inputs
        public FilterComplexCommand InputVideo(string filename)
        {
            return InputFile(filename, SimpleMediaType.Video);
        }

        /// <summary>
        /// 所有需要的资源输入部分
        /// </summary>
        public List<InputMediaFile> Inputs = new();

        public List<InputMediaFile> InputVideos = new List<InputMediaFile>();

        public List<InputMediaFile> InputAudios = new List<InputMediaFile>();

        #region 为ffmpeg -i参数提供数据来源
        public List<FilterComplexCommand> InputVideoCommands = new List<FilterComplexCommand>();
        public List<FilterComplexCommand> InputAudioCommands = new List<FilterComplexCommand>();
        #endregion

        public List<AudioParameter> AudioParameters = new List<AudioParameter>();
        public List<FilterComplexCommand> VideoProductClips { get; set; } = new List<FilterComplexCommand>();


        public void ImportAudioClip(AudioParameter audioParameter)
        {
            ArgumentNullException.ThrowIfNull(audioParameter, nameof(audioParameter));
            //#warning 简单判断如果是mp3文件
            //var filename = audioParameter.FileName;
            //if (!string.IsNullOrEmpty(filename) && Path.GetExtension(filename).ToLower() == ".mp3")
            //{
            //    var fn = filename;
            //    fn = filename + ".wav";
            //    FFmpegHelper.Mp32Wav(filename, fn);
            //    audioParameter.FileName = fn;
            //}
            AudioParameters.Add(audioParameter);
        }

        public FilterComplexCommand InputAudio(string filename)
        {
            return InputFile(filename, SimpleMediaType.Audio);
        }

        public FilterComplexCommand InputFile(string filename, SimpleMediaType type, int? duration = null)
        {
            var inputFile = new InputMediaFile { FileName = filename };
            if (!duration.HasValue)
            {
                duration = (int)FFmpegHelper.GetDuration(filename);
            }

            inputFile.Duration = duration.Value;

            Inputs.Add(inputFile);
            var cmd = CreateCommand("", type);
            switch (type)
            {
                case SimpleMediaType.Video:
                    cmd.OutputLable = $"[{cmd.Index}:v]";
                    InputVideos.Add(inputFile);
                    InputVideoCommands.Add(cmd);
                    break;
                case SimpleMediaType.Audio:
                    cmd.OutputLable = $"[{cmd.Index}:a]";
                    InputAudios.Add(inputFile);
                    InputAudioCommands.Add(cmd);
                    break;
                default:
                    break;
            }
            return cmd;
        }
        public List<FilterComplexCommand> Audios { get; set; } = new List<FilterComplexCommand>();

        #endregion

        #region Export
        public string GetComplexScript()
        {
            return Commands.Where(t => !string.IsNullOrEmpty(t.Command)).Select(t => t.Command).Join(";\n");
        }

        public int? ForceDuration { get; set; }

        public void Export()
        {
            ArgumentNullException.ThrowIfNull(OutputFileName, nameof(OutputFileName));

            var videoProductV1 = CreateVideoProduct(VideoProductClips);

            var videoProductV2_WithDrawTexts = CreateDrawTextScript(videoProductV1);

            var videoProductV3_WithDrawProgressBar =
                CreateCommand(
                    DrawProgressBar(videoProductV2_WithDrawTexts.OutputLable, "[VOut]")
                    , SimpleMediaType.Video, false, outputLabel: "[VOut]");

            var filterComplex = GetComplexScript();
            var forceDuration = ForceDuration.HasValue ? $"-t {ForceDuration.Value / 1000}" : "";

            var p = os.CreateObject<VisualFilterComplexScript>();

            p.InputOptions = "-report";
            p.InputFiles = Inputs.Select(t => $"-i {t.FileName}").Join(" ");
            p.FilterComplexText = filterComplex;
            p.OutputFiles = OutputFileName;
            p.OutputOptions = $"-c:v libx264 -crf 18 -y -map \"{videoProductV3_WithDrawProgressBar.OutputLable}\" -map \"{InputAudioCommands.First().OutputLable[1..^1]}\" {forceDuration}";
            p.ShowWindow = true;
            p.WriteDebugBatchFile = true;

            p.Execute();

            Console.WriteLine($"时长:{FFmpegHelper.GetDuration(OutputFileName)}");
        }

        FilterComplexCommand CreateVideoProduct(List<FilterComplexCommand> videoClips)
        {
            var outputLabel = "[video]";
            var cmds = $"{videoClips.Select(t => t.OutputLable).Join("")}concat=n={videoClips.Count}";
            var videoTrack = CreateCommand(cmds, SimpleMediaType.Video, true, outputLabel);
            return videoTrack;
        }
        VideoInfo video;


        public void CalcChapterBoxs()
        {
            int? lastX = null;
            var per = 1280/video.CnVideoDuration.TotalMilliseconds;

            foreach (var item in this.video.Chapters)
            {
                var x = 0;
                var w = (int)((item.FixedEndTime - item.FixedStartTime).TotalMilliseconds * per);
                if (lastX.HasValue)
                {
                    x = lastX.Value + 1;
                }
                lastX = x + w;
                item.BoxX = x;
                item.BoxW = w;
            }
        }
        public void DrawChaptersText()
        {
            if(this.video.Chapters.Any() == false)
            {
                return;
            }

            int i = 0;
            var opt = new TextOption(Session) { FontColor = Color.White, HasBorder = true,BorderColor = Color.Black,FontSize=12 };
            foreach (var item in this.video.Chapters)
            {
                DrawText(
                    item.BoxX +1 ,
                    36 + ( (i % 3) * 14)+1,
                    //51,
                    item.CnTitle, TimeSpan.Zero, video.CnVideoDuration, opt );
                i++;
            }
        }
        public string DrawProgressBar(string inputLabel, string outputLabel)
        {
            return $@"color=c=red@0.5:s=1280x14[bar];{inputLabel}[bar]overlay=-w+(w/{video.CnVideoDuration.TotalSeconds})*t:50:shortest=1{outputLabel}";
        }
        IEnumerable<string> DrawChapterBoxs()
        {
            var boxs = new List<string>();
            foreach (var item in this.video.Chapters)
            {
                boxs.Add($"drawbox=x={item.BoxX}:y=50:w={item.BoxW}:h=14:color=gray@0.5:t=fill");
            }
            return boxs;
        }

        private FilterComplexCommand CreateDrawTextScript(FilterComplexCommand video)
        {
            //用户填加的文字
            var drawTexts = new List<string>();
            //字幕
            drawTexts.AddRange(Subtitles.Select(t => t.GetScript()));

            drawTexts.AddRange(DrawChapterBoxs());

            drawTexts.AddRange(TextTrack.Select(t => t.GetScript()));


            var strTextsAndSubtitle = drawTexts.Join(",\n");
            var cmds = $"{video.OutputLable}{strTextsAndSubtitle}";

            var drawTextsCmd = CreateCommand(cmds, SimpleMediaType.Video, addOutputLabel: true);

            return drawTextsCmd;
        }
        #endregion

        #region 绘制文本

        List<DrawTextOption> TextTrack = new();
        public DrawTextOption DrawText(int x, int y, string text, TimeSpan start, TimeSpan end, TextOption option)
        {
            return DrawText(x.ToString(), y.ToString(), text, start, end, option);
        }
        public DrawTextOption DrawText(string x, string y, string text, TimeSpan start, TimeSpan end, TextOption option)
        {
            var textClip = new DrawTextOption(Session)
            {
                Left = x.ToString(),
                Top = y.ToString(),
                Option = option,
                StartTime = start,
                EndTime = end,
            };
            textClip.SetText(text);
            TextTrack.Add(textClip);
            return textClip;
        }

        public DrawTextOption DrawCurrentTime(int x = 10, int y = 10, int fontSize = 24, TimeSpan? start = null, TimeSpan? end = null)
        {
            var currentTime = new DrawTextOption(Session)
            {
                Left = x.ToString(),
                Top = y.ToString(),
                Option = new TextOption(Session) { FontSize = fontSize, HasBorder = false, FontColor = Color.White },
                StartTime = TimeSpan.Zero,
                EndTime = video.CnVideoDuration
            };

            var duration = video.CnVideoDuration;
            currentTime.SetDisplayCurrentVideoTime(duration);
            TextTrack.Add(currentTime);
            return currentTime;
        }

        #endregion

        #region 字幕
        public List<VideoSubtitleOption> Subtitles { get; private set; } = new List<VideoSubtitleOption>();

        public void AddSubtitle(VideoSubtitleOption option)
        {
            var srtFile = option.SrtFileName;
            if (!File.Exists(srtFile))
            {

                throw new FileNotFoundException(srtFile + "文件不存在!");
            }
            Subtitles.Add(option);
            //return $"subtitles='{DrawTextClip.FixText(srtFile)}':force_style='Fontsize=20,PrimaryColour=&H00ffff00&,MarginV=200'";
        }
        #endregion
    }
}
