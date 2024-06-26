﻿using AI.Labs.Module.BusinessObjects.TTS;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using Newtonsoft.Json;
using System.Diagnostics;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.Actions;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.SystemModule;
using WebSocketSharp;
using DevExpress.ExpressApp.Model;
using AI.Labs.Module.BusinessObjects.VideoTranslate;

namespace AI.Labs.Module.BusinessObjects.AudioBooks
{
    /// <summary>
    /// 有声书的最小条目
    /// </summary>
    [Appearance("没有赋值声音角色", Criteria = "AudioRole is null", BackColor = "#FF0000", TargetItems = nameof(AudioRole))]
    [Appearance("音频时长超过字幕", Criteria = "Duration > Subtitle.Duration", BackColor = "#FF0000", TargetItems = "Subtitle.Duration;Duration")]
    [XafDisplayName("段落")]
    [XafDefaultProperty(nameof(Index))]
    public class AudioBookTextAudioItem : TTSBase
    {
        public AudioBookTextAudioItem(Session s) : base(s)
        {

        }

        [XafDisplayName("关联字幕")]
        [ToolTip("视频翻译时可以关联到已有的字幕")]
        public SubtitleItem Subtitle
        {
            get { return GetPropertyValue<SubtitleItem>(nameof(Subtitle)); }
            set
            {
                SetPropertyValue(nameof(Subtitle), value);
                if (!IsLoading && value != null)
                {
                    ArticleText = value.CnText;
                    Index = value.Index;
                }
            }
        }

        [XafDisplayName("音频时长")]
        public int Duration
        {
            get { return GetPropertyValue<int>(nameof(Duration)); }
            set { SetPropertyValue(nameof(Duration), value); }
        }

        /// <summary>
        /// 是指音频时长超过了字幕时长多久（毫秒）
        /// </summary>
        [XafDisplayName("时长差异")]
        public int Diffence
        {
            get
            {
                return Duration - (Subtitle?.Duration ?? 0);
            }
        }

        protected override void OnChanged(string propertyName, object oldValue, object newValue)
        {
            base.OnChanged(propertyName, oldValue, newValue);
            if (!IsLoading)
            {
                //声音方案了就重新生成
                if (
                    propertyName == nameof(ArticleText) ||  //内容变化时
                    propertyName == nameof(AudioRole) ||    //角色变化时
                    (propertyName == nameof(OutputFileName) && string.IsNullOrEmpty(OutputFileName))    //文件名被设置成了空时
                    )
                {
                    State = TTSState.WaitGenerate;
                }
                //azure的功能增加时,影响ssml的属性增到加这里
            }
        }

        #region 属性
        [Association]
        [XafDisplayName("所属书籍")]
        public AudioBook AudioBook
        {
            get { return GetPropertyValue<AudioBook>(nameof(AudioBook)); }
            set { SetPropertyValue(nameof(AudioBook), value); }
        }
        [XafDisplayName("序号")]
        public int Index
        {
            get { return GetPropertyValue<int>(nameof(Index)); }
            set { SetPropertyValue(nameof(Index), value); }
        }

        [XafDisplayName("朗读角色")]
        [Association]
        [DataSourceProperty("AudioBook.Roles")]
        public AudioBookRole AudioRole
        {
            get { return GetPropertyValue<AudioBookRole>(nameof(AudioRole)); }
            set { SetPropertyValue(nameof(AudioRole), value); }
        }

        [XafDisplayName("文章原文")]
        [Size(-1)]
        public string ArticleText
        {
            get { return GetPropertyValue<string>(nameof(ArticleText)); }
            set { SetPropertyValue(nameof(ArticleText), value); }
        }

        [XafDisplayName("说话人")]
        public string Spreaker
        {
            get { return GetPropertyValue<string>(nameof(Spreaker)); }
            set { SetPropertyValue(nameof(Spreaker), value); }
        }

        [Size(-1)]
        [Persistent("Content")]
        [XafDisplayName("说话内容")]
        public string SpreakContent
        {
            get { return GetPropertyValue<string>(nameof(SpreakContent)); }
            set { SetPropertyValue(nameof(SpreakContent), value); }
        }

