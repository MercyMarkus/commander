using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Diagnostics;


var rootCommand = new RootCommand();


rootCommand.Description = "A Hello Greeter App";

var speedCommand = new Command("-s", "runs a speed test")
{
    Handler = CommandHandler.Create(() =>
    {
        CommandRunner($"speed-test --json && exit");
    })
};

static void CommandRunner(string command)
{
    var runProcess = new ProcessStartInfo
    {
        FileName = "cmd",
        RedirectStandardInput = true,
    };

    Console.WriteLine($"Process started.");

    var npmProcess = Process.Start(runProcess);
    npmProcess?.StandardInput.WriteLine(command);
    npmProcess?.WaitForExit();
    npmProcess?.Close();
}

rootCommand.AddCommand(speedCommand);

// Parse the incoming argument and invoke the handler
return rootCommand.Invoke(args);