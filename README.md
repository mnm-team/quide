# QuIDE 2

Quantum Integrated Development Environment

[![.NET](https://github.com/mnm-team/quide/actions/workflows/dotnet.yml/badge.svg)](https://github.com/mnm-team/quide/actions/workflows/dotnet.yml)

- fork of QuIDE 1.0.0 from [quide.eu](http://quide.eu/)
- refactored with [Avalonia](https://www.avaloniaui.net/) to be cross-compilable on Linux, Windows and macOS.

## Run the app

- simply download the application in the [Releases](https://github.com/mnm-team/quide/releases) section, there is a version for Windows, Linux (amd64) and macOS (arm64) precompiled.
- to run the program on your machine, you only need the dotnet-8.0-runtime. It can be obtained by [Microsoft](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) or at your local packet manager, e.g. `dotnet-runtime-8.0` on Ubuntu.
- on Linux or macOS it can be necessary to set the file as executable for your user (`chmod +x <file>`) after unzipping.
- we don't have any certificate etc. for macOS, so you may need to start via [right-click](https://support.apple.com/guide/mac-help/open-a-mac-app-from-an-unidentified-developer-mh40616/mac).

## Compile by yourself

- Install the .NET-SDK 8.0, which can be found at [Microsoft](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) or at your local packet manager, e.g. `dotnet-sdk-8.0` on Ubuntu.
- Run `dotnet build --configuration Release` in the repository

### Start program

- Run `QuIDE/bin/Release/net8.0/QuIDE<.exe/.app>`
- You can find the Quantum.dll at `Quantum/bin/Release/net8.0/Quantum.dll`

### Build single-file-app

- To build a single-file-app (Linux & Windows) you can use dotnet publish, e.g.: `dotnet publish QuIDE/QuIDE.csproj -r <linux/win>-x64 -p:PublishSingleFile=true --self-contained false -o out/`, then the App is at `out/QuIDE<.exe>` and all shared libraries (except dotnet-runtime) are included.
- To build a .app on macOS you need two commands: `dotnet publish QuIDE/QuIDE.csproj -r osx-arm64` and `dotnet msbuild QuIDE/QuIDE.csproj -t:BundleApp -p:RuntimeIdentifier=osx-arm64`.

## Documentation

- User Documentation of version 1.0.0 can be found [here](https://bitbucket.org/quide/quide/downloads/UserManual_EN.pdf), until now there are no features in QuIDE 2 which would extend the described functionality.