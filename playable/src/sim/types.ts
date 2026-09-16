export type Resource = 'ore' | 'food' | 'power';

export type Phase = 'playing' | 'won' | 'lost_hub' | 'lost_starve';

export type Tool =
  | 'none'
  | 'mine'
  | 'farm'
  | 'power'
  | 'route'
  | 'kinetic'
  | 'splash'
  | 'barrier'
  | 'upgrade';

export type NodeKind = 'hub' | 'depot' | 'pad' | 'choke' | 'tower' | 'spawn';

export type BuildingType =
  | 'hub'
  | 'depot'
  | 'mine'
  | 'farm'
  | 'power'
  | 'kinetic'
  | 'splash';

export type EnemyType = 'grunt' | 'brute' | 'runner';

export type JuiceKind =
  | 'deposit'
  | 'shot'
  | 'splash'
  | 'hit'
  | 'death'
  | 'wave'
  | 'sabotage'
  | 'warn_food'
  | 'brownout'
  | 'build'
  | 'upgrade'
  | 'win'
  | 'lose'
  | 'route'
  | 'barrier';

export interface SimNode {
  id: string;
  x: number;
  z: number;
  kind: NodeKind;
}

export interface SimEdge {
  id: string;
  a: string;
  b: string;
  routed: boolean;
  sabotagedUntil: number;
  barrier: boolean;
}

export interface Building {
  id: string;
  type: BuildingType;
  nodeId: string;
  buildLeft: number;
  staffed: boolean;
  buffer: Record<Resource, number>;
  cooldown: number;
}

export interface Hauler {
  id: string;
  x: number;
  z: number;
  nodeId: string;
  cargo: { kind: Resource; amount: number } | null;
  path: string[];
  wait: number;
  busyAt: string | null;
}

export interface Enemy {
  id: string;
  type: EnemyType;
  x: number;
  z: number;
  hp: number;
  maxHp: number;
  path: string[];
  nodeId: string;
  slowUntil: number;
  attackCd: number;
  flash: number;
}

export interface JuiceEvent {
  kind: JuiceKind;
  t: number;
  x?: number;
  z?: number;
  nodeId?: string;
  edgeId?: string;
  enemyId?: string;
  resource?: Resource;
  amount?: number;
  wave?: number;
  reason?: string;
  fromX?: number;
  fromZ?: number;
  toX?: number;
  toZ?: number;
}

export interface Stockpile {
  ore: number;
  food: number;
  power: number;
}

export interface Rates {
  ore: number;
  food: number;
  power: number;
}

export interface WaveSpec {
  after: number;
  packs: { type: EnemyType; count: number; spawn: string; stagger: number }[];
}

export interface GameConfig {
  wavesToWin: number;
  hubMaxHp: number;
  starveFail: number;
  start: Stockpile;
}

export interface Snapshot {
  t: number;
  phase: Phase;
  stock: Stockpile;
  rates: Rates;
  hubHp: number;
  hubMaxHp: number;
  hubLevel: number;
  hubUpgrading: boolean;
  workersTotal: number;
  workersFree: number;
  haulerCount: number;
  starveTimer: number;
  waveIndex: number;
  wavesToWin: number;
  nextWaveIn: number;
  waveLive: number;
  foodWarned: boolean;
  haulCut: boolean;
  powerBrownout: boolean;
  selectedTool: Tool;
  routeFrom: string | null;
  routeEnds: string[];
  offlineNodeIds: string[];
  waveThreat: string;
  hint: string;
  nodes: SimNode[];
  edges: SimEdge[];
  buildings: Building[];
  haulers: Hauler[];
  enemies: Enemy[];
  events: JuiceEvent[];
}
