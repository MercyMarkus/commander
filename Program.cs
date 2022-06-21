using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Diagnostics;


var cmdrRootCommand = new RootCommand();

cmdrRootCommand.Description = "CLI commands aggregator app.";

var speedCommand = new Command("speed", "runs a speed test")
{
    Handler = CommandHandler.Create(() =>
    {
        CommandRunner($"(npm list --global fast-cli || npm install --global fast-cli) && fast --upload --json");
        CommandRunner("exit");
    })
};

cmdrRootCommand.AddCommand(speedCommand);

// Parse the incoming argument and invoke the handler
return cmdrRootCommand.Invoke(args);


static void CommandRunner(string command)
{
    // Test on MacOS too
    var runProcess = new ProcessStartInfo
    {
        // 'C:\Program Files\PowerShell\7\pwsh.exe'
        FileName = "pwsh.exe",
        RedirectStandardInput = true,
    };

    Console.WriteLine($"PowerShell process started.");

    var powerShellProcess = Process.Start(runProcess);
    powerShellProcess?.StandardInput.WriteLine(command);
    powerShellProcess?.WaitForExit();
    powerShellProcess?.Close();
}