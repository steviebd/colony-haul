# Colony Haul

Sci-fi frontier colony + mag-rail logistics + tower defense. Hold Mesa 7 through six raider waves with Hub Level 2 online.

**Product is the Unity project.** Open this folder in Cursor, then open the same folder in Unity Hub.

GitHub (Cursor-friendly): **https://github.com/steviebd/colony-haul**

## Open in Cursor

```bash
git clone https://github.com/steviebd/colony-haul.git
cd colony-haul
```

File → Open Folder on that clone. Work under `Assets/_ColonyHaul`.

## Open in Unity Hub

Unity **2022.3 LTS** (project version `2022.3.50f1`) or **Unity 6 ≤ 6.3** with URP.

1. Unity Hub → **Open** → this repo folder (the folder that contains `Assets/`, `Packages/`, `ProjectSettings/`).
2. If prompted, add **URP** (`com.unity.render-pipelines.universal` 14.x on 2022.3). It is already in `Packages/manifest.json`.
3. Open `Assets/_ColonyHaul/Scenes/Boot.unity` (loads the slice after a short beat) **or** `Assets/_ColonyHaul/Scenes/Game_VerticalSlice.unity`.
4. Press Play, or use the menus:
   - **Colony Haul → Play Vertical Slice** — you take the watch (Farm first).
   - **Colony Haul → Play Demo (autopilot)** — seed-7 demo pilot.
   - **Colony Haul → Run Headless Sim** — ticks C# rules with no rendering; expect `win=True` on seed 7.

Scenes are thin on purpose. `VerticalSliceBootstrap` builds the dusk mesa, pads, rails, HUD, and juice in Play Mode.

### Play in the Editor

- Place a **Farm** on the south mesa pad (it pulses), then click the **Hub** to lay mag-rail. Haulers only move on live rails.
- Keys `1–7`: Farm, Mine, Power, Route, Kinetic, Splash, Barrier. `U` Hub L2. `H` Hold order (wave 4 + Hub L2). `D` demo. `R` restart.
- Left tray is clickable. Splash stays locked until Hub L2.
- Top-right logistics strip: loaded vs idle haulers, E/N/W lane counts (hottest marked `*`), next-wave copy, gun-fire fuse, and a larder clock (`~Ns of food`). HAUL CUT shows splice countdown.
- Opening coach: 1 Farm (south pad) → 2 rail to Hub → 3 first hauls. After that, a mid-watch coach names the next readable call (splice, Power, Hub L2, Splash west, CORE THIN, second farm, HOLD).
- Tray: opening Farm/Route still pulse. Mid-watch pulses the called tool. Short-stock tools dim with `· short`. Splash stays locked until Hub L2.
- Deposits during a raid fire **Rail Surge** (Hub takes less hit damage). Title line reads BRACE while the shield is up; a cyan ring sits on the Hub. Hits during BRACE flash SHRUG (cyan) instead of HIT. Splicing a runner cut flashes RAIL LIVE / SPLICED / ROLLING. After Hub L2, a ghost Splash ring teaches the west choke.
- Guns draw range rings. Loaded haulers paint cargo trails. Runners telegraph a magenta intent line to the next pad. Slowed raiders tint cyan. Hottest choke labels EAST/NORTH/WEST. Hub HP is a world bar. Grunts are cubes, brutes capsules, runners thin cylinders. Win/lose cards show time, Hub HP, and wave.
- Win: survive 6 waves **and** Hub Level 2. Lose: Hub HP 0, or food stays at 0 for 14s.
- Runners cut rails. The orange rail pulses, pads read SPLICE, stuck haulers throb amber, and a world timer counts down. Route is auto-armed — click the glowing pad.
- Brownout: guns try to fire with a dry pylon. Keep Power on live rails. From wave 4 with Hub L2, **H** cycles a Hold order: Auto → GUNS (haulers rush Power) → CREW (haulers rush Food). Idle haulers replan immediately. Splice still beats this. Demo stays on Auto.

### Pass 3 Unity-first (presentation + Rail Surge)

Tick stays one loop. Seed-7 demo still wins. Mid-game is readable, and a haul during a raid braces the Hub:

