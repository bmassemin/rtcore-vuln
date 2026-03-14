# rtcore-vuln

> **Work in progress** — This project is being developed alongside a blog article currently being written. Code and documentation may change significantly.

A BYOVD (Bring Your Own Vulnerable Driver) proof-of-concept that loads the RTCore64.sys kernel driver through the Windows Service Control Manager.

## Requirements

- Windows x64
- .NET 10 SDK
- Administrator privileges
- `RTCore64.sys` — The driver binary is **not included** in this repository. You'll need to find it yourself and place it at the root of the project.

## Usage

```bash
dotnet run
```
