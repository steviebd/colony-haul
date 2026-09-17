# FM-GAME-002 — Firstmate package (17:00 AEST Thu 17 Sep 2026)

GitHub (cold-open / Hub / Cursor): **https://github.com/steviebd/colony-haul**

| Field | Value |
| --- | --- |
| Hub-ready | **Yes** |
| Origin SHA | `8e9bb9eee4d29b726add10a5b15ac526f659bcc8` |
| GitHub SHA | `f1fa2400bf80190cc6ec37036b3ce397e2ae3c16` |
| Kernel | seed-7 playtest **win** (`phase=won`, t≈184.45, Hub L2, minHp≈50.4, sabotages=4, brownouts=1) |
| Nested opening | **pass** (Farm → Route → Hub still deposits) |
| Hold order | **pass** (locked until wave 4 + L2; then GUNS chase Power) |
| Product | Unity 2022.3.50f1 URP under `Assets/_ColonyHaul` — not Vite |
| Feature freeze | **No** — **course-correct:** exquisite gameplay / juice until **~16:30 AEST** (no more Hub counter chips), then lock SHAs for the 17:00 captain package. |

Pass 75 trees match on the four edited paths (byte-identical, 46/4). Pass 56–74 kept (BRACE SHRUG, SPLICE RELEASE, HOLD YANK, DRY→BACK, FIRST LINE, WEST LIFT, HOLD / FALL, DUSK CLOSE, SPLASH SLAM, SLOW GATE, CREW OUT, ONE LINE, FRESH R, FRESH D, STROKE, END PAIR, LINE OUT, PAD DROP, GUN SET). HOME n stays aborted. Hub counter family frozen. Cold clone still has `Assets/`, `Packages/`, `ProjectSettings/`, USB, `playable/`, artifacts tape. Import workflows self-delete.

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
    - **GUNS**: haulers peel to Power, even if they were carrying Food/Ore (that cargo stashes back on a live pad). Matching Power hauls still rush Hub and **BRACE**.
    - **CREW**: haulers peel to Food the same way. Matching Food hauls still rush Hub.
    - **Auto**: hungriest stock (demo / default). Loaded carts finish their trip.
    - Consequence is immediate: empty and loaded haulers replan on the same tick. If the preferred pad is empty or cut, they take whatever is live — no soft-lock. Hub ring + pad labels read GUNS or CREW.
    - Splice still outranks this. No new costs. If the preferred pad is empty or cut, haulers take whatever is live — no soft-lock.

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

**Pass 36 — kept**

45. **IN +n** — still-queued raiders chip **IN +n** beside RAID on the Hub. Amber if the core is thin. World chips only — CORE BOUND still owns hy-18 IN/PAD; PACK IN still owns the last 8s. Seed-7 fires every wave as the pack staggers in. Tick unchanged.

**Pass 37 — kept**

46. **DOWN n** — this-wave kills chip **DOWN n** above RAID on the Hub. Green after the pack drops. World chips plus a DOWN pip on the corpse — PACK IN still owns the last 8s. Seed-7 fires from the first kill of wave 1. Tick unchanged.

**Pass 38 — kept**

47. **HOLD PEEL** — GUNS / CREW restash unmatched cargo on a live pad and replan loaded haulers the same tick. Matching Power/Food hauls still rush Hub (BRACE). If the preferred pad is empty or cut, they take whatever is live. Auto still finishes the current trip. Demo stays Auto so seed-7 is bit-stable. Tick unchanged.

**Pass 39 — kept**

48. **LAST** — the last live raider chips **LAST**. World chips only — RAID 1 / CHEW / UNDER FIRE still own the Hub. Seed-7 fires at the end of every wave. Tick unchanged.

**Pass 40 — kept**

49. **RUN n** — live runners chip **RUN n** beside DOWN on the Hub. Magenta when a rail is threatened. World chips only — RAIL THREAT / CUT? still own the title. Seed-7 fires from wave 2. Tick unchanged.

**Pass 41 — kept**

50. **CUT n** — snapped rails chip **CUT n** left of DOWN on the Hub. World chips only — HAUL CUT / SPLICE still own the title and rail countdown. Seed-7 fires on all four sabotages. Tick unchanged.

**Pass 42 — kept**

51. **STUCK n** — frozen haulers chip **STUCK n** above CUT on the Hub. World chips only — HAUL CUT still owns the title; STUCK pips still fire on the carts. Seed-7 fires whenever a cut traps a haul. Tick unchanged.

