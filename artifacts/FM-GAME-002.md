# FM-GAME-002 — Firstmate package (17:00 AEST Thu 17 Sep 2026)

GitHub (cold-open / Hub / Cursor): **https://github.com/steviebd/colony-haul**

| Field | Value |
| --- | --- |
| Hub-ready | **Yes** |
| Origin SHA | `a2dc6a4e1d5e3e090f310fdbf5ff1a806503692e` |
| GitHub SHA | `61370cebbde2db1194499e84e7bd5a813c047b0f` |
| Kernel | seed-7 playtest **win** (`phase=won`, t≈184.45, Hub L2, minHp≈50.4, sabotages=4, brownouts=1) |
| Nested opening | **pass** (Farm → Route → Hub still deposits) |
| Hold order | **pass** (locked until wave 4 + L2; then GUNS chase Power) |
| Product | Unity 2022.3.50f1 URP under `Assets/_ColonyHaul` — not Vite |
| Feature freeze | **No** — keep deepening Unity until **~16:30 AEST**, then lock SHAs for the 17:00 captain package. |

Pass 36 trees match on the three edited paths (byte-identical). Pass 35 RAID n, Pass 34 WAVE CLOCK, Pass 33 HUB HP, and prior beats stay on both remotes. Cold clone still has `Assets/`, `Packages/`, `ProjectSettings/`, USB, `playable/`, artifacts tape. Import workflows self-delete.

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

**Pass 8 — kept**

17. **RAIL THREAT** — a runner on a live mag-rail paints that line magenta before sabotage. Midpoint chip **CUT?** then **CUT NOW** when they are 2.2 from the next pad (the tick that can snap the rail). Title: **RAIL THREAT**. If a BRACE haul is inbound it reads **BRACE haul in danger**. Already-cut rails stay orange SPLICE. Sabotage chance/timing unchanged.

**Pass 9 — kept**

18. **GUNS DRY** — when Power drops below a kinetic shot and towers are live, the title holds **GUNS DRY** until Power recovers. A Power haul paints an amber line and chips **POWER IN Xs** / **POWER NOW**. Hub pad reads DRY. Splice / UNDER FIRE / RAIL THREAT still outrank this. Brownout chance/timing unchanged. Seed-7 fires this once.

**Pass 10 — kept**

19. **CORE BOUND** — raiders whose next pad is Hub paint a red last-hop line. Title: **CORE BOUND**. Chip **IN** then **PAD** at 2.4 from the core (the tick before CHEW / UNDER FIRE). If a BRACE haul is inbound it reads **BRACE haul racing them**. Hub pad reads IN / PAD. Splice / UNDER FIRE / RAIL THREAT still outrank this. Tick unchanged. Fires every raid on seed-7.

**Pass 11 — kept**

20. **WAVE CLEAR** — when a raid dies and the next wave is still queued, the title holds **CLEAR** with the next-raid clock and the next readable call (raise Hub L2 / Splash WEST / haul Power). Hub pad reads CLEAR. Logistics shows food and gun clocks. Splice / UNDER FIRE / RAIL THREAT / CORE BOUND / GUNS DRY still outrank this. Tick unchanged. Fires after waves 1–5 on seed-7.

**Pass 12 — kept**

21. **HAUL CUT stakes** — a snapped rail names why splice now. Title/coach/chip: **BRACE stuck** if a loaded haul is trapped during a raid, **Power rail down** if the pylon pad is cut (guns starve until splice), **farm rail down** if the larder line is cut, **ore rail** otherwise. Midpoint chip reads BRACE / PWR / FARM / ORE + countdown. Pads still say SPLICE (the click). HAUL CUT still outranks UNDER FIRE / RAIL THREAT / CORE BOUND / GUNS DRY / WAVE CLEAR. Tick and sabotage chance unchanged. Seed-7 still fires 4 cuts.

**Pass 13 — kept**

22. **PAD OFFLINE** — a Farm / Power / Mine with no live rail home paints a gold ghost line to the Hub. Pad reads **OFFLINE**. Title (below WAVE CLEAR) names the missing rail (Power / farm / ore). Logistics swaps to OFFLINE copy. Splice still outranks this. Tick unchanged. Fires on seed-7 each time Demo plants a producer before the rail click.

