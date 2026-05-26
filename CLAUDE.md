# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

ModVerify is a .NET CLI tool that statically analyzes mods for *Star Wars: Empire at War* and *Forces of Corruption* by reimplementing parts of the Petroglyph Alamo engine in managed code and running verifiers over the loaded game data. It produces JSON + text reports of XML errors, missing assets, broken model references, malformed SFX samples, etc.

Vanilla EaW is currently not supported; FoC is the primary target.

## Build / test / run

The repo uses the Microsoft Testing Platform runner (configured via `global.json`), not VSTest. `dotnet test` works, but the test project is also an `Exe` and can be invoked directly.

```bash
# Restore submodules (required — ModdingToolBase is a git submodule)
git submodule update --init --recursive

# Build everything
dotnet build ModVerify.slnx

# Run all tests (Release matches CI)
dotnet test --configuration Release

# Run the CLI from source
dotnet run --project src/ModVerify.CliApp -- verify --path "<mod path>" --useDefaultBaseline

# Build the Windows self-updating release (.NET Framework 4.8.1, Costura-packed single exe)
dotnet build src/ModVerify.CliApp/ModVerify.CliApp.csproj -c Release -f net481

# Build the cross-platform release (.NET 10)
dotnet publish src/ModVerify.CliApp/ModVerify.CliApp.csproj -c Release -f net10.0
```

The CLI has two verbs: `verify` and `createBaseline`. Running with no arguments enters interactive mode and prompts for a target. See `README.md` for full option examples.

`deploy-local.ps1` builds the net481 exe, generates an update manifest, and stages it under `.local_deploy/` so the self-update flow can be exercised end-to-end without hitting the real CDN.

## Target frameworks and why

- `src/ModVerify` (the library): `netstandard2.0;netstandard2.1;net10.0` — keeps the core consumable from older tooling.
- `src/ModVerify.CliApp`: `net10.0;net481`. The **net481** build is the user-facing Windows executable (Costura-Fody packs all dependencies into a single `ModVerify.exe`, and the AnakinRaW self-updater is wired in only on this TFM). The **net10.0** build is the cross-platform / CI variant, has no self-updater, and is shipped as a framework-dependent zip (Linux/macOS users run it via `dotnet`).
- Test project: net10.0 always; net481 only on Windows.

When editing the CLI app, be aware that `IsUpdatable()` is conditional on the net481 build — code paths guarded by `IsUpdateableApplication` or `_applicationEnvironment.IsUpdatable()` only run there. PolySharp + `Microsoft.Bcl.Memory` + `IndexRange` are pulled in to make modern C# language features compile against net481.

## Architecture

### Two-layer split: `ModVerify` (library) vs `ModVerify.CliApp` (executable)

The library knows nothing about command-line parsing, console output, file I/O for results, or the updater. The CLI app composes the library with logging (Serilog), DI (`Microsoft.Extensions.DependencyInjection`), the AnakinRaW `ApplicationBase` lifecycle, and reporters. When adding functionality, decide which layer it belongs in: anything reusable by another host (CI integration, GUI, etc.) goes in `ModVerify`; anything tied to console UX, command-line verbs, or the Windows updater goes in `ModVerify.CliApp`.

### Verification pipeline

`GameVerifierService.VerifyAsync` is the entry point from `IGameVerifierService`. It builds a `GameVerifyPipeline` (a `StepRunnerPipelineBase<AsyncStepRunner>` from `AnakinRaW.CommonUtilities.SimplePipeline`) which:

1. Initializes a re-implementation of the game engine (`IPetroglyphStarWarsGameEngine` from the `PG.StarWarsGame.Engine` project under `src/PetroglyphTools/`) against the target's location/engine type. Engine init errors are collected by `GameEngineErrorCollector`, which becomes the first pipeline step.
2. Asks `IGameVerifiersProvider` (default: `DefaultGameVerifiersProvider`) for the set of `GameVerifier` instances to run.
3. Wraps each verifier in a `GameVerifierPipelineStep` and runs them via `AsyncStepRunner` (parallelism controlled by `VerifierServiceSettings.ParallelVerifiers`; 1 falls back to a `SequentialStepRunner`).
4. Aggregates all `VerificationError`s, then applies `VerificationBaseline` and `SuppressionList` filters before returning a `VerificationResult`.