**Pass 43 — kept**

52. **SLOW n** — slowed raiders chip **SLOW n** beside STUCK on the Hub. World chips only — SLOW EAST/NORTH/WEST still owns the Barrier title. Seed-7 fires on barrier walks and Splash slows. Tick unchanged.

**Pass 44 — kept**

53. **ROLL n** — haulers still moving during a cut chip **ROLL n** between STUCK and SLOW. World chips only — HAUL CUT still owns the title; HAUL ROLL still owns cargo ETA on the carts; ROLLING pips still fire on splice. Seed-7 fires when a snap leaves another haul on a live rail. Tick unchanged.

**Pass 45 — kept**

54. **GUN n** — live Kinetic / Splash chip **GUN n** beside STAFF on the Hub. Amber when hungry, red when dry. World chips only — GUNS LOW / GUNS DRY / GUNS UP still own the title; FUSE still sits on the barrels. Seed-7 fires from the first east kinetic. Tick unchanged.

**Pass 46 — kept**

55. **LOAD n** — loaded haulers chip **LOAD n** under WAVE CLOCK on the Hub. Cyan during a raid (BRACE), gold between raids (HOME). World chips only — BRACE inbound / HAUL HOME / HAUL ROLL still own the carts. Seed-7 fires from the opening farm hauls. Tick unchanged.

**Pass 47 — kept**

56. **OFF n** — unrouted producers chip **OFF n** left of RAID on the Hub. World chips only — PAD OFFLINE still owns the title and ghost rail. Hidden during a cut so HAUL CUT still owns splice. Seed-7 fires each time Demo plants before the rail click. Tick unchanged.

**Pass 48 — kept**

57. **SIT n** — piled pads with no inbound haul chip **SIT n** under STAFF on the Hub. Amber when guns are hungry. World chips only — SIT still owns the title; PAD STOCK still sits on the pylons and mines. Hidden during a cut or OFFLINE. Seed-7 fires when a staffed pad piles ≥6. Tick unchanged.

**Pass 49 — kept**

58. **OPEN n** — unarmed chokes with pressure chip **OPEN n** under GUN on the Hub. World chips only — OPEN EAST/NORTH/WEST still owns the title and choke chip. Seed-7 fires whenever a live lane has no gun. Tick unchanged.

**Pass 50 — kept**

59. **BAR n** — barred chokes chip **BAR n** under LOAD on the Hub. World chips only — SLOW EAST/NORTH/WEST still owns the Barrier title. Seed-7 fires from the first east Barrier. Tick unchanged.

**Pass 51 — kept**

60. **YARD n** — idle haulers chip **YARD n** under SIT on the Hub. Amber when a pad is sitting. World chips only — SIT / CREW STRETCH still own those titles (unstaffed pads still read IDLE). Seed-7 fires while carts stand in the yard. Tick unchanged.

**Pass 52 — kept**

61. **FUSE Ns** — remaining fire seconds chip **FUSE Ns** under OPEN on the Hub. Amber when hungry, red when dry. World chips only — GUNS LOW / GUNS DRY still own the title; FUSE still sits on the barrels. Seed-7 fires from the first east kinetic. Tick unchanged.

**Pass 53 — kept**

62. **LARDER Ns** — remaining food seconds chip **LARDER Ns** under BAR on the Hub. Amber when Food is under 11, red when starving. World chips only — HOLD READY / SIT / OFFLINE / CLEAR still own the title; LARDER still sits on the farms. Seed-7 fires from the first south farm. Tick unchanged.

**Pass 54 — kept**

63. **PWR n** — colony Power chips **PWR n** under FUSE on the Hub. Amber when hungry, red when dry. World chips only — GUNS LOW / GUNS DRY still own the title; FUSE still sits on the barrels; PAD STOCK still sits on the pylons. Seed-7 fires from t=0 and hits 0 on the brownout. Tick unchanged.

**Pass 55 — kept**

64. **ORE n** — colony Ore chips **ORE n** under LARDER on the Hub. Amber under 16 (can't Kinetic), red under 5 (can't Barrier). World chips only — L2 READY / OPEN / SLOW still own those titles; PAD STOCK still sits on the mines. Seed-7 fires from t=0. Tick unchanged.

**Pass 56 — kept (course-correct)**

