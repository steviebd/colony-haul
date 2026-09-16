import { BALANCE } from './balance.ts';
import { Game } from './game.ts';
import { runHeadless } from './demo.ts';

function assert(cond: unknown, msg: string): void {
  if (!cond) throw new Error(msg);
}

function nestedFarmRouteHub(): void {
  const game = new Game(7);
  game.setTool('farm');
  const placed = game.clickNode('pad_s');
  assert(placed.ok, 'farm should place on pad_s');
  assert(game.selectedTool === 'route', `auto-arm route, got ${game.selectedTool}`);
  assert(game.routeFrom === 'pad_s', `routeFrom pad_s, got ${game.routeFrom}`);
  game.setTool('route');
  assert(game.routeFrom === 'pad_s', 're-selecting Route must keep the armed pad');
  const routed = game.clickNode('hub');
  assert(routed.ok, `hub click should lay rail: ${routed.why ?? ''}`);
  const edge = game.edgeBetween('pad_s', 'hub');
  assert(edge?.routed, 'pad_s~hub must be routed');
  let deposits = 0;
  const step = 1 / 20;
  for (let i = 0; i < 20 * 18; i += 1) {
    game.tick(step);
    for (const ev of game.drainEvents()) {
      if (ev.kind === 'deposit') deposits += 1;
    }
    if (deposits > 0) break;
  }
  assert(deposits > 0, 'hauler must deposit after farm-hub rail');
}

function openingGateBlocksDeadClicks(): void {
  const game = new Game(7);
  game.setTool('kinetic');
  assert(game.selectedTool === 'farm', 'opening gate forces Farm');
  game.setTool('farm');
  game.clickNode('pad_s');
  game.setTool('mine');
  assert(game.selectedTool === 'route', 'until farm is routed, tools snap back to Route');
  assert(game.routeFrom === 'pad_s', 'armed pad survives the dead click');
}

function kineticAffordableAfterCore(): void {
  const game = new Game(7);
  game.setTool('farm');
  game.clickNode('pad_s');
  game.clickNode('hub');
  game.setTool('mine');
  game.clickNode('pad_n');
  game.clickNode('hub');
  game.setTool('power');
  game.clickNode('pad_se');
  game.clickNode('hub');
  assert(game.stock.ore >= 16, `must still afford a kinetic (ore ${game.stock.ore})`);
}

function cutRailRepairGhosts(): void {
  const game = new Game(7);
  game.setTool('farm');
  game.clickNode('pad_s');
  game.clickNode('hub');
  const edge = game.edgeBetween('pad_s', 'hub');
  assert(edge?.routed, 'farm rail must exist before the cut');
  edge!.sabotagedUntil = game.t + 10;
  game.selectedTool = 'route';
  game.routeFrom = null;
  const ends = game.routeEnds();
  assert(ends.includes('pad_s') && ends.includes('hub'), `cut endpoints ${ends.join(',')}`);
  game.routeFrom = 'pad_s';
  const other = game.routeEnds();
  assert(other.includes('hub') && other.length === 1, `armed cut should ghost Hub only, got ${other.join(',')}`);
}

function foodDrainIsEarned(): void {
  const barren = new Game(7);
  const step = 1 / 20;
  for (let i = 0; i < 20 * 50; i += 1) barren.tick(step);
  assert(barren.stock.food < 30, `idle food should drain, got ${barren.stock.food}`);
  const fed = new Game(7);
  fed.setTool('farm');
  fed.clickNode('pad_s');
  fed.clickNode('hub');
  for (let i = 0; i < 20 * 50; i += 1) fed.tick(step);
  assert(fed.phase === 'playing', `a farmed colony should still be playing at 50s, got ${fed.phase}`);
  assert(fed.stock.food > barren.stock.food, `routed farm should beat idle stores (${fed.stock.food} vs ${barren.stock.food})`);
}

function tickUntil(game: Game, pred: () => boolean, seconds: number): void {
  const step = 1 / 20;
  const cap = Math.ceil(seconds * 20);
  for (let i = 0; i < cap; i += 1) {
    if (pred()) return;
    if (game.phase !== 'playing') return;
    game.tick(step);
  }
}

function hubDiesWithoutDefense(): void {
  const game = new Game(7);
  tickUntil(game, () => game.phase !== 'playing', 240);
  assert(game.phase === 'lost_hub', `hub-die expected lost_hub, got ${game.phase} t=${game.t.toFixed(1)} hp=${game.hubHp} food=${game.stock.food.toFixed(1)}`);
}

