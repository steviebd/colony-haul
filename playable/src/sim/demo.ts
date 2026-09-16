import { BALANCE } from './balance.ts';
import { Game } from './game.ts';
import type { SimEdge, Tool } from './types.ts';

const PADS = ['pad_s', 'pad_n', 'pad_se', 'pad_sw', 'pad_nw', 'pad_ne'];

export class DemoPilot {
  cooldown = 0.15;
  cutHold = 0;
  seenCutId: string | null = null;

  step(game: Game, dt: number): void {
    if (game.phase !== 'playing') return;
    this.cooldown -= dt;
    this.cutHold = Math.max(0, this.cutHold - dt);
    if (this.cooldown > 0) return;
    if (this.act(game) && this.cooldown <= 0) this.cooldown = 0.42;
  }

  private act(game: Game): boolean {
    if (this.repairCut(game)) return true;
    if (this.ensureProducer(game, 'farm', 'pad_s')) return true;
    if (this.routeHome(game, 'pad_s')) return true;
    if (this.ensureProducer(game, 'mine', 'pad_n')) return true;
    if (this.routeHome(game, 'pad_n')) return true;
    if (this.ensureProducer(game, 'power', 'pad_se')) return true;
    if (this.routeHome(game, 'pad_se')) return true;
    if (this.ensureTower(game, 'kinetic', 'choke_e')) return true;
    if (this.ensureBarrier(game, 'choke_e')) return true;
    if (this.ensureTower(game, 'kinetic', 'choke_n')) return true;
    if (this.ensureBarrier(game, 'choke_n')) return true;
    if ([...game.buildings.values()].some((b) => b.type === 'kinetic')) {
      if (this.ensureProducer(game, 'farm', 'pad_sw')) return true;
      if (this.routeHome(game, 'pad_sw')) return true;
    }
    if (game.hubLevel < 2 && this.canUpgrade(game) && game.t > 90) {
      this.arm(game, 'upgrade');
      return game.tryUpgrade().ok;
    }
    if (game.hubLevel >= 2) {
      if (this.ensureTower(game, 'splash', 'choke_w')) return true;
      if (this.ensureBarrier(game, 'choke_w')) return true;
      if (this.ensureProducer(game, 'power', 'pad_ne')) return true;
      if (this.routeHome(game, 'pad_ne')) return true;
      if (this.ensureTower(game, 'splash', 'tower_ne')) return true;
    }
    if (this.ensureProducer(game, 'mine', 'pad_nw')) return true;
    if (this.routeHome(game, 'pad_nw')) return true;
    if (this.ensureTower(game, 'kinetic', 'tower_sw')) return true;
    return false;
  }

  private arm(game: Game, tool: Tool): void {
    if (game.selectedTool !== tool) game.setTool(tool);
  }

  private canUpgrade(game: Game): boolean {
    const c = BALANCE.hubLevel2;
    return game.stock.ore >= c.ore && game.stock.food >= c.food && game.stock.power >= c.power;
  }

  private ensureProducer(game: Game, type: 'mine' | 'farm' | 'power', prefer: string): boolean {
    if ([...game.buildings.values()].some((b) => b.type === type && b.nodeId === prefer)) return false;
    if ([...game.buildings.values()].filter((b) => b.type === type).length >= (type === 'farm' ? 2 : type === 'mine' ? 2 : 2)) {
      return false;
    }
    const node = game.buildings.has(prefer) ? PADS.find((p) => !game.buildings.has(p)) : prefer;
    if (!node) return false;
    this.arm(game, type);
    return game.clickNode(node).ok;
  }

  private routeHome(game: Game, from: string): boolean {
    if (!game.buildings.has(from)) return false;
    if (game.path(from, 'hub', { routedOnly: true })) return false;
    const path = game.path(from, 'hub', { routedOnly: false, ignoreBarriers: true });
    if (!path || path.length < 2) return false;
    for (let i = 0; i < path.length - 1; i += 1) {
      const edge = game.edgeBetween(path[i], path[i + 1]);
      if (edge && !edge.routed) {
        game.setTool('route');
        if (game.routeFrom !== path[i]) game.clickNode(path[i]);
        return game.clickNode(path[i + 1]).ok;
      }
    }
    return false;
  }