65. **BRACE SHRUG** — a combat deposit's shield now *feels* bought. Hub goes cyan while Surging (chew-red no longer eats the pad). Shield ring + inner core breathe. Chew beams thicken ice-blue. Hits pop **SHRUG** bursts. Mesa fog washes cyan instead of blood. Juice only — Tick, Surge window, and hit math unchanged. Seed-7 still braces from raid deposits. No new Hub counters.

**Pass 57 — kept**

66. **SPLICE RELEASE** — clicking the orange rail now *releases* the haul. SPLICED shockwave on the snap, stuck carts pop **ROLLING**, the recovered line fattens teal, and **RAIL LIVE** blooms with a mesa wash. Juice only — Tick, sabotage chance, and splice timing unchanged. Seed-7 still fires four cuts. No new Hub counters.

**Pass 58 — kept**

67. **HOLD YANK** — pressing H now *yanks* the carts. Each hauler traces to its first hop, pops **GUNS** or **CREW**, the destination pad pulses, and the mesa washes. Auto skips the yank so demo/seed-7 stays bit-stable. Juice only — Tick and Hold replan unchanged. No new Hub counters.

**Pass 59 — kept**

68. **DRY→BACK** — guns clicking empty now *die*, then *come back*. Every live barrel pops **DRY**, the mesa goes amber-dark, and range rings shrink. When Power returns, barrels bloom **FIRE**, LOCK beams go lime, rings fatten, and **GUNS BACK** washes the mesa. Juice only — Tick, brownout chance, and BACK window unchanged. Seed-7 still browns out once (t≈101.75). No new Hub counters.

**Pass 60 — kept**

69. **FIRST LINE** — the opening now *lands*. Planting the south Farm blooms **FARM**. Clicking Hub snaps **RAIL** down the corridor and fattens the line. The first cart pops **HAUL**; the first drop blooms **HOME**. Juice only — Tick, opening gate, and haul math unchanged. Seed-7 still farms south then rails Hub. No new Hub counters.

**Pass 61 — kept**

70. **WEST LIFT** — pressing U now *lifts* the Hub, then Splash *unlocks* west. RAISE bloom + gold fog + growing gold rings while the pad spends. L2 land pops **L2**, traces **WEST** to the choke, fattens the ghost Splash ring, and washes the mesa amber for 16s. CREW UP still names the extra haul. Juice only — Tick, Hub L2 time, and Splash unlock window unchanged. Seed-7 still raises during the raid (t≈79) then SplashFresh 16s. No new Hub counters.

**Pass 62 — kept**

71. **HOLD / FALL** — the watch now *ends*. A win blooms **HOLD**, washes the mesa gold-green, rails sing, and a shield breathes. Hub HP 0 cracks **DOWN** in blood fog; a dead larder goes olive **STARVED**. Juice only — Tick, win rule (6 waves + Hub L2), and lose timers unchanged. Seed-7 still wins at t≈184.45. Stress still reaches lost_hub / starve. No new Hub counters.

**Pass 63 — kept**

72. **DUSK CLOSE** — last raids now *close in*. Wave 5/6 punch **LAST**, the camera pulls onto the Hub, fog shortens, the sun drops ember-red, and dusk heat stays under the fight. BRACE cyan still owns the shield. HOLD / FALL still owns the ending. Juice only — Tick and last-raid wave math unchanged. Seed-7 still titles LAST RAIDS on waves 5–6. No new Hub counters.

**Pass 64 — kept**

73. **SPLASH SLAM** — west Splash now *slams*. Each shot pops **SLAM**, a fat amber shockwave, and a low boom — not a kinetic tick. The barrel ring fattens; slowed raiders crunch. WEST LIFT's payoff. Juice only — Tick, Splash radius/slow, and cooldown unchanged. Seed-7 still plants west Splash. No new Hub counters. GUNS UP / SLOW titles untouched.

**Pass 65 — kept**

74. **SLOW GATE** — planting a Barrier now *drops a gate*. GATE bloom + spawn-line tracers + fattened beam, not a rail-cut beep. SLOW EAST/NORTH/WEST still names the spend before the plant. Juice only — Tick, Barrier cost, and slow factor unchanged. Seed-7 still plants east then north. No new Hub counters. PingSlowChoke teach stays.

**Pass 66 — kept**