**Pass 14 — kept**

23. **SIT stock** — a staffed producer with ≥6 stock and no hauler bound for it reads **HAUL FOOD / PWR / ORE**. Power piles score higher when guns are hungry; farm piles when the larder is thin. An idle hauler paints a colored line to that pad. Title sits below WAVE CLEAR / PAD OFFLINE. Tick unchanged.

**Pass 15 — kept**

24. **GUNS LOW** — when towers have under 9s of fire left but are not yet dry, a Power haul paints amber **POWER IN Xs**. Title **GUNS LOW** (below WAVE CLEAR so CLEAR still owns the intermission). BRACE inbound still wins the same hauler during a raid. GUNS DRY still takes over at brownout. Tick unchanged.

**Pass 16 — kept**

25. **L2 READY** — when Hub L2 is in stock, a gold ring sits on the Hub and the pad reads **READY**. Title **L2 READY** (below CLEAR / GUNS LOW / OFFLINE / SIT so WAVE CLEAR still owns the intermission — CLEAR already says raise Hub L2). Tray pulses **Hub L2 — Splash next**. Press U to spend ore/food/pwr and unlock Splash. Tick unchanged.

**Pass 17 — kept**

26. **OPEN CHOKE** — the hottest unarmed lane with pressure paints **OPEN** on that choke. Title **OPEN EAST / NORTH / WEST** (below L2 READY) names Kinetic, or Splash on west after Hub L2. West stays **SPLASH** during the 16s unlock ghost. Ore-short still names the lane. Tick unchanged.

**Pass 18 — kept**

27. **SLOW** — once that hottest lane has a gun but no Barrier, the choke reads **SLOW**. Title **SLOW EAST / NORTH / WEST** (below OPEN / BRACE inbound) names Barrier. 5 ore slows the approach so guns and BRACE hauls get time. Tick unchanged.

**Pass 19 — kept**

28. **CREW STRETCH** — when producers outnumber crew, the unstaffed pad reads **IDLE**. Title **CREW STRETCH** (below Hold) names U if Hub L2 is in stock, otherwise rail beats a new pad. OFFLINE and SIT still own that pad when they apply. Tick unchanged.

**Pass 20 — kept**

29. **CORE THIN** — when Hub HP drops below 72 during a raid, the pad reads **THIN**. Title **CORE THIN** (below GUNS LOW so CLEAR still owns the intermission) names the BRACE haul if one is inbound. Seed-7 hits this (minHp≈50.4). Tick unchanged.

**Pass 21 — kept**

30. **HOLD READY** — wave 4 with Hub L2 and Hold still Auto: the H tray pulses **GUNS / CREW**, a gold ring sits on the Hub, and the pad reads **HOLD**. Title **HOLD READY** (below CREW STRETCH, so LAST RAIDS still owns waves 5–6) names H for GUNS if the fuse is hungry, H for CREW if the larder is thin. Seed-7 fires this (~26s from t≈125, Hub HP still ≥72 so CORE THIN does not steal the raid). Demo stays Auto. Tick unchanged.

**Pass 22 — kept**

31. **HAUL HOME** — between raids, the nearest loaded haul paints a cargo-colored line to the Hub and chips **FOOD / PWR / ORE IN Xs**. Title **HAUL HOME** (below HOLD READY so CLEAR / HOLD READY still outrank it) names the drop. BRACE inbound still owns the same haul during a raid. Seed-7 fires this on the opening hauls and every CLEAR (~115s of the run). Tick unchanged.

**Pass 23 — kept**

32. **PACK IN** — last 8s before a wave, spawn pads read **PACK** and ghost the pack harder. Title **PACK IN** (below HOLD READY so CLEAR still owns later intermissions) names the lane. Wave 1 on seed-7 shows the title (t≈34, not CLEAR); waves 2–6 still ping the pads under CLEAR. Tick unchanged.

**Pass 24 — kept**

33. **GUNS UP** — a new Kinetic / Splash going live reads **UP** for 8s, the Power pad reads **FEED**, and a teal range ring sits on that gun. Title **GUNS UP** (below PACK IN so PACK IN still owns the last 8s before a wave; CLEAR still owns later intermissions) names the lane and tells you to haul Power before the fuse is hungry. Seed-7 titles EAST (t≈3.85) then NORTH (t≈29.6, until PACK IN at t≈34); Splash WEST still pings under CLEAR. Tick unchanged.

