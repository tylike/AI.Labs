﻿using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Spreadsheet;
using DevExpress.Xpo;
using DevExpress.XtraRichEdit.Layout.Engine;
using System.Drawing;

namespace AI.Labs.Module.BusinessObjects
{
    public class SimpleFFmpegScript
    {
        IObjectSpace os;
        public SimpleFFmpegScript(IObjectSpace os)
        {
            this.os = os;
        }
        public Session Session => (os as XPObjectSpace).Session;

        public int GetNewIndex()
        {
            return Commands.Count + Audios.Count;
        }

        public List<SimpleFFmpegCommand> Commands { get; set; } = new List<SimpleFFmpegCommand>();

        public SimpleFFmpegCommand CreateEmptyVideo(Color color, int durationMS, int w = 1280, int h = 720)
        {
            var cmd = new SimpleFFmpegCommand(this) { Index = GetNewIndex() };
            var filterComplex = $"color=c={color.Name.ToString().ToLower()}:s={w}x{h}:d={durationMS / 1000d:0.0000},format=yuv420p{cmd.OutputLable}";
            cmd.Command = filterComplex;
            Commands.Add(cmd);
            return cmd;
        }

        public SimpleFFmpegCommand CreateEmptyAudio(int durationMS)
        {
            var cmd = new SimpleFFmpegCommand(this) { Index = GetNewIndex(), SimpleMediaType = SimpleMediaType.Audio };
            var filterComplex = $"anullsrc=r=44100:cl=stereo,atrim=duration={durationMS / 1000d}{cmd.OutputLable}";
            cmd.Command = filterComplex;
            Commands.Add(cmd);
            return cmd;
            //ffmpeg -i input_video.mp4 -f lavfi -i anullsrc=channel_layout=stereo:sample_rate=44100 -filter_complex "[1:a]atrim=duration=10[audiosilence];[0:a][audiosilence]amerge[audio]" -map 0:v -map "[audio]" -c:v copy -c:a aac output.mp4
        }

        public string OutputFileName { get; set; }


        #region inputs
        public List<SimpleFFmpegInput> Inputs = new();
        public List<SimpleFFmpegInput> InputVideos = new List<SimpleFFmpegInput>();

        public List<SimpleFFmpegInput> InputAudios = new List<SimpleFFmpegInput>();
        public List<SimpleFFmpegCommand> InputVideoCommands = new List<SimpleFFmpegCommand>();
        public List<SimpleFFmpegCommand> InputAudioCommands = new List<SimpleFFmpegCommand>();
        public SimpleFFmpegCommand InputVideo(string filename)
        {
            return InputFile(filename, SimpleMediaType.Video);
        }
        public SimpleFFmpegCommand InputAudio(string filename)
        {
#warning 简单判断如果是mp3文件
            var fn = filename;
            if (Path.GetExtension(filename).ToLower() == ".mp3")
            {
                fn = filename + ".wav";
                FFmpegHelper.Mp32Wav(filename, fn);
            }
            return InputFile(fn, SimpleMediaType.Audio);
        }

        public SimpleFFmpegCommand InputFile(string filename, SimpleMediaType type, int? duration = null)
        {
            var commands = Commands;

            var inputFile = new SimpleFFmpegInput { FileName = filename };
            if (!duration.HasValue)
            {
                duration = (int)FFmpegHelper.GetDuration(filename);
            }
            inputFile.Duration = duration.Value;


            Inputs.Add(inputFile);
            var cmd = new SimpleFFmpegCommand(this) { Index = GetNewIndex(), SimpleMediaType = type };
            switch (type)
            {
                case SimpleMediaType.Video:
                    cmd.OutputLable = $"[{GetNewIndex()}:v]";
                    InputVideos.Add(inputFile);
                    InputVideoCommands.Add(cmd);
                    break;
                case SimpleMediaType.Audio:
                    cmd.OutputLable = $"[{GetNewIndex()}:a]";
                    InputAudios.Add(inputFile);
                    InputAudioCommands.Add(cmd);
                    break;
                default:
                    break;
            }
            commands.Add(cmd);
            return cmd;
        }

        #endregion

        public string GetComplexScript()
        {
            return Commands.Where(t => !string.IsNullOrEmpty(t.Command)).Select(t => t.Command).Join(";");
        }

        public List<SimpleFFmpegCommand> Audios { get; set; } = new List<SimpleFFmpegCommand>();



        public void Export(SimpleFFmpegCommand video)
        {
            ArgumentNullException.ThrowIfNull(OutputFileName, nameof(OutputFileName));
            //var audio = Audios.AMix();
            var txts = TextTrack.Select(t => t.GetScript()).ToList();
            txts.Add(this.AddSubtitle("D:\\videoInfo\\3\\cnsrt.fix.srt"));

            var strTexts = txts.Join(";");
            var drawTexts = new SimpleFFmpegCommand(this) { OutputLable = "[VOut]" };
            drawTexts.Command = $"{video.OutputLable}{strTexts}[VOut]";

            this.Commands.Add(drawTexts);


            var filterComplex = GetComplexScript();
            FFmpegHelper.ExecuteFFmpegCommand(
                inputFiles: Inputs.Select(t => $"-i {t.FileName}").Join(" "),
                filterComplex: filterComplex,
                outputFiles: OutputFileName,//
                outputOptions: $"-c:v libx264 -crf 18 -y -map \"{drawTexts.OutputLable}\" -map \"{InputAudioCommands.First().OutputLable[1..^1]}\" "
            );

            Console.WriteLine($"时长:{FFmpegHelper.GetDuration(OutputFileName)}");
        }


        public TextOption DefaultTextOption { get; set; }

        List<DrawTextClip> TextTrack = new();
        public DrawTextClip DrawText(int x, int y, string text, int fontSize, TimeSpan start, TimeSpan end)
        {
            return DrawText(x.ToString(), y.ToString(), text, fontSize, start, end);
        }

        public DrawTextClip DrawText(string x, string y, string text, int fontSize, TimeSpan start, TimeSpan end)
        {
            var textClip = new DrawTextClip(Session)
            {
                Left = x.ToString(),
                Top = y.ToString(),
                Option = DefaultTextOption,
                StartTime = start,
                EndTime = end
            };
            textClip.SetText(text);
            TextTrack.Add(textClip);
            return textClip;
        }

        public DrawTextClip DrawCurrentTime(int x = 10, int y = 450, int fontSize = 24, TimeSpan? start = null, TimeSpan? end = null)
        {
            var currentTime = new DrawTextClip(Session)
            {
                Left = x.ToString(),
                Top = y.ToString(),
                Option = DefaultTextOption,
                StartTime = TimeSpan.Zero,
                EndTime = TimeSpan.FromSeconds(90)
            };

            var duration = TimeSpan.FromDays(1);
            currentTime.SetDisplayCurrentVideoTime(duration);
            return currentTime;
        }




        public string AddSubtitle(string srtFile)
        {
            return $"subtitles='{DrawTextClip.FixText(srtFile)}':force_style='Fontsize=20,PrimaryColour=&H00ffff00&,MarginV=200'";
        }
    }
}