75. **CREW OUT** — Hub L2's extra cart now *rolls out*. Yard pops **OUT**, traces onto the first hop, the new cube fattens, and a 580 Hz roll replaces the deposit tick. CREW UP still names the 12s. WEST LIFT still owns L2 land / WEST. Juice only — Tick, extra hauler count, and L2 time unchanged. Seed-7 still juices at L2 (t≈79). No new Hub counters. Kill punch skipped (42 deaths would spam).

**Pass 67 — kept**

76. **ONE LINE** — RAIL / WEST / GATE / OUT / HOLD YANK tracers now share one stroke. FIRST LINE HAUL beeps; HOME dual-bursts. GATE and OUT wash the mesa like FARM / RAIL. WEST LIFT still outranks CREW fog. Juice only — Tick unchanged. No new Hub counters. Kill punch skipped.

**Pass 68 — kept**

77. **FRESH R** — pressing R now *forgets* the last watch. GATE / OUT / SLAM pips, tracers, bursts, the cut alarm, dusk close, and banners clear so the opening is FIRST LINE again. Juice only — Tick unchanged. No new Hub counters. Kill punch skipped.

**Pass 69 — kept**

78. **FRESH D** — D now *starts a new demo watch*. It no longer bolts the autopilot onto a live or ended mesa (after HOLD / FALL the sim was frozen, so D did nothing). Juice clears like R. Tick unchanged. Seed-7 still wins. No new Hub counters. Kill punch skipped.

**Pass 70 — kept**

79. **STROKE** — RAIL / WEST / GATE / OUT / HOLD YANK now *stroke* a corridor. Gun shots, Splash bolts, death ticks, and deposit spokes stay thin. Juice only — Tick unchanged. No new Hub counters. Kill punch skipped.

**Pass 71 — kept**

80. **END PAIR** — after HOLD / FALL the card offers **Run it back** (R, you take the watch) and **Watch a run** (D, fresh seed-7 demo). FRESH R / FRESH D already clear leftover juice. Tick unchanged. No new Hub counters. Kill punch skipped.

**Pass 72 — kept**

81. **LINE OUT** — every new mag-rail now *strokes* like the first. Mine / Power / second Farm click-Hub drops **RAIL**, fattens the corridor, and beeps the rail — not a quiet deposit tick. FIRST LINE still owns the opening **LINE DOWN** banner, HAUL, and HOME. Splice still owns RAIL LIVE. Juice only — Tick unchanged. No new Hub counters. Kill punch skipped.

**Pass 73 — kept**

82. **PAD DROP** — planting a Mine or Power now *drops* on the pad. **MINE** / **PWR** bloom, the pad pulses, the cube fattens. FIRST LINE still owns FARM / LINE DOWN / HAUL / HOME. LINE OUT still owns the click-Hub stroke. No banner steal. Juice only — Tick unchanged. No new Hub counters. Kill punch skipped.

**Pass 74 — kept**

83. **GUN SET** — clicking Kinetic or Splash now *sets* the barrel. **SET** bloom on the choke for 0.85s, then **GUNS UP** still names going live. WEST LIFT still owns WEST. SLAM still owns shots. GATE still owns Barrier. No banner steal. Juice only — Tick unchanged. No new Hub counters. Kill punch skipped.

**Pass 75 — this beat**

84. **TAKE WATCH** — `P` during a demo now *takes* the live mesa. Autopilot drops, the Hub blooms **TAKE**, pale-gold fog, and you keep playing from that tick. Boot Play stays silent. After HOLD / FALL, `P` does not steal `R`. Juice only — Tick unchanged. Seed-7 still wins on Auto. No new Hub counters. Kill punch skipped.

## How to play (Editor)

```bash
git clone https://github.com/steviebd/colony-haul.git
```

Unity Hub → Open that folder → `Assets/_ColonyHaul/Scenes/Boot.unity` or `Game_VerticalSlice.unity` → Play.

- **Colony Haul → Play Vertical Slice** — you take the watch (Farm first). South Farm blooms **FARM**, Hub click drops **RAIL** as a fat teach stroke, first cart pops **HAUL**. Later pads stroke **RAIL** the same way. Planting a Mine or Power blooms **MINE** / **PWR**. Kinetic / Splash click **SET**s the barrel — **GUNS UP** still names going live. Press **U** for **WEST LIFT**. The extra cart pops **OUT** of the yard. West Splash **SLAMS**. Barrier spends drop a **GATE**. Last raids **DUSK CLOSE** onto the Hub. Survive six waves and the mesa *holds* — the card offers **Run it back** or **Watch a run**. Gun shots stay thin bolts.
- **Colony Haul → Play Demo (autopilot)** — seed-7 demo (Hold stays Auto). `D` in Play also starts a **fresh** seed-7 watch (clears leftover juice, does not bolt onto a frozen mesa). `P` takes the live mesa from a running demo (**TAKE** bloom). After HOLD / FALL, `P` does not steal `R`.
- **Colony Haul → Run Headless Sim** — expect `win=True` on seed 7.

