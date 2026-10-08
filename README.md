# Corona.SourceGeneration

Source-embedded building blocks for incremental C# source generators used by LauncherX, ConnectX and Hive.Framework.

## Consumption

Add this repository as a `SourceGeneration` Git submodule, pinned to a commit, then import the shared project from a generator project:

```xml
<Import Project="../SourceGeneration/SourceGeneration.props" />
```

Alternatively pack `src/Corona.SourceGeneration/Corona.SourceGeneration.csproj` and reference the source-only NuGet package with `PrivateAssets="all"`. The package imports internal helper sources through `build/Corona.SourceGeneration.props`. The consumer must reference `Microsoft.CodeAnalysis.CSharp` 4.14 or newer and use C# 12 or newer. Do not consume this as a runtime assembly or import it into application projects. Do not use both consumption methods at once.

This design embeds helpers inside each analyzer DLL, avoiding analyzer dependency resolution problems in IDEs and command-line builds. Business attributes stay in the owning application's existing runtime project. There is no dependency on LauncherX, ConnectX, Hive or CommunityToolkit.

## Building blocks

- `EquatableArray<T>`: structural immutable collection equality, including default/empty values.
- `TypeDeclarationModel`: symbol-free partial declarations for nested/generic/record types and global namespaces.
- `CodeWriter`: deterministic LF output with checked block nesting.
- `CSharpNames`: literal and identifier escaping, metadata identities and collision-free hint encoding.
- `SymbolExtensions`: metadata attribute matching, partial-type validation and inheritance checks.
- `SourceFile` / `SourceOutput`: immutable output and UTF-8 emission.
- `XmlInputs`: XML parsing without external entities or disk access.
- `testing/SourceGeneration.Testing.props`: optional, framework-independent generator driver helpers for net8.0+ test projects.

Keep semantic analysis in the initial transform; project symbols into records and equatable collections before output. Use `ForAttributeWithMetadataName` for marker discovery. Treat resource files as `AdditionalFiles`, not untracked disk reads. Keep diagnostics in a separate output stream where locations would otherwise break model caching. Collect only when an output genuinely needs all inputs.

Architecture references (original implementations, no copied Toolkit source):

- [CommunityToolkit ObservablePropertyGenerator](https://github.com/CommunityToolkit/dotnet/blob/main/src/CommunityToolkit.Mvvm.SourceGenerators/ComponentModel/ObservablePropertyGenerator.cs)
- [Roslyn incremental generator cookbook](https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.cookbook.md)

## Validation and release

```sh
dotnet test tests/Corona.SourceGeneration.Tests/Corona.SourceGeneration.Tests.csproj
dotnet pack src/Corona.SourceGeneration/Corona.SourceGeneration.csproj -c Release -o artifacts
```

CI runs these commands on Windows and Linux and uploads the NuGet package for review. The repository is private; package publishing to a registry is a separate configured release step. Update each consumer's submodule pin deliberately after testing. Clone consumers with `git clone --recurse-submodules`.

Private submodule checkout in a consumer's GitHub Actions requires an organization-approved token with read access to this repository (for example the existing `ACCESS_TOKEN` secret). The default `GITHUB_TOKEN` only grants access to the calling repository. Configure that secret on each consumer before enabling recursive checkout in CI. For test helpers, import `SourceGeneration/testing/SourceGeneration.Testing.props` in a test project that references Roslyn.
