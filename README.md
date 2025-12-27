# Cthulhu Squish

A tiny, single-screen WPF arcade game — tap the mischievous Cthulhu to score points.

## Features
- Fast, single-screen gameplay (30s rounds)
- Tap-to-score with squish sound effects
- Floating sprite with bounce behavior, teleport-on-tap
- Round progression (Round 2 unlocks at score >= 20)
- Polished UI: glow, vignette, info cards, animations

## Run locally
Requires .NET 8 SDK (Windows, WPF).

From the repository root:

```powershell
dotnet build "main\CthulhuSquish.csproj"
dotnet run --project "main\CthulhuSquish.csproj"
```

## Project layout
- `main/` — the WPF project
  - `MainWindow.xaml` / `MainWindow.xaml.cs` — game UI & logic
  - `Resources/Images` — sprite images
  - `Resources/Raw` — audio

## Development notes
- Game logic is intentionally placed in `MainWindow.xaml.cs` for quick iteration.
- To change visuals drop new images into `main/Resources/Images` and update the XAML.

## License
This project is released under the MIT License. See `LICENSE`.

---
If you'd like, I can add a CI workflow, package the game, or help publish to GitHub with a recommended tag and release notes.