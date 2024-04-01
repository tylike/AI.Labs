using DevExpress.DashboardCommon.DataProcessing;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using Microsoft.CognitiveServices.Speech;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AI.Labs.Module.BusinessObjects.TTS
{
    public class VoiceSolutionController : ObjectViewController<ObjectView, VoiceSolution>
    {
        public VoiceSolutionController()
        {
            var initialize = new SimpleAction(this, "初始列表", null);
            initialize.Execute += Initialize_Execute;

            var createAzure = new SimpleAction(this, "创建Azure声音", null);
            createAzure.Execute += CreateAzure_ExecuteAsync;
        }

        static string speechKey = "408ca52594684cc4b53e72f4c83bbc9b"; //Environment.GetEnvironmentVariable("SPEECH_KEY");
        static string speechRegion = "japanwest";// Environment.GetEnvironmentVariable("SPEECH_REGION");

        private async void CreateAzure_ExecuteAsync(object sender, SimpleActionExecuteEventArgs e)
        {
            var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
            // The language of the voice that speaks.
            speechConfig.SpeechSynthesisVoiceName = "en-US-JennyNeural";
            using (var speechSynthesizer = new SpeechSynthesizer(speechConfig))
            {
                var voices = await speechSynthesizer.GetVoicesAsync();
                //Microsoft.CognitiveServices.Speech.
                // Get text from the console and synthesize to the default speaker.
                //Console.WriteLine("Enter some text that you want to speak >");
                //string text = Console.ReadLine();
                //var speechSynthesisResult = await speechSynthesizer.SpeakTextAsync(text);
                //OutputSpeechSynthesisResult(speechSynthesisResult, text);
                var exist = ObjectSpace.GetObjectsQuery<VoiceSolution>().Where(t => t.Engine == VoiceEngine.AzureTTS).ToList();
                foreach (var item in voices.Voices)
                {
                    if (!exist.Any(t => t.DisplayName == item.Name))
                    {
                        var n = ObjectSpace.CreateObject<VoiceSolution>();
                        n.Name = item.Name;
                        n.DisplayName = item.Name;
                        n.Locale = item.Locale;
                        n.ShortName = item.ShortName;
                        n.Gender = Enum.Parse<Gender>(item.Gender.ToString());
                        n.Memo = item.LocalName;
                        n.VoicePersonalities = item.VoiceType.ToString() + ":" + string.Join(",", item.StyleList);

                        n.Engine = VoiceEngine.AzureTTS;
                        exist.Add(n);
                    }
                }
                ObjectSpace.CommitChanges();
            }
        }

        private void Initialize_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var path = GetType().Assembly.Location;
            var dir = new FileInfo(path);
            var text = File.ReadAllText(Path.Combine(dir.Directory.FullName, "Voice.json"));
            var json = (JArray)JsonConvert.DeserializeObject(text);
            foreach (JObject item in json.Children())
            {
                var obj = ObjectSpace.CreateObject<VoiceSolution>();
                obj.FriendlyName = item["FriendlyName"].ToString();
                obj.Gender = item["Gender"].ToString() == "Female" ? Gender.Female : Gender.Male;
                obj.Locale = item["Locale"].ToString();
                obj.Name = item["Name"].ToString();
                obj.DisplayName = item["ShortName"].ToString();
                obj.Status = item["Status"].ToString();
                obj.SuggestedCodec = item["SuggestedCodec"].ToString();
                var voiceTag = item["VoiceTag"];
                if (voiceTag != null && voiceTag.HasValues)
                {
                    if (voiceTag["ContentCategories"] != null)
                    {
                        obj.ContentCategories = voiceTag["ContentCategories"].ToString().Replace("\r\n", "");

                    }
                    if (voiceTag["ContentCategories"] != null)
                    {
                        obj.VoicePersonalities = voiceTag["VoicePersonalities"].ToString().Replace("\r\n", "");
                    }
                }
                obj.CommonlyUsed = obj.Locale.StartsWith("zh-");


            }
            ObjectSpace.CommitChanges();

        }
    }
}
