using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using DevExpress.XtraRichEdit.Import.Doc;
using EdgeTTSSharp;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Speech.Synthesis;

namespace AI.Labs.Module.BusinessObjects.TTS
{
    public enum Gender
    {
        Male,
        Female,
        Unknown
    }

    public enum VoiceEngine
    {
        EdgeTTS = 0,
        AzureTTS = 1
    }
    [XafDisplayName("声音风格")]
    [XafDefaultProperty(nameof(Caption))]
    [NavigationItem("设置")]
    public class VoiceStyle : XPObject
    {
        public VoiceStyle(Session s) : base(s)
        {

        }
        [Association]
        public XPCollection<VoiceSolution> VoiceSolutions { get => GetCollection<VoiceSolution>(nameof(VoiceSolutions)); }



        public string Caption
        {
            get { return GetPropertyValue<string>(nameof(Caption)); }
            set { SetPropertyValue(nameof(Caption), value); }
        }

        public string Name
        {
            get { return GetPropertyValue<string>(nameof(Name)); }
            set { SetPropertyValue(nameof(Name), value); }
        }
    }

    [XafDefaultProperty(nameof(Name))]
    [XafDisplayName("服务渠道")]
    [NavigationItem("设置")]
    public class TTSProvider : XPObject
    {
        public TTSProvider(Session s) : base(s)
        {

        }

        [XafDisplayName("试听文字")]
        public string TryReadText
        {
            get { return GetPropertyValue<string>(nameof(TryReadText)); }
            set { SetPropertyValue(nameof(TryReadText), value); }
        }
        [Action]
        public void UpdateStyles()
        {
            //1.创建所有不存的
            var rst = Session.Query<VoiceStyle>().ToList();
            var styles = string.Join(";", Session.Query<VoiceSolution>().Select(t => t.StyleList)).Split(";").OrderBy(t => t).Distinct();
            foreach (var style in styles)
            {
                var find = rst.FirstOrDefault(t => t.Name == style);
                if (find == null)
                {
                    find = new VoiceStyle(Session) { Name = style };
                    rst.Add(find);
                }
            }
            //使用列表更新
            foreach (var item in Voices)
            {
                if (!string.IsNullOrEmpty(item.StyleList))
                {
                    var ss = item.StyleList.Split(";");
                    foreach (var ss2 in ss)
                    {
                        item.Styles.Add(rst.First(t => t.Name == ss2));
                    }
                }
            }

        }

        [Action]
        public async Task Test()
        {
            var rst = await AzureTTSEngine.GetVoices(this.ApiKey, this.BaseUrl);
            foreach (var voice in rst.Voices)
            {
                if (voice.StyleList != null && voice.StyleList.Length > 0)
                {
                    var t = Voices.FirstOrDefault(t => t.DisplayName == voice.Name);
                    if (t != null)
                    {
                        t.StyleList = string.Join(";", voice.StyleList);
                    }
                }
            }
        }

        public async Task Read(string text, string voiceName)
        {
            if (this.Engine == VoiceEngine.EdgeTTS)
            {
                EdgeTTS.PlayText(text, voiceName);
            }
            else
            {
                await AzureTTSEngine.Play(text, voiceName, this.ApiKey, this.BaseUrl);
            }
        }

        public async Task<byte[]> GetTextToSpeechData(string text, string voiceName)
        {
            var rst = new List<byte>();

            if (this.Engine == VoiceEngine.EdgeTTS)
            {
                await EdgeTTS.PlayText(text, voiceName, play: false, resultBytes: rst);
                Debug.WriteLine("EdgeTTS.PlayText调用完成!");
            }
            else
            {
                await AzureTTSEngine.Play(text, voiceName, this.ApiKey, this.BaseUrl, rst);
            }
            return rst.ToArray();
        }
        public async Task GenerateAudioToFile(string text, string voiceName,string fileName)
        {
            var data =await GetTextToSpeechData(text, voiceName);            
            File.WriteAllBytes(fileName, data);
        }

        public string Name
        {
            get { return GetPropertyValue<string>(nameof(Name)); }
            set { SetPropertyValue(nameof(Name), value); }
        }

        public VoiceEngine Engine
        {
            get { return GetPropertyValue<VoiceEngine>(nameof(Engine)); }
            set { SetPropertyValue(nameof(Engine), value); }
        }

        public string BaseUrl
        {
            get { return GetPropertyValue<string>(nameof(BaseUrl)); }
            set { SetPropertyValue(nameof(BaseUrl), value); }
        }

        public string ApiKey
        {
            get { return GetPropertyValue<string>(nameof(ApiKey)); }
            set { SetPropertyValue(nameof(ApiKey), value); }
        }
        [Association
            //, DevExpress.Xpo.Aggregated
            ]
        public XPCollection<VoiceSolution> Voices
        {
            get => GetCollection<VoiceSolution>(nameof(Voices));
        }

    }