function starveWithoutFarm(): void {
  const game = new Game(7);
  const step = 1 / 20;
  game.setTool('farm');
  assert(game.clickNode('pad_s').ok, 'farm to unlock Kinetic before Wave 1');
  game.clickNode('hub');
  game.setTool('kinetic');
  assert(game.clickNode('choke_e').ok, 'kinetic choke_e');
  if (game.selectedTool !== 'kinetic') game.setTool('kinetic');
  assert(game.clickNode('choke_n').ok, 'kinetic choke_n');
  game.setTool('power');
  assert(game.clickNode('pad_se').ok, 'power pylon');
  game.clickNode('hub');
  game.setTool('barrier');
  game.clickNode('choke_e');
  const farmRail = game.edgeBetween('pad_s', 'hub');
  assert(farmRail?.routed, 'farm rail must exist so we can starve it');
  const extra = ['choke_w', 'tower_ne', 'tower_sw'] as const;
  for (let i = 0; i < 20 * 200; i += 1) {
    farmRail!.sabotagedUntil = game.t + 30;
    if (game.phase !== 'playing') break;
    if (game.stock.ore >= BALANCE.buildings.kinetic.ore && game.stock.power >= BALANCE.buildings.kinetic.power) {
      for (const pad of extra) {
        if (game.buildings.has(pad)) continue;
        if (game.selectedTool !== 'kinetic') game.setTool('kinetic');
        if (game.clickNode(pad).ok) break;
      }
    }
    game.tick(step);
  }
  assert(
    game.phase === 'lost_starve',
    `starve expected lost_starve, got ${game.phase} t=${game.t.toFixed(1)} hp=${game.hubHp} food=${game.stock.food.toFixed(1)} starve=${game.starveTimer.toFixed(1)}`,
  );
}

function onlyFarmRailCutMidWave(): void {
  const game = new Game(7);
  game.setTool('farm');
  game.clickNode('pad_s');
  game.clickNode('hub');
  tickUntil(game, () => game.waveIndex >= 1 && game.enemies.length > 0, 90);
  assert(game.waveIndex >= 1 && game.enemies.length > 0, `need a live wave before the cut, wave=${game.waveIndex} live=${game.enemies.length}`);
  const edge = game.edgeBetween('pad_s', 'hub');
  assert(edge?.routed, 'only farm rail must exist');
  edge!.sabotagedUntil = game.t + 10;
  game.selectedTool = 'route';
  game.routeFrom = null;
  const snap = game.snapshot();
  assert(snap.haulCut, 'cut farm rail must raise haulCut');
  assert(snap.offlineNodeIds.includes('pad_s'), `farm pad must go offline, got ${snap.offlineNodeIds.join(',')}`);
  const ends = game.routeEnds();
  assert(ends.includes('pad_s') && ends.includes('hub'), `cut endpoints ${ends.join(',')}`);
  game.routeFrom = 'pad_s';
  const armed = game.routeEnds();
  assert(armed.includes('hub') && armed.length === 1, `armed cut should ghost Hub only, got ${armed.join(',')}`);
}

function holdOrderRushesPower(): void {
  const early = new Game(7);
  const locked = early.cycleHold();
  assert(!locked.ok, 'hold must stay locked before wave 4 and Hub L2');
  assert(early.holdOrder === 'auto', 'default hold is auto');

  const game = new Game(7);
  game.setTool('farm');
  assert(game.clickNode('pad_s').ok, 'farm should place');
  assert(game.clickNode('hub').ok, 'farm rail');
  game.setTool('power');
  assert(game.clickNode('pad_se').ok, 'power should place');
  assert(game.clickNode('hub').ok, 'power rail');
  const step = 1 / 20;
  for (let i = 0; i < 20 * 28; i += 1) game.tick(step);
  game.hubLevel = 2;
  game.waveIndex = 4;
  const armed = game.cycleHold();
  assert(armed.ok, armed.why ?? 'hold should unlock');
  assert(game.holdOrder === 'power', `first cycle is GUNS, got ${game.holdOrder}`);
  for (let i = 0; i < 20 * 8; i += 1) game.tick(step);
  const chasing = game.haulers.some((h) => h.cargo?.kind === 'power' || h.path.includes('pad_se'));
  assert(chasing, 'GUNS order should send a hauler at Power within 8s');
}

nestedFarmRouteHub();
openingGateBlocksDeadClicks();
kineticAffordableAfterCore();
cutRailRepairGhosts();
foodDrainIsEarned();
hubDiesWithoutDefense();
starveWithoutFarm();
onlyFarmRailCutMidWave();
holdOrderRushesPower();
const headless = runHeadless({ seed: 7, seconds: 480, demo: true });
assert(headless.win, `demo must win, got ${headless.phase} t=${headless.t}`);
assert(headless.deposits >= 8, `too few deposits ${headless.deposits}`);
assert(headless.minHp < 150, `combat must bite the Hub (minHp ${headless.minHp})`);
assert(headless.sabotages >= 1, `runners must cut rails (sabotages ${headless.sabotages})`);
assert(headless.brownouts >= 1, `demo must fire a brownout event (brownouts ${headless.brownouts}, minPower ${headless.minPower})`);
const stress = [
  { case: 'hub die (no defense)', result: 'pass', notes: 'empty mesa reaches lost_hub' },
  { case: 'food starve (no farm)', result: 'pass', notes: 'farm rail held cut; guns keep Hub up until food 0 for 14s' },
  { case: 'soft-lock early costs', result: 'pass', notes: 'farm+mine+power+routes still afford kinetic (ore>=16)' },
  { case: 'only farm rail cut mid-wave', result: 'pass', notes: 'haulCut, pad_s offline, Route ghosts Hub' },
  { case: 'hold order GUNS', result: 'pass', notes: 'locked before wave 4/L2; then haulers chase Power' },
];
console.log(
  JSON.stringify(
    {
      nested: 'pass',
      openingGate: 'pass',
      affordKinetic: 'pass',
      cutRailGhosts: 'pass',
      foodDrain: 'pass',
      holdOrder: 'pass',
      stress,
      headless,
    },
    null,
    2,
  ),
);
