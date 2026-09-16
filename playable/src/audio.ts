const PAL = {
  sky: 0x071820,
  fog: 0x0a2430,
  mesa: 0x6e5846,
  dust: 0x3a3834,
  rock: 0x4a3c32,
  cream: 0xf0d8b0,
  teal: 0x5ef0e4,
  amber: 0xffb143,
  green: 0x7ad66f,
  blue: 0x4eb6ff,
  red: 0xff4d4d,
  rail: 0x8afff4,
  sabo: 0xff6a3c,
  pad: 0xefd4a4,
  hub: 0x1e5a62,
};

export class JuiceAudio {
  private ctx: AudioContext | null = null;
  private muted = false;

  unlock(): void {
    if (this.ctx) return;
    const Ctx = window.AudioContext || (window as unknown as { webkitAudioContext: typeof AudioContext }).webkitAudioContext;
    this.ctx = new Ctx();
  }

  toggleMute(): boolean {
    this.muted = !this.muted;
    return this.muted;
  }

  private tone(freq: number, dur: number, type: OscillatorType, gain = 0.08, at = 0): void {
    if (this.muted) return;
    this.unlock();
    const ctx = this.ctx;
    if (!ctx) return;
    const t = ctx.currentTime + at;
    const osc = ctx.createOscillator();
    const g = ctx.createGain();
    osc.type = type;
    osc.frequency.setValueAtTime(freq, t);
    g.gain.setValueAtTime(gain, t);
    g.gain.exponentialRampToValueAtTime(0.0008, t + dur);
    osc.connect(g);
    g.connect(ctx.destination);
    osc.start(t);
    osc.stop(t + dur + 0.02);
  }

  deposit(resource: string): void {
    const f = resource === 'ore' ? 420 : resource === 'food' ? 520 : 640;
    this.tone(f, 0.12, 'triangle', 0.07);
    this.tone(f * 1.5, 0.16, 'sine', 0.04, 0.04);
  }

  shot(): void {
    this.tone(880, 0.05, 'square', 0.03);
  }

  splash(): void {
    this.tone(180, 0.14, 'sawtooth', 0.05);
  }

  hit(): void {
    this.tone(140, 0.07, 'square', 0.04);
  }

  wave(): void {
    this.tone(90, 0.28, 'sawtooth', 0.09);
    this.tone(180, 0.22, 'square', 0.05, 0.08);
    this.tone(360, 0.18, 'triangle', 0.04, 0.16);
  }

  warn(): void {
    this.tone(240, 0.18, 'square', 0.05);
    this.tone(240, 0.18, 'square', 0.05, 0.22);
  }

  sabotage(): void {
    this.tone(70, 0.22, 'sawtooth', 0.1);
    this.tone(48, 0.28, 'square', 0.06, 0.06);
  }

  brownout(): void {
    this.tone(110, 0.16, 'square', 0.05);
    this.tone(70, 0.22, 'sawtooth', 0.04, 0.08);
  }

  win(): void {
    this.tone(392, 0.18, 'triangle', 0.07);
    this.tone(494, 0.18, 'triangle', 0.07, 0.14);
    this.tone(587, 0.28, 'triangle', 0.08, 0.28);
  }

  lose(): void {
    this.tone(196, 0.3, 'sawtooth', 0.07);
    this.tone(130, 0.45, 'triangle', 0.08, 0.12);
  }

  build(): void {
    this.tone(300, 0.08, 'triangle', 0.04);
  }
}

export { PAL };
