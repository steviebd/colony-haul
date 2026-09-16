import { runHeadless } from './demo.ts';

const result = runHeadless({ seed: 7, seconds: 480, demo: true });
console.log(JSON.stringify(result, null, 2));
if (result.deposits < 8) {
  console.error('too few deposits; transport is not alive');
  process.exit(2);
}
if (result.waveIndex < 2) {
  console.error('waves never escalated');
  process.exit(3);
}
if (result.phase === 'playing') {
  console.error('match did not finish inside the window');
  process.exit(4);
}
