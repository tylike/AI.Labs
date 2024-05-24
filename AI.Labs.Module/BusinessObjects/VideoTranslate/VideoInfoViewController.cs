using System.Text;
//using SubtitlesParser.Classes; 
// 引入SubtitlesParser的命名空间
using DevExpress.ExpressApp;
using System.Diagnostics;
using DevExpress.ExpressApp.Actions;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Demo.Cli;
using AI.Labs.Module.BusinessObjects.AudioBooks;
using Newtonsoft.Json;
using System.Globalization;
using OpenAI.ObjectModels.RequestModels;
using AI.Labs.Module.BusinessObjects.TTS;
using Xabe.FFmpeg.Downloader;
using AI.Labs.Module.BusinessObjects.Helper;
using AI.Labs.Module.BusinessObjects.FilterComplexScripts;
using DevExpress.ExpressApp.Xpo;
using AI.Labs.Module.BusinessObjects.STT;
using System.Drawing;
using DevExpress.ExpressApp.Editors;
//using SubtitlesParser.Classes.Parsers;

namespace AI.Labs.Module.BusinessObjects.VideoTranslate
{
    public class VideoInfoViewController : ObjectViewController<DetailView, VideoInfo>
    {
        #region 按钮定义
        public VideoInfoViewController()
        {
            var getVideoInfo = new SimpleAction(this, "1.获取视频信息", null);
            getVideoInfo.Execute += GetVideoInfo_Execute;

            var getChapters = new SimpleAction(this, "1.1解析章节", null);
            getChapters.Execute += GetChapters_Execute;

            var downloadVideo = new SimpleAction(this, "2.下载视频", null);
            downloadVideo.Execute += DownloadVideo_Execute;

            var getAudio = new SimpleAction(this, "3.提取音频", null);
            getAudio.Execute += GetAudio_Execute;

            var getSrt = new SimpleAction(this, "4.识别字幕", null);
            getSrt.Execute += GetSrt_Execute;

            var fixSrt = new SimpleAction(this, "4.0修复字幕", null);
            fixSrt.Execute += FixSrt_Execute;

            var loadSrt = new SimpleAction(this, "4.1加载字幕", null);
            loadSrt.Execute += LoadSrt_Execute;

            var getDontTranslateWords = new SimpleAction(this, "4.2取无需翻译词", null);
            getDontTranslateWords.Execute += GetDontTranslateWords_Execute;

            var saveENSr = new SimpleAction(this, "4.3保存系统SRT到文件", null);
            saveENSr.Execute += SaveENSr_Execute;


            var splitEnWav = new SimpleAction(this, "4.4按SRT拆原音频", null);
            splitEnWav.Execute += SplitEnWav_Execute;

            var translateSubtitles = new SimpleAction(this, "TranslateSubtitles", null);
            translateSubtitles.Caption = "5.翻译字幕";
            translateSubtitles.Execute += TranslateSubtitles_Execute;

            var translateSubtitlesV2 = new SimpleAction(this, "TranslateSubtitlesV2", null);
            translateSubtitlesV2.Caption = "5.1翻译字幕V2";
            translateSubtitlesV2.Execute += TranslateSubtitlesV2_Execute;

            var saveCNSRT = new SimpleAction(this, "5.2保存中文SRT", null);
            saveCNSRT.Execute += SaveCNSRT_Execute;

            var generateAudio = new SimpleAction(this, "6.生成音频", null);
            generateAudio.Execute += GenerateAudio_Execute;

            var generateAudioV2 = new SimpleAction(this, "6.1生成音频V2", null);
            generateAudioV2.Execute += GenerateAudioV2_Execute;

            var fixSubtitleTimes = new SimpleAction(this, "6.3修复字幕时间", null);
            fixSubtitleTimes.ToolTip = "根据中文音频修复字幕时间";
            fixSubtitleTimes.Execute += FixSubtitleTimes_Execute;


            var fixJianYingProSrtTime = new SimpleAction(this, "7.修复剪映字幕时间", null);
            fixJianYingProSrtTime.Execute += FixJianYingProSrtTime_Execute;

            var generateVideo = new SimpleAction(this, "10.生成视频", null);
            generateVideo.Execute += GenerateVideo_Execute;

            var generateSrtParsePromptFromEnglishContent = new SimpleAction(this, "从英文内容中生成识别提示", "GenerateSTTPrompt");
            generateSrtParsePromptFromEnglishContent.Execute += GenerateSrtParsePromptFromEnglishContent_Execute;

            var generateSrtParsePromptFromChineseContent = new SimpleAction(this, "从中文内容中生成识别提示", "GenerateSTTPrompt");
            generateSrtParsePromptFromChineseContent.Execute += GenerateSrtParsePromptFromChineseContent_Execute;

            var batchTranslate = new SimpleAction(this, "5.3批量翻译", null);
            batchTranslate.Execute += BatchTranslate_Execute;

            var oneKey = new SimpleAction(this, "11.一键生成", null);
            oneKey.Execute += OneKey_Execute;

            var oneKeyTranslate = new SimpleAction(this, "11.1一键翻译", null);
            oneKeyTranslate.Execute += OneKeyTranslate_Execute;
            oneKeyTranslate.ToolTip = "从下载到翻译完成,完成后可以检查翻译结果，修改后再生成最终视频";

            var translateToVideo = new SimpleAction(this, "11.2翻译结果生成视频", null);
            translateToVideo.Execute += TranslateToVideo_Execute;
            translateToVideo.ToolTip = "翻译结果生成视频，用于翻译修正完成后的生成视频";

            var updateFFmpeg = new SimpleAction(this, "更新FFmpeg", null);
            updateFFmpeg.Execute += UpdateFFmpeg_Execute;
        }

