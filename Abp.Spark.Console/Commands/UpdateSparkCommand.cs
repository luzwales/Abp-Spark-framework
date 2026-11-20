using Spark.Console.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spark.Console.Commands;

public class UpdateSparkCommand
{
    public void Execute()
    {
        ConsoleOutput.StartAlert(new List<string>() { "Updating Abp.Spark Templates" });
        Process.Start("dotnet", "new install Abp.Spark.Templates").WaitForExit();
        ConsoleOutput.SuccessAlert(new List<string>() { "Abp.Spark Templates successfully updated" });
    }
}
