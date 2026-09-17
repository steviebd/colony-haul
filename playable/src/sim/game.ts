import { BALANCE, resourceOf } from './balance.ts';
import { createMesa, otherEnd } from './map.ts';
import type {
  Building,
  BuildingType,
  Enemy,
  EnemyType,
  GameConfig,
  Hauler,
  HoldOrder,
  JuiceEvent,
  Phase,
  Rates,
  Resource,
  SimEdge,
  SimNode,
  Snapshot,
  Stockpile,
  Tool,
} from './types.ts';

let seq = 1;
function nid(prefix: string): string {
  seq += 1;
  return `${prefix}_${seq}`;
}

function dist(ax: number, az: number, bx: number, bz: number): number {
  const dx = ax - bx;
  const dz = az - bz;
  return Math.hypot(dx, dz);
}

function cloneStock(s: Stockpile): Stockpile {
  return { ore: s.ore, food: s.food, power: s.power };
}

type PendingSpawn = { at: number; type: EnemyType; spawn: string };

export class Game {
  t = 0;
  phase: Phase = 'playing';
  stock: Stockpile;
  rates: Rates = { ore: 0, food: 0, power: 0 };
  deliveredWindow: { t: number; kind: Resource; amount: number }[] = [];
  hubHp: number;
  hubLevel = 1;
  hubUpgradeLeft = 0;
  workersTotal: number;
  haulers: Hauler[] = [];
  starveTimer = 0;
  waveIndex = 0;
  nextWaveIn: number;
  pending: PendingSpawn[] = [];
  nodes: Map<string, SimNode>;
  edges: Map<string, SimEdge>;
  buildings: Map<string, Building> = new Map();
  enemies: Enemy[] = [];
  events: JuiceEvent[] = [];
  selectedTool: Tool = 'none';
  holdOrder: HoldOrder = 'auto';
  routeFrom: string | null = null;
  hint = 'Food is already draining. Place a Farm on a mesa pad, then click the Hub.';
  foodWarned = false;
  brownoutAt = -99;
  hubGunCd = 0;
  surgeUntil = 0;
  config: GameConfig;
  rng: () => number;

  constructor(seed = 1) {
    this.config = {
      wavesToWin: BALANCE.wavesToWin,
      hubMaxHp: BALANCE.hubMaxHp,
      starveFail: BALANCE.starveFail,
      start: BALANCE.start,
    };
    this.stock = cloneStock(BALANCE.start);
    this.hubHp = BALANCE.hubMaxHp;
    this.workersTotal = BALANCE.startWorkers;
    this.nextWaveIn = BALANCE.waves[0].after;
    this.rng = mulberry32(seed);
    const mesa = createMesa();
    this.nodes = new Map(mesa.nodes.map((n) => [n.id, n]));
    this.edges = new Map(mesa.edges.map((e) => [e.id, e]));
    this.placeFixed('hub', 'hub');
    this.placeFixed('depot', 'depot');
    const depot = this.nodes.get('depot')!;
    for (let i = 0; i < BALANCE.startHaulers; i += 1) {
      this.haulers.push({
        id: nid('haul'),
        x: depot.x + (i - 0.5) * 0.45,
        z: depot.z,
        nodeId: 'depot',
        cargo: null,
        path: [],
        wait: 0,
        busyAt: null,
      });
    }
  }

  emit(partial: Omit<JuiceEvent, 't'>): void {
    this.events.push({ ...partial, t: this.t });
  }

  drainEvents(): JuiceEvent[] {
    const out = this.events;
    this.events = [];
    return out;
  }

  private placeFixed(type: BuildingType, nodeId: string): void {
    const b: Building = {
      id: nid(type),
      type,
      nodeId,
      buildLeft: 0,
      staffed: true,
      buffer: { ore: 0, food: 0, power: 0 },
      cooldown: 0,
    };
    this.buildings.set(nodeId, b);
  }

  buildingAt(nodeId: string): Building | undefined {
    return this.buildings.get(nodeId);
  }

  edgeBetween(a: string, b: string): SimEdge | undefined {
    const id = a < b ? `${a}~${b}` : `${b}~${a}`;
    return this.edges.get(id);
  }

  neighbors(nodeId: string): SimEdge[] {
    const out: SimEdge[] = [];
    for (const edge of this.edges.values()) {
      if (edge.a === nodeId || edge.b === nodeId) out.push(edge);
    }
    return out;
  }

  openingGate(): 'farm' | 'route' | null {
    if (this.t > BALANCE.openingLock) return null;
    const farm = [...this.buildings.values()].find((b) => b.type === 'farm');
    if (!farm) return 'farm';
    if (!this.path(farm.nodeId, 'hub', { routedOnly: true })) return 'route';
    return null;
  }

  setTool(tool: Tool): void {
    const gate = this.openingGate();
    if (gate === 'farm' && tool !== 'farm' && tool !== 'none') {
      this.selectedTool = 'farm';
      this.hint = 'Farm first — the crew is already chewing stores.';
      return;
    }
    if (gate === 'route' && tool !== 'route' && tool !== 'none') {
      this.selectedTool = 'route';
      if (!this.routeFrom) this.routeFrom = this.unroutedProducer();
      this.hint = 'Click the Hub to finish the mag-rail from the Farm.';
      return;
    }
    if (tool === this.selectedTool && tool !== 'route' && !gate) {
      this.selectedTool = 'none';
      this.routeFrom = null;
      this.refreshHint();
      return;
    }
    this.selectedTool = tool;
    if (tool !== 'route') this.routeFrom = null;
    this.refreshHint();
  }

