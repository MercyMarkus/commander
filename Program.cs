using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Diagnostics;
using System.Net.NetworkInformation;

var baseWorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

var cmdrRootCommand = new RootCommand();

cmdrRootCommand.Description = "CLI commands aggregator app.";

var saveResultOption = new Option<bool>(new[] { "--save-result", "-s" }, getDefaultValue: () => false, "Should speed test result be saved?");
var fileNameOption = new Option<string>(new[] { "--filename", "-f" }, getDefaultValue: () => "speed-results2", "Json & CSV file name for speed test results.");

var httpsOption = new Option<bool>(new[] { "--https", "-s" }, getDefaultValue: () => false, "Use DNS over HTTPS (DoH) instead of DNS?");

var npmList = "npm list --global";
var npmInstall = "npm install --global";

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

            CommandRunner($"({npmList} fast-cli || {npmInstall} fast-cli) && {saveSpeedTestOutput} && {createSpeedTestCsv}", baseWorkingDirectory);

            CommandRunner($"Copy-Item -Path \"{combinedBaseDirectory($"{fileName}.csv")}\" -Destination \"{combinedBaseDirectory("python3env")}\"", baseWorkingDirectory);
            CommandRunner(".\\activate && cd ../ && python upload.py", combinedBaseDirectory($"python3env\\Scripts"));
        }
        else
        {
            CommandRunner($"({npmList} fast-cli || {npmInstall} fast-cli) && fast --json", baseWorkingDirectory);
        }
    }),
};

var wifiPasswordCommand = new Command("wifi", "checks your wifi password")
{
    Handler = CommandHandler.Create(() =>
        CommandRunner($"({npmList} wifi-password-cli || {npmInstall} wifi-password-cli) && wifi-password", baseWorkingDirectory)),
};

var publicIpCommand = new Command("ip", "checks your public IP address")
{
    Handler = CommandHandler.Create<bool>((https) =>
    {
        if (https)
        {
            CommandRunner($"({npmList} public-ip-cli || {npmInstall} public-ip-cli) && public-ip --https", baseWorkingDirectory);
        }
        else
        {
            CommandRunner($"({npmList} public-ip-cli || {npmInstall} public-ip-cli) && public-ip", baseWorkingDirectory);
        }
    }),
};

cmdrRootCommand.AddCommand(speedCommand);
cmdrRootCommand.AddCommand(wifiPasswordCommand);
cmdrRootCommand.AddCommand(publicIpCommand);

speedCommand.AddOption(saveResultOption);
speedCommand.AddOption(fileNameOption);
publicIpCommand.AddOption(httpsOption);

ChangeCommandsToLowerCase(args);
// Parse the incoming argument and invoke the handler
return cmdrRootCommand.Invoke(args);

static void CommandRunner(string command, string workingDirectory)
{
    Console.WriteLine($"Running command: {command}");

    var runProcess = new ProcessStartInfo("pwsh.exe", $"-Command {command}")
    {
        RedirectStandardInput = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        WorkingDirectory = workingDirectory,
    };

    var process = Process.Start(runProcess);
    Console.WriteLine(process?.StandardOutput.ReadToEnd());
    Console.WriteLine(process?.StandardError.ReadToEnd());
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
        var networkInterfaceType = adapter.NetworkInterfaceType.ToString();

        if (adapter.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
        {
            connectionType = networkInterfaceType[..8];
        }
        else
        {
            connectionType = networkInterfaceType;
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

static string combinedBaseDirectory(string secondDirectory)
{
    var baseWorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    var combinedBaseDir = Path.Combine(baseWorkingDirectory, secondDirectory);
    return combinedBaseDir;
}