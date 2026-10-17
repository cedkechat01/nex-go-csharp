# NEX Go C#

This repository is a C# porting project based on the `splatoon` branch of PretendoNetwork/nex-go.

It focuses on the core protocol pieces needed for a working NEX-like stack in C#:

- StreamIn / StreamOut
- Packet
- RmcRequest / RmcResponse
- Client / Server skeletons
- Core utility types

This is a direct C# adaptation of the original Go implementation, intended to serve as a base for a full protocol port.

## Build

```bash
dotnet build
```

## Project structure

- `src/NexGoCSharp/` - main library project
  - `Core/` - low-level protocol objects
  - `Net/` - client/server skeletons

## Notes

This is not a blind line-by-line conversion of the full original repo. It is a practical C# foundation derived from the `splatoon` branch and designed to be extendable for a complete NEX implementation.
