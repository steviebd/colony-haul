#!/usr/bin/env node
/**
 * Stepped canvas capture: pause rAF, tick+render N times, screenshot each frame.
 * Avoids CDP screencast + page.evaluate polling, which collapsed to ~3 fps.
 */
import fs from 'node:fs';
import { spawn } from 'node:child_process';
import puppeteer from 'puppeteer-core';

const out = process.argv[2] || '/workspace/artifacts/colony-haul-run.webm';
const fps = Number(process.argv[3] || 24);
const seconds = Number(process.argv[4] || 56);
const simRate = Number(process.argv[5] || 2);
const dir = process.env.HAUL_FRAME_DIR || '/tmp/haul-step-frames';
const chrome = process.env.CHROME || '/usr/bin/google-chrome-stable';
const url = process.env.HAUL_URL || 'http://127.0.0.1:43147/?demo=1&play=1';

fs.rmSync(dir, { recursive: true, force: true });
fs.mkdirSync(dir, { recursive: true });
fs.mkdirSync('/workspace/artifacts', { recursive: true });

const browser = await puppeteer.launch({
  executablePath: chrome,
  headless: true,
  args: [
    '--no-sandbox',
    '--disable-dev-shm-usage',
    '--use-gl=angle',
    '--use-angle=swiftshader',
    '--enable-unsafe-swiftshader',
    '--enable-webgl',
    '--ignore-gpu-blocklist',
    '--window-size=1280,720',
    '--hide-scrollbars',
  ],
});
const page = await browser.newPage();
await page.setViewport({ width: 1280, height: 720, deviceScaleFactor: 1 });
await page.goto(url, { waitUntil: 'networkidle0', timeout: 30000 });
await page.waitForFunction(() => Boolean(window.__colony?.stepFrame), { timeout: 15000 });
await page.evaluate(() => window.__colony.pauseLoop());
await new Promise((r) => setTimeout(r, 250));
await page.screenshot({ path: '/workspace/artifacts/demo-open.png', type: 'png' });

const framesWanted = Math.round(fps * seconds);
const dt = simRate / fps;
let n = 0;
let sawFarmRail = false;
let sawWave = false;
let sawCut = false;
let sawRouteHeld = false;
let sawBrownout = false;
let sawL2 = false;
let sawRepair = false;
let cutSeen = false;
let sawBarrier = false;
let last = {
  t: 0,
  phase: 'playing',
  wave: 0,
  live: 0,
  haulCut: false,
  hubLevel: 1,
  brownout: false,
  tool: 'farm',
  splashLocked: true,
  barrier: false,
};
const t0 = Date.now();

while (n < framesWanted) {
  last = await page.evaluate((stepDt) => window.__colony.stepFrame(stepDt), dt);
  const file = `${dir}/f${String(n).padStart(5, '0')}.jpg`;
  await page.screenshot({ path: file, type: 'jpeg', quality: 58 });
  n += 1;
  if (!sawFarmRail && last.t > 8 && last.t < 22) {
    await page.screenshot({ path: '/workspace/artifacts/demo-farm-rail.png', type: 'png' });
    sawFarmRail = true;
  }
  if (!sawWave && last.wave >= 1 && last.live > 0) {
    await page.screenshot({ path: '/workspace/artifacts/run-wave1.png', type: 'png' });
    await page.screenshot({ path: '/workspace/artifacts/demo-wave1.png', type: 'png' });
    sawWave = true;
  }
  if (last.haulCut) cutSeen = true;
  if (!sawCut && last.haulCut) {
    await page.screenshot({ path: '/workspace/artifacts/run-wave2.png', type: 'png' });
    await page.screenshot({ path: '/workspace/artifacts/demo-haul-cut.png', type: 'png' });
    sawCut = true;
  }
  if (!sawRouteHeld && last.haulCut && last.tool === 'route') {
    await page.screenshot({ path: '/workspace/artifacts/demo-route-held.png', type: 'png' });
    sawRouteHeld = true;
  }
  if (!sawBarrier && last.tool === 'barrier' && last.t > 50) {
    await page.screenshot({ path: '/workspace/artifacts/demo-barrier.png', type: 'png' });
    sawBarrier = true;
  }
  if (!sawBrownout && last.brownout) {
    await page.screenshot({ path: '/workspace/artifacts/demo-brownout.png', type: 'png' });
    sawBrownout = true;
  }
  if (!sawL2 && last.hubLevel >= 2) {
    await page.screenshot({ path: '/workspace/artifacts/demo-hub-l2.png', type: 'png' });
    sawL2 = true;
  }
  if (!sawRepair && cutSeen && !last.haulCut) {
    await page.screenshot({ path: '/workspace/artifacts/demo-repair.png', type: 'png' });
    sawRepair = true;
  }
  if (last.phase !== 'playing') break;
}

await page.screenshot({ path: '/workspace/artifacts/run-mid.png', type: 'png' });
const beats = { sawFarmRail, sawWave, sawCut, sawRouteHeld, sawBrownout, sawL2, sawRepair, sawBarrier };
await browser.close();

const frames = fs.readdirSync(dir).filter((f) => f.endsWith('.jpg')).sort();
const elapsed = (Date.now() - t0) / 1000;
console.log(JSON.stringify({ frames: frames.length, elapsed, last, fps, seconds, simRate, beats }));
if (frames.length < Math.min(200, framesWanted * 0.5)) process.exit(2);

const duration = frames.length / fps;
await new Promise((resolve, reject) => {
  const ff = spawn(
    'ffmpeg',
    [
      '-y',
      '-framerate',
      String(fps),
      '-i',
      `${dir}/f%05d.jpg`,
      '-c:v',
      'libvpx-vp9',
      '-b:v',
      '3200k',
      '-row-mt',
      '1',
      '-pix_fmt',
      'yuv420p',
      out,
    ],
    { stdio: 'inherit' },
  );
  ff.on('close', (code) => (code === 0 ? resolve() : reject(new Error('ffmpeg ' + code))));
});
console.log(JSON.stringify({ out, bytes: fs.statSync(out).size, fps, duration, frames: frames.length, last, beats }));
