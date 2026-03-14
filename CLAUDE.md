# rtcore-vuln

BYOVD (Bring Your Own Vulnerable Driver) tool that loads RTCore64.sys via the Windows Service Control Manager.

## Commands

```bash
dotnet build              # Build the project
dotnet run                # Build and run (requires admin)
```

## Architecture

- `Program.cs` — Entry point: installs driver, waits for input, uninstalls
- `DriverService.cs` — SCM wrapper: create/start/stop/delete kernel driver services
- `RTCore64.sys` — Vulnerable kernel driver binary (copied to output on build)
- `NativeMethods.txt` — CsWin32 P/Invoke generation list
- `app.manifest` — Requests `requireAdministrator` elevation

## Requirements

- Windows x64
- .NET 10 SDK
- Must run as Administrator (driver installation requires SCM access)

## Code Style

- P/Invoke bindings are generated at compile time by CsWin32 — types like `CloseServiceHandleSafeHandle` come from `NativeMethods.txt` entries, not from any `.cs` file
- To add new Win32 APIs, add the function/constant name to `NativeMethods.txt`

## Gotchas

- Kernel drivers do NOT appear in `services.msc` — use `sc query RTCoreVulnDemo` instead
- If the program crashes mid-run, the service may be left registered; `InstallDriver` handles this by cleaning up leftover services automatically
- `RTCore64.sys` must be at an absolute path when passed to `CreateService`
- Manual cleanup if needed: `sc stop RTCoreVulnDemo && sc delete RTCoreVulnDemo`