  get holdReady(): boolean {
    return this.hubLevel >= 2 && this.waveIndex >= 4;
  }

  cycleHold(): { ok: boolean; why?: string } {
    if (this.phase !== 'playing') return { ok: false, why: 'match over' };
    if (!this.holdReady) {
      this.hint = 'Hold orders unlock on wave 4 with Hub L2';
      return { ok: false, why: this.hint };
    }
    switch (this.holdOrder) {
      case 'auto':
        this.holdOrder = 'power';
        break;
      case 'power':
        this.holdOrder = 'food';
        break;
      case 'food':
        this.holdOrder = 'auto';
        break;
      default: {
        const _exhaustive: never = this.holdOrder;
        return { ok: false, why: String(_exhaustive) };
      }
    }
    for (const h of this.haulers) {
      if (this.holdOrder === 'auto') {
        if (h.wait > 0 || h.cargo) continue;
        h.path = [];
        this.planHauler(h);
        continue;
      }
      let want: Resource;
      switch (this.holdOrder) {
        case 'power':
          want = 'power';
          break;
        case 'food':
          want = 'food';
          break;
        default: {
          const _exhaustive: never = this.holdOrder;
          void _exhaustive;
          continue;
        }
      }
      if (h.cargo && h.cargo.kind !== want) this.stashCargo(h);
      h.wait = 0;
      h.busyAt = null;
      h.path = [];
      this.planHauler(h);
    }
    switch (this.holdOrder) {
      case 'power':
        this.hint = 'GUNS ORDER. Loaded haulers stash other cargo and peel to Power. Deposits still BRACE the Hub.';
        this.emit({ kind: 'hold', nodeId: 'hub', reason: 'power' });
        break;
      case 'food':
        this.hint = 'CREW ORDER. Loaded haulers stash other cargo and peel to Food. Splice still beats this.';
        this.emit({ kind: 'hold', nodeId: 'hub', reason: 'food' });
        break;
      case 'auto':
        this.hint = 'Hold auto. Haulers pick the hungriest stock.';
        this.emit({ kind: 'hold', nodeId: 'hub', reason: 'auto' });
        break;
      default: {
        const _exhaustive: never = this.holdOrder;
        return { ok: false, why: String(_exhaustive) };
      }
    }
    return { ok: true };
  }

  cancelSelection(): void {
    const gate = this.openingGate();
    if (gate === 'farm') {
      this.selectedTool = 'farm';
      this.routeFrom = null;
      this.hint = 'Place a Farm on a mesa pad. Wave clock is already running.';
      return;
    }
    if (gate === 'route') {
      this.selectedTool = 'route';
      if (!this.routeFrom) this.routeFrom = this.unroutedProducer();
      this.hint = 'Click the Hub to lay mag-rail from the Farm.';
      return;
    }
    this.selectedTool = 'none';
    this.routeFrom = null;
    this.refreshHint();
  }

  clickNode(nodeId: string): { ok: boolean; why?: string } {
    if (this.phase !== 'playing') return { ok: false, why: 'match over' };
    const node = this.nodes.get(nodeId);
    if (!node) return { ok: false, why: 'missing node' };
    const tool = this.selectedTool;
    switch (tool) {
      case 'none':
        return { ok: false, why: 'select a build tool' };
      case 'mine':
      case 'farm':
      case 'power': {
        const placed = this.tryPlaceProducer(tool, node);
        if (placed.ok) return placed;
        if (node.kind === 'hub' || node.kind === 'depot' || node.kind === 'pad') {
          const source = this.unroutedProducer();
          if (source) {
            this.selectedTool = 'route';
            this.routeFrom = source;
            return this.tryRoute(node);
          }
        }
        return placed;
      }
      case 'kinetic':
      case 'splash':
        return this.tryPlaceTower(tool, node);
      case 'route':
        return this.tryRoute(node);
      case 'barrier':
        return this.tryBarrier(node);
      case 'upgrade':
        return this.tryUpgrade();
      default: {
        const _exhaustive: never = tool;
        return { ok: false, why: String(_exhaustive) };
      }
    }
  }

  tryUpgrade(): { ok: boolean; why?: string } {
    if (this.hubLevel >= 2) return { ok: false, why: 'Hub is already Level 2' };
    if (this.hubUpgradeLeft > 0) return { ok: false, why: 'upgrade in progress' };
    const cost = BALANCE.hubLevel2;
    if (this.stock.ore < cost.ore || this.stock.food < cost.food || this.stock.power < cost.power) {
      return { ok: false, why: `Need ${cost.ore} ore, ${cost.food} food, ${cost.power} power` };
    }
    this.stock.ore -= cost.ore;
    this.stock.food -= cost.food;
    this.stock.power -= cost.power;
    this.hubUpgradeLeft = cost.time;
    this.emit({ kind: 'upgrade', nodeId: 'hub' });
    this.hint = 'Hub crews are bolting on mag-rail docks…';
    return { ok: true };
  }

  private tryPlaceProducer(type: 'mine' | 'farm' | 'power', node: SimNode): { ok: boolean; why?: string } {
    if (node.kind !== 'pad') return { ok: false, why: 'producers go on mesa pads' };
    if (this.buildings.has(node.id)) return { ok: false, why: 'pad occupied' };
    const spec = BALANCE.buildings[type];
    if (this.stock.ore < spec.ore) return { ok: false, why: `need ${spec.ore} ore` };
    this.stock.ore -= spec.ore;
    this.buildings.set(node.id, {
      id: nid(type),
      type,
      nodeId: node.id,
      buildLeft: spec.time,
      staffed: false,
      buffer: { ore: 0, food: 0, power: 0 },
      cooldown: 0,
    });
    this.emit({ kind: 'build', nodeId: node.id, x: node.x, z: node.z });
    this.assignWorkers();
    this.selectedTool = 'route';
    this.routeFrom = node.id;
    this.hint = 'Click the Hub to lay mag-rail from this pad.';
    return { ok: true };
  }