        private void SplitEnWav_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            SplitEnAudio();

        }
        /// <summary>
        /// 使用字幕将wav分成多份并识别出说话人
        /// </summary>
        /// <exception cref="UserFriendlyException"></exception>
        private async void SplitEnAudio()
        {
            //1.使用ffmpeghelper,将音频转换为单声道

            var mono = $"{ViewCurrentObject.AudioFile}.mono.wav";
            FFmpegHelper.ExecuteFFmpegCommand($"-i \"{ViewCurrentObject.AudioFile}\" -ac 1 -y \"{mono}\"");
            var subtitles = this.ViewCurrentObject.Subtitles.OrderBy(t => t.Index).ToArray();
            var times = subtitles.Take(ViewCurrentObject.Subtitles.Count - 1).Select(t => t.EndTime.TotalSeconds).ToArray();
            var files = FFmpegHelper.SplitFile(mono, times, Path.Combine(ViewCurrentObject.ProjectPath, "EnAudioClip", "%4d.wav"));

            if (files.Length != subtitles.Length)
            {
                throw new UserFriendlyException("拆分音频文件数量不正确");
            }

            var speakers = SpeakerIdentificationHelper.Parse(files);

            foreach (var x in subtitles.Select((t, i) => new { t, i }))
            {
                x.t.EnAudioFile = files[x.i];
                var role =await ViewCurrentObject.CnAudioSolution.CreateOrFindAudioRole("角色" + speakers[x.i]);
                x.t.CnVoiceRole = role;
            }
            ObjectSpace.CommitChanges();
        }

