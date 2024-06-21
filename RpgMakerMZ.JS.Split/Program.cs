using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    const string inputDirectory = @"D:\WalkMan\RPG\RPG.Client\wwwroot\js\";
    const string outputDirectory = @"D:\WalkMan\RPG\RPG.Client\split.js";
    static void Main(string[] args)
    {
        string directoryPath = inputDirectory;
        Directory.CreateDirectory(outputDirectory);
        string[] jsFiles = Directory.GetFiles(directoryPath, "*.js");

        foreach (string file in jsFiles)
        {
            var lines = File.ReadAllLines(file);
            var cnt = new StringBuilder();
            var fileName = Path.Combine(outputDirectory, "F_" + Path.GetFileName(file));
            int i = 0;
            foreach (var item in lines)
            {
                if (item.StartsWith("//----"))
                {
                    Console.WriteLine(fileName);
                    try
                    {
                        File.WriteAllText(fileName, cnt.ToString());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{fileName}\n{cnt.ToString()}\n {ex.Message}");
                    }
                    cnt = new StringBuilder();
                    var fn = "";
                    if (lines.Length > i + 1)
                    {
                        fn = "F_" + lines[i + 1][3..].Trim() + ".js";
                    }
                    if (fn == "F_.js")
                        fn = i.ToString() + ".js";
                    fileName = Path.Combine(outputDirectory, fn);
                }
                cnt.AppendLine(item);
                i++;
            }
        }
    }
}