  private tryPlaceTower(type: 'kinetic' | 'splash', node: SimNode): { ok: boolean; why?: string } {
    if (node.kind !== 'choke' && node.kind !== 'tower') {
      return { ok: false, why: 'towers go on choke platforms' };
    }
    if (this.buildings.has(node.id)) return { ok: false, why: 'platform occupied' };
    if (type === 'splash' && this.hubLevel < 2) return { ok: false, why: 'Splash needs Hub Level 2' };
    const spec = BALANCE.buildings[type];
    if (this.stock.ore < spec.ore || this.stock.power < spec.power) {
      return { ok: false, why: `need ${spec.ore} ore and ${spec.power} power` };
    }
    this.stock.ore -= spec.ore;
    this.stock.power -= spec.power;
    this.buildings.set(node.id, {
      id: nid(type),
      type,
      nodeId: node.id,
      buildLeft: spec.time,
      staffed: true,
      buffer: { ore: 0, food: 0, power: 0 },
      cooldown: 0,
    });
    this.emit({ kind: 'build', nodeId: node.id, x: node.x, z: node.z });
    this.refreshHint();
    return { ok: true };
  }

  private tryRoute(node: SimNode): { ok: boolean; why?: string } {
    if (!this.routeFrom) {
      this.routeFrom = node.id;
      this.hint = `Route from ${label(node)}. Click the Hub or a linked pad.`;
      return { ok: true };
    }
    if (this.routeFrom === node.id) {
      this.routeFrom = null;
      this.hint = 'Route cancelled.';
      return { ok: true };
    }
    const edge = this.edgeBetween(this.routeFrom, node.id);
    if (!edge) {
      this.hint = 'No mag-rail corridor between those nodes. Click a neighbor or the Hub.';
      return { ok: false, why: 'no mag-rail corridor between those nodes' };
    }
    if (edge.routed && edge.sabotagedUntil <= this.t) {
      this.hint = 'Already routed. Pick another pad.';
      return { ok: false, why: 'already routed' };
    }
    if (this.stock.ore < BALANCE.routeCost) return { ok: false, why: `need ${BALANCE.routeCost} ore` };
    const splice = edge.routed && edge.sabotagedUntil > this.t;
    this.stock.ore -= BALANCE.routeCost;
    edge.routed = true;
    edge.sabotagedUntil = 0;
    this.routeFrom = null;
    const a = this.nodes.get(edge.a)!;
    const b = this.nodes.get(edge.b)!;
    this.emit({
      kind: 'route',
      edgeId: edge.id,
      fromX: a.x,
      fromZ: a.z,
      toX: b.x,
      toZ: b.z,
      x: (a.x + b.x) / 2,
      z: (a.z + b.z) / 2,
      reason: splice ? 'splice' : undefined,
    });
    if (splice) this.hint = 'Rail live. Haulers are rolling again.';
    const next = this.unroutedProducer();
    if (next) {
      this.selectedTool = 'route';
      this.routeFrom = next;
      this.hint = 'Click the Hub to connect the next pad.';
      return { ok: true };
    }
    if (![...this.buildings.values()].some((b) => b.type === 'mine')) {
      this.selectedTool = 'mine';
      this.hint = 'Mine next — ore only counts after it rides the mag-rail home.';
    } else if (![...this.buildings.values()].some((b) => b.type === 'power')) {
      this.selectedTool = 'power';
      this.hint = 'Power pylon next. Towers go dark without hauled power.';
    } else {
      this.selectedTool = 'route';
    }
    this.refreshHint();
    return { ok: true };
  }

  routeEnds(from = this.routeFrom): string[] {
    const cut = [...this.edges.values()].filter((e) => e.routed && e.sabotagedUntil > this.t);
    if (cut.length && (this.selectedTool === 'route' || !from)) {
      const ids = new Set<string>();
      for (const e of cut) {
        ids.add(e.a);
        ids.add(e.b);
      }
      if (from && ids.has(from)) {
        const local = cut.flatMap((e) => (e.a === from ? [e.b] : e.b === from ? [e.a] : []));
        if (local.length) return local;
      }
      if (!from) return [...ids];
    }
    if (!from) return [];
    const neigh = this.neighbors(from).map((edge) => otherEnd(edge, from));
    if (neigh.includes('hub')) return ['hub'];
    return neigh.filter((id) => {
      const kind = this.nodes.get(id)?.kind;
      return kind === 'hub' || kind === 'depot' || kind === 'pad';
    });
  }

  private offlineNodes(): string[] {
    const out: string[] = [];
    const shot = BALANCE.buildings.kinetic.powerShot ?? 0.34;
    for (const b of this.buildings.values()) {
      if (b.type === 'mine' || b.type === 'farm' || b.type === 'power') {
        if (!this.path(b.nodeId, 'hub', { routedOnly: true })) out.push(b.nodeId);
      }
      if ((b.type === 'kinetic' || b.type === 'splash') && this.stock.power < shot) out.push(b.nodeId);
    }
    return out;
  }

