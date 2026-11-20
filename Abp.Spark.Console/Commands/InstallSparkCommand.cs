using Spark.Console.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using static System.Net.WebRequestMethods;

namespace Spark.Console.Commands;

public struct InstallSparkCommand
{
    public void Execute()
    {
        ConsoleOutput.StartAlert(new List<string>() { "Installing Abp.Spark" });
        Process.Start("dotnet", "new install Abp.Spark.Templates").WaitForExit();
        ConsoleOutput.SuccessAlert(new List<string>() { "Abp.Spark was installed! To learn more visit our docs." });
    }
}