1. Mid-watch coach after the Farm-Route-Hub opening — splice, offline pad, east kinetic, brownout, Hub L2, thin larder, hottest ungunned choke, barrier, splash west.
2. Logistics: E/N/W lane counts with hottest marked, inbound raiders, gun fuse, larder clock. Guns hungry calls out a Power haul without locking tools.
3. Tray affordability (`· short`) and pulse on the called tool. Splash pulses WEST for 16s after Hub L2.
4. **Rail Surge** — a deposit while raiders are live briefly cuts Hub hit damage (haul + colony stock + core defense). Lasts longer when the core is thin.
5. Combat reads: grunt cube / brute capsule / runner cylinder, ground blobs, thicker tracers, splash burst, lane wave banner (LAST RAIDS on 5–6).
6. Hub L2 raising coach, gold grow, then Splash unlock on the west choke.
7. Late watch: CORE THIN, crew stretch / idle pad, second Farm, open north choke, HOLD THE MESA. Staff chip warns when producers outnumber crew. Hold-copy in logistics.
8. Juice on deposit spokes, sabotage CUT, brownout DRY, barrier SLOW, kill punch. Hub cracks red when thin; waiting haulers throb.
9. **Pass 4 punch-up** — BRACE title + hub shield ring (no Surge banner spam), west Splash ghost ring after L2, SPLICED / RAIL LIVE on cut recovery, win/lose cards with stats and bigger VFX.
10. **Pass 4 runner-cut recovery** — orange rail pulse + SPLICE pads and world timer, STUCK/ROLLING hauler pips, cut alarm drone, magenta runner intent lines, BRACE hits shrug cyan. Banners sit above the hint so they no longer cover the coach.
11. **Pass 4 late-wave telegraph** — inbound IN bursts on spawns, LAST RAIDS title, dusk fog heats on waves 5–6.
12. **Pass 5 Hold order** — late-match decision. Wave 4 + Hub L2 unlocks H: Auto → GUNS (rush Power, feeds guns / BRACE) → CREW (rush Food). Visible hub ring, pad labels, hauler replan. No soft-lock; demo stays Auto.

Folder layout: `Assets/_ColonyHaul/{Scripts,Art,Prefabs,Scenes,UI,Audio}`.

## Unity Semantic Bridge (USB)

Vendored from https://github.com/Programalyst/unity-semantic-bridge at commit `41e8bca`. Do not move the package path.

- Unity package: `ThirdParty/unity-semantic-bridge/com.gamenami.unity-semantic-bridge` (manifest id `com.gamenami.unity-semantic-bridge` via `file:`). Upstream `package.json` still spells `com.gamenami.unity-scemantic-bridge`. Do not "fix" that in place unless you are updating the vendor.
- Python MCP: `ThirdParty/unity-semantic-bridge/mcp-editor-bridge`

### Listener

1. Open Unity.
2. **Tools → Unity Semantic Bridge**.
3. Start the HTTP listener on port **1073**.
4. Health: `GET http://127.0.0.1:1073/health`.

The Unity `/rpc` URL is the Python server's downstream connection. It is not an MCP server URL.

### uv MCP

Install [uv](https://docs.astral.sh/uv/getting-started/installation/). Point Cursor at the vendored Python server:

```json
{
  "mcpServers": {
    "unity-semantic-bridge": {
      "command": "uv",
      "args": [
        "--directory",
        "/absolute/path/to/this-repo/ThirdParty/unity-semantic-bridge/mcp-editor-bridge",
        "run",
        "main.py"
      ]
    }
  }
}
```

Python talks JSON-RPC to Unity at `http://127.0.0.1:1073/rpc`. After C# changes, instance IDs are invalid until you call `get_scene_hierarchy` again.

## Spare harness (not the product)

A lockstep TypeScript sim lives in `playable/` for agent VMs that have no Unity Editor.

```bash
cd playable
npm install
npm run sim      # headless seed 7
npm run playtest # nested Farm→Route→Hub plus stress
npm run dev      # http://127.0.0.1:43147  (?play=1, ?demo=1&rate=2)
```

Do not treat the Vite client as the ship target. Keep it only when kernel parity helps.

## Gaps

- This cloud VM has no Unity Editor, so Editor Play is documented, not screenshot-verified here. Headless C# is what the menu item runs.
- Runtime art is procedural low-poly, not authored FBX.
- Origin `tmp-*` remote cannot be renamed with the current CLI; GitHub is the repo to open in Cursor.
- GitHub clone is Hub/Cursor complete: `Assets/`, `Packages/manifest.json`, `ProjectSettings/`, USB C# package, `playable/`, artifacts tape including `colony-haul-run.webm`.
- Out of scope: multiplayer, cloud meta, deep tech tree, campaign.
