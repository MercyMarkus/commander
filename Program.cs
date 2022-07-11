using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Diagnostics;
using System.Net.NetworkInformation;

var cmdrRootCommand = new RootCommand();

cmdrRootCommand.Description = "CLI commands aggregator app.";

var saveResultOption = new Option<bool>(new[] { "--save-result", "-s" }, getDefaultValue: () => false, "Should speed test result be saved?");

var speedCommand = new Command("speed", "runs a speed test")
{
    Handler = CommandHandler.Create<bool>((saveResult) =>
    {
        // TODO: make file names an option but also set a default value.
        var connectionType = GetConnectionType();

        var jqFilterCmd = $"fast --upload --json" +
        $" | jq --arg dateTime '{DateTime.Now:yyyy-MM-ddTHH:mm:ss}' --arg connectionType '{connectionType}'" +
        $" '. | {{downloadSpeed: .downloadSpeed, uploadSpeed: .uploadSpeed, latency:.latency," +
        $" datetime: $dateTime, connectionType: $connectionType}}' | Out-File speed-results.json -Append";

        // TODO: upload csv to google sheet everytime the save command runs. Use variables to cleanup jq commands/make them more informative
        var jqCreateCsvCmd = $"Get-Content -Path .\\speed-results.json -Raw | jq -s ." +
        $" | jq -r '(map(keys) | add | unique) as $cols | map(. as $row | $cols | map($row[.])) as $rows | $cols, $rows[] | @csv'" +
        $" | Out-File speed-history.csv";

        if (saveResult)
        {
            CommandRunner($"(npm list --global fast-cli || npm install --global fast-cli) && {jqFilterCmd} && {jqCreateCsvCmd}");
        }
        else
        {
            CommandRunner($"(npm list --global fast-cli || npm install --global fast-cli) && fast --json");
        }
    }),
};


cmdrRootCommand.AddCommand(speedCommand);
speedCommand.AddOption(saveResultOption);

// Parse the incoming argument and invoke the handler
return cmdrRootCommand.Invoke(args);

static void CommandRunner(string command)
{
    // Test on MacOS too
    var runProcess = new ProcessStartInfo("pwsh.exe", $"-Command {command}");
    runProcess.RedirectStandardInput = true;

    Console.WriteLine($"PowerShell process started.");

    var powerShellProcess = Process.Start(runProcess);
    powerShellProcess?.WaitForExitAsync(default);
    powerShellProcess?.Close();
}

static string GetConnectionType()
{
    var connectionType = string.Empty;

    NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();

    foreach (NetworkInterface adapter in adapters.Where(a => a.OperationalStatus == OperationalStatus.Up
        && (a.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || a.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
        && !a.Name.StartsWith("vEthernet")))
    {
        if (adapter.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
        {
            connectionType = adapter.NetworkInterfaceType.ToString().Substring(0, 8);
        }
        else
        {
            connectionType = adapter.NetworkInterfaceType.ToString();
        }
    }
    return connectionType;
}