using System.Text;
using Newtonsoft.Json;

namespace AI.Labs.Module.Translate
{
    public class MicrosoftTranslate
    {
        private static readonly string key = "";//"8c42c792e54f4e5a9b7933e33ac0e837";
        private static readonly string endpoint = "https://api.cognitive.microsofttranslator.com";

        // location, also known as region.
        // required if you're using a multi-service or regional (not global) resource. It can be found in the Azure portal on the Keys and Endpoint page.
        private static readonly string location = "japaneast";

        public static async Task<string> Main(string text,bool ZhToEn = true)
        {
            return text;
            // Input and output languages are defined as parameters.
            string route = "/translate?api-version=3.0&from=zh-cn&to=en";
            if(!ZhToEn)
            {
                route = "/translate?api-version=3.0&from=en&to=zh-cn";
            }
            string textToTranslate = text;// "I would really like to drive your car around the block a few times!";
            object[] body = new object[] { new { Text = textToTranslate } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                // Build the request.
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(endpoint + route);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", key);
                // location required if you're using a multi-service or regional (not global) resource.
                request.Headers.Add("Ocp-Apim-Subscription-Region", location);

                // Send the request and get response.
                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                // Read response as a string.
                string result = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<List<Results>>(result);

                return string.Join("", obj.SelectMany(t => t.translations.SelectMany(t => t.text)));
                //Console.WriteLine(result);
            }
        }
    }
    public class Results
    {
        public List<Translations> translations { get; set; }
    }
    public class Translations
    {
        public string text { get; set; }
        public string to { get; set; }
    }
}
//[
//    {"translations":
//    [
//    {
//    "text":"If there is a surplus of teachers in the future, will we consider setting up a full-time class teacher position to reduce the burden of the class teacher as both a classroom teacher and a class teacher?\r\nWhy is there a surplus? How can there be a surplus?\r\nI have found that many people only consider the variable of declining total number of students when counting the number of teachers and students, and do not take into account the significant spillover of students in our secondary education over the past two decades or more.\r\nIn the past, a class could reach about 80 students, but now the average middle school teaching class has about 50 students. In actual teaching, the class size of about 50 students is also too large, and it is difficult for teachers to take care of it in teaching management. Some so-called top classes simply set up a class type of 30 people, which is convenient for teachers to manage.\r\nTherefore, the trend for some time to come will only be to achieve a normal teacher-student ratio, not some surplus.\r\nAs for the full-time class teacher, I personally think it is still very necessary, but whether the setting is too much with the teacher is probably difficult to form a causal relationship.",
//    "to":"en"
//    }
//    ]
//    }
//    ]