  private tryBarrier(node: SimNode): { ok: boolean; why?: string } {
    if (node.kind !== 'choke') return { ok: false, why: 'barriers drop on choke approaches' };
    const inbound = this.neighbors(node.id).filter((edge) => {
      const other = otherEnd(edge, node.id);
      return this.nodes.get(other)?.kind === 'spawn';
    });
    if (inbound.length === 0) return { ok: false, why: 'no spawn approach' };
    if (inbound.every((edge) => edge.barrier)) return { ok: false, why: 'already barred' };
    if (this.stock.ore < BALANCE.barrierCost) return { ok: false, why: `need ${BALANCE.barrierCost} ore` };
    this.stock.ore -= BALANCE.barrierCost;
    for (const edge of inbound) edge.barrier = true;
    this.emit({ kind: 'barrier', nodeId: node.id, x: node.x, z: node.z });
    this.refreshHint();
    return { ok: true };
  }

  unroutedProducer(): string | null {
    let found: string | null = null;
    for (const b of this.buildings.values()) {
      if (b.type !== 'mine' && b.type !== 'farm' && b.type !== 'power') continue;
      if (this.path(b.nodeId, 'hub', { routedOnly: true })) continue;
      found = b.nodeId;
    }
    return found;
  }

  assignWorkers(): void {
    const producers = [...this.buildings.values()].filter(
      (b) => b.type === 'mine' || b.type === 'farm' || b.type === 'power',
    );
    producers.sort((a, b) => diversityPriority(a, producers) - diversityPriority(b, producers));
    let free = this.workersTotal;
    for (const b of producers) {
      if (free > 0) {
        b.staffed = true;
        free -= 1;
      } else {
        b.staffed = false;
      }
    }
  }

  workersFree(): number {
    let used = 0;
    for (const b of this.buildings.values()) {
      if ((b.type === 'mine' || b.type === 'farm' || b.type === 'power') && b.staffed) used += 1;
    }
    return Math.max(0, this.workersTotal - used);
  }

  tick(dt: number): void {
    if (this.phase !== 'playing') return;
    const step = Math.min(dt, 0.05);
    this.t += step;
    this.tickUpgrade(step);
    this.tickBuildings(step);
    this.tickFood(step);
    this.tickHaulers(step);
    this.tickWaves(step);
    this.tickEnemies(step);
    this.tickTowers(step);
    this.tickHubGun(step);
    this.updateRates();
    this.refreshHint();
    this.checkEnd();
  }

  private tickUpgrade(dt: number): void {
    if (this.hubUpgradeLeft <= 0) return;
    this.hubUpgradeLeft -= dt;
    if (this.hubUpgradeLeft > 0) return;
    this.hubUpgradeLeft = 0;
    this.hubLevel = 2;
    this.workersTotal += BALANCE.hubLevel2.extraWorkers;
    const depot = this.nodes.get('depot')!;
    for (let i = 0; i < BALANCE.hubLevel2.extraHaulers; i += 1) {
      this.haulers.push({
        id: nid('haul'),
        x: depot.x,
        z: depot.z + 0.4,
        nodeId: 'depot',
        cargo: null,
        path: [],
        wait: 0,
        busyAt: null,
      });
    }
    this.assignWorkers();
    this.emit({ kind: 'upgrade', nodeId: 'hub', x: 0, z: 0 });
    this.hint = 'Hub Level 2 online. Splash towers and an extra hauler unlocked.';
  }

  private tickBuildings(dt: number): void {
    for (const b of this.buildings.values()) {
      if (b.buildLeft > 0) {
        b.buildLeft = Math.max(0, b.buildLeft - dt);
        continue;
      }
      if (b.type !== 'mine' && b.type !== 'farm' && b.type !== 'power') continue;
      if (!b.staffed) continue;
      const res = resourceOf(b.type);
      if (b.type !== 'power' && this.stock.power <= 0) continue;
      if (b.type !== 'power') this.stock.power = Math.max(0, this.stock.power - BALANCE.powerMineFarm * dt);
      const spec = BALANCE.buildings[b.type];
      b.buffer[res] = Math.min(spec.bufferCap, b.buffer[res] + spec.rate * dt);
    }
  }

  private tickFood(dt: number): void {
    const buildings = [...this.buildings.values()].filter((b) => b.type !== 'hub' && b.type !== 'depot').length;
    const drain = BALANCE.foodDrain + buildings * BALANCE.foodDrainPerBuilding;
    this.stock.food = Math.max(0, this.stock.food - drain * dt);
    if (this.stock.food <= 0.01) {
      this.starveTimer += dt;
      if (!this.foodWarned) {
        this.foodWarned = true;
        this.emit({ kind: 'warn_food' });
      }
    } else {
      this.starveTimer = 0;
      this.foodWarned = false;
    }
  }

  private tickHaulers(dt: number): void {
    for (const h of this.haulers) {
      if (h.wait > 0) {
        h.wait -= dt;
        if (h.wait > 0) continue;
        h.busyAt = null;
      }
      if (h.path.length === 0) this.planHauler(h);
      this.advanceAlongPath(h, dt, BALANCE.haulerSpeed, true);
      if (h.path.length === 0) this.onHaulerArrive(h);
    }
  }