        [Size(-1)]
        [XafDisplayName("说话前缀")]
        public string SpreakBefore
        {
            get { return GetPropertyValue<string>(nameof(SpreakBefore)); }
            set { SetPropertyValue(nameof(SpreakBefore), value); }
        }

        [XafDisplayName("情绪")]
        public string Emotion
        {
            get { return GetPropertyValue<string>(nameof(Emotion)); }
            set { SetPropertyValue(nameof(Emotion), value); }
        }

        [XafDisplayName("音量")]
        public int Volumn
        {
            get { return GetPropertyValue<int>(nameof(Volumn)); }
            set { SetPropertyValue(nameof(Volumn), value); }
        }

        [XafDisplayName("输出文件")]
        public string OutputFileName
        {
            get { return GetPropertyValue<string>(nameof(OutputFileName)); }
            set { SetPropertyValue(nameof(OutputFileName), value); }
        }
        #endregion

        [Action(ToolTip = "识别说人、情绪、音量")]
        public async Task ParseArticleText()
        {
#warning 待实现
            //            if (AudioBook?.AIModel == null)
            //            {
            //                throw new UserFriendlyException("错误,没有设置LLM模型!");
            //            }

            //            var p = @"
            //#要识别的小说内容:
            //" + ArticleText;
            //            Debug.WriteLine(new string('=', 80));
            //            Debug.WriteLine(p);
            //            (string Message, bool IsError) rst;

            //            try
            //            {
            //                rst = await AIHelper.Ask(AudioBook.SystemPrompt, p, AudioBook.AIModel);
            //            }
            //            catch (Exception ex)
            //            {
            //                throw new UserFriendlyException($"调用模型时出错:{ex.Message}");
            //            }

            //            if (!rst.IsError)
            //            {
            //                //idx++;
            //                //var n = new AudioBookSpreakItem(Session);
            //                //n.Index = idx;
            //                //n.Paragraph = item;
            //                //this.SpreakItems.Add(n);
            //                var n = this;
            //                try
            //                {
            //                    var json = JsonConvert.DeserializeObject<ParseResult>(rst.Message.RemoveJsonRem());
            //                    n.Spreaker = json.Spreaker;
            //                    n.SpreakContent = json.SpreakContent;
            //                    n.SpreakBefore = json.SpreakBefore;
            //                    n.Emotion = json.Emotion;
            //                    n.Volumn = json.Volume;
            //                    n.AudioRole = await AudioBook.CreateOrFindAudioRole(n.Spreaker);
            //                }
            //                catch (Exception ex)
            //                {
            //                    n.SpreakContent = rst.Message;
            //                    n.SpreakBefore = "报错了:" + ex.Message;
            //                }
            //            }
            //            Debug.WriteLine(new string('-', 80));
            //            Debug.WriteLine(rst.Message);
        }

        //[Action(Caption = "朗读")]
        public async Task Play()
        {
            //应该改成从文件播放
            await GenerateAudioFile(false, this);
            AudioPlayer.NAudioPlay(this.OutputFileName);
        }

        private VoiceSolution GetFinalSolution()
        {
            //第一优先级:从字幕上设置的中文音色角色上取
            var sln = Subtitle?.CnVoiceRole?.VoiceSolution;
            if (sln != null)
                return sln;
            //第二优先级:从当前设定上取
            var vs = this.Solution ?? this.AudioRole?.VoiceSolution;
            if (vs == null)
            {
                var rst = this.AudioBook.Roles.FirstOrDefault();
                if (rst != null)
                    return rst.VoiceSolution;
                throw new UserFriendlyException("没有选择声音方案!");
            }
            return vs;
        }

