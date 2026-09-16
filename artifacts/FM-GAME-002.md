# FM-GAME-002 — Firstmate package (17:00 AEST Thu 17 Sep 2026)

GitHub (cold-open / Hub / Cursor): **https://github.com/steviebd/colony-haul**

| Field | Value |
| --- | --- |
| Hub-ready | **Yes** |
| Origin SHA | `2615c0703b53823bcaacbea02b26618484e4d4b3` |
| Origin Unity | `bd112318c2de23ca2f0e4946ac43807cdac2d1aa` |
| GitHub SHA | `750e0443db430bb45c328b16237e3574bf60fa4b` |
| Kernel | seed-7 playtest **win** (`phase=won`, t≈184.45, Hub L2, 6 waves) |
| Product | Unity 2022.3.50f1 URP under `Assets/_ColonyHaul` — not Vite |

Histories differ (MCP/Actions vs Origin). Pass-3 trees match on the six edited paths (byte-identical). GitHub still has the Hub-complete tree from pass 2 (`Assets/`, `Packages/`, `ProjectSettings/`, USB, `playable/`, artifacts tape).

## Feature list (Unity-first, presentation only)

Tick, waves, and balance are unchanged. DemoPilot still wins seed 7.

1. **Opening coach** — Farm (south pad) → mag-rail to Hub → first hauls. Farm/Route tray pulse.
2. **Mid-watch coach** — after opening: splice cut rail, offline pad, east kinetic before wave 1, brownout/power, Hub L2 in stock, thin larder, hottest ungunned choke, barrier on pressure, splash west.
3. **Logistics strip** — loaded vs idle haulers, HAUL CUT splice timer, **E/N/W lane counts** (hottest `*`), inbound raiders, next-wave copy, gun-fire fuse, **larder clock**.
4. **Tray** — mid-watch pulses the called tool; short-stock tools dim with `· short`; Splash locked until Hub L2.
5. **TD clarity** — gun/Hub range rings, ghost rails, choke heat + EAST/NORTH/WEST labels, slowed raiders tint cyan, world Hub HP bar, enemy HP, inbound spawn labels, unstaffed dim, brownout gun tint, buffer pillars.
6. **Juice** — cargo-colored haul trails, shot/splash tracers, deposit pips, kill punch + scrap pip, hub flash, banners, beeps.
7. **Lockstep kernel** — C# `GameSim` + TS `playable/` spare harness. USB vendored at `ThirdParty/unity-semantic-bridge` (`41e8bca`).

## How to play

```bash
git clone https://github.com/steviebd/colony-haul.git
```

Unity Hub → Open that folder → `Assets/_ColonyHaul/Scenes/Boot.unity` or `Game_VerticalSlice.unity` → Play.

- **Colony Haul → Play Vertical Slice** — you take the watch (Farm first).
- **Colony Haul → Play Demo (autopilot)** — seed-7 demo.
- **Colony Haul → Run Headless Sim** — expect `win=True` on seed 7.

Keys `1–7` Farm/Mine/Power/Route/Kinetic/Splash/Barrier. `U` Hub L2. `D` demo. `R` restart. Win: 6 waves **and** Hub L2. Lose: Hub HP 0, or food at 0 for 14s.

Spare (no Editor): `cd playable && npm install && npm run playtest`. Do not treat Vite as the ship target.

## Fun call

Mid-game is a readable watch. The coach names the next call, the strip shows which lane is hot and how many seconds the larder has left, and the mesa paints cargo trails, choke heat, slow, and Hub HP so you can splice, gun, or haul without guessing.

## Gaps

- This VM has no Unity Editor — Play Mode is documented, not screenshot-verified for pass 3 HUD.
- Runtime art is procedural low-poly, not authored FBX.
- Origin remote stays `tmp-*` (cannot rename). GitHub is the Hub/Cursor clone.
- GitHub SHA ≠ Origin SHA (parallel history); pass-3 file bytes match.
- Out of scope: multiplayer, cloud meta, deep tech tree, campaign.