  private stashCargo(h: Hauler): void {
    if (!h.cargo) return;
    const kind = h.cargo.kind;
    let best: { nodeId: string; len: number } | null = null;
    for (const b of this.buildings.values()) {
      if (b.type !== 'mine' && b.type !== 'farm' && b.type !== 'power') continue;
      if (resourceOf(b.type) !== kind) continue;
      if (b.buildLeft > 0) continue;
      const cap = BALANCE.buildings[b.type].bufferCap;
      if (b.buffer[kind] + h.cargo.amount > cap) continue;
      const path = this.path(h.nodeId, b.nodeId, { routedOnly: true });
      if (!path) continue;
      if (best && path.length >= best.len) continue;
      best = { nodeId: b.nodeId, len: path.length };
    }
    if (!best) return;
    const pad = this.buildings.get(best.nodeId);
    if (!pad) return;
    pad.buffer[kind] += h.cargo.amount;
    h.cargo = null;
  }

  private planHauler(h: Hauler): void {
    if (h.cargo) {
      const path = this.path(h.nodeId, 'hub', { routedOnly: true });
      h.path = path ? path.slice(1) : [];
      return;
    }
    let best: { nodeId: string; score: number } | null = null;
    let foodNeed = this.stock.food < 12 ? 90 : this.stock.food < 20 ? 28 : 0;
    let pwrNeed = this.stock.power < 6 ? 110 : this.stock.power < 14 ? 36 : 0;
    let oreNeed = this.stock.ore < 18 ? 18 : 0;
    switch (this.holdOrder) {
      case 'auto':
        break;
      case 'power':
        pwrNeed += 240;
        foodNeed *= 0.2;
        oreNeed *= 0.2;
        break;
      case 'food':
        foodNeed += 240;
        pwrNeed *= 0.2;
        oreNeed *= 0.2;
        break;
      default: {
        const _exhaustive: never = this.holdOrder;
        void _exhaustive;
      }
    }
    for (const b of this.buildings.values()) {
      if (b.type !== 'mine' && b.type !== 'farm' && b.type !== 'power') continue;
      if (b.buildLeft > 0) continue;
      const res = resourceOf(b.type);
      if (b.buffer[res] < 1) continue;
      const path = this.path(h.nodeId, b.nodeId, { routedOnly: true });
      if (!path) continue;
      const hunger = res === 'food' ? foodNeed : res === 'power' ? pwrNeed : oreNeed;
      const score = b.buffer[res] * 4 - path.length + hunger;
      if (!best || score > best.score) best = { nodeId: b.nodeId, score };
    }
    if (!best) return;
    const path = this.path(h.nodeId, best.nodeId, { routedOnly: true });
    h.path = path ? path.slice(1) : [];
  }

  private onHaulerArrive(h: Hauler): void {
    if (h.cargo && h.nodeId === 'hub') {
      const kind = h.cargo.kind;
      const amount = h.cargo.amount;
      this.stock[kind] += amount;
      this.deliveredWindow.push({ t: this.t, kind, amount });
      this.emit({ kind: 'deposit', nodeId: 'hub', resource: kind, amount, x: 0, z: 0 });
      h.cargo = null;
      h.wait = BALANCE.depositBusy;
      h.busyAt = 'hub';
      if (this.enemies.length > 0 || this.pending.length > 0) {
        this.surgeUntil = this.t + (this.hubHp < 72 ? 0.85 : 0.55);
        this.emit({ kind: 'surge', nodeId: 'hub', resource: kind, amount, x: 0, z: 0 });
      }
      return;
    }
    const b = this.buildings.get(h.nodeId);
    if (!h.cargo && b && (b.type === 'mine' || b.type === 'farm' || b.type === 'power')) {
      const res = resourceOf(b.type);
      const take = Math.min(BALANCE.haulerCapacity, Math.floor(b.buffer[res]));
      if (take >= 1) {
        b.buffer[res] -= take;
        h.cargo = { kind: res, amount: take };
        h.wait = BALANCE.pickupBusy;
        h.busyAt = h.nodeId;
      }
    }
  }

  private advanceAlongPath(
    agent: { x: number; z: number; nodeId: string; path: string[] },
    dt: number,
    speed: number,
    congest: boolean,
  ): void {
    if (agent.path.length === 0) return;
    const nextId = agent.path[0];
    const next = this.nodes.get(nextId);
    if (!next) {
      agent.path = [];
      return;
    }
    if (congest && this.nodeBusy(nextId, agent)) return;
    const d = dist(agent.x, agent.z, next.x, next.z);
    const step = speed * dt;
    if (d <= step + 0.08) {
      agent.x = next.x;
      agent.z = next.z;
      agent.nodeId = nextId;
      agent.path.shift();
      return;
    }
    agent.x += ((next.x - agent.x) / d) * step;
    agent.z += ((next.z - agent.z) / d) * step;
  }

  private nodeBusy(nodeId: string, self: { x: number; z: number }): boolean {
    return this.haulers.some((h) => h !== self && h.busyAt === nodeId && h.wait > 0);
  }

  private tickWaves(dt: number): void {
    for (let i = this.pending.length - 1; i >= 0; i -= 1) {
      if (this.pending[i].at > this.t) continue;
      this.spawnEnemy(this.pending[i].type, this.pending[i].spawn);
      this.pending.splice(i, 1);
    }
    if (this.waveIndex >= BALANCE.waves.length) return;
    this.nextWaveIn = Math.max(0, this.nextWaveIn - dt);
    if (this.nextWaveIn > 0) return;
    const spec = BALANCE.waves[this.waveIndex];
    this.waveIndex += 1;
    this.emit({ kind: 'wave', wave: this.waveIndex });
    let t = this.t;
    for (const pack of spec.packs) {
      for (let i = 0; i < pack.count; i += 1) {
        this.pending.push({ at: t, type: pack.type, spawn: pack.spawn });
        t += pack.stagger;
      }
    }
    if (this.waveIndex < BALANCE.waves.length) {
      this.nextWaveIn = BALANCE.waves[this.waveIndex].after;
    } else {
      this.nextWaveIn = 0;
    }
  }

