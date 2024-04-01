using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

public class WebCrawler
{
    private HashSet<string> _visited = new HashSet<string>();
    private string _baseDomain;
    private string _baseDirectory;

    public WebCrawler(string baseDomain, string baseDirectory)
    {
        _baseDomain = baseDomain;
        _baseDirectory = baseDirectory;
    }

    public void DownloadWebsite(string url)
    {
        // Normalize the URL to handle cases like "../"
        string normalizedUrl = new Uri(new Uri(_baseDomain), url).AbsoluteUri;

        // Check if the normalized URL is already visited
        if (_visited.Contains(normalizedUrl))
            return;

        // Add to visited URLs
        _visited.Add(normalizedUrl);

        var web = new HtmlWeb();
        var sw = Stopwatch.StartNew();
        Console.WriteLine(DateTime.Now + ":" + normalizedUrl);
        var doc = web.Load(normalizedUrl);
        sw.Stop();
        Console.WriteLine(" 完成!" + sw.Elapsed);

        var baseUri = new Uri(normalizedUrl);

        // Create directory structure
        string directoryPath = Path.Combine(_baseDirectory, baseUri.Host + baseUri.LocalPath.TrimEnd('/'));
        var fi = new FileInfo(directoryPath);
        directoryPath = fi.Directory.FullName;
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        List<string> files = new List<string>();
        // Download linked pages
        if (doc.DocumentNode.SelectNodes("//a[@href]") != null)
        {
            foreach (var link in doc.DocumentNode.SelectNodes("//a[@href]"))
            {
                string hrefValue = link.GetAttributeValue("href", string.Empty);

                if (string.IsNullOrEmpty(hrefValue) || hrefValue == "/" || hrefValue.StartsWith("#") || hrefValue.StartsWith("mailto:") || hrefValue.StartsWith("javascript:"))
                    continue;

                // Ensure the link is absolute and belongs to the same base domain
                Uri hrefUri;
                if (!Uri.TryCreate(baseUri, hrefValue, out hrefUri))
                {
                    Console.WriteLine("无效:" + hrefValue);
                    continue; // Invalid URI
                }
                if (hrefValue.StartsWith("http"))
                {
                    if (!hrefValue.StartsWith(_baseDomain))
                    {
                        Console.WriteLine("外站:" + hrefValue);
                        continue; // External link
                    }
                }

                // Remove the query string and fragment part if present
                string absoluteUrl = hrefUri.GetLeftPart(UriPartial.Path);

                // Recursively download the linked page
                //DownloadWebsite(absoluteUrl);
                files.Add(absoluteUrl);
                // Update href to point to local file structure
                link.SetAttributeValue("href", GetRelativeFilePath(baseUri, new Uri(absoluteUrl)));
            }
        }

        // Save the updated document
        string filePath = fi.FullName.ToLower();// Path.Combine(directoryPath, "index.html");
        if(!(filePath.EndsWith(".html") || filePath.EndsWith(".htm")))
        {
            filePath = Path.Combine(directoryPath, "index.html");
        }

        doc.Save(filePath);
        foreach (var item in files)
        {
            DownloadWebsite(item);
        }
    }

    private string GetRelativeFilePath(Uri baseUri, Uri fileUri)
    {
        string relativePath = Uri.UnescapeDataString(baseUri.MakeRelativeUri(fileUri).ToString());
        relativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);
        return relativePath;
    }
}

class Downloader
{
    public static void Main()
    {
        string websiteUrl = "https://docs.llamaindex.ai/en/latest/"; // Your website URL
        string downloadPath = Path.Combine(Directory.GetCurrentDirectory(), "DownloadedWebsite");

        var crawler = new WebCrawler(websiteUrl, downloadPath);
        crawler.DownloadWebsite(websiteUrl);
    }
}