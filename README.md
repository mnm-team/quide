# QuIDE 2

Cross Platform Quantum Circuit Simulator

[![.NET](https://github.com/mnm-team/quide/actions/workflows/dotnet.yml/badge.svg)](https://github.com/mnm-team/quide/actions/workflows/dotnet.yml)

- fork of QuIDE 1.0.0 from [quide.eu](http://quide.eu/)
- refactored with [Avalonia](https://www.avaloniaui.net/) to be cross-compilable on Linux, Windows and macOS.
- maintained by [MNM-Team](https://www.mnm-team.org) at LMU Munich.

## Run the app

- simply download the application in the [Releases](https://github.com/mnm-team/quide/releases) section, there is a version for Windows, Linux (amd64) and macOS (arm64) precompiled.
- to run the program on your machine, you need the dotnet-8.0-runtime. It can be obtained by [Microsoft](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) or at your local packet manager, e.g. `dotnet-runtime-8.0` on Ubuntu.
- on Linux it can be necessary to set the file as executable for your user (`chmod +x QuIDE`) after unzipping.
- we don't have a developer ID for macOS, so you may need to use the following command: `xattr -cr /Applications/QuIDE.app` after installation (especially when macOS tells the app is broken).

## Compile by yourself

- Install the .NET-SDK 8.0, which can be found at [Microsoft](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) or at your local packet manager, e.g. `dotnet-sdk-8.0` on Ubuntu.
- Run `dotnet build --configuration Release` in the repository

### Start program

- Run `QuIDE/bin/Release/net8.0/QuIDE<.exe/.app>`
- You can find the Quantum.dll at `Quantum/bin/Release/net8.0/Quantum.dll`

### Build single-file-app

- To build a single-file-app (Linux & Windows) you can use dotnet publish, e.g.: `dotnet publish QuIDE/QuIDE.csproj -r <linux/win>-x64 -p:PublishSingleFile=true --self-contained false -o out/`, then the App is at `out/QuIDE<.exe>` and all shared libraries (except dotnet-runtime) are included.
- To build a .app on macOS you need two commands: `dotnet publish QuIDE/QuIDE.csproj -r osx-arm64` and `dotnet msbuild QuIDE/QuIDE.csproj -t:BundleApp -p:RuntimeIdentifier=osx-arm64 -property:Configuration=Release -p:UseAppHost=true`. Then you find it at `QuIDE/bin/Release/net8.0/osx-arm64/publish/QuIDE.app`.

## Documentation

- User Documentation of version 1.0.0 can be found [here](https://bitbucket.org/quide/quide/downloads/UserManual_EN.pdf), until now there are no features in QuIDE 2 which would extend the described functionality.

## Contributing

Feel free to open PRs, open issues and more. We are happy about everbody who helps to make this tool better.