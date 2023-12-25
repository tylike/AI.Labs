using System;
using System.Collections.Generic;
using System.Text;

namespace EdgeTTSSharp.Model
{

    public class eVoice
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Gender { get; set; }
        public string Locale { get; set; }
        public string SuggestedCodec { get; set; }
        public string FriendlyName { get; set; }
        public string Status { get; set; }
        public Voicetag VoiceTag { get; set; }
    }

    public class Voicetag
    {
        public string[] ContentCategories { get; set; }
        public string[] VoicePersonalities { get; set; }
    }

}
