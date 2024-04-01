using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI.Labs.Module.BusinessObjects.VideoTranslate
{

    public class JsonSubtitleFile
    {
        public string systeminfo { get; set; }
        public Model model { get; set; }
        public Params _params { get; set; }
        public Result result { get; set; }
        public Transcription[] transcription { get; set; }
    }

    public class Model
    {
        public string type { get; set; }
        public bool multilingual { get; set; }
        public int vocab { get; set; }
        public Audio audio { get; set; }
        public Text text { get; set; }
        public int mels { get; set; }
        public int ftype { get; set; }
    }

    public class Audio
    {
        public int ctx { get; set; }
        public int state { get; set; }
        public int head { get; set; }
        public int layer { get; set; }
    }

    public class Text
    {
        public int ctx { get; set; }
        public int state { get; set; }
        public int head { get; set; }
        public int layer { get; set; }
    }

    public class Params
    {
        public string model { get; set; }
        public string language { get; set; }
        public bool translate { get; set; }
    }

    public class Result
    {
        public string language { get; set; }
    }

    public class Transcription
    {
        public Timestamps timestamps { get; set; }
        public Offsets offsets { get; set; }
        public string text { get; set; }
        public Token[] tokens { get; set; }
    }

    public class Timestamps
    {
        public string from { get; set; }
        public string to { get; set; }
    }

    public class Offsets
    {
        public int from { get; set; }
        public int to { get; set; }
    }

    public class Token
    {
        public string text { get; set; }
        public Timestamps timestamps { get; set; }
        public Offsets offsets { get; set; }
        public int id { get; set; }
        public float p { get; set; }
    }



}