    [NavigationItem]
    [XafDisplayName("音色方案")]
    [XafDefaultProperty(nameof(Title))]
    public class VoiceSolution : XPObject
    {
        public VoiceSolution(Session s) : base(s)
        {

        }

        [Association]
        public XPCollection<VoiceStyle> Styles { get => GetCollection<VoiceStyle>(nameof(Styles)); }

        [Action(Caption = "试听")]
        public async Task TryRead()
        {
            await Read(Provider.TryReadText);
        }
        string GetVoiceName()
        {
            return Provider.Engine == VoiceEngine.EdgeTTS ? this.DisplayName : this.ShortName;
        }
        public async Task Read(string text)
        {
            await Provider.Read(text, GetVoiceName());
        }

        public async Task<byte[]> GetTextToSpeechData(string text)
        {
            return await Provider.GetTextToSpeechData(text, GetVoiceName());
        }

        public async Task GenerateAudioToFile(string text,string fileName)
        {
            await Provider.GenerateAudioToFile(text, this.GetVoiceName(), fileName);
        }

        [XafDisplayName("服务渠道")]
        [Association]
        public TTSProvider Provider
        {
            get { return GetPropertyValue<TTSProvider>(nameof(Provider)); }
            set { SetPropertyValue(nameof(Provider), value); }
        }

        [XafDisplayName("标题")]
        public string Title
        {
            get => Memo;
        }

        public VoiceEngine Engine
        {
            get { return GetPropertyValue<VoiceEngine>(nameof(Engine)); }
            set { SetPropertyValue(nameof(Engine), value); }
        }

        [XafDisplayName("常用")]
        public bool CommonlyUsed
        {
            get { return GetPropertyValue<bool>(nameof(CommonlyUsed)); }
            set { SetPropertyValue(nameof(CommonlyUsed), value); }
        }

        [XafDisplayName("名称")]
        public string Name
        {
            get { return GetPropertyValue<string>(nameof(Name)); }
            set { SetPropertyValue(nameof(Name), value); }
        }

        //[XafDisplayName("角色")]
        public string DisplayName
        {
            get { return GetPropertyValue<string>(nameof(DisplayName)); }
            set { SetPropertyValue(nameof(DisplayName), value); }
        }

        [Size(-1)]
        public string StyleList
        {
            get { return GetPropertyValue<string>(nameof(StyleList)); }
            set { SetPropertyValue(nameof(StyleList), value); }
        }



        [XafDisplayName("备注")]
        public string Memo
        {
            get { return GetPropertyValue<string>(nameof(Memo)); }
            set { SetPropertyValue(nameof(Memo), value); }
        }

        //[XafDisplayName("声音")]
        public string VoiceKey
        {
            get { return GetPropertyValue<string>(nameof(VoiceKey)); }
            set { SetPropertyValue(nameof(VoiceKey), value); }
        }
        public Gender Gender
        {
            get { return GetPropertyValue<Gender>(nameof(Gender)); }
            set { SetPropertyValue(nameof(Gender), value); }
        }
        public string ShortName
        {
            get { return GetPropertyValue<string>(nameof(ShortName)); }
            set { SetPropertyValue(nameof(ShortName), value); }
        }
        [XafDisplayName("区域")]
        public string Locale
        {
            get { return GetPropertyValue<string>(nameof(Locale)); }
            set { SetPropertyValue(nameof(Locale), value); }
        }

        public string SuggestedCodec
        {
            get { return GetPropertyValue<string>(nameof(SuggestedCodec)); }
            set { SetPropertyValue(nameof(SuggestedCodec), value); }
        }

        public string FriendlyName
        {
            get { return GetPropertyValue<string>(nameof(FriendlyName)); }
            set { SetPropertyValue(nameof(FriendlyName), value); }
        }

        public string Status
        {
            get { return GetPropertyValue<string>(nameof(Status)); }
            set { SetPropertyValue(nameof(Status), value); }
        }

        public string ContentCategories
        {
            get { return GetPropertyValue<string>(nameof(ContentCategories)); }
            set { SetPropertyValue(nameof(ContentCategories), value); }
        }

        public string VoicePersonalities
        {
            get { return GetPropertyValue<string>(nameof(VoicePersonalities)); }
            set { SetPropertyValue(nameof(VoicePersonalities), value); }
        }


        //{
        //    "Name": "Microsoft Server Speech Text to Speech Voice (zu-ZA, ThembaNeural)",
        //    "ShortName": "zu-ZA-ThembaNeural",
        //    "Gender": "Male",
        //    "Locale": "zu-ZA",
        //    "SuggestedCodec": "audio-24khz-48kbitrate-mono-mp3",
        //    "FriendlyName": "Microsoft Themba Online (Natural) - Zulu (South Africa)",
        //    "Status": "GA",
        //    "VoiceTag": {
        //      "ContentCategories": [
        //        "General"
        //      ],
        //      "VoicePersonalities": [
        //        "Friendly",
        //        "Positive"
        //      ]
        //    }
        //}
    }
}
