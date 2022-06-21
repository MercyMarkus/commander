using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Diagnostics;


var rootCommand = new RootCommand();


rootCommand.Description = "CLI commands aggregator app.";

var speedCommand = new Command("-s", "runs a speed test")
{
    Handler = CommandHandler.Create(() =>
    {
        CommandRunner($"(npm list --global fast-cli || npm install --global fast-cli) && fast --upload --json");
        CommandRunner("exit");
    })
};

static void CommandRunner(string command)
{
    // Test on MacOS too
    var runProcess = new ProcessStartInfo
    {
        // 'C:\Program Files\PowerShell\7\pwsh.exe'
        FileName = "pwsh.exe",
        RedirectStandardInput = true,
    };

    Console.WriteLine($"Process started.");

    var commandProcess = Process.Start(runProcess);
    commandProcess?.StandardInput.WriteLine(command);
    commandProcess?.WaitForExit();
    commandProcess?.Close();
}

rootCommand.AddCommand(speedCommand);

// Parse the incoming argument and invoke the handler
return rootCommand.Invoke(args);