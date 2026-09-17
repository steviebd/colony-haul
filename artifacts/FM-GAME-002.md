# FM-GAME-002 — Firstmate package (17:00 AEST Thu 17 Sep 2026)

**FEATURE FREEZE = Yes** (early lock, Firstmate call). No more juice, Hub chips, or kill punch.

GitHub (cold-open / Hub / Cursor): **https://github.com/steviebd/colony-haul**

| Field | Value |
| --- | --- |
| Hub-ready | **Yes** |
| Origin Unity | `98f97efedfda3d478c05b86f0e7732fd0bf9e333` |
| Origin HEAD | `273d00bda059acbfc361c79cc0a5ea4e7642a7d3` |
| GitHub Unity | `10bcd1688cb1e2eeead6a443be0e9728438c6ea6` |
| GitHub HEAD | `23009f72051fe9ae00ac4ff14b71ff2a43f7f063` |
| Kernel | seed-7 playtest **win** (`phase=won`, t≈184.45, Hub L2, minHp≈50.4, sabotages=4, brownouts=1, deposits=115, kills=42, minFood≈32.84) |
| Nested opening | **pass** (Farm → Route → Hub still deposits) |
| Hold order | **pass** (locked until wave 4 + L2; then GUNS chase Power) |
| Product | Unity 2022.3.50f1 URP under `Assets/_ColonyHaul` — not Vite |
| Feature freeze | **Yes** |

Cold clone has `Assets/`, `Packages/`, `ProjectSettings/`, USB at `ThirdParty/unity-semantic-bridge` (`41e8bca`), `playable/`, `artifacts/`. PUT DOWN C# (Bootstrap / SliceHud / SliceJuice) byte-matches GitHub Unity tip. Parallel history: GitHub SHA ≠ Origin SHA; edited-path bytes match.

## Pitch

Colony Haul is a dusk-mesa logistics tower defense. You hold **Mesa 7** through **six raider waves** with **Hub Level 2** online. Haulers only move on live mag-rail. Runners cut lines. Guns need Power. The larder needs Food. A combat deposit **BRACEs** the Hub. The captain bar is how that watch *feels* — not another Hub number.

## How to play (Editor)

```bash
git clone https://github.com/steviebd/colony-haul.git
```

Unity Hub → **Open** that folder (the one with `Assets/`, `Packages/`, `ProjectSettings/`) → Unity **2022.3.50f1** (or Unity 6 ≤ 6.3) with URP → `Assets/_ColonyHaul/Scenes/Boot.unity` or `Game_VerticalSlice.unity` → Play.

**The watch:** Farm south → rail Hub → haul. Plant pads, gun the chokes, splice orange rails, raise Hub L2, Splash west. Hold six waves with the Hub still up. Right-click *puts a tool down*.

- **Colony Haul → Play Vertical Slice** — you take the watch (Farm first).
- **Colony Haul → Play Demo (autopilot)** — seed-7 demo, Hold stays Auto. `P` takes the live mesa.
- **Colony Haul → Run Headless Sim** — expect `win=True` on seed 7.

**Keys:** `1–7` Farm / Mine / Power / Route / Kinetic / Splash / Barrier. `U` Hub L2. **`H` Hold** (unlocks wave 4 + Hub L2: Auto → GUNS → CREW). `D` fresh seed-7 demo. `P` take watch. `R` restart (clears leftover juice). RMB put down (not on the tray). Demo Auto ignores RMB.

**Win:** survive 6 waves **and** Hub L2. **Lose:** Hub HP 0, or food stays at 0 for 14s.

**Hold in a raid:** coach says `H for GUNS` or `H for CREW`. Press H — loaded carts stash unmatched cargo and peel to Power or Food; matching cargo still BRACEs the Hub. Press again to flip. Press once more to Auto — carts *let* go (**LET**). Splice still beats this. Demo stays Auto so seed-7 never fires LET.

