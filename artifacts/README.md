**Unity Editor Play Mode is not verified.** See `FM-GAME-003.md`: Linux Editor **2022.3.50f1** installed and launched; Play Mode **FAIL** on `No valid Unity Editor license found`. Screenshots: `fm-game-003-license-error.png`, `fm-game-003-hub-signin.png`. No Editor gameplay video.

Vite client below is the spare harness only (FM-GAME-002). Do not treat it as Unity-verified.

Gameplay capture from the Vite client (`?demo=1&play=1`):

- `colony-haul-run.webm` — **Pass 3** stepped capture: **56s VP9 1280×720, 30 fps**, sim rate 2. Opening Farm→rail, Wave 1 teal bolts, HAUL CUT with **▶ Route** held, Hub L2 during the cut, west **Barrier up** banner, **real brownout event** (PWR 0, Wave 3 live), Splash, mag-rail repair.
- Hosted (72h temp): https://litter.catbox.moe/aik80e.webm
- Nested UI stills: `mesa-open.png`, `route-fix.png`, `haul-deposit.png`.
- `demo-haul-cut.png` — HAUL CUT, ▶ Route, orange rail.
- `demo-barrier.png` — BARRIER UP banner, thick red spawn beams.
- `demo-brownout.png` — PWR 0, Wave 3 brute on the mesa, towers-dry copy.
- `demo-hub-l2.png` / `run-mid.png` — Hub L2 + Splash.

Kernel: `cd playable && npm run playtest` (asserts `brownouts >= 1` on seed 7 demo).
UI nested: `node tools/nested-playtest.mjs` against http://127.0.0.1:43147/?play=1
Headless seed 7: win ~184.5s, Hub L2, 1 brownout event, min power 0.
