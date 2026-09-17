# FM-GAME-002 — Firstmate package (17:00 AEST Thu 17 Sep 2026)

GitHub (cold-open / Hub / Cursor): **https://github.com/steviebd/colony-haul**

| Field | Value |
| --- | --- |
| Hub-ready | **Yes** |
| Origin Unity | `98f97efedfda3d478c05b86f0e7732fd0bf9e333` |
| GitHub Unity | `10bcd1688cb1e2eeead6a443be0e9728438c6ea6` |
| Kernel | seed-7 playtest **win** (`phase=won`, t≈184.45, Hub L2, minHp≈50.4, sabotages=4, brownouts=1, deposits=115, kills=42, minFood≈32.84) |
| Nested opening | **pass** (Farm → Route → Hub still deposits) |
| Hold order | **pass** (locked until wave 4 + L2; then GUNS chase Power) |
| Product | Unity 2022.3.50f1 URP under `Assets/_ColonyHaul` — not Vite |
| Feature freeze | **No** — exquisite juice until **~16:30 AEST**, then lock SHAs for 17:00. No new Hub counter chips. |

Pass 83 is this file: captain how-to-play, juice-beat list, fun call, SHAs, gaps. C# juice unchanged from PUT DOWN (Bootstrap / SliceHud / SliceJuice). README only adds this BRIEF line. Hub counter family frozen (HOME n aborted). Cold clone still has `Assets/`, `Packages/`, `ProjectSettings/`, USB (`41e8bca`), `playable/`, artifacts tape. Import workflows self-delete.

## How to play (Editor)

```bash
git clone https://github.com/steviebd/colony-haul.git
```

Unity Hub → Open that folder → `Assets/_ColonyHaul/Scenes/Boot.unity` or `Game_VerticalSlice.unity` → Play.

**The watch:** Farm south → rail Hub → haul. Plant pads, gun the chokes, splice orange rails, raise Hub L2, Splash west. Hold six waves with the Hub still up. Right-click *puts a tool down*.

- **Colony Haul → Play Vertical Slice** — you take the watch. South Farm blooms **FARM**, Hub click drops **RAIL**, first cart pops **HAUL**, first drop **HOME**. Later pads **MINE** / **PWR** / **SOW**. Chokes **SET** then **GUNS UP**. Dead click **NOPE** / **NEED** / **LOCK**. Opening mash **STAY**. Armed Route pad again **FREE**. Same tool key **STOW**. Right-click *puts it down* (STAY / FREE / STOW). **U** WEST LIFT; extra cart **OUT**; west Splash **SLAMS**; Barrier **GATE**. Last raids **DUSK CLOSE**. Six waves and the mesa *holds* — **Run it back** or **Watch a run**. **P** on a demo **TAKE**s the mesa. Gun shots stay thin bolts.
- **Colony Haul → Play Demo (autopilot)** — seed-7, Hold stays Auto. `D` starts a *fresh* seed-7 watch. `P` takes a live demo. After HOLD / FALL, `P` does not steal `R`. Demo ignores RMB.
- **Colony Haul → Run Headless Sim** — expect `win=True` on seed 7.

**Keys:** `1–7` Farm / Mine / Power / Route / Kinetic / Splash / Barrier. `U` Hub L2. `H` Hold (wave 4 + L2). `D` demo. `P` take. `R` restart (clears leftover juice). RMB put down (not on the tray).

**Win:** 6 waves **and** Hub L2. **Lose:** Hub HP 0, or food stays at 0 for 14s.

**Hold in a raid:** coach says `H for GUNS` or `H for CREW`. Press H — loaded carts stash unmatched cargo, yank to Power or Food, matching cargo still BRACEs the Hub. Press again to flip. Press once more to Auto — carts *let* go (**LET**). Splice still beats this. Demo stays Auto.

**Readable mesa (frozen chips, not the juice):** titles HAUL CUT > UNDER FIRE / CHEW > RAIL THREAT > CORE BOUND > GUNS DRY > RAIL LIVE > GUNS BACK > CREW UP > WAVE CLEAR / PACK IN / GUNS UP / HOLD READY / … End titles MESA HOLDS / HUB DOWN / STARVED OUT outrank while Phase ≠ Playing. World chips RAID / CUT / GUN / LOAD / OFF / SIT / OPEN / BAR / YARD / FUSE / LARDER / PWR / ORE / STAFF / HP / WAVE CLOCK / ROLL / STUCK / SLOW / DOWN / RUN / IN stay; that family is frozen.

## Juice beats (Pass 56–83)

One Game tick. Demo Hold **Auto** so seed-7 is bit-identical. No new Hub counters. Kill punch skipped.

