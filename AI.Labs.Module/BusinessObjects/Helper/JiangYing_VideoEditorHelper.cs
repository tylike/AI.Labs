//using SubtitlesParser.Classes; // 引入SubtitlesParser的命名空间
//using SubtitlesParser.Classes.Parsers;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
namespace AI.Labs.Module.BusinessObjects
{
    public class JiangYing_VideoEditorHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="jianYingProject_Draft_Content_Json">C:\\Users\\46035\\AppData\\Local\\JianyingPro\\User Data\\Projects\\com.lveditor.draft\\Ollama.FuncationCall\\draft_content.json</param>
        public static void FixSrtTime(string jianYingProject_Draft_Content_Json)
        {
            string jsonFilePath = jianYingProject_Draft_Content_Json; //"C:\\Users\\46035\\AppData\\Local\\JianyingPro\\User Data\\Projects\\com.lveditor.draft\\Ollama.FuncationCall\\draft_content.json";
            string outputFilePath = jianYingProject_Draft_Content_Json; //"C:\\Users\\46035\\AppData\\Local\\JianyingPro\\User Data\\Projects\\com.lveditor.draft\\Ollama.FuncationCall\\draft_content.json";

            var jsonContent = File.ReadAllText(jsonFilePath);
            var jsonObject = JObject.Parse(jsonContent);
            var project = new ProjectWrapper(jsonObject);

            // Create a dictionary to map text ID to its corresponding text wrapper
            var textDict = project.Materials.Texts.ToDictionary(t => t.Id, t => t);

            //所有轨道上的文字片断
            var textSegments = project.Tracks.Where(t => t.Type == "text").SelectMany(t => t.TextSegments).ToArray();


            // Iterate through all audio tracks and adjust their target timerange and speed
            foreach (var track in project.Tracks.Where(t => t.Type == "audio"))
            {
                foreach (var audioSgment in track.AudioSegments)
                {
                    //在所有文字中查找 当前音频片断对应的文字
                    var t = audioSgment.FindTextMaterial(project.Materials.Texts);
                    if (t == null)
                    {
                        Debug.WriteLine("出错了，没找到t!");
                        continue;
                    }
                    //在所有文字片断中查找 文字物料对应的文字片断
                    var txtSegment = textSegments.FirstOrDefault(v => v.MaterialID == t.Id);
                    if (txtSegment != null)
                    {
                        double speed = (double)audioSgment.SourceTimeRange.Duration / txtSegment.TargetTimerange.Duration;
                        speed = Math.Min(1.4f, speed);
                        if (speed > 1)
                        {
                            audioSgment.TargetTimeRange.Start = txtSegment.TargetTimerange.Start;
                            audioSgment.TargetTimeRange.Duration = txtSegment.TargetTimerange.Duration;
                            var s = project.Materials.Speeds.FirstOrDefault(v => audioSgment.Extra_Material_Refs.Contains(v.id));
                            if (s != null)
                            {
                                s.speed = (float)speed;
                            }
                            else
                            {
                                Debug.WriteLine("error not found!");
                            }
                        }
                        //audioSgment.Speed = speed;
                    }
                    //if (textDict.TryGetValue(audio.TextId, out var text))
                    {

                        // Calculate the speed and adjust the target timerange of the audio
                        //double speed = (double)audio.Duration / text.TargetTimerange.Duration;
                        //audio.TargetTimerange.Start = text.TargetTimerange.Start;
                        //audio.TargetTimerange.Duration = text.TargetTimerange.Duration;

                        //// Update the speed in the original JObject
                        //var audioJObject = project.Materials.Audios.First(a => a.Id == audio.Id);//._audio;
                        //audioJObject.Speed = speed;
                        ////audioJObject["speed"] = speed;
                    }
                }
            }

            // Write the updated JObject back to the file
            File.WriteAllText(outputFilePath, jsonObject.ToString());
        }
    }

    public class TrackAudioSegmentWrapper
    {
        private readonly JObject _audio;
        public TrackAudioSegmentWrapper(JObject audio)
        {
            _audio = audio;
            var p = _audio.Property("extra_material_refs");
            Extra_Material_Refs = ((JArray)_audio["extra_material_refs"]).Select(id => (string)id).ToArray();
        }
        public string Id => (string)_audio["id"];

        public string TextId => (string)_audio["text_id"];

        public TextMaterialWrapper FindTextMaterial(IEnumerable<TextMaterialWrapper> texts)
        {
            var x = texts.FirstOrDefault(t => t.TextToAudioIds.Any(v => v == Id));
            return x;
        }
        //extra_material_refs

        public string[] Extra_Material_Refs { get; }

        //public long Duration => (long)_audio["duration"];
        public double Speed { get => (double)_audio["speed"]; set => _audio["speed"] = value; }
        public TimeRangeWrapper TargetTimeRange => new TimeRangeWrapper((JObject)_audio["target_timerange"]);
        public TimeRangeWrapper SourceTimeRange => new TimeRangeWrapper((JObject)_audio["source_timerange"]);
    }

    public class TextMaterialWrapper
    {
        private readonly JObject _text;
        public TextMaterialWrapper(JObject text) { _text = text; }
        public string Id => (string)_text["id"];

        public string[] TextToAudioIds => ((JArray)_text["text_to_audio_ids"]).Select(id => (string)id).ToArray();
    }

    public class TrackTextSegmentWrapper
    {
        private readonly JObject _text;
        public TrackTextSegmentWrapper(JObject text) { _text = text; }
        public string Id => (string)_text["id"];
        public TimeRangeWrapper TargetTimerange => new TimeRangeWrapper((JObject)_text["target_timerange"]);

        public string MaterialID { get => _text["material_id"].Value<string>(); }
    }

    public class TrackWrapper
    {
        private readonly JObject _track;
        public TrackWrapper(JObject track)
        {
            _track = track;
            if (Type == "audio")
                this.AudioSegments = ((JArray)_track["segments"])
                        //.Where(seg => (string)seg["type"] == "audio")
                        .Select(seg => new TrackAudioSegmentWrapper((JObject)seg)).ToList();

            if (Type == "text")
                this.TextSegments = ((JArray)_track["segments"])
                        //.Where(seg => (string)seg["type"] == "text")
                        .Select(seg => new TrackTextSegmentWrapper((JObject)seg)).ToList();

        }
        public string Type => (string)_track["type"];
        public List<TrackAudioSegmentWrapper> AudioSegments { get; }

        public List<TrackTextSegmentWrapper> TextSegments { get; }
    }

    public class TimeRangeWrapper
    {
        private readonly JObject _timerange;
        public TimeRangeWrapper(JObject timerange) { _timerange = timerange; }
        public long Start
        {
            get => (long)_timerange["start"];
            set => _timerange["start"] = value;
        }
        public long Duration
        {
            get => (long)_timerange["duration"];
            set => _timerange["duration"] = value;
        }
    }

    public class ProjectWrapper
    {
        private readonly JObject _project;
        public ProjectWrapper(JObject project)
        {
            _project = project;

            Tracks = ((JArray)_project["tracks"]).Select(tr => new TrackWrapper((JObject)tr)).ToList();
            this.Materials = new MaterialWrapper((JObject)_project["materials"]);
        }

        public List<TrackWrapper> Tracks { get; }
        public MaterialWrapper Materials { get; }
    }

    public class MaterialWrapper
    {
        JObject material;
        public MaterialWrapper(JObject material)
        {
            this.material = material;
            this.Texts = ((JArray)material["texts"]).Select(t => new TextMaterialWrapper((JObject)t)).ToList();
            //Audios = ((JArray)this.material["audios"]).Select(a => new TrackAudioSegmentWrapper((JObject)a)).ToList();
            Speeds = ((JArray)this.material["speeds"]).Select(a => new SpeedWrapper((JObject)a)).ToList();
        }
        //public List<TrackAudioSegmentWrapper> Audios { get; }
        public List<TextMaterialWrapper> Texts { get; }

        public List<SpeedWrapper> Speeds { get; }
        //    {
        //    "curve_speed": null,
        //    "id": "9288736B-141E-4b87-A439-42409221A6A3",
        //    "mode": 0,
        //    "speed": 1.0,
        //    "type": "speed"
        //}
    }
    public class SpeedWrapper
    {
        JObject obj;
        public SpeedWrapper(JObject obj)
        {
            this.obj = obj;
        }
        //public object curve_speed { get; set; }
        public string id => obj["id"].Value<string>();
        public int mode { get => obj["mode"].Value<int>(); set => obj["mode"] = value; }
        public float speed { get => obj["speed"].Value<float>(); set => obj["speed"] = value; }
        public string type { get => obj["type"].Value<string>(); set => obj["type"] = value; }
    }
}
