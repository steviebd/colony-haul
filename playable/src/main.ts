import { JuiceAudio } from './audio.ts';
import { DemoPilot } from './sim/demo.ts';
import { Game } from './sim/game.ts';
import type { JuiceEvent, Tool } from './sim/types.ts';
import { Hud } from './hud.ts';
import { World } from './world.ts';
import './style.css';

const params = new URLSearchParams(window.location.search);
const demoOn = params.get('demo') === '1';
const rate = Number(params.get('rate') ?? '1') || 1;
const skipBoot = demoOn || params.get('play') === '1';
const record = params.get('record') === '1';

const app = document.querySelector<HTMLDivElement>('#app')!;
app.innerHTML = '<canvas id="view"></canvas>';
const canvas = app.querySelector<HTMLCanvasElement>('#view')!;

const world = new World(canvas);
const hud = new Hud(document.body);
const audio = new JuiceAudio();
let game = new Game(7);
let demo: DemoPilot | null = demoOn ? new DemoPilot() : null;
let acc = 0;
const step = 1 / 20;
let last = performance.now();
let started = skipBoot;
const recorder = record ? createRecorder(canvas) : null;

if (skipBoot) hud.hideBoot();

hud.onTool = (tool: Tool) => {
  audio.unlock();
  started = true;
  hud.hideBoot();
  game.setTool(tool);
};
hud.onUpgrade = () => {
  audio.unlock();
  started = true;
  hud.hideBoot();
  game.tryUpgrade();
};
hud.onRestart = () => restart(demo !== null);
hud.onDemo = () => restart(true);
hud.onPlay = () => {
  audio.unlock();
  started = true;
  hud.hideBoot();
};

window.addEventListener('resize', () => world.resize());
window.addEventListener('pointermove', (ev) => {
  world.pick(ev.clientX, ev.clientY);
});
window.addEventListener('pointerdown', (ev) => {
  if ((ev.target as HTMLElement).closest('#hud')) return;
  audio.unlock();
  started = true;
  hud.hideBoot();
  if (ev.button === 2) {
    game.cancelSelection();
    return;
  }
  const id = world.pick(ev.clientX, ev.clientY);
  if (!id) {
    game.cancelSelection();
    return;
  }
  const res = game.clickNode(id);
  if (!res.ok && res.why) game.hint = res.why;
});
window.addEventListener('contextmenu', (ev) => ev.preventDefault());
window.addEventListener('keydown', (ev) => {
  const map: Record<string, Tool> = {
    Digit1: 'farm',
    Digit2: 'mine',
    Digit3: 'power',
    Digit4: 'route',
    Digit5: 'kinetic',
    Digit6: 'splash',
    Digit7: 'barrier',
    KeyU: 'upgrade',
  };
  const tool = map[ev.code];
  if (tool === 'upgrade') game.tryUpgrade();
  else if (tool) game.setTool(tool);
  if (ev.code === 'Escape') game.cancelSelection();
  if (ev.code === 'KeyH') game.cycleHold();
  if (ev.code === 'KeyD') restart(true);
  if (ev.code === 'KeyM') audio.toggleMute();
});

world.bindNodes(game.snapshot());
hud.sync(game.snapshot());
recorder?.start();

let loopPaused = false;

declare global {
  interface Window {
    __colony: {
      game: Game;
      world: World;
      pauseLoop: () => void;
      resumeLoop: () => void;
      stepFrame: (dt: number) => {
        t: number;
        phase: string;
        wave: number;
        live: number;
        haulCut: boolean;
        hubLevel: number;
        brownout: boolean;
        tool: string;
        splashLocked: boolean;
        barrier: boolean;
      };
    };
  }
}

function paint(): {
  t: number;
  phase: string;
  wave: number;
  live: number;
  haulCut: boolean;
  hubLevel: number;
  brownout: boolean;
  tool: string;
  splashLocked: boolean;
  barrier: boolean;
} {
  const snap = game.snapshot();
  const events = game.drainEvents();
  juice(events);
  if (events.some((ev) => ev.kind === 'wave')) hud.flashBanner(snap.waveThreat, 2800);
  if (events.some((ev) => ev.kind === 'hit' && ev.nodeId === 'hub')) hud.ping('HUB HIT', 'hit');
  world.sync(snap, events);
  world.render();
  hud.sync(snap);
  return {
    t: game.t,
    phase: game.phase,
    wave: game.waveIndex,
    live: game.enemies.length,
    haulCut: snap.haulCut,
    hubLevel: game.hubLevel,
    brownout: snap.powerBrownout,
    tool: snap.selectedTool,
    splashLocked: snap.hubLevel < 2,
    barrier: snap.edges.some((e) => e.barrier),
  };
}