`FailFast` mode is non-trivial: the pipeline checks each thrown `GameVerificationException` against the baseline + suppressions and *swallows* it if every error is already accounted for, so fail-fast does not abort on known issues. Don't simplify this away.

When adding a new verifier: subclass `GameVerifier` (or `GameVerifier<T>` / `NamedGameEntityVerifier`), register it in `DefaultGameVerifiersProvider`, and pick a unique error code prefix (see `Verifiers/VerifierErrorCodes.cs`). `IAlreadyVerifiedCache` (registered via `RegisterVerifierCache()`) is shared across verifiers — use it to skip work that's already been done in the same run (e.g. the same texture being referenced by multiple GameObjects).

### Baselines

A baseline is a frozen JSON snapshot of "errors we already knew about, ignore them." `VerificationBaseline.LatestVersion` is **2.2** and the schema lives at `src/ModVerify/Resources/Schemas/2.2/baseline.json` (embedded). When bumping the baseline format, add a new versioned schema folder rather than mutating the existing one — old baselines on user disks must keep parsing.

`src/ModVerify.CliApp/Resources/Baselines/baseline-foc.json` is the embedded default baseline shipped with the CLI (selected via `--useDefaultBaseline`). The legacy top-level `focBaseline.json` is *not* the embedded one and exists separately; don't conflate them.

### Settings flow (CLI)

```
argv → ModVerifyOptionsParser → ModVerifyOptionsContainer
     → SettingsBuilder.BuildSettings → AppVerifySettings | AppBaselineSettings
     → ModVerifyApplication → VerifyAction | CreateBaselineAction
```

`SelfUpdateableAppLifecycle` (from ModdingToolBase) drives `InitializeAppAsync` → `CreateAppServices` → `RunAppAsync`. The app argument parser strips trailing arguments injected by the external updater (see `ModVerifyOptionsParser.StripExternalUpdateResults`) before handing off to CommandLineParser — needed so unknown-argument errors stay strict.

`VerifyVerbOption.WithoutArguments` is the sentinel returned when the user just double-clicks the exe; this triggers interactive target selection in `ConsoleSelector` and enables the interactive update prompt.

### Logging

Two Serilog sinks, configured in `Program.ConfigureLogging`:

- **Console sink** filters by an `EventId.Id == ModVerifyConstants.ConsoleEventIdValue` expression in normal mode (so verifier output stays clean), shows everything from the `AET.ModVerify` namespace at Debug level, and shows everything at Verbose. Fatals are excluded — they're handled by the global exception handler instead.
- **File sink** writes to `ApplicationLocalPath/ModVerify_log.txt` and excludes XML-parser noise (the engine and `XmlFileParser<>` namespaces are reported by the verification pipeline).

When emitting log lines that the user *should* see in normal operation, attach `ModVerifyConstants.ConsoleEventId`. Plain `Logger.LogInformation(...)` without that EventId will be silently dropped at the console.

### Petroglyph engine reimplementation (`src/PetroglyphTools/`)

These projects (`PG.StarWarsGame.Engine`, `PG.StarWarsGame.Engine.FileSystem`, `PG.StarWarsGame.Files.ALO`, `PG.StarWarsGame.Files.ChunkFiles`, `PG.StarWarsGame.Files.XML`, etc.) are vendored copies that will eventually move to the standalone `AlamoEngine-Tools/PetroglyphTools` repo (per `src/PetroglyphTools/README.md`). Treat them as a separate library boundary — changes here may need to land upstream.

### Submodule

`modules/ModdingToolBase` is a git submodule (https://github.com/AnakinRaW/ModdingToolBase). It supplies the shared `ApplicationBase`, the AppUpdaterFramework, the manifest creator, and the FTP uploader used by the deploy pipeline. If you're touching the app lifecycle, updater, or release packaging, the relevant code likely lives there, not in this repo.

## CI

`.github/workflows/test.yml` builds + tests on Windows and Linux against .NET 10. `release.yml` (triggered on push to `main`) builds the two release artifacts (net481 self-updating exe, net10.0 portable zip), generates an update manifest via `ApplicationManifestCreator`, SFTPs the self-update bits to republicatwar.com, and creates a GitHub release tagged with the Nerdbank.GitVersioning SemVer.

Versioning is fully driven by `version.json` + `Nerdbank.GitVersioning` — do not hand-edit assembly versions.
