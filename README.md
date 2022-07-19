## Commander

Commander is a CLI tool aggregator that allows me customize my favorite CLI commands.

## How to Install
1. Clone the repo.
2. Open in Visual Studio (or an IDE of your choice) and build the solution.
3. Open a terminal window pointing to the location of the cloned project.
4. In the terminal, install commander as a dotnet tool by running `dotnet tool install --global --add-source ./bin/Debug Commander --version 1.0.0`. 
5. Commander can be invoked as a dotnet tool by running `cmdr` from a terminal shell.

## Cmdr Commands
1. speed: Running `cmdr speed` results in:
```shell
PowerShell process started.
C:\Users\mercymarkus\AppData\Roaming\npm
`-- fast-cli@3.2.0
{
        "downloadSpeed": 5.8,
        "downloaded": 9.3,
        "latency": 129,
        "bufferBloat": 331,
        "userLocation": "Kaduna, NG",
        "userIp": "108.86.39.54"
}
```