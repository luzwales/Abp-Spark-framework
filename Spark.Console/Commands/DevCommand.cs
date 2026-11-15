using Spark.Console.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spark.Console.Commands;

public class DevCommand
{
    public void Execute()
    {
        ConsoleOutput.StartAlert(new List<string>() { "Starting dev server" });
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "watch",
            UseShellExecute = false
        });
        process?.WaitForExit();
    }
}
