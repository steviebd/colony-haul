import type { Snapshot, Tool } from './sim/types.ts';

const TOOLS: { id: Tool; label: string; hint: string }[] = [
  { id: 'farm', label: 'Farm', hint: '10 ore · mesa pad · food' },
  { id: 'mine', label: 'Mine', hint: '12 ore · mesa pad · ore' },
  { id: 'power', label: 'Power', hint: '13 ore · mesa pad · power' },
  { id: 'route', label: 'Route', hint: '3 ore · pad then Hub' },
  { id: 'kinetic', label: 'Kinetic', hint: '16 ore + 3 pwr · choke pad' },
  { id: 'splash', label: 'Splash', hint: 'Hub L2 · 28 ore + 6 pwr' },
  { id: 'barrier', label: 'Barrier', hint: '5 ore · slows grunts/brutes' },
  { id: 'upgrade', label: 'Hub L2', hint: '64 ore + 30 food + 22 pwr' },
];

export class Hud {
  root: HTMLElement;
  onTool: (tool: Tool) => void = () => {};
  onUpgrade: () => void = () => {};
  onRestart: () => void = () => {};
  onDemo: () => void = () => {};
  onPlay: () => void = () => {};

  constructor(parent: HTMLElement) {
    this.root = document.createElement('div');
    this.root.id = 'hud';
    this.root.innerHTML = `
      <header class="top">
        <div class="brand">
          <div class="mark">CH</div>
          <div>
            <h1>Colony Haul</h1>
            <p>Mesa 7 · dusk cycle</p>
          </div>
        </div>
        <div class="chips">
          <div class="chip ore" id="chip-ore"><span>ORE</span><b>0</b><small>+0/s</small></div>
          <div class="chip food" id="chip-food"><span>FOOD</span><b>0</b><small>+0/s</small></div>
          <div class="chip power" id="chip-power"><span>PWR</span><b>0</b><small>+0/s</small></div>
          <div class="chip crew"><span>STAFF</span><b id="chip-crew">0</b><small id="chip-haul">0 haul</small></div>
        </div>
        <div class="wavebox">
          <div class="wave-label" id="wave-label">Wave 1 / 6</div>
          <div class="wave-bar"><i id="wave-fill"></i></div>
          <div class="hubhp">Hub HP <b id="hub-hp">175</b> · L<span id="hub-lv">1</span></div>
        </div>
      </header>
      <aside class="tray" id="tray"></aside>
      <div class="hint" id="hint"></div>
      <div class="pips" id="pips"></div>
      <div class="banner" id="banner" hidden></div>
      <div class="warn" id="warn" hidden>FOOD STORES EMPTY · colony starving</div>
      <div class="end" id="end" hidden>
        <div class="panel">
          <h2 id="end-title"></h2>
          <p id="end-copy"></p>
          <button type="button" id="end-restart">Run it back</button>
        </div>
      </div>
      <div class="boot" id="boot">
        <div class="panel">
          <p class="kicker">Vertical slice</p>
          <h2>Hold Mesa 7</h2>
          <p>Raise a farm, string mag-rails, and keep the Hub alive through six raider waves. Win by surviving those waves with Hub Level 2 online. Starve or lose the Hub and the slice is over.</p>
          <div class="boot-actions">
            <button type="button" id="boot-play">Take the watch</button>
            <button type="button" class="ghost" id="boot-demo">Watch a run</button>
          </div>
        </div>
      </div>
    `;
    parent.appendChild(this.root);
    const tray = this.root.querySelector('#tray')!;
    for (const tool of TOOLS) {
      const btn = document.createElement('button');
      btn.type = 'button';
      btn.dataset.tool = tool.id;
      btn.innerHTML = `<b>${tool.label}</b><small>${tool.hint}</small>`;
      btn.addEventListener('click', () => {
        if (tool.id === 'upgrade') this.onUpgrade();
        else this.onTool(tool.id);
      });
      tray.appendChild(btn);
    }
    this.root.querySelector('#end-restart')!.addEventListener('click', () => this.onRestart());
    this.root.querySelector('#boot-play')!.addEventListener('click', () => {
      this.hideBoot();
      this.onPlay();
    });
    this.root.querySelector('#boot-demo')!.addEventListener('click', () => {
      this.hideBoot();
      this.onDemo();
    });
  }

  hideBoot(): void {
    this.root.querySelector('#boot')?.setAttribute('hidden', '');
  }

  hideEnd(): void {
    const end = this.root.querySelector('#end') as HTMLElement;
    end.hidden = true;
  }

