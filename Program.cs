// See https://aka.ms/new-console-template for more information
using System.CommandLine;

// Create name option:
var nameOption = new Option<string>(
    new[] { "--name", "-n" },
    description: "A 'name' option whose argument is a string representing a name.");

nameOption.IsRequired = true;
nameOption.SetDefaultValue("World");

var rootCommand = new RootCommand
{
    nameOption,
};

rootCommand.Description = "A Hello Greeter App";

rootCommand.SetHandler((string name) =>
{
    Console.WriteLine($"The value for --name is: {name}");
    Console.WriteLine($"Hello, {name}!");
}, nameOption);

// Parse the incoming argument and invoke the handler
return rootCommand.Invoke(args);