65. **BRACE SHRUG** — combat deposit: cyan Hub, ice chew, **SHRUG** hits.
66. **SPLICE RELEASE** — orange rail click: **ROLLING**, fat teal line, **RAIL LIVE**.
67. **HOLD YANK** — H GUNS / CREW tracers to the first hop (Auto skips).
68. **DRY→BACK** — barrels **DRY**, mesa dark; Power returns **FIRE** / **GUNS BACK**.
69. **FIRST LINE** — south **FARM**, Hub **RAIL**, first **HAUL**, first **HOME**.
70. **WEST LIFT** — U gold raise, **WEST** to Splash ghost (16s). CREW UP still names the extra haul.
71. **HOLD / FALL** — win **HOLD** gold-green; Hub 0 **DOWN** blood; starve olive **STARVED**.
72. **DUSK CLOSE** — waves 5–6 camera in, ember sun. BRACE cyan still owns the shield.
73. **SPLASH SLAM** — west shots **SLAM**, not a kinetic tick.
74. **SLOW GATE** — Barrier plant **GATE**. SLOW still names the spend.
75. **CREW OUT** — L2 third cart **OUT** of the yard.
76. **ONE LINE** — RAIL / WEST / GATE / OUT / yank share one stroke.
77. **FRESH R** — R forgets leftover juice so FIRST LINE can land again.
78. **FRESH D** — D starts a new demo watch (does not bolt onto a frozen mesa).
79. **STROKE** — corridors fat; gun shots stay thin bolts.
80. **END PAIR** — end card **Run it back** (R) + **Watch a run** (D).
81. **LINE OUT** — later mag-rails stroke like the first. FIRST LINE still owns LINE DOWN / HAUL / HOME.
82. **PAD DROP** — Mine / Power **MINE** / **PWR**. FARM stays the opening.
83. **GUN SET** — Kinetic / Splash **SET**, then **GUNS UP** names live.
84. **TAKE WATCH** — P grabs a live demo (**TAKE**). Boot Play stays silent.
85. **SOW** — later Farm **SOW**. FARM / FARM UP stay the opening.
86. **HOLD LET** — cycle Hold to Auto, carts **LET** go.
87. **NOPE** — dead click **NOPE** / **NEED** / **LOCK**.
88. **STAY** — mash 2–7 before Farm / Hub, pad **STAY**s.
89. **FREE** — second click of armed RouteFrom **FREE**s. Demo never cancels.
90. **STOW** — same tool key holsters. Route does not toggle (FREE owns that).
91. **PUT DOWN** — RMB uses STAY / FREE / STOW. Demo ignores RMB. Tray RMB ignored.
92. **BRIEF** — this package. How-to-play, juice list, fun call, SHAs, gaps. Unity Tick unchanged.

**Kernel (Pass 1–55, kept):** opening + mid-watch coach; Rail Surge / BRACE; Hold order + HOLD PEEL restash; gun LOCK; BRACE inbound; cargo stamps; spawn PACK; titles through CREW UP / RAIL LIVE / GUNS BACK; frozen Hub world chips (see Readable mesa). USB vendored at `41e8bca`. Vite `playable/` is spare lockstep only.

## Fun call

Plant the south Farm and the mesa answers — FARM, RAIL, first HAUL home. Later pads answer **MINE** / **PWR** / **SOW**. A choke **SET**s, then **GUNS UP**. A dead click *refuses*. Mash 2–7 too early and it *holds* you. Click the armed Route pad again and it *frees*. Same key *stows*. Right-click *puts it down*. Press U and the Hub lifts gold, WEST blooms, a third cart pops **OUT**, west Splash **SLAMS**. Five ore drops a **GATE**. Guns click empty then bloom **FIRE**. Press H and carts yank; flip Auto and they *let* go. Click the orange rail and it lets go. Dump a haul into the chew and the Hub **SHRUG**s cyan. Last raids pull the camera in. Hold six waves and the mesa *holds* gold-green — crack the core and it *falls*. That is the watch, not another Hub number.

## Gaps

- This VM has no Unity Editor — Play Mode is documented, not screenshot-verified.
- Runtime art is procedural low-poly, not authored FBX.
- GitHub SHA ≠ Origin SHA (parallel history). Unity file bytes match on PUT DOWN paths. This pass is the captain tape.
- Course-correct: no new Hub counter chips. Juice Pass 56–82 shipped. Kill punch skipped (42 deaths would spam). Input-ACK + RMB closed. First-click RouteFrom stays quiet (demo would spam). Restart’s pip/burst foreach is a no-op.
- Freeze still open until **~16:30 AEST** for polish if crisp, then lock SHAs for 17:00.
- Hold Auto still lets loaded carts finish the trip (demo / default).
- Out of scope: multiplayer, cloud meta, deep tech tree, campaign.
