using System;
using System.Diagnostics;
using System.IO;

namespace YoutubeExplode.Demo.Cli.Utils;
public class ProgressReporter : IProgress<double>, IDisposable
{
    Action<double> progress;
    public ProgressReporter(Action<double> progress)
    {
        this.progress = progress;
    }
    public void Dispose()
    {
        //throw new NotImplementedException();
    }

    public void Report(double value)
    {
        //Debug.WriteLine($"进度:{value}");
        progress?.Invoke(value);
    }
}

