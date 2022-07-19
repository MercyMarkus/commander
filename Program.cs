using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Diagnostics;
using System.Net.NetworkInformation;

var cmdrRootCommand = new RootCommand();

cmdrRootCommand.Description = "CLI commands aggregator app.";

var saveResultOption = new Option<bool>(new[] { "--save-result", "-s" }, getDefaultValue: () => false, "Should speed test result be saved?");
var fileNameOption = new Option<string>(new[] { "--filename", "-f" }, getDefaultValue: () => "speed-results", "Json & CSV file name for speed test results.");

var speedCommand = new Command("speed", "runs a speed test")
{
    Handler = CommandHandler.Create<bool, string>((saveResult, fileName) =>
    {
        if (saveResult)
        {
            var formatDateTime = $"{DateTime.Now:yyyy-MM-ddTHH:mm:ss}";

            var constructSpeedTestObject = "{downloadSpeed: .downloadSpeed, uploadSpeed: .uploadSpeed, latency: .latency, " +
            "datetime: $dateTime, connectionType: $connectionType, location: .userLocation}";

            var filterSpeedTestOutput = $"jq --arg dateTime '{formatDateTime}' --arg connectionType '{GetConnectionType()}' '. | {constructSpeedTestObject}'";

            var saveSpeedTestOutput = $"fast --upload --json | {filterSpeedTestOutput} | Tee-Object -FilePath {fileName}.json -Append";

            var createCsvWithJq = "jq -r '(map(keys) | add | unique) as $cols | map(. as $row | $cols | map($row[.])) as $rows | $cols, $rows[] | @csv'";

            // TODO: upload csv to google sheet everytime the save command runs.
            var createSpeedTestCsv = $"Get-Content -Path .\\{fileName}.json -Raw | jq -s . | {createCsvWithJq} | Out-File {fileName}.csv";

            CommandRunner($"(npm list --global fast-cli || npm install --global fast-cli) && {saveSpeedTestOutput} && {createSpeedTestCsv}");
        }
        else
        {
            CommandRunner($"(npm list --global fast-cli || npm install --global fast-cli) && fast --json");
        }
    }),
};

cmdrRootCommand.AddCommand(speedCommand);
speedCommand.AddOption(saveResultOption);
speedCommand.AddOption(fileNameOption);

ChangeCommandsToLowerCase(args);
// Parse the incoming argument and invoke the handler
return cmdrRootCommand.Invoke(args);

static void CommandRunner(string command)
{
    var runProcess = new ProcessStartInfo("pwsh.exe", $"-Command {command}")
    {
        RedirectStandardInput = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    };

    Console.WriteLine($"PowerShell process started.");

    var process = Process.Start(runProcess);
    Console.WriteLine(process?.StandardOutput.ReadToEnd());
    process?.WaitForExit();

    if (process?.ExitCode != 0)
    {
        throw new Exception($"cmdr encountered an issue: {process?.StandardError.ReadToEnd()}");
    }

    process?.Close();
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

static void ChangeCommandsToLowerCase(string[] args)
{
    for (int i = 0; i < args.Length; i++)
    {
        args[i] = args[i].ToLower();
    }
}