## Unity feature list (gameplay that matters)

One Game tick. Demo Hold **Auto**, so seed-7 is bit-identical.

**Kernel**
- Opening coach: Farm south → mag-rail to Hub → first hauls. Mid-watch coach names splice, Power, Hub L2, Splash west, CORE THIN, second farm, HOLD.
- Mag-rail logistics. Unrouted pads read OFFLINE and ghost home. Runners cut; Route is armed for splice.
- **Rail Surge / BRACE** — a combat deposit cuts Hub hit damage. Cyan shield; hits **SHRUG**.
- Guns (Kinetic, Splash after L2), Barriers, cargo FOOD / PWR / ORE, gun LOCK beams.
- Hub L2 spend (`U`): gold raise, Splash west unlock, extra hauler from the yard.
- **Hold order** (`H`, wave 4 + L2) + HOLD PEEL restash. No soft-lock.
- Win / lose: MESA HOLDS / HUB DOWN / STARVED OUT.

**Course-correct (Pass 56+):** stop Hub counter chips. Existing RAID / CUT / GUN / LOAD / OFF / SIT / OPEN / BAR / YARD / FUSE / LARDER / PWR / ORE / STAFF / HP / WAVE CLOCK / ROLL / STUCK / SLOW / DOWN / RUN / IN stay; that family is **frozen**. HOME n aborted. Captain bar is exquisite juice, not more labels.

**Juice / feel (Pass 56–82, frozen)**
- BRACE SHRUG, SPLICE RELEASE, HOLD YANK, DRY→BACK
- FIRST LINE, WEST LIFT, HOLD / FALL, DUSK CLOSE
- SPLASH SLAM, SLOW GATE, CREW OUT, ONE LINE, STROKE, LINE OUT
- FRESH R / FRESH D, END PAIR, TAKE WATCH
- PAD DROP, GUN SET, SOW
- HOLD LET, NOPE, STAY, FREE, STOW, PUT DOWN
- Pass 83 BRIEF is this captain tape (no new feel).

Kill punch skipped (42 deaths would spam).

## Verification (freeze playtest)

```
nested: pass
openingGate: pass
holdOrder: pass
stress: all pass (lost_hub, starve, costs, cut, GUNS peel, matching Power, CREW peel)
headless: phase=won t≈184.45 waveIndex=6 hubLevel=2 win=true
          minHp≈50.4 sabotages=4 brownouts=1 deposits=115 kills=42 minFood≈32.84
```

## Fun call

Plant the south Farm and the mesa answers — FARM, RAIL, first HAUL home. Later pads answer MINE / PWR / SOW. A choke SETs, then GUNS UP. A dead click refuses. Mash 2–7 too early and it holds you. Click the armed Route pad again and it frees. Same key stows. Right-click puts it down. Press U and the Hub lifts gold, WEST blooms, a third cart pops OUT, west Splash SLAMS. Five ore drops a GATE. Guns click empty then bloom FIRE. Press H and carts yank; flip Auto and they let go. Click the orange rail and it lets go. Dump a haul into the chew and the Hub SHRUGS cyan. Last raids pull the camera in. Hold six waves and the mesa holds gold-green — crack the core and it falls. That is the watch, not another Hub number.

## Gaps

- This VM has no Unity Editor — Play Mode is documented, not screenshot-verified.
- Runtime art is procedural low-poly, not authored FBX.
- Origin remote stays `tmp-*`. GitHub is the Hub/Cursor clone.
- GitHub SHA ≠ Origin SHA (parallel history); product file bytes match on edited paths.
- Hub-chip era (Pass 12–55) exists on the mesa; captain bar is juice 56–82. No more of either.
- First-click RouteFrom stays quiet (demo would spam). Restart pip/burst foreach is a no-op.
- Hold Auto still lets loaded carts finish the trip (demo / default).
- Out of scope: multiplayer, cloud meta, deep tech tree, campaign.
