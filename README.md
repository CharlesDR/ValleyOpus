# ValleyOpus
Alchemy for the Valley.

## Repository
- `docs/design`, `docs/research`: the design docs (start with `docs/design/README.md`).
- `src/ValleyOpus.Engine`: the game-free engine (rules, validator, saves); `src/ValleyOpus.Tests`: its unit and data tests; `src/ValleyOpus.Tools`: the validator and packaging tools. CI builds and tests these on every push.
- `src/ValleyOpus`: the SMAPI mod. It needs the game installed to build (`dotnet build src/ValleyOpus/ValleyOpus.csproj`).
- `content`: the Content Patcher packs. `[CP] ValleyOpus - Test Fixtures` and `[CP] ValleyOpus - Spikes` are never shipped to players.
- `TESTING.md`: how to set up, build, install and test each test build.
