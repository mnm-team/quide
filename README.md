# QuIDE 2.0

Quantum Integrated Development Environment

- fork of QuIDE 1.0.0 from [quide.eu](http://quide.eu/)
- refactored with [Avalonia](https://www.avaloniaui.net/) to be cross-compilable on Linux and Windows

## Run the app

- simply download the program in [Releases](https://github.com/mnm-team/quide/releases) section, there is a version for Windows and Linux precompiled
- to run the program on your machine, you only need the dotnet-8.0-runtime. It can be obtained by [Microsoft](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) or at your local packet manager, e.g. `dotnet-runtime-8.0` on Ubuntu.

## Compile everything by yourself

- Install the .NET-SDK 8.0, which can be found at [Microsoft](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) or at your local packet manager, e.g. `dotnet-sdk-8.0` on Ubuntu.
- Run `dotnet build --configuration Release` in the repository

### Start program

- Run `QuIDE/bin/Release/net8.0/QuIDE<.exe>`
- You can find the Quantum.dll at `Quantum/bin/Release/net8.0/Quantum.dll`

### Build single-file-app

- To build a single-file-app you can use dotnet publish, e.g.: `dotnet publish QuIDE/QuIDE.csproj -r <linux/win>-x64 -f net8.0 -p:PublishSingleFile=true --self-contained false -o out/`, then the App is at `out/QuIDE<.exe>` and all shared libraries (except dotnet-runtime) are included.

## Documentation

- User Documentation for Version 1.0.0 can be found [here](https://bitbucket.org/quide/quide/downloads/UserManual_EN.pdf), which should also be suitable for newer versions. 