        private void GetChapters_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            CreateChapters(TimeSpan.Parse(ViewCurrentObject.Duration));
        }

        private async void TranslateToVideo_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            await TranslateResultToVideo();
        }

        private async void OneKeyTranslate_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            await OneKeyTranslate();
        }

        private void FixSubtitleTimes_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            this.ViewCurrentObject.CnAudioSolution.FixSubtitleTimes();
            ObjectSpace.CommitChanges();
        }

        void Output(string message, bool showTime = true)
        {
            this.ViewCurrentObject.VideoScript.Output += $"{Environment.NewLine} {DateTime.Now.TimeOfDay} {message}";
        }



        private async void UpdateFFmpeg_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, @"d:\ffmpeg.gui\last");
        }

        private async void OneKey_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            await OneKeyTranslate();
            await TranslateResultToVideo();
        }

        private async Task TranslateResultToVideo()
        {
            //生成音频片断
            await CreateAudioSolution();

            await ViewCurrentObject.CnAudioSolution.CreateAudioBook(false);

            //await AudioBook.GenerateAudioBook(ViewCurrentObject.CnAudioSolution.AudioItems, false);

            ObjectSpace.CommitChanges();

            await CreateVideoProduct(this.ObjectSpace, ViewCurrentObject);
            //调用生成视频
        }

        private async Task OneKeyTranslate()
        {
            //var s = (ObjectSpace as XPObjectSpace).Session;
            //s.ExecuteNonQuery($"update {nameof(VideoInfo)} set {nameof(VideoInfo.VideoScript)} = null");
            //ObjectSpace.CommitChanges();

            if (ViewCurrentObject.Model == null)
            {
                ViewCurrentObject.Model = ObjectSpace.GetObjectsQuery<AIModel>().FirstOrDefault(t => t.IsDefault);
            }
            if (ViewCurrentObject.STTModel == null)
            {
                ViewCurrentObject.STTModel = ObjectSpace.GetObjectsQuery<STTModel>().FirstOrDefault(t => t.IsDefault);
            }

            ArgumentNullException.ThrowIfNull(ViewCurrentObject.Model);
            ArgumentNullException.ThrowIfNull(ViewCurrentObject.STTModel);

            准备();
            if (string.IsNullOrEmpty(ViewCurrentObject.VideoFile))
            {
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                GetVideoInfoCore();
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                await YE.Download(ViewCurrentObject);
            }

            if (!File.Exists(ViewCurrentObject.AudioFile))
            {
                GetAudioFromVideo();
            }

            if (!File.Exists(ViewCurrentObject.VideoJsonSRT))
            {
                GetSrtJsonFromAudio();
            }

            if (!File.Exists(ViewCurrentObject.VideoChineseSRT))
            {
                await TranslateSrtToChinese();
            }
        }

        public async static Task CreateVideoProduct(IObjectSpace objectSpace, VideoInfo video, int? forceDuration = null)
        {
            var vi = video;
            vi.CnAudioSolution.FixSubtitleTimes();
            vi.CnVideoDuration = vi.Subtitles.Max(t => t.FixedEndTime);

            if (vi.Chapters.Any())
            {
                var first = vi.Chapters.FirstOrDefault(t => t.StartTime == TimeSpan.Zero);
                //如果没有从0开始的
                if (first == null)
                {
                    first = objectSpace.CreateObject<Chapter>();
                    first.StartTime = TimeSpan.Zero;
                    first.EndTime = vi.Chapters.Min(t => t.EndTime);
                }

                foreach (var item in vi.Chapters)
                {
                    if (string.IsNullOrEmpty(item.CnTitle))
                    {
                        await AIHelper.Ask(item.Title, t =>
                        {
                            item.CnTitle += t.Content;
                        }, aiModel: vi.Model, systemPrompt: "超级精简的将英文翻译为中文");
                    }
                }

            }

            //如果没有最大视频结束的。

            var srt = vi.SaveFixedSRT();
            if (string.IsNullOrEmpty(vi.VideoFileCn))
            {
                vi.VideoFileCn = Path.Combine(vi.ProjectPath, $"{vi.Oid}.cn.mp4");
            }
            var file = vi.VideoFileCn;

            var testScript = new FilterComplexScript(objectSpace, vi);
            testScript.OutputFileName = file;
            testScript.ForceDuration = forceDuration;

            var rootVideo = testScript.InputVideo(vi.VideoFile);

            var sw = Stopwatch.StartNew();
            var topClips = 1000;
            var audios = vi.Audios.OrderBy(t => t.Index)
                .Take(topClips)
                .ToArray();

            #region 准备音频
            //这里的音频成为了视频的最后标准。并行有时会有null的元素
            //为什么?
            Parallel.ForEach(audios, item =>
            {

            });
            foreach (var item in audios)
            {
                Console.WriteLine("预处理音频" + item.Index);
                var p = new AudioParameter { Index = item.Index, FileName = item.OutputFileName, StartTimeMS = (int)item.Subtitle.FixedStartTime.TotalMilliseconds, EndTimeMS = (int)item.Subtitle.FixedEndTime.TotalMilliseconds };
                testScript.ImportAudioClip(p);
            }

            sw.Stop();
            var pmsg = $"并行，预处理音频耗时:{sw.ElapsedMilliseconds}ms";
            Console.WriteLine($"{pmsg}");

            FFmpegHelper.PutAudiosToTimeLine(testScript.AudioParameters, file + ".wav");

            testScript.InputAudio(file + ".wav");
            #endregion

            foreach (var item in audios)
            {
                var videoClip = rootVideo.Select(
                    (int)item.Subtitle.StartTime.TotalMilliseconds,
                    (int)item.Subtitle.StartTime.AddMilliseconds((item.Subtitle.FixedEndTime - item.Subtitle.FixedStartTime).TotalMilliseconds).TotalMilliseconds
                    );

                testScript.VideoProductClips.Add(videoClip);

                //var y1 = 50 * (item.Index % 10);
                //if ((item.Subtitle.FixedEndTime - item.Subtitle.EndTime).TotalMilliseconds >= 1500)
                //{
                //    testScript.DrawText(300, y1, $"{item.Index}-原片:{item.Subtitle.StartTime}-{item.Subtitle.EndTime} {y1}", 24, item.Subtitle.StartTime, item.Subtitle.EndTime);
                //    testScript.DrawText(640, y1, $"{item.Index}-延长:{item.Subtitle.EndTime}-{item.Subtitle.FixedEndTime}", 24, item.Subtitle.EndTime, item.Subtitle.FixedEndTime);
                //}
            }
            if (vi.ForceDuration > 0)
                testScript.ForceDuration = vi.ForceDuration;
            testScript.AddSubtitle(new VideoSubtitleOption { SrtFileName = srt.CnSRT, FontSize = 12, PrimaryColor = Color.Black.ToFFmpegColorString(), OutlineColor = Color.White.ToFFmpegColorString(), Outline = 1, MarginV = 60 });
            testScript.AddSubtitle(new VideoSubtitleOption { SrtFileName = srt.EnSRT, FontSize = 10, PrimaryColor = Color.Yellow.ToFFmpegColorString(), OutlineColor = Color.Black.ToFFmpegColorString(), Outline = 1, MarginV = 20 });

            testScript.DrawCurrentTime();

            testScript.CalcChapterBoxs();

            testScript.DrawChaptersText();

            testScript.Export();

            Console.WriteLine($"时长:{FFmpegHelper.GetDuration(file)}");
        }

        private async void BatchTranslate_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var t = ViewCurrentObject;
            if (t.Model == null)
            {
                throw new UserFriendlyException("请选择模型!");
            }
            var subtitles = ViewCurrentObject.Subtitles.OrderBy(t => t.Index).ToArray();

            TranslateSubtitle(t, subtitles, subtitles[0], this, ObjectSpace, true);
            TranslateSubtitle(t, subtitles, subtitles[1], this, ObjectSpace, true);
            TranslateSubtitle(t, subtitles, subtitles[2], this, ObjectSpace, true);
            TranslateSubtitle(t, subtitles, subtitles[3], this, ObjectSpace, true);
            TranslateSubtitle(t, subtitles, subtitles[4], this, ObjectSpace, true);
            TranslateSubtitle(t, subtitles, subtitles[5], this, ObjectSpace, true);
            TranslateSubtitle(t, subtitles, subtitles[6], this, ObjectSpace, true);
            TranslateSubtitle(t, subtitles, subtitles[7], this, ObjectSpace, true);
            TranslateSubtitle(t, subtitles, subtitles[8], this, ObjectSpace, true);
            TranslateSubtitle(t, subtitles, subtitles[9], this, ObjectSpace, true);


            //SaveSRTToFile(t, SrtLanguage.中文);
            ObjectSpace.CommitChanges();
            await Task.CompletedTask;
        }
        private async void GenerateVideo_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            CreateVideoProduct(this.ObjectSpace, this.ViewCurrentObject);
        }

        private void FixJianYingProSrtTime_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            JiangYing_VideoEditorHelper.FixSrtTime(ViewCurrentObject.JianYingProjectFile);
        }

        private void SaveCNSRT_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            ViewCurrentObject.SaveSRTToFile(SrtLanguage.中文);
        }

        private async void GetDontTranslateWords_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            await Translate(
                $"这是一个视频教程，总结在翻译任务中哪些名词是专有的，不需要翻译的，给出这些词，不需要解释.内容:\n{ViewCurrentObject.ContentEn}",
                t =>
                {
                    ViewCurrentObject.TranslateIgnoreWords += t.Content;
                },
                " "
                );
        }

        private void GenerateSrtParsePromptFromChineseContent_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            ViewCurrentObject.STTPrompt += ViewCurrentObject.ContentCn;
        }

        private void GenerateSrtParsePromptFromEnglishContent_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            ViewCurrentObject.STTPrompt += ViewCurrentObject.ContentEn;
        }
        #endregion

        #region 1.获取视频信息
        private async void GetVideoInfo_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            准备();
            await GetVideoInfoCore();
        }

        private async Task GetVideoInfoCore()
        {
            var url = ViewCurrentObject.VideoURL;
            if (string.IsNullOrEmpty(url))
            {
                throw new UserFriendlyException("没有输入Youtube视频网址!");
            }

            var youtube = new YoutubeClient();
            // Get the video ID
            var videoId = VideoId.Parse(url);
            GetVideoInfo(youtube, videoId);

            // Get available streams and choose the best muxed (audio + video) stream
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoId);
            var list = streamManifest.Streams.ToList();
            ObjectSpace.Delete(ViewCurrentObject.Infos);
            foreach (var item in list)
            {
                //await youtube.Videos.GetAsync(item.Url);

                var info = ObjectSpace.CreateObject<YoutubeVideoInfo>();
                info.Url = item.Url;
                info.尺寸 = item.Size.ToString();
                if (item is MuxedStreamInfo muxed)
                {
                    info.分辨率 = $"{muxed.VideoResolution.Width}x{muxed.VideoResolution.Height}";
                    info.格式 = muxed.Container.Name;
                    info.类型 = YoutubeVideoCategory.完整视频;
                }
                else if (item is VideoOnlyStreamInfo video)
                {
                    info.分辨率 = $"{video.VideoResolution.Width}x{video.VideoResolution.Height}";
                    info.格式 = video.Container.Name;
                    info.类型 = YoutubeVideoCategory.仅有视频;
                }
                else if (item is AudioOnlyStreamInfo audio)
                {
                    info.格式 = audio.Container.Name;
                    info.类型 = YoutubeVideoCategory.仅有音频;
                }
                ViewCurrentObject.Infos.Add(info);
            }
            ObjectSpace.CommitChanges();
        }

        private void 准备()
        {
            ObjectSpace.CommitChanges();

            if (string.IsNullOrEmpty(ViewCurrentObject.ProjectPath))
            {
                ViewCurrentObject.ProjectPath = Path.Combine(@"d:\VideoInfo", this.ViewCurrentObject.Oid.ToString());
            }

            if (!Directory.Exists(ViewCurrentObject.ProjectPath))
            {
                Directory.CreateDirectory(ViewCurrentObject.ProjectPath);
            }

            EncodingProvider provider = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(provider);
        }

        private async Task GetVideoInfo(YoutubeClient youtube, VideoId videoId)
        {
            var mvideo = await youtube.Videos.GetAsync(videoId);

            ViewCurrentObject.Title = mvideo.Title;
            ViewCurrentObject.Description = (mvideo.Description + "").Replace("\n", Environment.NewLine);
            CreateChapters(mvideo.Duration.Value);
            ViewCurrentObject.Keywords = string.Join("\n", mvideo.Keywords);

            ViewCurrentObject.Duration = mvideo.Duration.ToString();
            ViewCurrentObject.CnVideoDuration = mvideo.Duration ?? TimeSpan.Zero;

            ViewCurrentObject.Like = (int)mvideo.Engagement.LikeCount;
            ViewCurrentObject.DisLike = (int)mvideo.Engagement.DislikeCount;
            ViewCurrentObject.ViewCount = (int)mvideo.Engagement.ViewCount;
            ViewCurrentObject.AverageRating = (decimal)mvideo.Engagement.AverageRating;
            ViewCurrentObject.UploadDate = mvideo.UploadDate.LocalDateTime;
            ViewCurrentObject.ImageTitle = mvideo.Thumbnails.OrderByDescending(t => t.Resolution.Width).FirstOrDefault()?.Url;
            #region 作者
            var findAuthor = ObjectSpace.GetObjectsQuery<YoutubeChannel>().FirstOrDefault(t => t.ChannelUrl == mvideo.Author.ChannelUrl);
            if (findAuthor == null)
            {
                findAuthor = ObjectSpace.CreateObject<YoutubeChannel>();
                findAuthor.ChannelID = mvideo.Author.ChannelId;
                findAuthor.ChannelUrl = mvideo.Author.ChannelUrl;
                findAuthor.ChannelName = mvideo.Author.ChannelTitle;
            }
            ViewCurrentObject.Channel = findAuthor;
            #endregion
            //ViewCurrentObject.VideoFile = $"{videoId}.{mvideo.Duration.ToString()}.mp4";
        }

        private void CreateChapters(TimeSpan duration)
        {
            var descs = new VideoDescription(ViewCurrentObject.Description).ParseChapters();
            if (!ViewCurrentObject.Chapters.Any())
            {
                foreach (var item in descs)
                {
                    var c = ObjectSpace.CreateObject<Chapter>();
                    c.Title = item.Name;
                    c.StartTime = item.StartTimespan;
                    c.EndTime = item.EndTimespan ?? duration;
                    ViewCurrentObject.Chapters.Add(c);
                }
            }
        }
        #endregion

        #region 2.下载视频
        private async void DownloadVideo_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            await YE.Download(ViewCurrentObject);

            ObjectSpace.CommitChanges();
        }
        #endregion

        #region 3.提取音频
        private void GetAudio_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            GetAudioFromVideo();
        }

        private void GetAudioFromVideo()
        {
            var obj = this.ViewCurrentObject;
            var wavFileName = $"{obj.ProjectPath}\\{obj.Oid}.wav";

            if (File.Exists(wavFileName))
            {
                File.Delete(wavFileName);
            }

            var pi = new ProcessStartInfo();
            pi.FileName = FFmpegHelper.ffmpegFile;// $@"D:\ffmpeg.gui\ffmpeg\bin\ffmpeg.exe";
            pi.Arguments = $"-i \"{obj.VideoFile}\" -ar 16000 -acodec pcm_s16le \"{wavFileName}\"";
            pi.UseShellExecute = true;
            var inf = Process.Start(pi);
            inf.WaitForExit();
            Debug.WriteLine($"{pi.FileName} {pi.Arguments}");
            ViewCurrentObject.AudioFile = wavFileName;
            ObjectSpace.CommitChanges();
        }
        #endregion

        #region 4.识别字幕
        private void GetSrt_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            GetSrtJsonFromAudio();
        }

        private void GetSrtJsonFromAudio()
        {
            if (AIHelper.LlmServerProcess != null)
            {
                AIHelper.LlmServerProcess.Kill();
            }

            #region 1.生成字幕文件
            var pi = new ProcessStartInfo();
            ArgumentNullException.ThrowIfNull(ViewCurrentObject.STTModel, "语音识别模型");
            ArgumentNullException.ThrowIfNull(ViewCurrentObject.STTModel?.ServerApplication, "语音识别模型启动程序");
            ArgumentNullException.ThrowIfNull(ViewCurrentObject.STTModel.ModelFilePath, "语音识别模型文件");

            pi.FileName = ViewCurrentObject.STTModel.ServerApplication;// $@"F:\ai.stt\whisper.cpp.cublax-11.8.0.x64\main.exe";
            var outputFile = Path.Combine(ViewCurrentObject.ProjectPath, "en_subtitle");
            var parseSpreaker = ViewCurrentObject.ParseSpreaker ? "-di" : "";
            var prompt = "";// ViewCurrentObject.STTPrompt ? "每段尽量长" : "每句尽量长";
            if (!string.IsNullOrEmpty(ViewCurrentObject.STTPrompt))
            {
                prompt = $"--prompt \"{ViewCurrentObject.STTPrompt.Replace("\"", "'").Replace("\n", "")}\"";
            }
            var model = "f:\\ai.stt\\whisper-cpp-ggml-medium-v3.bin";
            if (ViewCurrentObject.STTModel != null)
            {
                model = ViewCurrentObject.STTModel.ModelFilePath;
            }
            var tdrz = "";
            if (ViewCurrentObject.TinyDiarize)
            {
                tdrz = "-tdrz";
            }

            pi.Arguments = $@" -m {model} {tdrz} -of {outputFile} {parseSpreaker} -osrt -ojf -otxt {ViewCurrentObject.AudioFile} {prompt}";

            pi.UseShellExecute = false;
            var inf = Process.Start(pi);
            inf.WaitForExit();

            Debug.WriteLine($"{pi.FileName} {pi.Arguments}");
            ViewCurrentObject.VideoDefaultSRT = $"{outputFile}.srt";
            ViewCurrentObject.VideoJsonSRT = $"{outputFile}.json";
            ObjectSpace.CommitChanges();
            #endregion

            #region 2.加载字幕内容到系统(数据库)
            LoadSrt();
            #endregion
            ViewCurrentObject.SaveSRTToFile(SrtLanguage.英文);

            SplitEnAudio();
        }

        private async void FixSrt_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var t = ViewCurrentObject;
            if (t.Model == null)
            {
                throw new UserFriendlyException("请选择模型!");
            }
            var subtitles = ViewCurrentObject.Subtitles.OrderBy(t => t.Index).ToArray();
            foreach (SubtitleItem item in subtitles)
            {
                await FixV1EnglishSRT(t, subtitles, item, this, ObjectSpace);
            }
            //保存翻译结果
            ObjectSpace.CommitChanges();
        }

        public static async Task FixV1EnglishSRT(VideoInfo t, SubtitleItem[] subtitles, SubtitleItem item, ViewController controller, IObjectSpace os)
        {
            var text = item.PlainText; //string.Join("\n\n", item.PlainText);
            item.Lines = "";
            var contextItems = subtitles.Where(t => t.Index > item.Index - 10 && t.Index < item.Index + 10).Take(20);
            var contexts = string.Join("\n", contextItems.Select(t => t.PlainText));

            var systemPrompt = $"重新组织文字内容,去除掉不必要的So,Now,等不必要语气词,保持句意完整并做精简缩写.";

            if (!string.IsNullOrEmpty(t.FixSRTPrompt))
            {
                systemPrompt = t.FixSRTPrompt;
            }

            if (t.FixSRTIncludeContext)
            {
                systemPrompt += $"# 参考上下文:\n{contexts}";
            }

            await AIHelper.Ask("# 内容:\n" + text,
                cm =>
                {
                    item.Lines += cm.Content;
                    controller.Application.UIThreadDoEvents();
                },
                aiModel: t.Model,
                streamOut: true,
                n_ctx: 1024,
                systemPrompt: systemPrompt
                );
            os.CommitChanges();
        }

        private void LoadSrt_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            LoadSrt();
        }

        private void SaveENSr_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            ViewCurrentObject.SaveSRTToFile(SrtLanguage.英文);
        }

        private static readonly HashSet<string> Abbreviations = new HashSet<string>
        {
            "E.g", "e.g", "i.e", "etc", "Mr", "Mrs", "Dr", "vs"
            // 添加更多的缩写...
        };
        /// <summary>
        /// 移除掉[music]标记
        /// </summary>
        /// <param name="list"></param>
        /// <param name="dynamicTokens">new[]{ " [","music","]"}</param>
        public void RemoveDynamicTokens(List<Token> list, string[] dynamicTokens)
        {
            bool IsRemove(Token token)
            {
                return token.text.StartsWith("[_") && token.text.EndsWith("]");
            }
            int i = 0;
            while (i <= list.Count - dynamicTokens.Length)
            {
                bool match = Enumerable.Range(0, dynamicTokens.Length).All(j => list[i + j].text == dynamicTokens[j]);
                if (match)
                {
                    list.RemoveRange(i, dynamicTokens.Length);
                    continue; // 继续下一次循环，不需要增加i，因为我们刚移除了元素
                }

                if (i < list.Count && IsRemove(list[i]))
                {
                    list.RemoveAt(i);
                    continue; // 继续下一次循环，不需要增加i，因为我们刚移除了元素
                }

                i++; // 只有当没有移除元素时，才增加i
            }

            var last = list.LastOrDefault();
            if (last != null && IsRemove(last))
            {
                list.Remove(last);
            }
        }

        private void LoadSrt()
        {
            var video = ViewCurrentObject;
            ObjectSpace.Delete(video.Subtitles);

            #region 停用方式:从srt读取字幕
            //var t = SRTHelper.ParseSrtFileToObject(video.VideoDefaultSRT, video.Session, true)
            //    .Where(t => !string.IsNullOrEmpty(t.Lines) && t.Lines.Trim().Length > 0);
            //int idx = 0;

            //foreach (var item in t)
            //{
            //    item.Index = idx++;
            //    video.Subtitles.Add(item);
            //}
            //ObjectSpace.CommitChanges(); 
            #endregion

            var jsonFile = JsonConvert.DeserializeObject<JsonSubtitleFile>(File.ReadAllText(video.VideoJsonSRT));
            var tokens = jsonFile.transcription.SelectMany(t => t.tokens).ToList();
            RemoveDynamicTokens(tokens, new[] { " [", "music", "]" });

            var rst = new List<List<Token>>();
            var sentence = new List<Token>();
            rst.Add(sentence);
            for (int i = 0; i < tokens.Count; i++)
            {
                Token item = tokens[i];
                sentence.Add(item);

                // 检查当前token是否是句子的结束符号
                if (item.text == "." || item.text == "!" || item.text == "?")
                {
                    // 检查是否还有后续的token
                    var next = FindNext(tokens, i);
                    if (next != null)
                    {
                        // 检查下一个token是否以大写字母开头，且前面有空格（即下一个token是新句子的开始）
                        bool isNextTokenStartOfSentence =
                            char.IsWhiteSpace(next.text[0]) &&
                            next.text.Length > 1 &&
                            char.IsUpper(next.text[1]);

                        // 检查前一个单词是否是缩写
                        bool isPrevTokenAbbreviation = i > 0 && Abbreviations.Contains(tokens[i - 1].text.TrimEnd('.'));

                        // 如果下一个token是新句子的开始，并且前一个token不是缩写，则开始一个新句子
                        if (isNextTokenStartOfSentence && !isPrevTokenAbbreviation)
                        {
                            sentence = new List<Token>();
                            rst.Add(sentence);
                        }

                    }
                }
            }
            int index = 1;

            SubtitleItem pre = null;
            foreach (var item in rst)
            {
                if (item.Any())
                {
                    var text = string.Join("", item
                        //.Where(t => !(t.text.StartsWith("[_") && t.text.EndsWith("]"))
                        //)
                        .Select(t => t.text));
                    var sub = ObjectSpace.CreateObject<SubtitleItem>();
                    sub.PlainText = text;
                    sub.Lines = text;
                    sub.Index = index;
                    sub.StartTime = TimeSpan.ParseExact(item.First().timestamps.from, @"hh\:mm\:ss\,fff", CultureInfo.InvariantCulture);
                    sub.EndTime = TimeSpan.ParseExact(item.Last().timestamps.to, @"hh\:mm\:ss\,fff", CultureInfo.InvariantCulture);
                    index++;
                    video.Subtitles.Add(sub);
                    foreach (var c in video.Chapters)
                    {
                        if (c.IsInChapter(sub))
                        {
                            c.Subtitles.Add(sub);
                        }
                    }
                    //强制的让字幕中间没有空白时间
                    if (pre != null)
                    {
                        if (pre.EndTime != sub.StartTime)
                        {
                            pre.EndTime = sub.StartTime;
                        }
                    }
                    pre = sub;
                }
            }

            ObjectSpace.CommitChanges();


        }

        private static Token FindNext(List<Token> tokens, int i)
        {
            i++;
            while (i < tokens.Count)
            {
                var next = tokens[i];

                if (next.text.StartsWith("[") && next.text.EndsWith("]"))
                {
                    i++;
                }
                else
                {
                    return next;
                }
            }
            return null;
        }

        #endregion

        #region 5.翻译字幕
        async Task Translate(string text, Action<ChatMessage> action, string prompt = "你精通英文到中文的翻译,将下面要翻译的句子翻译成中文,直接给出翻译结果,不要其他说明或解释.")
        {
            var t = ViewCurrentObject;
            if (t.Model == null)
            {
                throw new UserFriendlyException("请选择模型!");
            }
            await AIHelper.Ask(text,
                    cm =>
                    {
                        action(cm);
                        Application.UIThreadDoEvents();
                    },
                    aiModel: t.Model,
                    streamOut: true,
                    systemPrompt: prompt
                    );
        }

        private async void TranslateSubtitles_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            await TranslateSrtToChinese();
        }

        private async Task TranslateSrtToChinese()
        {

            var video = ViewCurrentObject;

            if (string.IsNullOrEmpty(ViewCurrentObject.TitleCn))
            {
                ViewCurrentObject.TitleCn = "";
                Translate(GetWithIgnoreText(ViewCurrentObject.Title, video), t => { ViewCurrentObject.TitleCn += t.Content; });
            }

            if (string.IsNullOrEmpty(ViewCurrentObject.DescriptionCn))
            {
                ViewCurrentObject.DescriptionCn = "";
                Translate(GetWithIgnoreText(ViewCurrentObject.Description, video), t => { ViewCurrentObject.DescriptionCn += t.Content; });
            }

            if (string.IsNullOrEmpty(ViewCurrentObject.KeywordsCn))
            {
                ViewCurrentObject.KeywordsCn = "";
                Translate(GetWithIgnoreText(ViewCurrentObject.Keywords, video), t => { ViewCurrentObject.KeywordsCn += t.Content; });
            }

            #region 说明
            //系统提示:
            //你精通英文到中文的翻译,将下面要翻译的句子翻译成中文,直接给出翻译结果,不要其他说明或解释.
            //参考内容:
            //Definitely, they are going to create a product, but they are going to have unnecessary conversation between each other.
            //So the conversation is going to be random.
            //This will lead to loss of time, increase cost and it is not effective to manage a team.
            //By controlling who should talk to whom and who to deliver the task to whom can be predefined.\n That's when we use autogen graph.
            //You might think this is more sequential, but the real power of graphs will come when you add multiple number of agents and multiple number of teams.
            //In this way, you are efficiently creating a flow which saves time, saves cost and finally, it's more effective and get a good result.
            //That's exactly what we're going to see today.
            //Let's get started.\n Hi everyone, I'm really excited to show you about autogen graph.
            //In this, we are going to see how we can control agents flow.\n We're also going to see a use case where we have three teams.
            //Team A, Team B, Team C.
            //And we are going to control the flow between those teams and get a final answer.
            //I'm going to take it through step by step.
            //But before that, I regularly create videos in regards to artificial intelligence on my YouTube channel.
            //So do subscribe and click the bell icon to stay tuned.
            //Make sure you click the like button so this video can be helpful for many others like you.
            //So in this we are going to randomly give each member of the team chocolates.

            //用户提示:
            //# 要翻译的句子:
            //Hi everyone, I'm really excited to show you about autogen graph.
            //
            //# 重要的!!!以下专有名词列表中的词不需要翻译：
            //1.autogen graph
            //2.GPT - 4 turbo preview
            //3.openAI API key
            //4.Ollama
            //contenxt size 32K
            //temp:0 
            #endregion

            //创建音频方案
            await CreateAudioSolution();

            var t = ViewCurrentObject;
            if (t.Model == null)
            {
                throw new UserFriendlyException("请选择模型!");
            }
            var subtitles = ViewCurrentObject.Subtitles.OrderBy(t => t.Index).ToArray();
            var tasks = new List<Task<(AudioBookTextAudioItem Item, int Duration, string FileName)>>();
            foreach (SubtitleItem item in subtitles)
            {
                Debug.WriteLine($"***{DateTime.Now:mm:ss.fff}-{item.Index}:开始翻译");


                await TranslateSubtitle(t, subtitles, item, this, ObjectSpace, true);
                Debug.WriteLine($"***{DateTime.Now:mm:ss.fff}-{item.Index}:翻译完成:{item.Index},生成音频");
                tasks.Add(AudioBookTextAudioItem.GenerateAudioFile(false, item.AudioItem));
                Debug.WriteLine($"***{DateTime.Now:mm:ss.fff}-{item.Index}:生成音频任务已提交！");
            }

            await Task.WhenAll(tasks.ToArray());
            Debug.WriteLine("音频生成全部完成");
            foreach (var item in tasks)
            {
                if (item.Result.Item.Duration != -1)
                {
                    item.Result.Item.Duration = item.Result.Duration;
                    item.Result.Item.OutputFileName = item.Result.FileName;
                }
            }

            t.SaveSRTToFile(SrtLanguage.中文);

            //全完成了:


            ObjectSpace.CommitChanges();
        }

        private async void TranslateSubtitlesV2_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var t = ViewCurrentObject;
            if (t.Model == null)
            {
                throw new UserFriendlyException("请选择模型!");
            }
            var subtitles = ViewCurrentObject.Subtitles.OrderBy(t => t.Index).ToArray();
            foreach (SubtitleItem item in subtitles)
            {
                await TranslateSubtitle(t, subtitles, item, this, ObjectSpace, false);
            }
            //保存翻译结果
            var fileName = Path.Combine(t.ProjectPath, $"{t.Oid}.cn.srt");
            SRTHelper.SaveToSrtFile(t.Subtitles, fileName, SrtLanguage.中文, false);
            t.VideoChineseSRT = fileName;
            ObjectSpace.CommitChanges();
        }

        static string GetWithIgnoreText(string text, VideoInfo video)
        {
            var sb = new StringBuilder($"# 要翻译的句子:\n{text}");
            if (!string.IsNullOrEmpty(video.TranslateIgnoreWords))
            {
                sb.Append($"\n\n# 重要的!!!以下专有名词列表中的词不需要翻译：\n{video.TranslateIgnoreWords}");
            }
            if (!string.IsNullOrEmpty(video.TranslateTaskPrompt))
            {
                sb.Append($"\n\n{video.TranslateTaskPrompt}");
            }
            return sb.ToString();
        }
        public static async Task TranslateSubtitle(VideoInfo t, SubtitleItem[] subtitles, SubtitleItem item, ViewController controller, IObjectSpace objectSpace, bool isV1)
        {
            var text = isV1 ? item.PlainText : item.Lines; //string.Join("\n\n", item.PlainText);
            if (isV1)
            {
                item.CnText = "";
            }
            else
            {
                item.CnTextV2 = "";
            }
            var contexts = string.Join("\n", subtitles.Where(t => t.Index > item.Index - 10 && t.Index < item.Index + 10).Take(20).Select(t => t.Lines));

            await AIHelper.Ask(GetWithIgnoreText(text, t),
                cm =>
                {
                    if (isV1)
                    {
                        item.CnText += cm.Content;
                    }
                    else
                    {
                        item.CnTextV2 += cm.Content;
                    }
                    controller.Application.UIThreadDoEvents();
                },
                aiModel: t.Model,
                streamOut: true,
                temperature: 0.1f,
                systemPrompt: $"你精通英文到中文的翻译,将下面要翻译的句子翻译成中文,直接给出翻译结果,不要其他说明或解释.参考上下文:\n{contexts}"
                );
            item.CnText = item.CnText.Replace("[PAD151643]", "").Replace("[PAD151645]", "");
            objectSpace.CommitChanges();
        }
        #endregion

        #region 6.生成音频
        //1.生成音频
        //2.根据字幕时长对齐音频时长
        //3.合并为一个音频文件
        private async void GenerateAudio_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            await CreateAudioSolution();
            await ViewCurrentObject.CnAudioSolution.CreateAudioBook(false);
            //打开:
            //var os = Application.CreateObjectSpace(typeof(AudioBook));
            //var s = os.GetObject(audioSolution);
            //e.ShowViewParameters.CreatedView = Application.CreateDetailView(os, "AudioBook_DetailView", true, s);
            Application.ShowViewStrategy.ShowMessage("音频方案生成完成!");
        }

        private async Task CreateAudioSolution()
        {
            var audioSolution = ViewCurrentObject.GetCnAudioSolution();
#warning 这里与是否删除了音频项目有关，已经有的情况下没有创建新的。
            if (audioSolution.AudioItems.Count <= 0)
            {
                foreach (var item in ViewCurrentObject.Subtitles.OrderBy(t => t.Index))
                {
                    var n = ObjectSpace.CreateObject<AudioBookTextAudioItem>();
                    n.Subtitle = item;
                    item.Audio = n;
                    n.AudioRole = item.CnVoiceRole;
                    audioSolution.AudioItems.Add(n);
                    item.AudioItem = n;
                }
            }
            ObjectSpace.CommitChanges();
        }

        private void GenerateAudioV2_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var audioSolution = ObjectSpace.CreateObject<AudioBook>();
            audioSolution.Content = ViewCurrentObject.ContentCn;
            audioSolution.Name = ViewCurrentObject.Title;
            foreach (var item in ViewCurrentObject.Subtitles.OrderBy(t => t.Index))
            {
                var n = ObjectSpace.CreateObject<AudioBookTextAudioItem>();
                n.Subtitle = item;
                n.ArticleText = item.CnTextV2;
                audioSolution.AudioItems.Add(n);
            }

            ObjectSpace.CommitChanges();
            //打开:
            var os = Application.CreateObjectSpace(typeof(AudioBook));
            var s = os.GetObject(audioSolution);
            e.ShowViewParameters.CreatedView = Application.CreateDetailView(os, "AudioBook_DetailView", true, s);
        }

        #endregion

        #region 7.生成视频
        //1.生成片头的图片文件
        //2.保存完整视频
        #endregion

        #region 8.发布

        #endregion
    }
}
