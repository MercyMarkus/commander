using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Diagnostics;


var cmdrRootCommand = new RootCommand();

cmdrRootCommand.Description = "CLI commands aggregator app.";

var saveResultOption = new Option<bool>(new[] { "--save-result", "-s" }, getDefaultValue: () => false, "Should speed test result be saved?");

var speedCommand = new Command("speed", "runs a speed test")
{
    Handler = CommandHandler.Create<bool>((saveResult) =>
    {
        var jqCommand = "fast --upload --json | jq '[. | {downloadSpeed: .downloadSpeed, uploadSpeed: .uploadSpeed, latency:.latency}]' | Out-File speed-test-history.json";

        if (saveResult)
        {
            // Exiting out out Powershell process is not working
            CommandRunner($"(npm list --global fast-cli || npm install --global fast-cli) && {jqCommand} && Exit-PSSession");
        }

        CommandRunner($"(npm list --global fast-cli || npm install --global fast-cli) && fast --upload --json && Exit-PSSession");
    }),
};


cmdrRootCommand.AddCommand(speedCommand);
speedCommand.AddOption(saveResultOption);

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