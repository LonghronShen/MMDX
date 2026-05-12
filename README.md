# MMDX
Mikumiku Dance for XNA/MonoGame

## Current Progress
The MonoGame branch compiles well, but failed to run due to some capability problems such as removal of PreShader in MonoGame.

## Linux Build (net40 with Mono + dotnet SDK 6)

The `monogame` branch targets `net40` and uses the legacy `Microsoft.Bcl.Async` package,
which depends on `Microsoft.Bcl.Build`. The `Microsoft.Bcl.Build.targets` file contains
a `<UsingTask>` that loads `Microsoft.Build.Utilities.v4.0` — an assembly only available
in Mono's GAC, not in modern dotnet SDK MSBuild (16.x+).

### Prerequisites

1. **Mono 6.12+** from the [official repository](https://www.mono-project.com/download/stable/):
   ```bash
   sudo apt install gnupg ca-certificates
   sudo gpg --homedir /tmp --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
   sudo sh -c 'echo "deb https://download.mono-project.com/repo/ubuntu stable-focal main" > /etc/apt/sources.list.d/mono-official-stable.list'
   sudo apt update
   sudo apt install mono-complete
   ```

2. **dotnet SDK 6.x** (for NuGet restore and SDK resolution):
   ```bash
   # Download from https://dotnet.microsoft.com/download/dotnet/6.0
   # or use the install script:
   wget https://dot.net/v1/dotnet-install.sh -O /tmp/dotnet-install.sh
   chmod +x /tmp/dotnet-install.sh
   /tmp/dotnet-install.sh --channel 6.0
   ```

### Build

```bash
export PATH="$HOME/.dotnet:$PATH"
export DOTNET_ROOT="$HOME/.dotnet"

# Restore NuGet packages (handled automatically by msbuild, but pre-restoring helps)
dotnet restore -p:TargetFramework=net40 EntryPoints/MikumikuDance.Windows/MikumikuDance.Windows.csproj

# Build with Mono msbuild
msbuild /p:TargetFramework=net40 /p:Configuration=Debug \
  EntryPoints/MikumikuDance.Windows/MikumikuDance.Windows.csproj
```

Build artifacts will be placed in:
- `EntryPoints/MikumikuDance.Windows/bin/Debug/net40/MikumikuDance.Windows.exe`
- `Framework/*/bin/Debug/net40/*.dll` (6 framework assemblies)

### Why Mono msbuild?

The `global.json` in this repo pins the dotnet SDK to version 6.x and specifies
`MSBuild.Sdk.Extras` version 3.0.44. This ensures Mono's msbuild (v16.10.1) can:
- Parse SDK-style `.csproj` files (`<Project Sdk="MSBuild.Sdk.Extras">`)
- Resolve the `EnsureBindingRedirects` task from `Microsoft.Bcl.Build`
  (requires `Microsoft.Build.Utilities.v4.0.dll` in Mono's GAC)
- Compile the net40 target with all original NuGet dependencies intact

## Windows Build

Building on Windows requires:
- Visual Studio or dotnet SDK
- MonoGame. WindowsDX.9000 NuGet package (custom fork)

```bash
dotnet build -f net461 -c Debug EntryPoints/MikumikuDance.Windows/MikumikuDance.Windows.csproj
```
