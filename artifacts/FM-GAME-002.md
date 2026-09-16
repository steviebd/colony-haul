# FM-GAME-002 — Firstmate package (17:00 AEST Thu 17 Sep 2026)

GitHub (cold-open / Hub / Cursor): **https://github.com/steviebd/colony-haul**

| Field | Value |
| --- | --- |
| Hub-ready | **Yes** |
| Origin SHA | `931ac238d34c0b9f7a1ba71a4eaf6d97818c9c19` |
| GitHub SHA | `169ffbcc88580927a9e6c63df99b9e5bbaa888f4` |
| Kernel | seed-7 playtest **win** (`phase=won`, t≈184.45, Hub L2, minHp≈50.4, sabotages=4) |
| Nested opening | **pass** (Farm → Route → Hub still deposits) |
| Hold order | **pass** (locked until wave 4 + L2; then GUNS chase Power) |
| Product | Unity 2022.3.50f1 URP under `Assets/_ColonyHaul` — not Vite |
| Feature freeze | **No** — keep deepening Unity until **~16:30 AEST**, then lock SHAs for the 17:00 captain package. |

Pass 8 trees match on the five edited paths (byte-identical). Cold clone still has `Assets/`, `Packages/`, `ProjectSettings/`, USB, `playable/`, artifacts tape. Import workflows self-delete.

## Unity feature list

One Game tick. Demo stays on Hold **Auto**, so seed-7 is the same run as Pass 4/5.

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

**Pass 5 — kept**

12. **Hold order** (late-match decision). Unlocks **wave 4 + Hub L2**. `H` (or the tray button) cycles **Auto → GUNS → CREW**.
    - **GUNS**: idle haulers rush Power. Feeds towers. Power deposits still **BRACE** the Hub.
    - **CREW**: idle haulers rush Food. Feeds the larder.
    - **Auto**: hungriest stock (demo / default).
    - Consequence is immediate: idle haulers replan on the same tick. Hub ring + pad labels read GUNS or CREW.
    - Splice still outranks this. No new costs. If the preferred pad is empty, haulers take whatever is live — no soft-lock.

**Pass 6 — kept**

13. **Gun LOCK** — live Kinetic / Splash / Hub guns draw a beam to the raider they will shoot (same brute → grunt → runner score as fire). Dry guns go red. Locked raiders flash. Range rings brighten on lock.
14. **BRACE inbound** — during a raid, the nearest loaded haul paints a cyan line to the Hub and calls **BRACE IN Xs** (or BRACE NOW) before the shield pops. INBOUND pip at ~2.4s.
15. **Cargo stamps + spawn forecast** — loaded haulers read FOOD / PWR / ORE. Last 14s before a wave, spawn pads ghost the pack (`4G · 8s`).

**Pass 7 — kept**

16. **UNDER FIRE** — raiders in Hub melee paint CHEW beams. Title: UNDER FIRE. Inbound haul → UNDER FIRE · BRACE IN Xs. Shield up → UNDER FIRE · BRACE shrugs (beams cyan). Splice still outranks this.

**Pass 8 — this beat**

17. **RAIL THREAT** — a runner on a live mag-rail paints that line magenta before sabotage. Midpoint chip **CUT?** then **CUT NOW** when they are 2.2 from the next pad (the tick that can snap the rail). Title: **RAIL THREAT**. If a BRACE haul is inbound it reads **BRACE haul in danger**. Already-cut rails stay orange SPLICE. Sabotage chance/timing unchanged.

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

**Pass 6–8 in a raid:** LOCK beam = who the gun will shoot. FOOD/PWR/ORE haul = BRACE IN Xs. Spawn pads ghost the next mix. Raiders on the Hub = CHEW / UNDER FIRE. Magenta rail + CUT? = a runner is about to snap that line — splice the moment it goes orange.

## Fun call

You get a warning shot. The runner paints the rail magenta before it dies. If a Power haul is on that line, the title already says the BRACE is in danger. Then orange SPLICE, same as Pass 4. Hold order still flips GUNS vs CREW.

## Gaps

- This VM has no Unity Editor — Play Mode is documented, not screenshot-verified.
- Runtime art is procedural low-poly, not authored FBX.
- Origin remote stays `tmp-*`. GitHub is the Hub/Cursor clone.
- GitHub SHA ≠ Origin SHA (parallel history); pass-8 file bytes match.
- Hold order does not reroute haulers already carrying cargo — they finish the trip, then obey.
- Out of scope: multiplayer, cloud meta, deep tech tree, campaign.