        /// <summary>
        /// 生成文件
        /// </summary>
        /// <param name="reGenerate"></param>
        //[Action(Caption = "生成音频")]
        public static async Task<(AudioBookTextAudioItem Item, int Duration, string FileName)> GenerateAudioFile(bool reGenerate, AudioBookTextAudioItem item)
        {
            #region 是否需要重新生成
            bool exist = !string.IsNullOrEmpty(item.OutputFileName) && File.Exists(item.OutputFileName);
            if (!reGenerate && exist)
            {
                return (item, -1, item.OutputFileName);
            }

            //重新生成,并且文件名不为空,并且文件存在,则删除
            if (exist)
            {
                File.Delete(item.OutputFileName);
                exist = false;
            }
            #endregion

            if (exist)
            {
                throw new Exception("不会发生的错误!");
            }

            var vs = item.GetFinalSolution();

            var p = Path.Combine(item.AudioBook.OutputPath, $"{item.Index}.mp3");
            Debug.WriteLine($"***{DateTime.Now:mm:ss.fff}-{item.Index}:开始生成音频");
            await (await vs.Text2AudioData(item.Subtitle.CnText)).SaveAudioDataToFile(p);
            Debug.WriteLine($"***{DateTime.Now:mm:ss.fff}-{item.Index}:完成生成音频");

            var audioDuration = (int)FFmpegHelper.GetDuration(p).Value;
            var newDuration = ChangeSpeed(p, audioDuration, item.Subtitle.Duration);
            if (newDuration != 0)
            {
                audioDuration = newDuration;
            }
            item.OutputFileName = p;
            Debug.WriteLine($"***{DateTime.Now:mm:ss.fff}-{item.Index}:完成变速" + item.OutputFileName);
            return (item, audioDuration, p);
        }

        #region 速度调整
        //[Action(Caption = "自动调速")]
        public static int ChangeSpeed(string OutputFileName, int AudioDuration, int SubtitleDuration)
        {
            var speed = 1d;
            if (AudioDuration > SubtitleDuration)
            {
                var rst = 计算调速(AudioDuration, SubtitleDuration);
                if (rst.实际倍速 > 0)
                {
                    speed = rst.实际倍速;
                }
                else
                {
                    speed = 1;
                }
            }

            var output = OutputFileName + ".mp3";
            FFmpegHelper.ChangeAudioSpeed(OutputFileName, output, speed: speed);
            //FFmpegHelper.Mp32Wav(OutputFileName, OutputFileName + ".wav", speed: speed);
            File.Delete(OutputFileName);
            File.Move(output, OutputFileName);
            return (int)FFmpegHelper.GetDuration(OutputFileName).Value;
        }

        public static (double 计划倍速, double 实际倍速, bool 调整成功) 计算调速(int Duration, int SubtitleDuration)
        {
            if (Duration > SubtitleDuration)
            {
                //音频时长超过字幕时长
                var planSource = ((double)Duration / SubtitleDuration);
                var 计划倍速 = planSource.RoundUp(3);
                var 实际倍速 = 计划倍速;
                var 调整成功 = false;
                if (计划倍速 > 1.3)
                {
                    实际倍速 = 1.3;
                }
                else
                {
                    实际倍速 = 计划倍速;
                    调整成功 = true;
                }
                return (计划倍速, 实际倍速, 调整成功);
            }
            return (0, 0, true);
        }
        #endregion

    }

    public class AudioBookTextAudioItemViewController : ObjectViewController<ListView, AudioBookTextAudioItem>
    {
        public AudioBookTextAudioItemViewController()
        {
            var batchSetupRole = new SimpleAction(this, "BatchSetupRole", null);
            batchSetupRole.Caption = "设置角色";
            batchSetupRole.ToolTip = "设置角色,本功能主要是为了实现批量设置一批条目快速的设置为目标角色";
            batchSetupRole.SelectionDependencyType = SelectionDependencyType.RequireMultipleObjects;
            batchSetupRole.Execute += BatchSetupRole_Execute;

            var mergeItem = new SimpleAction(this, "MergeAudioTextItem", null);
            mergeItem.Caption = "合并";
            mergeItem.ToolTip = "合并多条项目";
            mergeItem.SelectionDependencyType = SelectionDependencyType.RequireMultipleObjects;
            mergeItem.Execute += MergeItem_Execute;

            var splitItem = new SimpleAction(this, "SplitAudioTextItem", null);
            splitItem.Caption = "拆分";
            splitItem.ToolTip = "拆分一个条目为多条项目,选择多个则为选中的都进行拆分";
            splitItem.SelectionDependencyType = SelectionDependencyType.RequireMultipleObjects;
            splitItem.Execute += SplitItem_Execute;

            var generate = new SimpleAction(this, "GenerateTextAudioItem", null);
            generate.Caption = "生成";
            generate.Execute += Generate_Execute;

            var play = new SimpleAction(this, "PlayAudioTextItem", null);
            play.Caption = "播放";
            play.Execute += Play_Execute;
            var updateDuration = new SimpleAction(this, "更新时长", null);
            updateDuration.Execute += UpdateDuration_Execute;
        }

