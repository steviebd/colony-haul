# FM-GAME-002 — Firstmate package (17:00 AEST Thu 17 Sep 2026)

GitHub (cold-open / Hub / Cursor): **https://github.com/steviebd/colony-haul**

| Field | Value |
| --- | --- |
| Hub-ready | **Yes** |
| Origin SHA | `5e374a559557e399be56db63b8d48d83d0c14358` |
| GitHub SHA | `1bf83fa7359e9748420fa92e7e1cc5a216cc702a` |
| Kernel | seed-7 playtest **win** (`phase=won`, t≈184.45, Hub L2, minHp≈50.4, sabotages=4) |
| Product | Unity 2022.3.50f1 URP under `Assets/_ColonyHaul` — not Vite |

Pass 4d trees match on the edited paths (byte-identical). Cold clone still has `Assets/`, `Packages/`, `ProjectSettings/`, USB, `playable/`, artifacts tape. Import workflows self-delete.

## Unity feature list (delta vs Pass 3)

Pass 3 Origin was `9fe7db4` (late-watch CORE THIN, crew stretch, HOLD). Pass 4 kept one Tick and seed-7 win.

**Still from Pass 3**

1. **Opening coach** — Farm → mag-rail to Hub → first hauls.
2. **Mid-watch coach** — splice, offline pad, east kinetic, brownout, Hub L2, thin larder, hottest choke, barrier, splash west.
3. **Hub L2 raising** — gold grow + countdown; Splash pulses WEST for 16s.
4. **Late watch** — CORE THIN, crew stretch / idle pad, second Farm, open north choke, HOLD THE MESA.
5. **Logistics** — E/N/W pressure (`*`), inbound, gun fuse, larder clock, guns-hungry Power teach.
6. **Rail Surge** — combat deposit braces Hub (0.72× hits; 0.55s, or 0.85s when Hub HP < 72).
7. **Combat reads** — grunt cube, brute capsule, runner cylinder, ground blobs, tracers, splash burst, LAST RAIDS.
8. USB vendored at `41e8bca`. TS kernel carries Surge for lockstep. Vite is spare.

**Pass 4 punch-up** (Origin `96d533c` / GitHub `9eb1f74`)

9. BRACE title + cyan Hub shield ring (one banner per surge window). West Splash ghost ring after L2. SPLICED / RAIL LIVE on cut recovery. Win/lose cards with T, Hub HP, L, wave.

**Pass 4c** (Origin `398fbed` / GitHub `f25370c`)

10. **Runner-cut recovery** — orange rail pulse, SPLICE pad labels, world-space splice timer, stuck-hauler count in logistics and coach. STUCK pip then ROLLING on splice. Cut alarm drone. Magenta runner intent lines. BRACE hits flash SHRUG (cyan) instead of HIT. Banners sit above the hint so they no longer cover the coach.

**Pass 4d — this drop** (Origin `5e374a5` / GitHub `1bf83fa`)

11. **Late-wave telegraph** — wave spawn IN bursts, LAST RAIDS title, dusk fog heats red on waves 5–6.

Tick / balance unchanged. DemoPilot still wins seed 7.

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

Early game teaches Farm-rail-Hub. Mid-game names the next call. A runner cut is now a readable panic: the rail goes orange, pads shout SPLICE, stuck haulers throb, and splicing snaps them ROLLING. Late waves heat the dusk red and stamp IN on the spawns. The hold is a brace — core cracks red, BRACE shrugs hits when you haul through a raid, and win/lose cards punch the result.

## Gaps

- This VM has no Unity Editor — Play Mode is documented, not screenshot-verified.
- Runtime art is procedural low-poly, not authored FBX.
- Origin remote stays `tmp-*`. GitHub is the Hub/Cursor clone.
- GitHub SHA ≠ Origin SHA (parallel history); pass-4d file bytes match.
- Out of scope: multiplayer, cloud meta, deep tech tree, campaign.