  private spawnEnemy(type: EnemyType, spawnId: string): void {
    const spawn = this.nodes.get(spawnId);
    if (!spawn) return;
    const spec = BALANCE.enemies[type];
    const jitter = (this.rng() - 0.5) * 0.5;
    const enemy: Enemy = {
      id: nid('e'),
      type,
      x: spawn.x + jitter,
      z: spawn.z + jitter,
      hp: spec.hp,
      maxHp: spec.hp,
      path: [],
      nodeId: spawnId,
      slowUntil: 0,
      attackCd: 0,
      flash: 0,
    };
    const path = this.path(spawnId, 'hub', {
      routedOnly: false,
      ignoreBarriers: type === 'runner',
      preferRouted: true,
    });
    enemy.path = path ? path.slice(1) : [];
    this.enemies.push(enemy);
  }

  private tickEnemies(dt: number): void {
    const hub = this.nodes.get('hub')!;
    for (const e of this.enemies) {
      e.flash = Math.max(0, e.flash - dt);
      const spec = BALANCE.enemies[e.type];
      const slowed = e.slowUntil > this.t ? 1 - BALANCE.buildings.splash.slow : 1;
      if (e.path.length === 0) {
        if (dist(e.x, e.z, hub.x, hub.z) > 0.9) {
          const path = this.path(e.nodeId, 'hub', {
            routedOnly: false,
            ignoreBarriers: e.type === 'runner',
            preferRouted: true,
          });
          e.path = path ? path.slice(1) : [];
        } else {
          e.attackCd -= dt;
          if (e.attackCd <= 0) {
            let dmg = spec.damage;
            if (this.surgeUntil > this.t) dmg *= 0.72;
            this.hubHp = Math.max(0, this.hubHp - dmg);
            e.attackCd = 0.85;
            this.emit({ kind: 'hit', nodeId: 'hub', x: 0, z: 0, amount: dmg });
          }
        }
      } else {
        const nextId = e.path[0];
        const edge = this.edgeBetween(e.nodeId, nextId);
        if (edge?.barrier && e.type !== 'runner') {
          this.advanceAlongPath(e, dt, spec.speed * slowed * 0.32, false);
          continue;
        }
        const before = e.nodeId;
        this.advanceAlongPath(e, dt, spec.speed * slowed, false);
        if (e.type === 'runner' && e.nodeId !== before) {
          const crossed = this.edgeBetween(before, e.nodeId);
          const chance = BALANCE.enemies.runner.sabotageChance ?? 0.55;
          if (crossed?.routed && crossed.sabotagedUntil <= this.t && this.rng() < chance) {
            const sabotageFor = BALANCE.enemies.runner.sabotage ?? 11;
            crossed.sabotagedUntil = this.t + sabotageFor;
            this.replanIfCut(crossed.id);
            const a = this.nodes.get(crossed.a)!;
            const b = this.nodes.get(crossed.b)!;
            this.emit({
              kind: 'sabotage',
              edgeId: crossed.id,
              x: (a.x + b.x) / 2,
              z: (a.z + b.z) / 2,
            });
            if (!this.openingGate()) {
              this.selectedTool = 'route';
              this.routeFrom = crossed.a;
              this.hint = 'HAUL CUT. Route is armed — click the glowing pad to splice the rail.';
            }
          }
        }
      }
    }
  }

  private tickTowers(dt: number): void {
    for (const b of this.buildings.values()) {
      if (b.type !== 'kinetic' && b.type !== 'splash') continue;
      if (b.buildLeft > 0) continue;
      this.stock.power = Math.max(0, this.stock.power - BALANCE.towerIdlePower * dt);
      b.cooldown = Math.max(0, b.cooldown - dt);
      if (b.cooldown > 0) continue;
      const node = this.nodes.get(b.nodeId)!;
      const spec = BALANCE.buildings[b.type];
      let target: Enemy | null = null;
      let best = Infinity;
      const range = Number(spec.range);
      for (const e of this.enemies) {
        const d = dist(node.x, node.z, e.x, e.z);
        if (d > range) continue;
        const pri = e.type === 'brute' ? 0 : e.type === 'grunt' ? 1 : 2;
        const score = pri * 100 + d;
        if (score < best) {
          best = score;
          target = e;
        }
      }
      if (!target) continue;
      const shotCost = spec.powerShot ?? 0.34;
      if (this.stock.power < shotCost) {
        b.cooldown = 0.22;
        if (this.t - this.brownoutAt > 1.1) {
          this.brownoutAt = this.t;
          this.emit({ kind: 'brownout', nodeId: b.nodeId, x: node.x, z: node.z });
        }
        continue;
      }
      this.stock.power = Math.max(0, this.stock.power - shotCost);
      if (b.type === 'kinetic') {
        this.hurt(target, spec.damage);
        b.cooldown = spec.cooldown;
        this.emit({
          kind: 'shot',
          fromX: node.x,
          fromZ: node.z,
          toX: target.x,
          toZ: target.z,
          x: target.x,
          z: target.z,
          enemyId: target.id,
        });
      } else {
        const splash = BALANCE.buildings.splash;
        for (const e of this.enemies) {
          if (dist(target.x, target.z, e.x, e.z) <= splash.radius) {
            this.hurt(e, splash.damage);
            e.slowUntil = Math.max(e.slowUntil, this.t + splash.slowTime);
          }
        }
        b.cooldown = splash.cooldown;
        this.emit({
          kind: 'splash',
          fromX: node.x,
          fromZ: node.z,
          toX: target.x,
          toZ: target.z,
          x: target.x,
          z: target.z,
        });
      }
    }
    this.enemies = this.enemies.filter((e) => e.hp > 0);
  }

