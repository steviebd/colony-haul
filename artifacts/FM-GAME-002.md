# FM-GAME-002 — Firstmate package (17:00 AEST Thu 17 Sep 2026)

GitHub (cold-open / Hub / Cursor): **https://github.com/steviebd/colony-haul**

| Field | Value |
| --- | --- |
| Hub-ready | **Yes** |
| Origin SHA | `517d1146a1b92f68c4729af97a11bdbc2a959f05` |
| GitHub SHA | `e928b96c2dd50fa998598abc8e66686d898368d6` |
| Kernel | seed-7 playtest **win** (`phase=won`, t≈184.45, Hub L2, minHp≈50.4) |
| Product | Unity 2022.3.50f1 URP under `Assets/_ColonyHaul` — not Vite |

Pass-3b trees match on the ten edited paths (byte-identical). Cold clone still has `Assets/`, `Packages/`, `ProjectSettings/`, USB, `playable/`, artifacts tape. Workflows self-delete after import.

## Unity feature list

One Game tick. Balance numbers unchanged. Rail Surge is the only new rule: a deposit while raiders are live cuts Hub hit damage for 0.55s (haul + colony stock + core defense). DemoPilot still wins seed 7 (easier Hub).

1. **Opening coach** — Farm → mag-rail to Hub → first hauls.
2. **Mid-watch coach** — splice, offline pad, east kinetic, brownout, Hub L2, thin larder, hottest choke, barrier, splash west.
3. **Hub L2 raising** — gold grow + coach countdown; then Splash pulses WEST for 16s.
4. **Logistics** — E/N/W pressure (`*`), inbound, gun fuse, larder clock. Guns hungry calls Power haul without locking tools. Power pads label FEED.
5. **Rail Surge** — combat deposit braces Hub (0.72× hits). Cyan burst + SURGE pip.
6. **Combat reads** — grunt cube, brute capsule, runner cylinder, ground blobs, thicker tracers, splash burst, lane wave banner.
7. **Juice** — deposit spokes, sabotage CUT, brownout DRY, barrier SLOW, kill punch, cargo trails, choke heat, world Hub HP.
8. USB vendored at `41e8bca`. TS kernel carries Surge for lockstep. Vite is spare.

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

You can read a mid-wave at a glance: which lane is hot, how long the larder and guns have, and whether the next call is splice, Power, or Splash. Hauling through a raid visibly braces the Hub. Hub L2 feels earned — the core grows, then the west choke asks for Splash.

## Gaps

- This VM has no Unity Editor — Play Mode is documented, not screenshot-verified.
- Runtime art is procedural low-poly, not authored FBX.
- Origin remote stays `tmp-*`. GitHub is the Hub/Cursor clone.
- GitHub SHA ≠ Origin SHA (parallel history); pass-3b file bytes match.
- Out of scope: multiplayer, cloud meta, deep tech tree, campaign.
