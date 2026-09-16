# FM-GAME-002 — Firstmate freeze (17:00 AEST Thu 17 Sep 2026)

GitHub (cold-open / Hub / Cursor): **https://github.com/steviebd/colony-haul**

| Field | Value |
| --- | --- |
| Hub-ready | **Yes** |
| Origin SHA | `9decaca07b23ec7fa68820707833b07b52102a2e` |
| GitHub SHA | `a94e81212b57f4f1c66fe8d97427580c44762d49` |
| Kernel | seed-7 playtest **win** (`phase=won`, t≈184.45, Hub L2, minHp≈50.4, sabotages=4) |
| Nested opening | **pass** (Farm → Route → Hub still deposits) |
| Product | Unity 2022.3.50f1 URP under `Assets/_ColonyHaul` — not Vite |
| Feature freeze | **Yes** — Hold order is the last mechanic. Stop churn for the 17:00 captain package. |

Pass 5 trees match on the ten edited paths (byte-identical). Cold clone still has `Assets/`, `Packages/`, `ProjectSettings/`, USB, `playable/`, artifacts tape. Import workflows self-delete.

## Unity feature list

One Game tick. Demo stays on Hold **Auto**, so seed-7 is the same run as Pass 4.

**Through Pass 3**

1. **Opening coach** — Farm → mag-rail to Hub → first hauls.
2. **Mid-watch coach** — splice, offline pad, east kinetic, brownout, Hub L2, thin larder, hottest choke, barrier, splash west.
3. **Hub L2 raising** — gold grow + countdown; Splash pulses WEST for 16s.
4. **Late watch** — CORE THIN, crew stretch / idle pad, second Farm, open north choke.
5. **Logistics** — E/N/W pressure (`*`), inbound, gun fuse, larder clock, guns-hungry Power teach.
6. **Rail Surge / BRACE** — combat deposit braces Hub (0.72× hits; 0.55s, or 0.85s when Hub HP < 72).
7. **Combat reads** — grunt cube, brute capsule, runner cylinder, trails, splash burst, LAST RAIDS.
8. USB vendored at `41e8bca`. Vite is spare lockstep.

**Pass 4**

9. BRACE title + cyan Hub shield, west Splash ghost ring, SPLICED / RAIL LIVE, win/lose cards.
10. Runner-cut recovery — orange rail pulse, SPLICE pads + world timer, STUCK → ROLLING, cut alarm, magenta runner intent, BRACE hits SHRUG. Banners no longer cover the coach.
11. Late-wave telegraph — IN bursts on spawns, LAST RAIDS title, dusk fog heats on waves 5–6.

**Pass 5 — this freeze**

12. **Hold order** (late-match decision). Unlocks **wave 4 + Hub L2**. `H` (or the tray button) cycles **Auto → GUNS → CREW**.
    - **GUNS**: idle haulers rush Power. Feeds towers. Power deposits still **BRACE** the Hub.
    - **CREW**: idle haulers rush Food. Feeds the larder.
    - **Auto**: hungriest stock (demo / default).
    - Consequence is immediate: idle haulers replan on the same tick. Hub ring + pad labels read GUNS or CREW.
    - Splice still outranks this. No new costs. If the preferred pad is empty, haulers take whatever is live — no soft-lock.

## How to play (Editor)

```bash
git clone https://github.com/steviebd/colony-haul.git
```

Unity Hub → Open that folder → `Assets/_ColonyHaul/Scenes/Boot.unity` or `Game_VerticalSlice.unity` → Play.

- **Colony Haul → Play Vertical Slice** — you take the watch (Farm first).
- **Colony Haul → Play Demo (autopilot)** — seed-7 demo (Hold stays Auto).
- **Colony Haul → Run Headless Sim** — expect `win=True` on seed 7.

Keys `1–7` Farm/Mine/Power/Route/Kinetic/Splash/Barrier. `U` Hub L2. **`H` Hold order** (locked until wave 4 with Hub L2). `D` demo. `R` restart.

Win: 6 waves **and** Hub L2. Lose: Hub HP 0, or food stays at 0 for 14s.

**Hold order in a raid:** when guns go hungry, coach says `H for GUNS`. Press H — haulers peel toward the Power pad, deposits BRACE the Hub. If the larder is thin instead, `H for CREW`. Press again to flip. Cut rails still flash SPLICE first.

## Fun call

The late match is now a real call, not just a banner. You either feed the guns (Power, BRACE, Hub lives) or feed the crew (Food, starve clock). Splice still interrupts. Auto still wins seed 7 if you never touch H.

## Gaps

- This VM has no Unity Editor — Play Mode is documented, not screenshot-verified.
- Runtime art is procedural low-poly, not authored FBX.
- Origin remote stays `tmp-*`. GitHub is the Hub/Cursor clone.
- GitHub SHA ≠ Origin SHA (parallel history); pass-5 file bytes match.
- Hold order does not reroute haulers already carrying cargo — they finish the trip, then obey.
- Out of scope: multiplayer, cloud meta, deep tech tree, campaign.