  private repairCut(game: Game): boolean {
    let cut: SimEdge | null = null;
    for (const edge of game.edges.values()) {
      if (!edge.routed || edge.sabotagedUntil <= game.t) continue;
      cut = edge;
      break;
    }
    if (!cut) {
      this.seenCutId = null;
      this.cutHold = 0;
      return false;
    }
    game.setTool('route');
    if (game.routeFrom !== cut.a) game.clickNode(cut.a);
    if (this.seenCutId !== cut.id) {
      this.seenCutId = cut.id;
      this.cutHold = this.powerCut(game, cut) ? 5.6 : 2.2;
      this.cooldown = 0.08;
      return true;
    }
    if (this.cutHold > 0) {
      if (this.powerCut(game, cut) && game.hubLevel < 2 && this.canUpgrade(game)) {
        this.arm(game, 'upgrade');
        game.tryUpgrade();
        this.arm(game, 'route');
        if (game.routeFrom !== cut.a) game.clickNode(cut.a);
      }
      this.cooldown = 0.08;
      return true;
    }
    const spliced = game.clickNode(cut.b).ok;
    if (spliced) {
      this.seenCutId = null;
      this.cooldown = 2.8;
    }
    return spliced;
  }

  private powerCut(game: Game, edge: SimEdge): boolean {
    for (const id of [edge.a, edge.b]) {
      if (game.buildingAt(id)?.type === 'power') return true;
    }
    return false;
  }

  private ensureTower(game: Game, type: 'kinetic' | 'splash', nodeId: string): boolean {
    const occ = game.buildingAt(nodeId);
    if (occ) return false;
    if (type === 'splash' && game.hubLevel < 2) return false;
    this.arm(game, type);
    return game.clickNode(nodeId).ok;
  }

  private ensureBarrier(game: Game, choke: string): boolean {
    const inbound = game.neighbors(choke).filter((e) => {
      const other = e.a === choke ? e.b : e.a;
      return game.nodes.get(other)?.kind === 'spawn';
    });
    if (inbound.every((e) => e.barrier)) return false;
    this.arm(game, 'barrier');
    return game.clickNode(choke).ok;
  }
}

export function runHeadless(opts?: { seed?: number; seconds?: number; demo?: boolean }): {
  phase: string;
  t: number;
  waveIndex: number;
  hubLevel: number;
  deposits: number;
  buildings: number;
  routes: number;
  kills: number;
  win: boolean;
  minHp: number;
  hubHits: number;
  sabotages: number;
  brownouts: number;
  minPower: number;
  minFood: number;
} {
  const game = new Game(opts?.seed ?? 7);
  const demo = opts?.demo === false ? null : new DemoPilot();
  const limit = opts?.seconds ?? 420;
  let deposits = 0;
  let kills = 0;
  let hubHits = 0;
  let sabotages = 0;
  let brownouts = 0;
  let minHp = game.hubHp;
  let minPower = game.stock.power;
  let minFood = game.stock.food;
  const step = 1 / 20;
  for (let t = 0; t < limit && game.phase === 'playing'; t += step) {
    demo?.step(game, step);
    game.tick(step);
    minHp = Math.min(minHp, game.hubHp);
    minPower = Math.min(minPower, game.stock.power);
    minFood = Math.min(minFood, game.stock.food);
    for (const ev of game.drainEvents()) {
      if (ev.kind === 'deposit') deposits += 1;
      if (ev.kind === 'death') kills += 1;
      if (ev.kind === 'hit' && ev.nodeId === 'hub') hubHits += 1;
      if (ev.kind === 'sabotage') sabotages += 1;
      if (ev.kind === 'brownout') brownouts += 1;
    }
  }
  const routes = [...game.edges.values()].filter((e) => e.routed).length;
  return {
    phase: game.phase,
    t: game.t,
    waveIndex: game.waveIndex,
    hubLevel: game.hubLevel,
    deposits,
    buildings: game.buildings.size,
    routes,
    kills,
    win: game.phase === 'won',
    minHp,
    hubHits,
    sabotages,
    brownouts,
    minPower,
    minFood,
  };
}