  ping(text: string, kind: 'ore' | 'food' | 'power' | 'hit' | 'cut' = 'ore'): void {
    const wrap = this.root.querySelector('#pips') as HTMLElement;
    const el = document.createElement('div');
    el.className = `pip ${kind}`;
    el.textContent = text;
    wrap.appendChild(el);
    window.setTimeout(() => el.remove(), 900);
  }

  flashBanner(text: string, ms = 2200): void {
    const el = this.root.querySelector('#banner') as HTMLElement;
    el.hidden = false;
    el.textContent = text;
    window.setTimeout(() => {
      el.hidden = true;
    }, ms);
  }

  sync(s: Snapshot): void {
    setChip(this.root, 'ore', s.stock.ore, s.rates.ore);
    setChip(this.root, 'food', s.stock.food, s.rates.food);
    setChip(this.root, 'power', s.stock.power, s.rates.power);
    const staffed = s.workersTotal - s.workersFree;
    (this.root.querySelector('#chip-crew') as HTMLElement).textContent = `${staffed}/${s.workersTotal}`;
    (this.root.querySelector('#chip-haul') as HTMLElement).textContent = `${s.haulerCount} haul`;
    const upcoming = s.waveIndex < s.wavesToWin;
    (this.root.querySelector('#wave-label') as HTMLElement).textContent = s.waveLive > 0
      ? s.waveThreat
      : upcoming
        ? `Wave ${s.waveIndex + 1} / ${s.wavesToWin} in ${Math.ceil(s.nextWaveIn)}s`
        : `Final hold · ${s.waveLive} raiders`;
    const fill = this.root.querySelector('#wave-fill') as HTMLElement;
    fill.style.width = upcoming ? `${Math.max(8, 100 - (s.nextWaveIn / 36) * 100)}%` : '100%';
    (this.root.querySelector('#hub-hp') as HTMLElement).textContent = String(Math.ceil(s.hubHp));
    this.root.querySelector('.hubhp')!.classList.toggle('hurt', s.hubHp < s.hubMaxHp * 0.92);
    this.root.querySelector('.wavebox')!.classList.toggle('live', s.waveLive > 0);
    (this.root.querySelector('#hub-lv') as HTMLElement).textContent = String(s.hubLevel);
    (this.root.querySelector('#hint') as HTMLElement).textContent = s.hint;
    const warn = this.root.querySelector('#warn') as HTMLElement;
    warn.hidden = !s.foodWarned;
    this.root.querySelector('#chip-power')!.classList.toggle('critical', s.powerBrownout || s.stock.power < 6);
    this.root.querySelector('#chip-food')!.classList.toggle('critical', s.stock.food < 8);
    this.root.querySelector('#chip-ore')!.classList.toggle('cut', s.haulCut);
    for (const btn of this.root.querySelectorAll<HTMLButtonElement>('.tray button')) {
      const id = btn.dataset.tool;
      btn.classList.toggle('on', id === s.selectedTool);
      btn.setAttribute('aria-pressed', id === s.selectedTool ? 'true' : 'false');
      if (id === 'splash') {
        const locked = s.hubLevel < 2;
        btn.disabled = locked;
        btn.classList.toggle('locked', locked);
        const small = btn.querySelector('small');
        if (small) small.textContent = locked ? 'Locked · needs Hub Level 2' : '28 ore + 6 pwr · choke';
        btn.title = locked ? 'Splash stays locked until Hub Level 2 comes online.' : 'Area slow + damage on choke pads';
      }
    }
    if (s.phase !== 'playing') {
      const end = this.root.querySelector('#end') as HTMLElement;
      end.hidden = false;
      const title = this.root.querySelector('#end-title')!;
      const copy = this.root.querySelector('#end-copy')!;
      if (s.phase === 'won') {
        title.textContent = 'Mesa holds';
        copy.textContent = `Hub Level ${s.hubLevel} stood through ${s.wavesToWin} waves. Mag-rails still humming.`;
      } else if (s.phase === 'lost_hub') {
        title.textContent = 'Hub down';
        copy.textContent = 'Raiders cracked the core. The rails go dark.';
      } else {
        title.textContent = 'Starved out';
        copy.textContent = 'Food hit zero too long. The crew walked off the mesa.';
      }
    } else {
      const end = this.root.querySelector('#end') as HTMLElement;
      end.hidden = true;
    }
  }
}

function setChip(root: HTMLElement, id: string, value: number, rate: number): void {
  const chip = root.querySelector(`#chip-${id}`)!;
  chip.querySelector('b')!.textContent = String(Math.floor(value));
  chip.querySelector('small')!.textContent = `${rate >= 0 ? '+' : ''}${rate.toFixed(1)}/s`;
}