**Pass 25 — kept**

34. **RAIL LIVE** — after a splice, the recovered rail pulses teal and the pads read **LIVE** for 4s. Title **RAIL LIVE** (below GUNS DRY, above CLEAR) names what is rolling again — Power / farm / BRACE. Combat titles still outrank it during a raid. Seed-7 fires this four times in intermission (t≈82 Power, t≈110 Power, t≈137 haul, t≈164 Power). Tick unchanged.

**Pass 26 — kept**

35. **GUNS BACK** — after a brownout recovers, live guns pulse lime and the Hub pad reads **BACK** for 8s. Title **GUNS BACK** (below RAIL LIVE, above CLEAR) names keep Power rolling so they do not dry again. Combat titles still outrank it during a raid. Seed-7 titles after wave 3 (dry-off t≈101.75, raid-off t≈106, ~3.7s before RAIL LIVE at t≈110). Tick unchanged.

**Pass 27 — kept**

36. **CREW UP** — when Hub L2 grants the extra hauler, that cart pulses green and the yard reads **CREW** for 12s. Title **CREW UP** (below GUNS BACK, above CLEAR) names the third haul so you staff another pad. Combat / RAIL LIVE still outrank it. Seed-7 juices at L2 (t≈79 during the raid) and titles after RAIL LIVE (t≈86–91). Tick unchanged.

**Pass 28 — kept**

37. **GUN FUSE** — live Kinetic / Splash chip remaining fire seconds (**FUSE Ns**). Amber under 9s, red when dry. World chips only — GUNS LOW / GUNS BACK / RAIL LIVE / CREW UP / CLEAR still own the title. Seed-7 fires from the first east kinetic. Tick unchanged.

**Pass 29 — kept**

38. **LARDER** — staffed farms chip remaining food seconds (**LARDER Ns**). Amber when Food is under 11, red when starving. World chips only — HOLD READY / SIT / OFFLINE / CLEAR still own the title. Seed-7 fires from the first south farm. Tick unchanged.

**Pass 30 — kept**

39. **HAUL ROLL** — every other loaded haul chips cargo ETA (**FOOD / PWR / ORE Ns**). BRACE / POWER / HOME still own the named cart. World chips only. Seed-7 fires from the opening hauls (HOME is gated until t=28) and on the extra L2 cart. Tick unchanged.

**Pass 31 — kept**

40. **PAD STOCK** — staffed pylons and mines chip pad stock (**PWR n** / **ORE n**). Amber when piled (≥6) or guns hungry, red when dry. Farms stay on LARDER. World chips only — SIT / OFFLINE / FEED still own those pads. Seed-7 fires from the first mine and pylon. Tick unchanged.

**Pass 32 — kept**

41. **STAFF** — the Hub chips crew vs pads (**STAFF n/m**) under the HP bar. Amber when stretched, green on CREW UP. World chips only — CREW STRETCH / CREW UP / CLEAR still own the title. Seed-7 fires from the first farm. Tick unchanged.

**Pass 33 — kept**

42. **HUB HP** — Hub HP number sits beside the world bar (**HP n**). Red when the core is thin. World chips only — CORE THIN still owns the raid title. Seed-7 fires from t=0 and stays live between raids (minHp≈50.4). Tick unchanged.

**Pass 34 — kept**

43. **WAVE CLOCK** — next-wave clock sits on the other side of the Hub bar (**Wn Xs**). Amber in the last 8s. World chips only — CLEAR still owns the intermission chip; PACK IN still owns the title. Seed-7 fires from t=0 (`W1`) and during raids. Tick unchanged.

**Pass 35 — kept**

44. **RAID n** — live raiders chip **RAID n** above CHEW on the Hub. Red while chewing, rust otherwise, amber if the core is thin. World chips only — UNDER FIRE / CHEW still own the title. Seed-7 fires every raid. Tick unchanged.

**Pass 36 — this beat**

45. **IN +n** — still-queued raiders chip **IN +n** beside RAID on the Hub. Amber if the core is thin. World chips only — CORE BOUND still owns hy-18 IN/PAD; PACK IN still owns the last 8s. Seed-7 fires every wave as the pack staggers in. Tick unchanged.

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

