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
- Guns draw range rings and a LOCK beam to the raider they will shoot. Dry guns go red. Loaded haulers stamp FOOD / PWR / ORE. During a raid, the nearest cargo haul paints a cyan line to the Hub and calls **BRACE IN Xs** before the shield pops. Between raids that haul paints a cargo-colored line and chips **FOOD / PWR / ORE IN Xs** (**HAUL HOME**). Other loaded hauls chip the same cargo ETA (**HAUL ROLL**) so the second and third carts stay readable — BRACE / POWER / HOME still own the named cart. Last 14s before a wave, spawn pads ghost the pack (`4G · 8s`); the last 8s they read **PACK** and the title names the lane (**PACK IN** — CLEAR still owns later intermissions). Live Kinetic / Splash towers chip remaining fire seconds (**FUSE Ns**, amber when hungry, red when dry) so you haul Power before GUNS LOW / GUNS DRY. Staffed farms chip remaining food seconds (**LARDER Ns**, amber when Food is under 11, red when starving) so you haul Food before the larder fails. Staffed pylons and mines chip pad stock (**PWR n** / **ORE n**, amber when piled or guns hungry, red when dry) so you haul the pad before SIT. The Hub chips crew vs pads (**STAFF n/m**, amber when stretched, green on CREW UP) so you raise L2 or stop planting idle pads. Hub HP sits as a number beside the world bar (**HP n**, red when thin) so CORE THIN is readable between raids. Next wave sits as **Wn Xs** on the other side of the bar (amber in the last 8s) so CLEAR is readable during a raid — CLEAR still owns the intermission chip. Live raiders chip **RAID n** above CHEW so UNDER FIRE still has a count between chew bursts. Still-queued raiders chip **IN +n** beside it so the rest of the pack stays readable. This-wave kills chip **DOWN n** above RAID so the raid shrinking is readable — PACK IN still owns the last 8s. The last live raider chips **LAST** so the pack closing is readable. Live runners chip **RUN n** beside DOWN so RAIL THREAT still has a count between CUT? bursts. Snapped rails chip **CUT n** so HAUL CUT still has a count until splice. Frozen carts chip **STUCK n** above that so splice is a recovery, not a generic repair. Slowed raiders chip **SLOW n** beside STUCK so Barrier / Splash still have a count. Hauls still moving on live rails chip **ROLL n** between them so splice is a recovery, not a full stop. Live Kinetic / Splash chip **GUN n** beside STAFF so RAID still has a barrel count (FUSE stays on the guns). Loaded carts chip **LOAD n** under WAVE CLOCK so BRACE / HOME still have a count. Unrouted producers chip **OFF n** left of RAID so PAD OFFLINE still has a count. A new Kinetic / Splash going live reads **UP** for 8s and the title names **GUNS UP** (PACK IN still wins the last 8s before wave 1; CLEAR still owns later intermissions) so you haul Power before the fuse is hungry. Raiders on the Hub paint a **CHEW** beam and the title reads UNDER FIRE — if a haul is inbound it becomes UNDER FIRE · BRACE IN Xs. A runner on a live rail paints it magenta (**CUT?** → **CUT NOW**) before the snap. After you splice, **RAIL LIVE** names what is rolling again (Power / farm / BRACE) for 4s — CLEAR still owns the rest of the intermission. When Power drops below a shot, the title holds **GUNS DRY** and a Power haul paints amber **POWER IN Xs**. After Power recovers, **GUNS BACK** names the towers live again for 8s (RAIL LIVE still outranks a splice; CLEAR still owns the rest of the intermission). When Hub L2 lands, **CREW UP** names the extra haul from the yard for 12s (below GUNS BACK, above CLEAR) so you staff another pad. Raiders on the last hop to Hub paint a red line and the title reads **CORE BOUND** (PAD when they are 2.4 from the core) before CHEW starts. Between raids the title holds **CLEAR** with the next-wave clock. When Hub L2 is in stock the pad reads **READY** (CLEAR still owns the intermission) and the tray pulses **Hub L2 — Splash next**. An unarmed hottest lane with raiders on it reads **OPEN** and names Kinetic (or Splash west after L2). After the gun is up, that choke reads **SLOW** and names Barrier — 5 ore buys time for guns and BRACE. An overbuilt pad with no crew reads **IDLE** — raise Hub L2 or rail what you have. When Hub HP cracks below 72 in a raid the pad reads **THIN** and names the BRACE haul. Wave 4 with Hub L2 and Hold still Auto reads **HOLD** on the pad and pulses **H Hold — GUNS / CREW** (LAST RAIDS still owns waves 5–6).
- Brownout: guns try to fire with a dry pylon. Keep Power on live rails. From wave 4 with Hub L2, **H** cycles a Hold order: Auto → GUNS (haulers peel to Power) → CREW (haulers peel to Food). Loaded carts stash unmatched cargo on a live pad and replan the same tick. Splice still beats this. Demo stays on Auto.

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
12. **Pass 5 Hold order** — late-match decision. Wave 4 + Hub L2 unlocks H: Auto → GUNS (peel to Power, even loaded carts) → CREW (peel to Food). Visible hub ring, pad labels, same-tick replan. No soft-lock; demo stays Auto.
13. **Pass 6 raid reads** — gun LOCK beams to the tracked raider (dry guns go red). Loaded haulers paint FOOD/PWR/ORE. A combat haul telegraphs BRACE IN Xs on the rail before the shield pops. Next-wave spawn ghosts + `4G · 8s` labels in the last 14s.
14. **Pass 7 UNDER FIRE** — raiders in Hub melee paint CHEW beams. Title reads UNDER FIRE (BRACE IN Xs if a haul is inbound, BRACE shrugs if the shield is up). Hub pad says CHEW. Splice still outranks this.
15. **Pass 8 RAIL THREAT** — a runner on a live rail paints that line magenta and calls CUT? / CUT NOW before sabotage. Title reads RAIL THREAT; if a BRACE haul is on that line it says BRACE haul in danger. Already-cut rails still go orange SPLICE.
16. **Pass 9 GUNS DRY** — when towers click empty, the title holds **GUNS DRY** until Power recovers. A Power haul paints an amber line and chips POWER IN Xs / POWER NOW. Hub pad reads DRY. Splice / UNDER FIRE / RAIL THREAT still outrank this. Brownout chance unchanged.
17. **Pass 10 CORE BOUND** — raiders whose next pad is Hub paint a red last-hop line and the title reads **CORE BOUND**. Chip **IN** then **PAD** at 2.4 from the core (the tick before CHEW). If a BRACE haul is inbound it says BRACE haul racing them. UNDER FIRE still takes over in melee. Tick unchanged.
18. **Pass 11 WAVE CLEAR** — when a raid dies and the next wave is still queued, the title holds **CLEAR** with the next-raid clock and the next readable call (Hub L2 / Splash WEST / haul Power). Hub pad reads CLEAR. Splice / UNDER FIRE / RAIL THREAT / CORE BOUND / GUNS DRY still outrank this. Tick unchanged.
19. **Pass 12 HAUL CUT stakes** — a snapped rail names why splice now: **BRACE stuck** if a loaded haul is trapped during a raid, **Power rail down** if the pylon pad is cut, **farm rail down** if the larder line is cut. Midpoint chip reads BRACE / PWR / FARM / ORE. Tick and sabotage chance unchanged.
20. **Pass 13 PAD OFFLINE** — a producer with no live rail home paints a gold ghost line to the Hub. Pad reads **OFFLINE**. Title (below CLEAR) names farm / Power / ore. Splice still outranks this. Tick unchanged.
21. **Pass 14 SIT stock** — a staffed producer with ≥6 stock and no hauler bound for it reads **HAUL FOOD / PWR / ORE**. If an idle hauler is standing around, a colored line points them at that pad. Power piles bump the score when guns are hungry; farm piles when the larder is thin. Cut / OFFLINE / CLEAR still outrank the title. Tick unchanged.
22. **Pass 15 GUNS LOW** — before brownout, when the fuse is under 9s, a Power haul paints amber **POWER IN Xs**. Title **GUNS LOW** (below CLEAR). BRACE inbound still wins the same hauler during a raid. GUNS DRY still takes over when the pylon clicks empty. Tick unchanged.
23. **Pass 16 L2 READY** — when Hub L2 is in stock, the Hub L2 tray pulses **Splash next**, a gold ring sits on the Hub, and the pad reads **READY**. Title **L2 READY** sits below CLEAR / GUNS LOW / OFFLINE / SIT so WAVE CLEAR still owns the intermission (CLEAR already says raise Hub L2). Press U to spend ore/food/pwr and unlock Splash. Tick unchanged.
24. **Pass 17 OPEN CHOKE** — the hottest unarmed lane with pressure reads **OPEN**. Title **OPEN EAST / NORTH / WEST** (below L2 READY) names Kinetic, or Splash on west after Hub L2. West stays **SPLASH** during the 16s unlock ghost. Tick unchanged.
25. **Pass 18 SLOW** — once that lane has a gun but no Barrier, the choke reads **SLOW**. Title **SLOW EAST / NORTH / WEST** (below OPEN / BRACE inbound) names Barrier. Buys time for guns and BRACE hauls. Tick unchanged.
26. **Pass 19 CREW STRETCH** — when producers outnumber crew, the idle pad reads **IDLE**. Title **CREW STRETCH** (below Hold) names U if L2 is in stock, otherwise rail beats a new pad. OFFLINE / SIT still outrank the pad. Tick unchanged.
27. **Pass 20 CORE THIN** — when Hub HP drops below 72 during a raid, the pad reads **THIN**. Title **CORE THIN** (below GUNS LOW, so CLEAR still owns intermission) names the BRACE haul if one is inbound. Tick unchanged.
28. **Pass 21 HOLD READY** — wave 4 with Hub L2 and Hold still Auto: the H tray pulses **GUNS / CREW**, a gold ring sits on the Hub, and the pad reads **HOLD**. Title **HOLD READY** (below CREW STRETCH, so LAST RAIDS still owns waves 5–6) names H for GUNS if the fuse is hungry, H for CREW if the larder is thin. Demo stays Auto. Tick unchanged.
29. **Pass 22 HAUL HOME** — between raids, the nearest loaded haul paints a cargo-colored line to the Hub and chips **FOOD / PWR / ORE IN Xs**. Title **HAUL HOME** (below HOLD READY so CLEAR still owns the intermission) names the drop. BRACE inbound still owns the same haul during a raid. Tick unchanged.
30. **Pass 23 PACK IN** — last 8s before a wave, spawn pads read **PACK** and ghost the pack harder. Title **PACK IN** (below HOLD READY, so CLEAR still owns later intermissions) names the lane. Wave 1 on seed-7 shows the title; waves 2–6 still ping the pads under CLEAR. Tick unchanged.
31. **Pass 24 GUNS UP** — a new Kinetic / Splash going live reads **UP** for 8s, the Power pad reads **FEED**, and a teal range ring sits on that gun. Title **GUNS UP** (below PACK IN so PACK IN still owns the last 8s before a wave; CLEAR still owns later intermissions) names the lane and tells you to haul Power. Seed-7 titles EAST then NORTH in the opening; Splash WEST still pings under CLEAR. Tick unchanged.
32. **Pass 25 RAIL LIVE** — after a splice, the recovered rail pulses teal and the pads read **LIVE** for 4s. Title **RAIL LIVE** (below GUNS DRY, above CLEAR) names what is rolling again — Power / farm / BRACE. Combat titles still outrank it. Seed-7 fires this four times in intermission (t≈82, 110, 137, 164). Tick unchanged.
33. **Pass 26 GUNS BACK** — after a brownout recovers, live guns pulse lime and the Hub pad reads **BACK** for 8s. Title **GUNS BACK** (below RAIL LIVE, above CLEAR) names keep Power rolling so they do not dry again. Combat titles still outrank it during a raid. Seed-7 titles after wave 3 (dry-off t≈101.75, raid-off t≈106, ~3.7s before RAIL LIVE at t≈110). Tick unchanged.
34. **Pass 27 CREW UP** — when Hub L2 grants the extra hauler, that cart pulses green and the yard reads **CREW** for 12s. Title **CREW UP** (below GUNS BACK, above CLEAR) names the third haul so you staff another pad. Combat / RAIL LIVE still outrank it. Seed-7 juices at L2 (t≈79 during the raid) and titles after RAIL LIVE (t≈86–91). Tick unchanged.
35. **Pass 28 GUN FUSE** — live Kinetic / Splash chip remaining fire seconds (**FUSE Ns**). Amber under 9s, red when dry. World chips only — GUNS LOW / GUNS BACK / RAIL LIVE / CREW UP / CLEAR still own the title. Seed-7 fires from the first east kinetic. Tick unchanged.
36. **Pass 29 LARDER** — staffed farms chip remaining food seconds (**LARDER Ns**). Amber when Food is under 11, red when starving. World chips only — HOLD READY / SIT / OFFLINE / CLEAR still own the title. Seed-7 fires from the first south farm. Tick unchanged.
37. **Pass 30 HAUL ROLL** — every other loaded haul chips cargo ETA (**FOOD / PWR / ORE Ns**). BRACE / POWER / HOME still own the named cart. World chips only — no title steal. Seed-7 fires from the opening hauls (HOME is gated until t=28) and on the extra L2 cart. Tick unchanged.
38. **Pass 31 PAD STOCK** — staffed pylons and mines chip pad stock (**PWR n** / **ORE n**). Amber when piled (≥6) or guns hungry, red when dry. Farms stay on LARDER. World chips only — SIT / OFFLINE / FEED still own those pads. Seed-7 fires from the first mine and pylon. Tick unchanged.
39. **Pass 32 STAFF** — the Hub chips crew vs pads (**STAFF n/m**) under the HP bar. Amber when stretched, green on CREW UP. World chips only — CREW STRETCH / CREW UP / CLEAR still own the title. Seed-7 fires from the first farm. Tick unchanged.
40. **Pass 33 HUB HP** — Hub HP number sits beside the world bar (**HP n**). Red when the core is thin. World chips only — CORE THIN still owns the raid title. Seed-7 fires from t=0 and stays live between raids (minHp≈50.4). Tick unchanged.
41. **Pass 34 WAVE CLOCK** — next-wave clock sits on the other side of the Hub bar (**Wn Xs**). Amber in the last 8s. World chips only — CLEAR still owns the intermission chip; PACK IN still owns the title. Seed-7 fires from t=0 (`W1`) and during raids. Tick unchanged.
42. **Pass 35 RAID n** — live raiders chip **RAID n** above CHEW on the Hub. Red while chewing, rust otherwise, amber if the core is thin. World chips only — UNDER FIRE / CHEW still own the title. Seed-7 fires every raid. Tick unchanged.
43. **Pass 36 IN +n** — still-queued raiders chip **IN +n** beside RAID on the Hub. Amber if the core is thin. World chips only — CORE BOUND still owns hy-18 IN/PAD; PACK IN still owns the last 8s. Seed-7 fires every wave as the pack staggers in. Tick unchanged.
44. **Pass 37 DOWN n** — this-wave kills chip **DOWN n** above RAID on the Hub. Green after the pack drops. World chips plus a DOWN pip on the corpse — PACK IN still owns the last 8s. Seed-7 fires from the first kill of wave 1. Tick unchanged.
45. **Pass 38 HOLD PEEL** — GUNS / CREW now restash unmatched cargo on a live pad and replan loaded haulers the same tick. Matching Power/Food hauls still rush Hub (BRACE). Auto still finishes the current trip. Demo stays Auto so seed-7 is bit-stable. Tick unchanged.
46. **Pass 39 LAST** — the last live raider chips **LAST**. World chips only — RAID 1 / CHEW / UNDER FIRE still own the Hub. Seed-7 fires at the end of every wave. Tick unchanged.
47. **Pass 40 RUN n** — live runners chip **RUN n** beside DOWN on the Hub. Magenta when a rail is threatened. World chips only — RAIL THREAT / CUT? still own the title. Seed-7 fires from wave 2. Tick unchanged.
48. **Pass 41 CUT n** — snapped rails chip **CUT n** left of DOWN on the Hub. World chips only — HAUL CUT / SPLICE still own the title and rail countdown. Seed-7 fires on all four sabotages. Tick unchanged.
49. **Pass 42 STUCK n** — frozen haulers chip **STUCK n** above CUT on the Hub. World chips only — HAUL CUT still owns the title; STUCK pips still fire on the carts. Seed-7 fires whenever a cut traps a haul. Tick unchanged.
50. **Pass 43 SLOW n** — slowed raiders chip **SLOW n** beside STUCK on the Hub. World chips only — SLOW EAST/NORTH/WEST still owns the Barrier title. Seed-7 fires on barrier walks and Splash slows. Tick unchanged.
51. **Pass 44 ROLL n** — haulers still moving during a cut chip **ROLL n** between STUCK and SLOW. World chips only — HAUL CUT still owns the title; HAUL ROLL still owns cargo ETA on the carts; ROLLING pips still fire on splice. Seed-7 fires when a snap leaves another haul on a live rail. Tick unchanged.
52. **Pass 45 GUN n** — live Kinetic / Splash chip **GUN n** beside STAFF on the Hub. Amber when hungry, red when dry. World chips only — GUNS LOW / GUNS DRY / GUNS UP still own the title; FUSE still sits on the barrels. Seed-7 fires from the first east kinetic. Tick unchanged.
53. **Pass 46 LOAD n** — loaded haulers chip **LOAD n** under WAVE CLOCK on the Hub. Cyan during a raid (BRACE), gold between raids (HOME). World chips only — BRACE inbound / HAUL HOME / HAUL ROLL still own the carts. Seed-7 fires from the opening farm hauls. Tick unchanged.
54. **Pass 47 OFF n** — unrouted producers chip **OFF n** left of RAID on the Hub. World chips only — PAD OFFLINE still owns the title and ghost rail. Hidden during a cut so HAUL CUT still owns splice. Seed-7 fires each time Demo plants before the rail click. Tick unchanged.

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
