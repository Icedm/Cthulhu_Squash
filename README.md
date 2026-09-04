# Cthulhu Squish

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![Windows](https://img.shields.io/badge/Platform-Windows-0078D4.svg)](https://www.microsoft.com/en-us/windows)
[![WPF](https://img.shields.io/badge/UI-WPF-512BD4.svg)](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/)

A tiny, single-screen WPF arcade game — tap the mischievous Cthulhu to score points.

## Features
- Fast, single-screen gameplay (30s rounds)
- Tap-to-score with squish sound effects
- **Animated sprite** with smooth wing and floating effects (GIF support)
- Floating sprite with bounce behavior, teleport-on-tap
- Round progression (Round 2 unlocks at score >= 20)
- 5-second pre-game countdown with gameplay instructions
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
- **Animated GIF support**: Uses [WpfAnimatedGif](https://github.com/thomaslevesque/WpfAnimatedGif) NuGet package for sprite animation.
- To change visuals drop new images (PNG or animated GIF) into `main/Resources/Images` and update the XAML.
- GIF sprites should be loaded using `ImageBehavior.SetAnimatedSource()` for proper animation playback.

## Dependencies
- .NET 8 SDK (Windows, WPF)
- WpfAnimatedGif 2.0.2 (NuGet)

## License
This project is released under the MIT License. See `LICENSE`.

---
If you'd like, I can add a CI workflow, package the game, or help publish to GitHub with a recommended tag and release notes.