  private tickHubGun(dt: number): void {
    this.hubGunCd = Math.max(0, this.hubGunCd - dt);
    if (this.hubGunCd > 0) return;
    const hub = this.nodes.get('hub')!;
    let target: Enemy | null = null;
    let best = 3.4;
    for (const e of this.enemies) {
      const d = dist(hub.x, hub.z, e.x, e.z);
      if (d <= best) {
        best = d;
        target = e;
      }
    }
    if (!target) return;
    const cost = 0.22;
    if (this.stock.power < cost) {
      if (this.t - this.brownoutAt > 1.1) {
        this.brownoutAt = this.t;
        this.emit({ kind: 'brownout', nodeId: 'hub', x: 0, z: 0 });
      }
      return;
    }
    this.stock.power -= cost;
    this.hurt(target, 12);
    this.hubGunCd = 0.5;
    this.emit({
      kind: 'shot',
      fromX: 0,
      fromZ: 0,
      toX: target.x,
      toZ: target.z,
      x: target.x,
      z: target.z,
      enemyId: target.id,
    });
    this.enemies = this.enemies.filter((e) => e.hp > 0);
  }

  private replanIfCut(edgeId: string): void {
    for (const h of this.haulers) {
      if (h.path.length === 0) continue;
      const chain = [h.nodeId, ...h.path];
      for (let i = 0; i < chain.length - 1; i += 1) {
        if (this.edgeBetween(chain[i], chain[i + 1])?.id === edgeId) {
          h.path = [];
          break;
        }
      }
    }
  }

  private hurt(e: Enemy, amount: number): void {
    e.hp -= amount;
    e.flash = 0.22;
    this.emit({ kind: 'hit', enemyId: e.id, x: e.x, z: e.z, amount });
    if (e.hp > 0) return;
    const scrap = BALANCE.enemies[e.type].scrap;
    this.stock.ore += scrap;
    this.emit({ kind: 'death', enemyId: e.id, x: e.x, z: e.z, amount: scrap, resource: 'ore' });
  }

  path(
    from: string,
    to: string,
    opts: { routedOnly: boolean; ignoreBarriers?: boolean; preferRouted?: boolean },
  ): string[] | null {
    if (from === to) return [from];
    const distMap = new Map<string, number>();
    const prev = new Map<string, string>();
    const pq: { id: string; d: number }[] = [{ id: from, d: 0 }];
    distMap.set(from, 0);
    while (pq.length) {
      pq.sort((a, b) => a.d - b.d);
      const cur = pq.shift()!;
      if (cur.id === to) break;
      if (cur.d !== distMap.get(cur.id)) continue;
      for (const edge of this.neighbors(cur.id)) {
        const liveRoute = edge.routed && edge.sabotagedUntil <= this.t;
        if (opts.routedOnly && !liveRoute) continue;
        const nxt = otherEnd(edge, cur.id);
        const a = this.nodes.get(cur.id)!;
        const b = this.nodes.get(nxt)!;
        let w = dist(a.x, a.z, b.x, b.z);
        if (edge.barrier && !opts.ignoreBarriers) w *= 3.2;
        if (opts.preferRouted && liveRoute) w *= 0.45;
        if (opts.preferRouted && !liveRoute) w *= 1.7;
        const nd = cur.d + w;
        if (nd < (distMap.get(nxt) ?? Infinity)) {
          distMap.set(nxt, nd);
          prev.set(nxt, cur.id);
          pq.push({ id: nxt, d: nd });
        }
      }
    }
    if (!distMap.has(to)) return null;
    const path = [to];
    while (path[0] !== from) {
      const p = prev.get(path[0]);
      if (!p) return null;
      path.unshift(p);
    }
    return path;
  }

  private updateRates(): void {
    const window = 6;
    this.deliveredWindow = this.deliveredWindow.filter((d) => this.t - d.t <= window);
    const sums: Stockpile = { ore: 0, food: 0, power: 0 };
    for (const d of this.deliveredWindow) sums[d.kind] += d.amount;
    this.rates = {
      ore: sums.ore / window,
      food: sums.food / window,
      power: sums.power / window,
    };
  }

  private checkEnd(): void {
    if (this.hubHp <= 0) {
      this.phase = 'lost_hub';
      this.emit({ kind: 'lose', reason: 'hub' });
      return;
    }
    if (this.starveTimer >= this.config.starveFail) {
      this.phase = 'lost_starve';
      this.emit({ kind: 'lose', reason: 'starve' });
      return;
    }
    const wavesDone = this.waveIndex >= this.config.wavesToWin && this.enemies.length === 0 && this.pending.length === 0;
    if (wavesDone && this.hubLevel >= 2) {
      this.phase = 'won';
      this.emit({ kind: 'win' });
    }
  }

