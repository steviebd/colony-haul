#!/usr/bin/env node
/**
 * UI nested playtest: Farm tray → pad → Route stays armed → Hub lays rail → hauler deposits.
 * Run from a folder that has puppeteer-core (e.g. /tmp/record-haul).
 */
import fs from 'node:fs';
import puppeteer from 'puppeteer-core';

const shots = '/workspace/artifacts';
fs.mkdirSync(shots, { recursive: true });

const browser = await puppeteer.launch({
  executablePath: '/usr/bin/google-chrome-stable',
  headless: true,
  args: [
    '--no-sandbox',
    '--disable-dev-shm-usage',
    '--use-gl=angle',
    '--use-angle=swiftshader',
    '--enable-unsafe-swiftshader',
    '--enable-webgl',
    '--ignore-gpu-blocklist',
    '--window-size=1400,900',
  ],
});
const page = await browser.newPage();
page.setDefaultTimeout(20000);
await page.setViewport({ width: 1400, height: 900, deviceScaleFactor: 1 });
await page.goto('http://127.0.0.1:43147/?play=1', { waitUntil: 'networkidle0' });
await page.waitForFunction(() => Boolean(window.__colony));
await new Promise((r) => setTimeout(r, 400));
await page.screenshot({ path: `${shots}/mesa-open.png` });

async function clickNode(id) {
  const pos = await page.evaluate((nid) => window.__colony.world.screenPos(nid), id);
  if (!pos) throw new Error('no screen pos ' + id);
  await page.mouse.click(pos.x, pos.y);
  await new Promise((r) => setTimeout(r, 220));
}

const steps = [];

await page.click('button[data-tool="farm"]');
await clickNode('pad_s');
const afterFarm = await page.evaluate(() => {
  const s = window.__colony.game.snapshot();
  return { tool: s.selectedTool, routeFrom: s.routeFrom, hint: s.hint };
});
steps.push({ step: 'Farm tray → pad_s auto-arm Route', pass: afterFarm.tool === 'route' && afterFarm.routeFrom === 'pad_s', afterFarm });

await page.click('button[data-tool="route"]');
const afterTray = await page.evaluate(() => {
  const s = window.__colony.game.snapshot();
  return { tool: s.selectedTool, routeFrom: s.routeFrom };
});
steps.push({ step: 're-click Route tray keeps armed pad', pass: afterTray.tool === 'route' && afterTray.routeFrom === 'pad_s', afterTray });

await page.click('button[data-tool="kinetic"]');
const afterDead = await page.evaluate(() => {
  const s = window.__colony.game.snapshot();
  return { tool: s.selectedTool, routeFrom: s.routeFrom };
});
steps.push({ step: 'opening gate blocks Kinetic dead-click', pass: afterDead.tool === 'route' && afterDead.routeFrom === 'pad_s', afterDead });

await clickNode('hub');
const afterRoute = await page.evaluate(() => {
  const g = window.__colony.game;
  const edge = g.edgeBetween('pad_s', 'hub');
  const s = g.snapshot();
  return {
    routed: Boolean(edge?.routed),
    live: Boolean(edge?.routed && edge.sabotagedUntil <= g.t),
    farm: [...g.buildings.values()].some((b) => b.type === 'farm'),
    tool: s.selectedTool,
  };
});
steps.push({ step: 'Hub click lays live mag-rail', pass: afterRoute.routed === true, afterRoute });
await page.screenshot({ path: `${shots}/route-fix.png` });

let deposit = null;
let haulerMoved = false;
const startPos = await page.evaluate(() => {
  const h = window.__colony.game.haulers[0];
  return { x: h.x, z: h.z, id: h.id };
});
for (let i = 0; i < 80; i += 1) {
  await new Promise((r) => setTimeout(r, 250));
  const snap = await page.evaluate((sx, sz) => {
    const g = window.__colony.game;
    const h = g.haulers[0];
    const moved = Math.hypot(h.x - sx, h.z - sz) > 0.4;
    const deposits = g.deliveredWindow.length;
    return { moved, deposits, cargo: h.cargo, x: h.x, z: h.z, t: g.t };
  }, startPos.x, startPos.z);
  if (snap.moved) haulerMoved = true;
  if (snap.deposits > 0) {
    deposit = snap;
    break;
  }
}
steps.push({ step: 'hauler moves on live rail', pass: haulerMoved, startPos });
steps.push({ step: 'deposit at Hub', pass: Boolean(deposit && deposit.deposits > 0), deposit });
await page.screenshot({ path: `${shots}/haul-deposit.png` });

const splash = await page.evaluate(() => {
  const btn = document.querySelector('button[data-tool="splash"]');
  const s = window.__colony.game.snapshot();
  return {
    disabled: btn instanceof HTMLButtonElement ? btn.disabled : false,
    lockedClass: btn?.classList.contains('locked') ?? false,
    label: btn?.querySelector('small')?.textContent ?? '',
    hubLevel: s.hubLevel,
  };
});
steps.push({
  step: 'Splash locked until Hub L2 with reason',
  pass: splash.hubLevel < 2 && splash.disabled && splash.lockedClass && /Hub Level 2/i.test(splash.label),
  splash,
});

const failed = steps.filter((s) => !s.pass);
console.log(JSON.stringify({ steps, failed: failed.map((s) => s.step) }, null, 2));
await browser.close();
if (failed.length) process.exit(2);