        private void UpdateDuration_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var items = e.SelectedObjects.OfType<AudioBookTextAudioItem>();
            foreach (var item in items)
            {
                item.Duration = (int)FFmpegHelper.GetDuration(item.OutputFileName).Value;
            }
            ObjectSpace.CommitChanges();
        }

        private async void Play_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            foreach (AudioBookTextAudioItem item in e.SelectedObjects)
            {
                await item.Play();
            }
        }

        private async void Generate_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            await AudioBook.GenerateAudioBook(e.SelectedObjects.OfType<AudioBookTextAudioItem>(), true);

            //音频生成完成,修正字幕时间.
            var items = e.SelectedObjects.OfType<AudioBookTextAudioItem>();

            AudioBook book = items.First().AudioBook;
            book.FixSubtitleTimes();

            var cnt = ObjectSpace.ModifiedObjects.OfType<AudioBookTextAudioItem>().Count();
            ObjectSpace.CommitChanges();
            Application.ShowViewStrategy.ShowMessage($"生成完成,保存了{cnt}个段落!");
        }

        private void SplitItem_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var items = e.SelectedObjects.OfType<AudioBookTextAudioItem>();
            var old = this.View.CollectionSource.List.OfType<AudioBookTextAudioItem>().ToList();
            foreach (var item in items)
            {
                //要拆分的这一条的原来位置
                var itemIndex = old.IndexOf(item);
                //一条要拆分的:
                var texts = item.ArticleText.Split("\n", StringSplitOptions.RemoveEmptyEntries);
                var newIndexOffset = 0;
                foreach (var text in texts)
                {
                    var n = ObjectSpace.CreateObject<AudioBookTextAudioItem>();
                    n.ArticleText = text;
                    n.AudioRole = item.AudioRole;
                    old.Insert(itemIndex + newIndexOffset, n);
                    newIndexOffset++;
                    this.View.CollectionSource.Add(n);
                }
                this.View.CollectionSource.Remove(item);
                old.Remove(item);
                item.Delete();

            }
            AutoSetIndex(old);
        }


        private void MergeItem_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var items = e.SelectedObjects.OfType<AudioBookTextAudioItem>();
            var text = string.Join("\n", items.Select(x => x.ArticleText).ToArray());
            items.First().ArticleText = text;
            foreach (var x in items.Skip(1))
            {
                this.View.CollectionSource.Remove(x);
                x.Delete();
            }
            AutoSetIndex(this.View.CollectionSource.List.OfType<AudioBookTextAudioItem>().OrderBy(t => t.Index));
        }

        private void AutoSetIndex(IEnumerable<AudioBookTextAudioItem> items)
        {
            int idx = 1;
            foreach (var x in items)
            {
                x.Index = idx;
                idx++;
            }
        }

        private void BatchSetupRole_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            if (e.SelectedObjects.Count > 0)
            {
                var view = $"{nameof(AudioBookRole)}_ListView";
                var cs = Application.CreateCollectionSource(this.ObjectSpace, typeof(AudioBookRole), view, CollectionSourceMode.Normal);
                cs.SetCriteria("default", $"{nameof(AudioBookTextAudioItem.AudioBook)}=='{this.ViewCurrentObject.AudioBook.Oid}'");
                e.ShowViewParameters.CreatedView = Application.CreateListView(view, cs, false);
                e.ShowViewParameters.TargetWindow = TargetWindow.NewModalWindow;
                e.ShowViewParameters.Context = TemplateContext.FindPopupWindowContextName;
                var dc = Application.CreateController<DialogController>();
                dc.SaveOnAccept = false;
                dc.Accepting += (s, evt) =>
                {
                    var selected = (AudioBookRole)evt.AcceptActionArgs.CurrentObject;
                    if (selected != null)
                    {
                        foreach (AudioBookTextAudioItem item in e.SelectedObjects)
                        {
                            item.AudioRole = selected;
                        }
                    }
                };
                e.ShowViewParameters.Controllers.Add(dc);
            }
        }
    }

}