window.__colony = {
  game,
  world,
  pauseLoop: () => {
    loopPaused = true;
  },
  resumeLoop: () => {
    loopPaused = false;
    last = performance.now();
  },
  stepFrame: (dt: number) => {
    if (started && game.phase === 'playing') {
      acc += dt;
      while (acc >= step) {
        demo?.step(game, step);
        game.tick(step);
        acc -= step;
      }
    }
    window.__colony.game = game;
    return paint();
  },
};

function restart(useDemo: boolean): void {
  game = new Game(7);
  demo = useDemo ? new DemoPilot() : null;
  started = true;
  acc = 0;
  last = performance.now();
  hud.hideBoot();
  hud.hideEnd();
  world.reset();
  world.bindNodes(game.snapshot());
  hud.sync(game.snapshot());
  window.__colony.game = game;
}

function juice(events: JuiceEvent[]): void {
  for (const ev of events) {
    switch (ev.kind) {
      case 'deposit':
        audio.deposit(ev.resource ?? 'ore');
        hud.ping(`+${ev.amount ?? 0} ${(ev.resource ?? 'ore').toUpperCase()}`, ev.resource ?? 'ore');
        break;
      case 'shot':
        audio.shot();
        break;
      case 'splash':
        audio.splash();
        break;
      case 'hit':
        audio.hit();
        break;
      case 'wave':
        audio.wave();
        break;
      case 'warn_food':
        audio.warn();
        hud.flashBanner('Food stores empty', 1800);
        break;
      case 'brownout':
        audio.brownout();
        hud.flashBanner('Brownout — towers dry', 2200);
        break;
      case 'sabotage':
        audio.sabotage();
        hud.flashBanner('HAUL CUT — splice the orange rail', 1600);
        break;
      case 'win':
        audio.win();
        recorder?.stop();
        break;
      case 'lose':
        audio.lose();
        recorder?.stop();
        break;
      case 'build':
      case 'upgrade':
        audio.build();
        break;
      case 'route':
        audio.build();
        if (ev.reason === 'splice') hud.flashBanner('RAIL LIVE — haulers rolling', 2000);
        break;
      case 'barrier':
        audio.build();
        hud.flashBanner('Barrier up — spawn approach slowed', 1600);
        break;
      case 'surge':
        hud.flashBanner('BRACE — haul bought the Hub a breath', 1400);
        break;
      case 'hold':
        if (ev.reason === 'power') hud.flashBanner('GUNS ORDER — haulers rush Power', 1800);
        else if (ev.reason === 'food') hud.flashBanner('CREW ORDER — haulers rush Food', 1800);
        else hud.flashBanner('Hold auto — hungriest stock', 1400);
        break;
      case 'death':
        break;
      default: {
        const _exhaustive: never = ev.kind;
        void _exhaustive;
      }
    }
  }
}

function frame(now: number): void {
  if (!loopPaused) {
    const dt = Math.min(0.25, (now - last) / 1000) * rate;
    last = now;
    if (started && game.phase === 'playing') {
      acc += dt;
      while (acc >= step) {
        demo?.step(game, step);
        game.tick(step);
        acc -= step;
      }
    }
    paint();
  }
  requestAnimationFrame(frame);
}

requestAnimationFrame(frame);

function createRecorder(target: HTMLCanvasElement): { start: () => void; stop: () => void } {
  const stream = target.captureStream(30);
  const mime = MediaRecorder.isTypeSupported('video/webm;codecs=vp9')
    ? 'video/webm;codecs=vp9'
    : 'video/webm';
  const rec = new MediaRecorder(stream, { mimeType: mime, videoBitsPerSecond: 4_000_000 });
  const chunks: Blob[] = [];
  rec.ondataavailable = (ev) => {
    if (ev.data.size) chunks.push(ev.data);
  };
  rec.onstop = () => {
    const blob = new Blob(chunks, { type: mime });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'colony-haul-run.webm';
    a.click();
    (window as unknown as { __colonyHaulVideo?: Blob }).__colonyHaulVideo = blob;
  };
  return {
    start: () => rec.start(1000),
    stop: () => {
      if (rec.state !== 'inactive') rec.stop();
    },
  };
}