Keys `1–7` Farm/Mine/Power/Route/Kinetic/Splash/Barrier. `U` Hub L2. **`H` Hold order** (locked until wave 4 with Hub L2). `D` demo (fresh seed-7 watch). `P` take (grabs a live demo). `R` restart (clears leftover juice).

Win: 6 waves **and** Hub L2. Lose: Hub HP 0, or food stays at 0 for 14s.

**Hold order in a raid:** when guns go hungry, coach says `H for GUNS`. Press H — loaded carts stash other cargo and peel toward the Power pad, each cart yanks with a GUNS tracer, Power deposits BRACE the Hub. If the larder is thin instead, `H for CREW`. Press again to flip. Cut rails still flash SPLICE first.

**Pass 6–68 in a raid:** LOCK beam = who the gun will shoot. FOOD/PWR/ORE haul = BRACE IN Xs. Magenta rail + CUT? = splice before the snap. After the snap, the orange chip names the stake — BRACE / PWR / FARM — so splice is a recovery, not a generic repair. After you splice, the line fattens teal, stuck carts pop **ROLLING**, and **RAIL LIVE** blooms what is rolling again for 4s (CLEAR still owns the rest of the intermission). After brownout recovers, barrels bloom **FIRE**, LOCK beams go lime, and **GUNS BACK** washes the mesa so you keep Power rolling (RAIL LIVE still outranks a splice on the title; CLEAR still owns later intermission). When Hub L2 lands, **CREW UP** names the extra haul from the yard so you staff another pad — the third cart pops **OUT** and traces onto the line. A producer with no rail home reads **OFFLINE** and ghosts a gold line to Hub. A piled pad with no inbound hauler reads **HAUL FOOD / PWR / ORE**. Before brownout, **GUNS LOW** paints amber POWER IN Xs. GUNS DRY = towers clicked empty. CORE BOUND = they are on the Hub pad next. After the last raider drops, **CLEAR** names the next 26s — Hub L2, Splash WEST, or haul Power before the next pack. Last 8s before a wave, spawn pads read **PACK** and **PACK IN** names the lane (CLEAR still owns the intermission title). Live Kinetic / Splash chip **FUSE Ns** (amber hungry, red dry) so you haul Power before GUNS LOW. Staffed farms chip **LARDER Ns** (amber Food under 11, red starving) so you haul Food before the clock fails. Staffed pylons and mines chip **PWR n / ORE n** so you haul the pad before SIT. The Hub chips **STAFF n/m** so you raise L2 or stop planting idle pads. Hub HP sits as **HP n** beside the world bar so CORE THIN is readable between raids. Next wave sits as **Wn Xs** on the other side so CLEAR is readable during a raid. Live raiders chip **RAID n** above CHEW so UNDER FIRE still has a count between chew bursts. Still-queued raiders chip **IN +n** beside RAID so the rest of the pack stays readable (CORE BOUND still owns hy-18 IN/PAD). This-wave kills chip **DOWN n** above RAID so the raid shrinking is readable (PACK IN still owns the last 8s). The last live raider chips **LAST**. Live runners chip **RUN n** beside DOWN so RAIL THREAT still has a count between CUT? bursts. Snapped rails chip **CUT n** so HAUL CUT still has a count until splice. Frozen carts chip **STUCK n** above that so splice is a recovery. Slowed raiders chip **SLOW n** beside STUCK so Barrier / Splash still have a count. Hauls still moving on live rails chip **ROLL n** between them so splice is a recovery, not a full stop. Live Kinetic / Splash chip **GUN n** beside STAFF so RAID still has a barrel count (FUSE stays on the guns). Loaded carts chip **LOAD n** under WAVE CLOCK so BRACE / HOME still have a count. Unrouted producers chip **OFF n** left of RAID so PAD OFFLINE still has a count. Piled pads with no inbound haul chip **SIT n** under STAFF so SIT still has a count. Unarmed chokes with pressure chip **OPEN n** under GUN so OPEN still has a count. Barred chokes chip **BAR n** under LOAD so Barrier still has a count. Idle haulers chip **YARD n** under SIT so a sitting pile still has carts to send. Remaining fire chips **FUSE Ns** under OPEN so GUN n still has a clock. Remaining food chips **LARDER Ns** under BAR so YARD still has a food clock. Colony Power chips **PWR n** under FUSE so GUN n still has a tank. Colony Ore chips **ORE n** under LARDER so OPEN / L2 still have a spend tank. A combat deposit **BRACEs** the Hub — cyan pad, breathing shield, ice chew beams, **SHRUG** bursts on hits (chew-red no longer eats the core while the shield is up). **H GUNS / CREW** peels loaded carts and *yanks* them toward the pad — unmatched cargo stashes on a live pad, matching Power/Food still rush Hub. Other loaded hauls chip **FOOD / PWR / ORE Ns** so the second cart stays readable (BRACE / POWER / HOME still own the named cart). A new gun going live reads **UP** and **GUNS UP** names the lane so you haul Power before the fuse is hungry (PACK IN still wins the last 8s; CLEAR still owns later intermissions). Between raids a loaded haul paints **HAUL HOME** as FOOD / PWR / ORE IN Xs (BRACE still owns that cart in a raid). When Hub L2 is in stock the pad reads **READY** (CLEAR still owns the intermission) and the tray pulses **Hub L2 — Splash next**. Press U and **WEST LIFT** raises the Hub gold then traces **WEST** — CREW UP still names the extra haul, and the third cart pops **OUT** of the yard. An unarmed hottest lane with raiders on it reads **OPEN** and names Kinetic (or Splash west after L2). After the gun is up, that choke reads **SLOW** and names Barrier — planting it drops a **GATE**. An overbuilt pad with no crew reads **IDLE**. When Hub HP cracks below 72 in a raid the pad reads **THIN** and names the BRACE haul. Wave 4 with Hub L2 and Hold still Auto reads **HOLD** and pulses **H Hold — GUNS / CREW** (LAST RAIDS still owns waves 5–6).

