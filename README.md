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
- Top-right logistics strip: loaded vs idle haulers, E/N/W lane counts (hottest marked `*`), next-wave copy, gun-fire fuse, and a larder clock (`~Ns of food`). HAUL CUT names the stake (BRACE / Power / farm / ore) plus splice countdown.
- Opening coach: 1 Farm (south pad) → 2 rail to Hub → 3 first hauls. After that, a mid-watch coach names the next readable call (splice, Power, Hub L2, Splash west, CORE THIN, second farm, HOLD). An unrouted producer pad reads **OFFLINE** with a gold ghost rail home.
- Tray: opening Farm/Route still pulse. Mid-watch pulses the called tool. Short-stock tools dim with `· short`. Splash stays locked until Hub L2.
- Deposits during a raid fire **Rail Surge** (Hub takes less hit damage). Title line reads BRACE while the shield is up; a cyan ring sits on the Hub. Hits during BRACE flash SHRUG (cyan) instead of HIT. Splicing a runner cut flashes RAIL LIVE / SPLICED / ROLLING. After Hub L2, a ghost Splash ring teaches the west choke.
- Guns draw range rings. Loaded haulers paint cargo trails. Runners telegraph a magenta intent line to the next pad. Slowed raiders tint cyan. Hottest choke labels EAST/NORTH/WEST. Hub HP is a world bar. Grunts are cubes, brutes capsules, runners thin cylinders. Win/lose cards show time, Hub HP, and wave.
- Win: survive 6 waves **and** Hub Level 2. Lose: Hub HP 0, or food stays at 0 for 14s.
- Runners cut rails. The orange rail pulses, pads read SPLICE, stuck haulers throb amber, and a world chip names the stake (**BRACE** / **PWR** / **FARM** / **ORE**) plus countdown. Route is auto-armed — click the glowing pad.
- Guns draw range rings and a LOCK beam to the raider they will shoot. Dry guns go red. Loaded haulers stamp FOOD / PWR / ORE. During a raid, the nearest cargo haul paints a cyan line to the Hub and calls **BRACE IN Xs** before the shield pops. Last 14s before a wave, spawn pads show `4G · 8s` ghosts. Raiders on the Hub paint a **CHEW** beam and the title reads UNDER FIRE — if a haul is inbound it becomes UNDER FIRE · BRACE IN Xs. A runner on a live rail paints it magenta (**CUT?** → **CUT NOW**) before the snap. When Power drops below a shot, the title holds **GUNS DRY** and a Power haul paints amber **POWER IN Xs**. Raiders on the last hop to Hub paint a red line and the title reads **CORE BOUND** (PAD when they are 2.4 from the core) before CHEW starts. Between raids the title holds **CLEAR** with the next-wave clock.
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
13. **Pass 6 raid reads** — gun LOCK beams to the tracked raider (dry guns go red). Loaded haulers paint FOOD/PWR/ORE. A combat haul telegraphs BRACE IN Xs on the rail before the shield pops. Next-wave spawn ghosts + `4G · 8s` labels in the last 14s.
14. **Pass 7 UNDER FIRE** — raiders in Hub melee paint CHEW beams. Title reads UNDER FIRE (BRACE IN Xs if a haul is inbound, BRACE shrugs if the shield is up). Hub pad says CHEW. Splice still outranks this.
15. **Pass 8 RAIL THREAT** — a runner on a live rail paints that line magenta and calls CUT? / CUT NOW before sabotage. Title reads RAIL THREAT; if a BRACE haul is on that line it says BRACE haul in danger. Already-cut rails still go orange SPLICE.
16. **Pass 9 GUNS DRY** — when towers click empty, the title holds **GUNS DRY** until Power recovers. A Power haul paints an amber line and chips POWER IN Xs / POWER NOW. Hub pad reads DRY. Splice / UNDER FIRE / RAIL THREAT still outrank this. Brownout chance unchanged.
17. **Pass 10 CORE BOUND** — raiders whose next pad is Hub paint a red last-hop line and the title reads **CORE BOUND**. Chip **IN** then **PAD** at 2.4 from the core (the tick before CHEW). If a BRACE haul is inbound it says BRACE haul racing them. UNDER FIRE still takes over in melee. Tick unchanged.
18. **Pass 11 WAVE CLEAR** — when a raid dies and the next wave is still queued, the title holds **CLEAR** with the next-raid clock and the next readable call (Hub L2 / Splash WEST / haul Power). Hub pad reads CLEAR. Splice / UNDER FIRE / RAIL THREAT / CORE BOUND / GUNS DRY still outrank this. Tick unchanged.
19. **Pass 12 HAUL CUT stakes** — a snapped rail names why splice now: **BRACE stuck** if a loaded haul is trapped during a raid, **Power rail down** if the pylon pad is cut, **farm rail down** if the larder line is cut. Midpoint chip reads BRACE / PWR / FARM / ORE. Tick and sabotage chance unchanged.
20. **Pass 13 PAD OFFLINE** — a producer with no live rail home paints a gold ghost line to the Hub. Pad reads **OFFLINE**. Title (below CLEAR) names farm / Power / ore. Splice still outranks this. Tick unchanged.
21. **Pass 14 SIT stock** — a staffed producer with ≥6 stock and no hauler bound for it reads **HAUL FOOD / PWR / ORE**. If an idle hauler is standing around, a colored line points them at that pad. Power piles bump the score when guns are hungry; farm piles when the larder is thin. Cut / OFFLINE / CLEAR still outrank the title. Tick unchanged.

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
