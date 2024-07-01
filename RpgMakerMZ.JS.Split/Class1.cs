using System;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

class ProgramBing
{
    public static async Task Main(string[] args)
    {
        string query = "热搜";
        string searchUrl = $"https://www.bing.com/search?q={Uri.EscapeDataString(query)}&qs=n&form=QBRE&sp=-1&lq=0&pq=%E7%83%ADrvh&sc=10-4&sk=&cvid=5BBF107C0E994CCDAB8E14E2530F1153&ghsh=0&ghacc=0&ghpl=";

        try
        {
            string html = await FetchSearchResults(searchUrl);
            ParseAndDisplayResults(html);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
        }
    }

    static async Task<string> FetchSearchResults(string url)
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new Exception($"Failed to fetch search results. Status code: {response.StatusCode}");
            }
        }
    }

    static void ParseAndDisplayResults(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var results = doc.DocumentNode.SelectNodes("//li[@class='b_algo']");

        if (results != null)
        {
            foreach (var result in results)
            {
                var titleNode = result.SelectSingleNode(".//h2/a");
                var snippetNode = result.SelectSingleNode(".//div[@class='b_caption']//p");

                string title = titleNode?.InnerText ?? "No title";
                string url = titleNode?.GetAttributeValue("href", "No URL");
                string snippet = snippetNode?.InnerText ?? "No snippet";

                Console.WriteLine($"Title: {title}");
                Console.WriteLine($"URL: {url}");
                Console.WriteLine($"Snippet: {snippet}");
                Console.WriteLine("-------------------------------");
            }
        }
        else
        {
            Console.WriteLine("No results found.");
        }
    }
}