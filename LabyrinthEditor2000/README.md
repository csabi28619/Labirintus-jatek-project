# Labyrinth Editor — Phase 1

## Project Structure

```
LabyrinthEditor/
├── Models/
│   ├── TileInfo.cs          ← char → open directions lookup table
│   └── MapData.cs           ← the grid + metadata
├── ViewModels/
│   └── MainViewModel.cs     ← state, tools, undo/redo, auto-tile
├── Views/
│   ├── MainWindow.xaml      ← UI layout (dark doom-style theme)
│   ├── MainWindow.xaml.cs   ← code-behind, event handlers
│   ├── MapCanvas.cs         ← custom FrameworkElement, renders & handles mouse
│   └── NewMapDialog.cs      ← new map size dialog
├── Services/
│   ├── MapFileService.cs    ← UTF-8 save/load of .txt files
│   ├── MapRenderer.cs       ← draws tiles on DrawingContext
│   ├── ValidationService.cs ← all 4 required methods + full validation
│   └── LanguageService.cs   ← HU/EN bilingual support
├── Resources/
│   ├── lang_hu.txt          ← Hungarian strings
│   ├── lang_en.txt          ← English strings
│   └── sample.txt           ← example map
└── LabyrinthEditor.csproj
```

## Requirements
- Visual Studio 2022 (or VS Code with C# extension)
- .NET 6 SDK (Windows)

## How to Run
1. Open `LabyrinthEditor.csproj` in Visual Studio
2. Press F5 to build and run

## Controls
| Action | Input |
|--------|-------|
| Paint tile | Left click / drag |
| Erase (void) | Right click |
| Pan view | Middle mouse drag |
| Zoom | Mouse wheel |
| Undo | Ctrl+Z |
| Redo | Ctrl+Y |
| Save | Ctrl+S |

## Tile Characters (saved in .txt)
| Char | Meaning | Open directions |
|------|---------|----------------|
| `╬` | Cross    | N E S W |
| `═` | H-line   | E W     |
| `║` | V-line   | N S     |
| `╔` | TL corner| E S     |
| `╗` | TR corner| S W     |
| `╚` | BL corner| N E     |
| `╝` | BR corner| N W     |
| `╦` | T-down   | E S W   |
| `╩` | T-up     | N E W   |
| `╠` | T-right  | N E S   |
| `╣` | T-left   | N S W   |
| `█` | Room     | all     |
| `.` | Void     | none    |

## Phase Roadmap
- [x] Phase 1 — Core data model, renderer, file I/O, all 4 required methods
- [ ] Phase 2 — Tile palette preview rendering (mini canvas per tile)
- [ ] Phase 3 — Auto-tile already implemented in ViewModel
- [ ] Phase 4 — Zoom/pan (partially done in MapCanvas)
- [ ] Phase 5 — Validation UI polish
- [ ] Phase 6 — Save/load dialogs (done), resize map feature
- [ ] Phase 7 — Full bilingual string coverage

## Required Methods (methods.cs)
All four are implemented in `Services/ValidationService.cs`:
- `GetRoomNumber(char[,] map)` — counts █ tiles
- `GetSuitableEntrance(char[,] map)` — counts border exits
- `IsInvalidElement(char[,] map)` — detects illegal characters  
- `GetUnavailableElements(char[,] map)` — finds isolated tiles
- `GenerateLabyrinth(List<string>)` — builds map from position list