  refreshHint(): void {
    if (this.phase !== 'playing') return;
    const farms = [...this.buildings.values()].filter((b) => b.type === 'farm');
    const hasFarm = farms.length > 0;
    const farmRouted = farms.some((b) => !!this.path(b.nodeId, 'hub', { routedOnly: true }));
    const hasMine = [...this.buildings.values()].some((b) => b.type === 'mine');
    const hasPower = [...this.buildings.values()].some((b) => b.type === 'power');
    const routedPads = [...this.buildings.values()].filter((b) => {
      if (b.type !== 'mine' && b.type !== 'farm' && b.type !== 'power') return false;
      return !!this.path(b.nodeId, 'hub', { routedOnly: true });
    }).length;
    const towers = [...this.buildings.values()].filter((b) => b.type === 'kinetic' || b.type === 'splash').length;
    const cut = [...this.edges.values()].some((e) => e.routed && e.sabotagedUntil > this.t);
    if (!hasFarm) this.hint = 'Food is already draining. Place a Farm on a mesa pad, then click the Hub.';
    else if (!farmRouted) this.hint = 'Mag-rail next. Click the Hub to connect this Farm — haulers will not leave the pad until you do.';
    else if (cut) this.hint = 'HAUL CUT. Route is armed — click the glowing pad to splice the rail.';
    else if (this.holdOrder === 'power') this.hint = 'GUNS ORDER — loaded haulers peel to Power. H to flip.';
    else if (this.holdOrder === 'food') this.hint = 'CREW ORDER — loaded haulers peel to Food. H to flip.';
    else if (this.stock.power < 4 && towers > 0) this.hint = 'Brownout. Towers are dry. Keep the Power pylon on live rails.';
    else if (!hasMine) this.hint = 'Drop a Mine. Haulers only move ore that reaches the Hub.';
    else if (!hasPower) this.hint = 'Plant a Power pylon. Mines, farms, and towers stall without hauled power.';
    else if (routedPads < 3) this.hint = 'Draw mag-rail routes from each producer back to the Hub.';
    else if (this.nextWaveIn < 18 && towers < 1) this.hint = 'Raiders inbound. Plant a Kinetic tower on a choke.';
    else if (this.hubLevel < 2) this.hint = 'Stockpile for Hub Level 2. That unlocks Splash and a spare hauler.';
    else if (towers < 2) this.hint = 'Cover a second approach. Splash slows brutes on the rails.';
    else if (this.stock.food < 8) this.hint = 'Food critical. Keep farms staffed and the rails live.';
    else this.hint = `Wave ${Math.min(this.waveIndex + 1, this.config.wavesToWin)} of ${this.config.wavesToWin}. Hold the mesa.`;
  }

  snapshot(): Snapshot {
    return {
      t: this.t,
      phase: this.phase,
      stock: cloneStock(this.stock),
      rates: { ...this.rates },
      hubHp: this.hubHp,
      hubMaxHp: this.config.hubMaxHp,
      hubLevel: this.hubLevel,
      hubUpgrading: this.hubUpgradeLeft > 0,
      workersTotal: this.workersTotal,
      workersFree: this.workersFree(),
      haulerCount: this.haulers.length,
      starveTimer: this.starveTimer,
      waveIndex: this.waveIndex,
      wavesToWin: this.config.wavesToWin,
      nextWaveIn: this.nextWaveIn,
      waveLive: this.enemies.length,
      foodWarned: this.foodWarned,
      haulCut: [...this.edges.values()].some((e) => e.routed && e.sabotagedUntil > this.t),
      powerBrownout: this.stock.power < (BALANCE.buildings.kinetic.powerShot ?? 0.34),
      holdOrder: this.holdOrder,
      holdReady: this.holdReady,
      selectedTool: this.selectedTool,
      routeEnds: this.routeEnds(),
      offlineNodeIds: this.offlineNodes(),
      waveThreat: waveThreat(this.waveIndex, this.config.wavesToWin),
      routeFrom: this.routeFrom,
      hint: this.hint,
      nodes: [...this.nodes.values()].map((n) => ({ ...n })),
      edges: [...this.edges.values()].map((e) => ({ ...e })),
      buildings: [...this.buildings.values()].map((b) => ({
        ...b,
        buffer: { ...b.buffer },
      })),
      haulers: this.haulers.map((h) => ({ ...h, cargo: h.cargo ? { ...h.cargo } : null, path: [...h.path] })),
      enemies: this.enemies.map((e) => ({ ...e, path: [...e.path] })),
      events: [...this.events],
    };
  }
}

function waveThreat(waveIndex: number, wavesToWin: number): string {
  if (waveIndex <= 0) return `Wave 1 / ${wavesToWin}`;
  const spec = BALANCE.waves[waveIndex - 1];
  if (!spec) return `Wave ${waveIndex}`;
  const parts = spec.packs.map((p) => `${p.count} ${p.type}${p.count > 1 ? 's' : ''}`);
  return `Wave ${waveIndex} — ${parts.join(', ')}`;
}

function diversityPriority(b: Building, all: Building[]): number {
  const first = all.find((x) => x.type === b.type);
  const isFirst = first?.id === b.id;
  switch (b.type) {
    case 'farm':
      return isFirst ? 0 : 5;
    case 'power':
      return isFirst ? 1 : 6;
    case 'mine':
      return isFirst ? 2 : 4;
    default:
      return 9;
  }
}

function label(node: SimNode): string {
  switch (node.kind) {
    case 'hub':
      return 'Hub';
    case 'depot':
      return 'Depot';
    case 'pad':
      return 'pad';
    case 'choke':
      return 'choke';
    case 'tower':
      return 'tower pad';
    case 'spawn':
      return 'raid path';
    default: {
      const _exhaustive: never = node.kind;
      return _exhaustive;
    }
  }
}

function mulberry32(a: number): () => number {
  return function rng() {
    let t = (a += 0x6d2b79f5);
    t = Math.imul(t ^ (t >>> 15), t | 1);
    t ^= t + Math.imul(t ^ (t >>> 7), t | 61);
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}
