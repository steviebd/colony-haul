import type { EnemyType, Resource, Stockpile, WaveSpec } from './types.ts';

export const BALANCE = {
  wavesToWin: 6,
  hubMaxHp: 175,
  starveFail: 14,
  start: { ore: 72, food: 40, power: 24 } satisfies Stockpile,
  foodDrain: 0.42,
  foodDrainPerBuilding: 0.04,
  powerMineFarm: 0.06,
  towerIdlePower: 0.11,
  depositBusy: 0.32,
  pickupBusy: 0.28,
  haulerSpeed: 3.9,
  haulerCapacity: 8,
  startHaulers: 2,
  startWorkers: 4,
  hubLevel2: { ore: 64, food: 30, power: 22, time: 2.8, extraHaulers: 1, extraWorkers: 2 },
  routeCost: 3,
  barrierCost: 5,
  openingLock: 60,
  buildings: {
    mine: { ore: 12, time: 1.2, rate: 1.35, bufferCap: 22 },
    farm: { ore: 10, time: 1.1, rate: 1.15, bufferCap: 22 },
    power: { ore: 13, time: 1.25, rate: 1.05, bufferCap: 22 },
    kinetic: { ore: 16, power: 3, time: 0.9, range: 5.25, damage: 16, cooldown: 0.48, powerShot: 0.64 },
    splash: { ore: 28, power: 6, time: 1.15, range: 4.9, damage: 12, cooldown: 0.85, radius: 2.1, slow: 0.45, slowTime: 1.45, powerShot: 0.78 },
  },
  enemies: {
    grunt: { hp: 36, speed: 2.2, damage: 8, scrap: 3, radius: 0.28 },
    brute: { hp: 120, speed: 1.05, damage: 18, scrap: 8, radius: 0.42 },
    runner: { hp: 52, speed: 4.35, damage: 5, scrap: 4, radius: 0.24, sabotage: 12, sabotageChance: 0.78 },
  } satisfies Record<EnemyType, { hp: number; speed: number; damage: number; scrap: number; radius: number; sabotage?: number; sabotageChance?: number }>,
  waves: [
    { after: 42, packs: [{ type: 'grunt', count: 4, spawn: 'spawn_e', stagger: 0.55 }] },
    { after: 28, packs: [{ type: 'grunt', count: 4, spawn: 'spawn_e', stagger: 0.45 }, { type: 'runner', count: 2, spawn: 'spawn_e', stagger: 0.35 }, { type: 'grunt', count: 3, spawn: 'spawn_n', stagger: 0.45 }] },
    { after: 28, packs: [{ type: 'grunt', count: 4, spawn: 'spawn_w', stagger: 0.4 }, { type: 'brute', count: 1, spawn: 'spawn_e', stagger: 0.7 }, { type: 'runner', count: 1, spawn: 'spawn_e', stagger: 0.4 }] },
    { after: 27, packs: [{ type: 'grunt', count: 5, spawn: 'spawn_e', stagger: 0.35 }, { type: 'runner', count: 2, spawn: 'spawn_e', stagger: 0.3 }, { type: 'brute', count: 1, spawn: 'spawn_w', stagger: 0.6 }] },
    { after: 26, packs: [{ type: 'grunt', count: 5, spawn: 'spawn_n', stagger: 0.32 }, { type: 'brute', count: 2, spawn: 'spawn_e', stagger: 0.55 }, { type: 'runner', count: 2, spawn: 'spawn_e', stagger: 0.3 }] },
    { after: 26, packs: [{ type: 'grunt', count: 4, spawn: 'spawn_e', stagger: 0.32 }, { type: 'brute', count: 1, spawn: 'spawn_n', stagger: 0.55 }, { type: 'runner', count: 1, spawn: 'spawn_e', stagger: 0.35 }] },
  ] satisfies WaveSpec[],
} as const;

export function resourceOf(type: 'mine' | 'farm' | 'power'): Resource {
  switch (type) {
    case 'mine':
      return 'ore';
    case 'farm':
      return 'food';
    case 'power':
      return 'power';
    default: {
      const _exhaustive: never = type;
      return _exhaustive;
    }
  }
}