**Pass 6–36 in a raid:** LOCK beam = who the gun will shoot. FOOD/PWR/ORE haul = BRACE IN Xs. Magenta rail + CUT? = splice before the snap. After the snap, the orange chip names the stake — BRACE / PWR / FARM — so splice is a recovery, not a generic repair. After you splice, **RAIL LIVE** names what is rolling again for 4s (CLEAR still owns the rest of the intermission). After brownout recovers, **GUNS BACK** names the towers live again for 8s so you keep Power rolling (RAIL LIVE still outranks a splice; CLEAR still owns later intermission). When Hub L2 lands, **CREW UP** names the extra haul from the yard so you staff another pad. A producer with no rail home reads **OFFLINE** and ghosts a gold line to Hub. A piled pad with no inbound hauler reads **HAUL FOOD / PWR / ORE**. Before brownout, **GUNS LOW** paints amber POWER IN Xs. GUNS DRY = towers clicked empty. CORE BOUND = they are on the Hub pad next. After the last raider drops, **CLEAR** names the next 26s — Hub L2, Splash WEST, or haul Power before the next pack. Last 8s before a wave, spawn pads read **PACK** and **PACK IN** names the lane (CLEAR still owns the intermission title). Live Kinetic / Splash chip **FUSE Ns** (amber hungry, red dry) so you haul Power before GUNS LOW. Staffed farms chip **LARDER Ns** (amber Food under 11, red starving) so you haul Food before the clock fails. Staffed pylons and mines chip **PWR n / ORE n** so you haul the pad before SIT. The Hub chips **STAFF n/m** so you raise L2 or stop planting idle pads. Hub HP sits as **HP n** beside the world bar so CORE THIN is readable between raids. Next wave sits as **Wn Xs** on the other side so CLEAR is readable during a raid. Live raiders chip **RAID n** above CHEW so UNDER FIRE still has a count between chew bursts. Still-queued raiders chip **IN +n** beside RAID so the rest of the pack stays readable (CORE BOUND still owns hy-18 IN/PAD). Other loaded hauls chip **FOOD / PWR / ORE Ns** so the second cart stays readable (BRACE / POWER / HOME still own the named cart). A new gun going live reads **UP** and **GUNS UP** names the lane so you haul Power before the fuse is hungry (PACK IN still wins the last 8s; CLEAR still owns later intermissions). Between raids a loaded haul paints **HAUL HOME** as FOOD / PWR / ORE IN Xs (BRACE still owns that cart in a raid). When Hub L2 is in stock the pad reads **READY** (CLEAR still owns the intermission) and the tray pulses **Hub L2 — Splash next**. An unarmed hottest lane with raiders on it reads **OPEN** and names Kinetic (or Splash west after L2). After the gun is up, that choke reads **SLOW** and names Barrier. An overbuilt pad with no crew reads **IDLE**. When Hub HP cracks below 72 in a raid the pad reads **THIN** and names the BRACE haul. Wave 4 with Hub L2 and Hold still Auto reads **HOLD** and pulses **H Hold — GUNS / CREW** (LAST RAIDS still owns waves 5–6).

## Fun call

Live raiders chip RAID n above CHEW, so UNDER FIRE still has a count between chew bursts. Still-queued raiders chip IN +n beside it so the rest of the pack stays readable. Next wave sits as Wn Xs beside the Hub bar. Hub HP sits as a number on the other side. STAFF shows crew headroom. Pylons and mines chip pad stock. Farms still chip LARDER. Every loaded haul chips drop ETA — BRACE, POWER, and HOME still name the cart that matters most. Live guns still chip FUSE. CLEAR still owns the intermission. PACK IN still owns the last 8s.

## Gaps

- This VM has no Unity Editor — Play Mode is documented, not screenshot-verified.
- Runtime art is procedural low-poly, not authored FBX.
- Origin remote stays `tmp-*`. GitHub is the Hub/Cursor clone.
- GitHub SHA ≠ Origin SHA (parallel history); pass-36 file bytes match.
- Hold order does not reroute haulers already carrying cargo — they finish the trip, then obey.
- Out of scope: multiplayer, cloud meta, deep tech tree, campaign.
