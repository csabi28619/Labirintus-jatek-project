# Labyrinth Editor 

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

## Required Methods (methods.cs)
All four are implemented in `Services/ValidationService.cs`:
- `GetRoomNumber(char[,] map)` — counts █ tiles
- `GetSuitableEntrance(char[,] map)` — counts border exits
- `IsInvalidElement(char[,] map)` — detects illegal characters  
- `GetUnavailableElements(char[,] map)` — finds isolated tiles
- `GenerateLabyrinth(List<string>)` — builds map from position list