## Fun call

Plant the south Farm and the mesa answers — FARM bloom, RAIL snap, first HAUL home. Drop a Mine or Power and that pad *answers* **MINE** / **PWR** — FARM UP still owns the opening. Click a choke and the barrel **SET**s, then **GUNS UP** names it live. Click Hub on the next pad and that rail *strokes* too. RAIL / WEST / GATE / OUT / yank *stroke* a corridor; gun shots stay bolts. Press R and leftover juice *leaves* so the opening can land again. Press D after a win and a new seed-7 watch *starts* — the end card now offers both **Run it back** and **Watch a run**. Press P on a demo and the mesa is yours — **TAKE** bloom, autopilot drops. Press U and the Hub lifts gold, then WEST blooms on the choke and a third cart pops **OUT** of the yard. West Splash slams the bunch. Five ore on a choke drops a **GATE**, not a cut beep. Guns click empty and the mesa goes dark, then they bloom FIRE. Press H and carts yank. Click the orange rail and it lets go. Dump a haul into the chew and the Hub shrugs cyan. Last raids pull the camera in and the dusk goes ember. Hold six waves and the mesa *holds* gold-green — crack the core and it *falls*. That is the watch, not another Hub number. Existing Hub chips stay; that family is frozen. CLEAR still owns the intermission. CREW UP still names the extra haul after L2. SLOW still names the Barrier spend.

## Gaps

- This VM has no Unity Editor — Play Mode is documented, not screenshot-verified.
- Runtime art is procedural low-poly, not authored FBX.
- Origin remote stays `tmp-*`. GitHub is the Hub/Cursor clone.
- GitHub SHA ≠ Origin SHA (parallel history); pass-75 file bytes match.
- Course-correct: no new Hub counter chips. Named leftovers plus ONE LINE, FRESH R, FRESH D, STROKE, END PAIR, LINE OUT, PAD DROP, GUN SET, and TAKE WATCH polish shipped. Kill punch skipped (42 deaths would spam). Freeze still open until ~16:30 for polish if crisp. Extra Farm still quiet (FARM pip stays FIRST LINE).
- Hold order on Auto still lets loaded carts finish the trip (demo / default). GUNS / CREW peel them the same tick; yank juice only fires on those orders.
- Out of scope: multiplayer, cloud meta, deep tech tree, campaign.
