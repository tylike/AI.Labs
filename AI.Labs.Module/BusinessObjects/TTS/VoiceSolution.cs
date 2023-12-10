using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

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



    [NavigationItem]
    //[XafDisplayName("音色方案")]
    [XafDefaultProperty(nameof(Title))]
    public class VoiceSolution : XPObject
    {
        public VoiceSolution(Session s) : base(s)
        {

        }

        public string Title
        {
            get => Memo;            
        }



        public VoiceEngine Engine
        {
            get { return GetPropertyValue<VoiceEngine>(nameof(Engine)); }
            set { SetPropertyValue(nameof(Engine), value); }
        }


        public bool CommonlyUsed
        {
            get { return GetPropertyValue<bool>(nameof(CommonlyUsed)); }
            set { SetPropertyValue(nameof(CommonlyUsed), value); }
        }


        //[XafDisplayName("方案名称")]
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


        //[XafDisplayName("备注")]
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
