using System;
using System.Diagnostics;
using System.IO;

namespace YoutubeExplode.Demo.Cli.Utils;
public class ProgressReporter : IProgress<double>, IDisposable
{
    public void Dispose()
    {
        //throw new NotImplementedException();
    }

    public void Report(double value)
    {
        Debug.WriteLine($"进度:{value}");
    }
}
internal class ConsoleProgress : IProgress<double>, IDisposable
{
    TextWriter writer;
    public ConsoleProgress(TextWriter writer)
    {
        this.writer = writer;
    }
    private readonly int _posX = Console.CursorLeft;
    private readonly int _posY = Console.CursorTop;

    private int _lastLength;

    public ConsoleProgress()
        : this(Console.Out) { }

    private void EraseLast()
    {
        if (_lastLength > 0)
        {
            Console.SetCursorPosition(_posX, _posY);
            writer.Write(new string(' ', _lastLength));
            Console.SetCursorPosition(_posX, _posY);
        }
    }

    private void Write(string text)
    {
        EraseLast();
        writer.Write(text);
        _lastLength = text.Length;
    }

    public void Report(double progress) => Write($"{progress:P1}");

    public void Dispose() => EraseLast();
}
