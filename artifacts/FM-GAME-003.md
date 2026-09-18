# FM-GAME-003 — Unity Linux Editor Play Mode

**Proof type: interactive Editor Play Mode** (Game view), not batchmode/headless CLI, not Vite.

README path: Unity Hub → this folder → `Game_VerticalSlice` / **Colony Haul → Play Vertical Slice** or **Play Demo**. That is what we ran.

| Field | Value |
| --- | --- |
| Unity | **2022.3.50f1** (`c3db7f8bf9b1`) Linux Editor, OpenGL 4.5 llvmpipe, Hub 3.21.3 |
| License | **Unity Personal** |
| Repo SHA | `e6f6713` + this tape; game pin was `d81c5b4ee8460b05c51d0510217c5f7ef6cafc1f` |
| Play Mode | **Entered.** Mesa, HUD, juice, end card all render. |
| Demo loop | **Watch a run** (seed-7 autopilot): Farm → rail → hauls → Hub L2 → Wave 5 live |
| Idle watch | **Play Vertical Slice** with no input: **HUB DOWN** at 81s, wave 2/6, STAFF 0/4 |
| Video | `artifacts/colony-haul-unity-watch-a-run.mp4`, `artifacts/colony-haul-unity-play-mode.mp4` (short captures; stills are the readable proof) |
| HTTPS | Repo artifacts only — no separate durable host |

## What we followed

Colony Haul README: product is the Unity project; press Play or the Colony Haul menus. Did **not** force `-batchmode -executeMethod Headless` as the proof. USB listener is optional agent tooling, not the playable-slice bar.

Wiring required so the Editor could open (not juice):

1. UPM id in `Packages/manifest.json` must match vendor `package.json` `"name"` `com.gamenami.unity-scemantic-bridge` (typo as shipped).
2. CS0136: inner `pulse` in `VerticalSliceBootstrap` shadowed the method local — renamed `gatePulse` so 2022.3 compiles.

## Play sessions

**A. Play Vertical Slice (you take the watch), no clicks**  
Coach stayed on Farm south. STAFF 0/4. Food drained. Waves hit an ungunned mesa. **HUB DOWN** 81s, L1, wave 2/6. End card: Run it back / Watch a run. Expected for an unplayed watch, not a kernel fail.

**B. Watch a run (seed-7 demo)** from that card  
STAFF 0/4 → 3/4 → 4/4 → 6/6. Farm pads, mag-rail, HAUL HOME, Kinetic/Splash, Hub **L2**, 3 haulers. Wave 1 → 5. GUNS LOW / BRACE / LAST RAIDS copy live. Console showed **0** errors.

## Bugs vs playable-slice bar

- **Intro/end card stuck.** `VERTICAL SLICE` / Take the watch / Watch a run stays centered over live Play (Farm, rails, Wave 5 all behind it). Pass 71 END PAIR is not dismissing. Occludes the mesa.
- **Screen-record files are short** (≈5–14s) vs the minutes we watched. Trust the stills for Wave 5 / Farm-rail.
- First-open compile failed until CS0136. Safe Mode dialog appeared once; relaunch after the rename compiled clean.
- llvmpipe, no GPU. Play ran; Editor CPU was high (~180%). No pink missing-shader.

## Artifacts

| File | What |
| --- | --- |
| `fm-game-003-editor-open.png` | Editor open, Colony Haul menu, USB in Packages |
| `fm-game-003-play-opening.png` | Play Vertical Slice, Farm coach, STAFF 0/4 |
| `fm-game-003-play-hub-down.png` | Idle watch lost, HUB DOWN 81s |
| `fm-game-003-demo-farm-rail.png` | Watch a run: STAFF 4/4, rails, HAUL HOME (card still overlaid) |
| `fm-game-003-demo-wave5.png` | Wave 5, Hub L2, STAFF 6/6, GUNS LOW / BRACE |
| `colony-haul-unity-watch-a-run.mp4` | Interactive Play, Watch a run |
| `colony-haul-unity-play-mode.mp4` | Interactive Play, idle Vertical Slice → HUB DOWN |
| `fm-game-003-unity-cli-package-fail.txt` | Earlier UPM mismatch (before id align) |

## Remaining

Card overlay is the captain-facing Play bug. Demo loop otherwise ran in Editor. No Vite substitution.
