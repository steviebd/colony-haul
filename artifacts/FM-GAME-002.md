# FM-GAME-002 — Firstmate package (17:00 AEST Thu 17 Sep 2026)

GitHub (cold-open / Hub / Cursor): **https://github.com/steviebd/colony-haul**

| Field | Value |
| --- | --- |
| Hub-ready | **Yes** |
| Origin SHA | `9fe7db47f8aa0ecfbe9fd5c68e7d99bc459ec7e3` |
| GitHub SHA | `51528b9512167c13efcee3b292afd14ff88628c7` |
| Kernel | seed-7 playtest **win** (`phase=won`, t≈184.45, Hub L2, minHp≈50.4) |
| Product | Unity 2022.3.50f1 URP under `Assets/_ColonyHaul` — not Vite |

Pass 4 trees match on the six edited paths (byte-identical). Cold clone still has `Assets/`, `Packages/`, `ProjectSettings/`, USB, `playable/`, artifacts tape. Import workflows self-delete.

## Unity feature list

One Game tick. Balance numbers unchanged. Rail Surge is the combat-haul rule: a deposit while raiders are live cuts Hub hit damage (0.55s, or 0.85s when Hub HP < 72). DemoPilot still wins seed 7.

1. **Opening coach** — Farm → mag-rail to Hub → first hauls.
2. **Mid-watch coach** — splice, offline pad, east kinetic, brownout, Hub L2, thin larder, hottest choke, barrier, splash west.
3. **Hub L2 raising** — gold grow + countdown; Splash pulses WEST for 16s.
4. **Late watch** — CORE THIN, crew stretch / idle pad, second Farm, open north choke, HOLD THE MESA. Hold-copy in logistics. Staff chip warns when producers outnumber crew.
5. **Logistics** — E/N/W pressure (`*`), inbound, gun fuse, larder clock, guns-hungry Power teach (no tool lock).
6. **Rail Surge** — combat deposit braces Hub (0.72× hits). Longer brace when the core is cracked.
7. **Combat reads** — grunt cube, brute capsule, runner cylinder, ground blobs, thicker tracers, splash burst, LAST RAIDS wave banner (5–6).
8. **Juice** — deposit spokes, CUT / DRY / SLOW / SURGE, kill punch, cargo trails, choke heat, world Hub HP, hub crack tint, waiting haulers throb.
9. USB vendored at `41e8bca`. TS kernel carries Surge for lockstep. Vite is spare.

## How to play

```bash
git clone https://github.com/steviebd/colony-haul.git
```

Unity Hub → Open that folder → `Assets/_ColonyHaul/Scenes/Boot.unity` or `Game_VerticalSlice.unity` → Play.

- **Colony Haul → Play Vertical Slice** — you take the watch (Farm first).
- **Colony Haul → Play Demo (autopilot)** — seed-7 demo.
- **Colony Haul → Run Headless Sim** — expect `win=True` on seed 7.

Keys `1–7` Farm/Mine/Power/Route/Kinetic/Splash/Barrier. `U` Hub L2. `D` demo. `R` restart. Win: 6 waves **and** Hub L2.

## Fun call

Early game teaches Farm-rail-Hub. Mid-game names the next call. Late game is a hold: core cracks red, crew stretch warns against overbuilding, second farm and north gun show up as readable choices, and hauling through a raid braces a thin Hub.

## Gaps

- This VM has no Unity Editor — Play Mode is documented, not screenshot-verified.
- Runtime art is procedural low-poly, not authored FBX.
- Origin remote stays `tmp-*`. GitHub is the Hub/Cursor clone.
- GitHub SHA ≠ Origin SHA (parallel history); pass-4 file bytes match.
- Out of scope: multiplayer, cloud meta, deep tech tree, campaign.
