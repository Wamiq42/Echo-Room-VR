# Echo Room VR — Documentation

| Doc | What's inside |
|-----|---------------|
| [GDD.md](GDD.md) | **The living, canonical Game Design Document.** Supersedes the original one-page PDF; expanded section by section (narrative, structure, mechanics, VR, audio, technical, production). Start here for design canon. |
| [ROADMAP.md](ROADMAP.md) | **Dependency-ordered build plan** from current state to the full game — 12 phases, critical path, what to do next. |
| [GAME_OVERVIEW.md](GAME_OVERVIEW.md) | What the game is, design pillars, and **art direction** — color palette (with hex codes), how the maze walls should look, lighting, and audio direction. Start here for visual/design decisions. |
| [CODEBASE_ANALYSIS.md](CODEBASE_ANALYSIS.md) | Architecture, system-flow diagrams, file-by-file summary, and the prioritized bug/issue list. |
| [GDD_PROGRESS.md](GDD_PROGRESS.md) | Feature-by-feature audit of the GDD vs. what's built (~30–35% done), with the steps that unblock a playable first level. |

## Related design assets

- Maze blueprints: `Assets/_EchoRoom/Design/Maze_5x5_A.svg`, `_B.svg`, `_C.svg`
  (top-down, grid-labeled, guaranteed-solvable; reuse the existing button/door scripts).
- Blueprint generator: `Downloads/maze_blueprint.js` (seed-based; tweak size/buttons and re-run).
