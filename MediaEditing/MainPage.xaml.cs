using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Editing;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace MediaEditing
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }
        private async void ProcessVideoButton_Click(object sender, RoutedEventArgs e)
        {
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
            openPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.VideosLibrary;
            openPicker.FileTypeFilter.Add(".mp4");

            StorageFile originalFile = await openPicker.PickSingleFileAsync();
            if (originalFile != null)
            {
                // 创建一个新的文件，这里直接使用临时文件
                StorageFile storageFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync("myfile.mp4", CreationCollisionOption.GenerateUniqueName);

                // 复制原始视频到新文件中
                await originalFile.CopyAndReplaceAsync(storageFile);

                // 创建MediaComposition
                var mc = new MediaComposition();

                // 添加颜色剪辑
                var b = Colors.Blue;
                var c = Colors.Cyan;
                mc.Clips.Add(await MediaClip.CreateFromFileAsync(originalFile));
                mc.Clips.Add(MediaClip.CreateFromColor(b, TimeSpan.FromSeconds(5)));
                mc.Clips.Add(MediaClip.CreateFromColor(c, TimeSpan.FromSeconds(5)));

                // 渲染并保存媒体组合到文件
                var saveOperation = mc.RenderToFileAsync(storageFile, MediaTrimmingPreference.Precise);
                var renderResult = await saveOperation;

                if (renderResult != Windows.Media.Transcoding.TranscodeFailureReason.None)
                {
                    // 处理错误
                    System.Diagnostics.Debug.WriteLine("视频保存失败: " + renderResult.ToString());
                }
                else
                {
                    // 播放视频或执行其他操作
                    System.Diagnostics.Debug.WriteLine("视频已保存至: " + storageFile.Path);
                }

                //TODO: 在这里执行你想要的操作，比如播放视频等
            }
            else
            {
                // 用户没有选择文件
            }
        }

        // ...

        private async Task CreateEditedVideoAsync()
        {
            // 创建 MediaComposition 用于编辑
            var composition = new MediaComposition();

            // 加载视频文件
            var videoFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/video.mp4"));
            var videoClip = await MediaClip.CreateFromFileAsync(videoFile);

            // 1. 对某个视频片段变速播放
            // 假设视频片段A是前5秒
            var clipA = videoClip.Clone();
            clipA.TrimTimeFromStart = TimeSpan.Zero;
            clipA.TrimTimeFromEnd = TimeSpan.FromSeconds(5);
           
            //clipA.PlaybackRate = 2.0;  // 变速播放，2倍速度
            composition.Clips.Add(clipA);

            // 2. 对某个音频片段变速播放
            // 假设音频片断B是第5秒到第10秒
            var clipB = videoClip.Clone();
            clipB.TrimTimeFromStart = TimeSpan.FromSeconds(5);
            clipB.TrimTimeFromEnd = TimeSpan.FromSeconds(5);
            // 变速播放音频需要使用 MediaClip 的原始音频轨道
            var audioClipB = await MediaClip.CreateFromFileAsync(videoFile);// MediaClip.CreateFromEmbeddedAudioTrack(clipB.EmbeddedAudioTracks[0]);
            audioClipB.TrimTimeFromStart = TimeSpan.FromSeconds(5);
            audioClipB.TrimTimeFromEnd = TimeSpan.FromSeconds(5);
            //audioClipB.EmbeddedAudioTracks[0].PlaybackRate = 0.5; // 0.5倍速度
            composition.Clips.Add(audioClipB);

            // 3. 烧录字幕
            var subtitleFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/subtitle.srt"));
            var timedTextSource = TimedTextSource.CreateFromStream(await subtitleFile.OpenReadAsync());
            timedTextSource.Resolved += (s, e) =>
            {
                if (e.Error == null)
                {
                    foreach (var track in e.Tracks)
                    {
                        //composition.TimedTextSources.Add(track);
                    }
                }
            };

            // 4. 绘制文字
            var textClip = MediaClip.CreateFromColor(Colors.Transparent, TimeSpan.FromSeconds(5));
            var textOverlay = new MediaOverlay(textClip);
            var textBlock = new TextBlock
            {
                Text = "示例文字",
                Foreground = new SolidColorBrush(Colors.Yellow),
                FontSize = 72
            };
            textOverlay.Position = new Rect(100, 100, 400, 200);
            textOverlay.Opacity = 0.8;
            var overlayLayer = new MediaOverlayLayer();
            overlayLayer.Overlays.Add(textOverlay);
            composition.OverlayLayers.Add(overlayLayer);

            // 渲染并保存编辑后的视频到文件
            var saveFile = await KnownFolders.VideosLibrary.CreateFileAsync("editedVideo.mp4", CreationCollisionOption.GenerateUniqueName);
            var renderResult = await composition.RenderToFileAsync(saveFile, MediaTrimmingPreference.Precise);
            if (renderResult != Windows.Media.Transcoding.TranscodeFailureReason.None)
            {
                // 处理错误
                System.Diagnostics.Debug.WriteLine("视频保存失败: " + renderResult.ToString());
            }
            else
            {
                // 视频保存成功
                System.Diagnostics.Debug.WriteLine("视频已保存至: " + saveFile.Path);
            }
        }
    }



}
