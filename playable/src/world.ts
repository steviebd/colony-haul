import * as THREE from 'three';
import { PAL } from './audio.ts';
import type { JuiceEvent, Snapshot } from './sim/types.ts';

function mat(color: number, extras?: THREE.MeshStandardMaterialParameters): THREE.MeshStandardMaterial {
  return new THREE.MeshStandardMaterial({ color, roughness: 0.62, metalness: 0.12, flatShading: true, ...extras });
}

function box(w: number, h: number, d: number, color: number, y = 0): THREE.Mesh {
  const m = new THREE.Mesh(new THREE.BoxGeometry(w, h, d), mat(color));
  m.position.y = y + h / 2;
  m.castShadow = true;
  m.receiveShadow = true;
  return m;
}

function cyl(r: number, h: number, color: number, y = 0, segs = 6): THREE.Mesh {
  const m = new THREE.Mesh(new THREE.CylinderGeometry(r, r, h, segs), mat(color));
  m.position.y = y + h / 2;
  m.castShadow = true;
  return m;
}

export class World {
  renderer: THREE.WebGLRenderer;
  scene = new THREE.Scene();
  camera: THREE.OrthographicCamera;
  nodeMarks = new Map<string, THREE.Group>();
  buildingViews = new Map<string, THREE.Group>();
  haulerViews = new Map<string, THREE.Group>();
  enemyViews = new Map<string, THREE.Group>();
  railViews = new Map<string, THREE.Mesh>();
  vfx: { obj: THREE.Object3D; life: number; max: number }[] = [];
  hoverId: string | null = null;
  kick = 0;
  private nodePos = new Map<string, { x: number; z: number }>();

  constructor(canvas: HTMLCanvasElement) {
    this.renderer = new THREE.WebGLRenderer({ canvas, antialias: true, alpha: false });
    this.renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
    this.renderer.shadowMap.enabled = true;
    this.renderer.toneMapping = THREE.ACESFilmicToneMapping;
    this.renderer.setClearColor(PAL.sky, 1);
    this.scene.fog = new THREE.Fog(PAL.fog, 22, 48);
    this.scene.background = new THREE.Color(PAL.sky);
    this.camera = new THREE.OrthographicCamera(-1, 1, 1, -1, 0.1, 80);
    this.camera.position.set(18, 20, 18);
    this.camera.lookAt(0, 0.4, 0);
    this.lights();
    this.mesa();
    this.resize();
  }

  private lights(): void {
    this.scene.add(new THREE.HemisphereLight(0x7ecad0, 0x2a241c, 1.05));
    const sun = new THREE.DirectionalLight(0xffb07a, 1.12);
    sun.position.set(-12, 16, 8);
    sun.castShadow = true;
    sun.shadow.mapSize.set(1024, 1024);
    sun.shadow.camera.left = -22;
    sun.shadow.camera.right = 22;
    sun.shadow.camera.top = 22;
    sun.shadow.camera.bottom = -22;
    this.scene.add(sun);
    const rim = new THREE.DirectionalLight(0x3ad0c8, 0.62);
    rim.position.set(10, 6, -8);
    this.scene.add(rim);
  }

  private mesa(): void {
    const dust = new THREE.Mesh(new THREE.PlaneGeometry(80, 80), mat(PAL.dust, { roughness: 1 }));
    dust.rotation.x = -Math.PI / 2;
    dust.position.y = -0.35;
    dust.receiveShadow = true;
    this.scene.add(dust);
    const top = new THREE.Mesh(new THREE.CylinderGeometry(19.5, 21.5, 0.7, 8), mat(PAL.mesa, { roughness: 0.9 }));
    top.position.y = 0;
    top.receiveShadow = true;
    this.scene.add(top);
    const lip = new THREE.Mesh(new THREE.CylinderGeometry(21.6, 23.2, 1.4, 8), mat(0x4a4038, { roughness: 0.95 }));
    lip.position.y = -0.9;
    this.scene.add(lip);
    for (let i = 0; i < 14; i += 1) {
      const ang = (i / 14) * Math.PI * 2;
      const r = 17 + (i % 3) * 1.4;
      const rock = box(1.4 + (i % 3) * 0.5, 0.7 + (i % 4) * 0.35, 1.1, PAL.rock);
      rock.position.set(Math.cos(ang) * r, 0.1, Math.sin(ang) * r);
      rock.rotation.y = ang;
      this.scene.add(rock);
    }
  }

  resize(): void {
    const w = window.innerWidth;
    const h = window.innerHeight;
    this.renderer.setSize(w, h, false);
    const frustum = 14;
    const aspect = w / h;
    this.camera.left = -frustum * aspect;
    this.camera.right = frustum * aspect;
    this.camera.top = frustum;
    this.camera.bottom = -frustum;
    this.camera.updateProjectionMatrix();
  }

  bindNodes(snapshot: Snapshot): void {
    for (const node of snapshot.nodes) {
      this.nodePos.set(node.id, { x: node.x, z: node.z });
      if (this.nodeMarks.has(node.id)) continue;
      const g = new THREE.Group();
      g.position.set(node.x, 0.38, node.z);
      const ring = new THREE.Mesh(
        new THREE.TorusGeometry(node.kind === 'spawn' ? 0.55 : node.kind === 'hub' ? 1.05 : 0.78, 0.06, 8, 18),
        mat(node.kind === 'spawn' ? PAL.red : node.kind === 'hub' ? PAL.teal : PAL.pad, {
          emissive: node.kind === 'spawn' ? PAL.red : node.kind === 'hub' ? PAL.teal : PAL.amber,
          emissiveIntensity: node.kind === 'pad' ? 0.28 : node.kind === 'hub' ? 0.35 : node.kind === 'spawn' ? 0.45 : 0.22,
        }),
      );
      ring.rotation.x = Math.PI / 2;
      g.add(ring);
      if (node.kind === 'pad' || node.kind === 'choke' || node.kind === 'tower') {
        const padCol = node.kind === 'choke' ? 0xd05a38 : node.kind === 'tower' ? 0x3a8a92 : 0xf0d4a0;
        g.add(box(1.38, 0.12, 1.38, padCol, -0.34));
        const fill = cyl(0.52, 0.05, node.kind === 'choke' ? 0xe87848 : node.kind === 'tower' ? 0x6ad0d8 : 0xffe8bc, -0.22, 8);
        fill.name = 'fill';
        const fillMat = fill.material as THREE.MeshStandardMaterial;
        fillMat.emissive.set(node.kind === 'choke' ? PAL.sabo : node.kind === 'tower' ? PAL.teal : PAL.amber);
        fillMat.emissiveIntensity = 0.35;
        g.add(fill);
      }
      if (node.kind === 'hub') {
        g.add(cyl(1.35, 0.12, PAL.cream, -0.36, 8));
        g.add(tagSprite('HUB', PAL.teal));
      }
      if (node.kind === 'spawn') g.add(tagSprite('RAID', PAL.red));
      this.nodeMarks.set(node.id, g);
      this.scene.add(g);
    }
  }

  pick(clientX: number, clientY: number): string | null {
    const rect = this.renderer.domElement.getBoundingClientRect();
    const cursor = new THREE.Vector3();
    let best: string | null = null;
    let bestD = 120;
    for (const [id, p] of this.nodePos) {
      const y = id === 'hub' ? 1.35 : 0.55;
      cursor.set(p.x, y, p.z).project(this.camera);
      const sx = (cursor.x * 0.5 + 0.5) * rect.width + rect.left;
      const sy = (-cursor.y * 0.5 + 0.5) * rect.height + rect.top;
      const d = Math.hypot(sx - clientX, sy - clientY);
      const limit = id === 'hub' ? 118 : id.startsWith('pad') ? 62 : 52;
      if (d <= limit && d < bestD) {
        bestD = d;
        best = id;
      }
    }
    this.hoverId = best;
    this.renderer.domElement.style.cursor = best ? 'pointer' : 'crosshair';
    return best;
  }

  screenPos(id: string): { x: number; y: number } | null {
    const p = this.nodePos.get(id);
    if (!p) return null;
    const rect = this.renderer.domElement.getBoundingClientRect();
    const v = new THREE.Vector3(p.x, id === 'hub' ? 1.35 : 0.55, p.z).project(this.camera);
    return {
      x: (v.x * 0.5 + 0.5) * rect.width + rect.left,
      y: (-v.y * 0.5 + 0.5) * rect.height + rect.top,
    };
  }

  reset(): void {
    for (const map of [this.buildingViews, this.haulerViews, this.enemyViews]) {
      for (const g of map.values()) this.scene.remove(g);
      map.clear();
    }
    for (const mesh of this.railViews.values()) this.scene.remove(mesh);
    this.railViews.clear();
    for (const fx of this.vfx) this.scene.remove(fx.obj);
    this.vfx = [];
    this.hoverId = null;
  }

  sync(s: Snapshot, events: JuiceEvent[]): void {
    this.bindNodes(s);
    for (const [id, g] of this.nodeMarks) {
      const end = s.routeEnds.includes(id);
      const on = id === s.routeFrom || id === this.hoverId || end;
      const hubHot = id === 'hub' && s.routeFrom !== null && end;
      const occupied = s.buildings.some((b) => b.nodeId === id);
      g.scale.setScalar(hubHot ? 1.55 : on ? 1.28 : 1);
      const ring = g.children[0] as THREE.Mesh | undefined;
      const mat = ring?.material as THREE.MeshStandardMaterial | undefined;
      if (mat) {
        const hot = id === s.routeFrom || hubHot;
        if (hot) {
          mat.emissive.set(PAL.rail);
          mat.emissiveIntensity = 1.1;
        } else if (end) {
          mat.emissive.set(PAL.teal);
          mat.emissiveIntensity = 0.7;
        } else if (id === this.hoverId) {
          mat.emissive.set(PAL.teal);
          mat.emissiveIntensity = 0.35;
        } else if (id === 'hub') {
          mat.emissive.set(PAL.teal);
          mat.emissiveIntensity = 0.55 + 0.3 * Math.abs(Math.sin(s.t * 2.4));
        } else if (id.startsWith('pad')) {
          mat.emissive.set(PAL.amber);
          mat.emissiveIntensity = occupied ? 0.18 : 0.48;
        } else if (id.startsWith('choke') || id.startsWith('tower')) {
          mat.emissive.set(id.startsWith('choke') ? PAL.sabo : PAL.teal);
          mat.emissiveIntensity = occupied ? 0.12 : 0.32;
        } else {
          mat.emissive.set(0x000000);
          mat.emissiveIntensity = 0;
        }
      }
      const fill = g.getObjectByName('fill') as THREE.Mesh | undefined;
      if (fill) {
        fill.visible = !occupied;
      }
    }
    this.syncRails(s);
    this.syncBuildings(s);
    this.syncHaulers(s);
    this.syncEnemies(s);
    for (const ev of events) this.spawnVfx(ev);
    const dt = 1 / 60;
    for (const fx of this.vfx) {
      fx.life -= dt;
      fx.obj.scale.multiplyScalar(1.03);
      const mats = (fx.obj as THREE.Mesh).material as THREE.MeshStandardMaterial | undefined;
      if (mats && 'opacity' in mats) mats.opacity = Math.max(0, fx.life / fx.max);
    }
    this.vfx = this.vfx.filter((fx) => {
      if (fx.life > 0) return true;
      this.scene.remove(fx.obj);
      return false;
    });
  }

  private syncRails(s: Snapshot): void {
    for (const edge of s.edges) {
      let mesh = this.railViews.get(edge.id);
      if (!mesh) {
        mesh = new THREE.Mesh(new THREE.BoxGeometry(1, 0.14, 0.48), mat(PAL.rail, { emissive: PAL.rail, emissiveIntensity: 0.45 }));
        this.railViews.set(edge.id, mesh);
        this.scene.add(mesh);
      }
      const a = this.nodePos.get(edge.a)!;
      const b = this.nodePos.get(edge.b)!;
      const dx = b.x - a.x;
      const dz = b.z - a.z;
      const len = Math.hypot(dx, dz);
      mesh.position.set((a.x + b.x) / 2, 0.42, (a.z + b.z) / 2);
      mesh.scale.set(len, 1, 1);
      mesh.rotation.y = -Math.atan2(dz, dx);
      const live = edge.routed && edge.sabotagedUntil <= s.t;
      const from = s.routeFrom;
      const other = from && (edge.a === from ? edge.b : edge.b === from ? edge.a : null);
      const ghost = Boolean(other && s.routeEnds.includes(other) && !edge.routed);
      mesh.visible = edge.routed || edge.barrier || ghost;
      const m = mesh.material as THREE.MeshStandardMaterial;
      if (edge.barrier && !edge.routed) {
        m.color.set(0x5a281e);
        m.emissive.set(PAL.red);
        m.emissiveIntensity = 0.95;
        mesh.scale.set(len, 6.4, 0.62);
        mesh.position.y = 1.55;
      } else if (edge.sabotagedUntil > s.t) {
        const pulse = 0.7 + 0.7 * Math.abs(Math.sin(s.t * 10));
        m.color.set(PAL.sabo);
        m.emissive.set(PAL.sabo);
        m.emissiveIntensity = pulse;
        mesh.scale.set(len, 1.8, 1.35);
        mesh.position.y = 0.5;
      } else if (live) {
        m.color.set(PAL.rail);
        m.emissive.set(PAL.rail);
        m.emissiveIntensity = 0.72;
      } else if (ghost) {
        m.color.set(PAL.rail);
        m.emissive.set(PAL.rail);
        m.emissiveIntensity = 0.22 + 0.2 * Math.abs(Math.sin(s.t * 6));
        mesh.scale.set(len, 1.35, 1.15);
      }
    }
  }

  private syncBuildings(s: Snapshot): void {
    const live = new Set<string>();
    for (const b of s.buildings) {
      live.add(b.id);
      let g = this.buildingViews.get(b.id);
      if (!g) {
        g = makeBuilding(b.type);
        const tag = b.type === 'farm' ? 'FARM' : b.type === 'mine' ? 'MINE' : b.type === 'power' ? 'PWR' : b.type === 'kinetic' ? 'GUN' : b.type === 'splash' ? 'SPL' : '';
        const col = b.type === 'farm' ? PAL.green : b.type === 'mine' ? PAL.amber : b.type === 'power' ? PAL.blue : b.type === 'kinetic' ? PAL.teal : b.type === 'splash' ? PAL.amber : PAL.cream;
        if (tag) {
          const spr = tagSprite(tag, col);
          spr.position.y = b.type === 'hub' ? 3.6 : 2.35;
          spr.scale.set(2.15, 0.54, 1);
          g.add(spr);
        }
        this.buildingViews.set(b.id, g);
        this.scene.add(g);
      }
      const n = this.nodePos.get(b.nodeId)!;
      g.position.set(n.x, 0.4, n.z);
      const built = b.buildLeft <= 0 ? 1 : 0.35 + (1 - b.buildLeft / 1.6) * 0.5;
      const offline = s.offlineNodeIds.includes(b.nodeId);
      g.scale.setScalar(Math.max(0.4, built) * (offline ? 0.92 : 1));
      g.traverse((obj) => {
        const mesh = obj as THREE.Mesh;
        const mat = mesh.material as THREE.MeshStandardMaterial | undefined;
        if (!mat || !('emissiveIntensity' in mat)) return;
        if (mat.userData.baseEmissive === undefined) mat.userData.baseEmissive = mat.emissiveIntensity;
        if (mat.userData.baseEmissive > 0.05) mat.emissiveIntensity = offline ? 0.12 : mat.userData.baseEmissive;
      });
    }
    for (const [id, g] of this.buildingViews) {
      if (live.has(id)) continue;
      this.scene.remove(g);
      this.buildingViews.delete(id);
    }
  }

  private syncHaulers(s: Snapshot): void {
    const live = new Set<string>();
    for (const h of s.haulers) {
      live.add(h.id);
      let g = this.haulerViews.get(h.id);
      if (!g) {
        g = new THREE.Group();
        g.add(box(0.95, 0.26, 0.58, 0x1c3840, 0));
        const cab = box(0.36, 0.28, 0.4, PAL.teal, 0.26);
        g.add(cab);
        const cargo = cyl(0.28, 0.48, PAL.amber, 0.26, 5);
        cargo.name = 'cargo';
        g.add(cargo);
        this.haulerViews.set(h.id, g);
        this.scene.add(g);
      }
      g.position.set(h.x, 0.48 + Math.sin(s.t * 9 + h.x) * 0.07, h.z);
      if (h.path[0]) {
        const n = this.nodePos.get(h.path[0]);
        if (n) g.rotation.y = Math.atan2(n.x - h.x, n.z - h.z);
      }
      const cargo = g.getObjectByName('cargo') as THREE.Mesh;
      cargo.visible = !!h.cargo;
      const cm = cargo.material as THREE.MeshStandardMaterial;
      if (h.cargo?.kind === 'ore') {
        cm.color.set(PAL.amber);
        cm.emissive.set(PAL.amber);
        cm.emissiveIntensity = 0.85;
      }
      if (h.cargo?.kind === 'food') {
        cm.color.set(PAL.green);
        cm.emissive.set(PAL.green);
        cm.emissiveIntensity = 0.85;
      }
      if (h.cargo?.kind === 'power') {
        cm.color.set(PAL.blue);
        cm.emissive.set(PAL.blue);
        cm.emissiveIntensity = 0.95;
      }
    }
    for (const [id, g] of this.haulerViews) {
      if (live.has(id)) continue;
      this.scene.remove(g);
      this.haulerViews.delete(id);
    }
  }

  private syncEnemies(s: Snapshot): void {
    const live = new Set<string>();
    for (const e of s.enemies) {
      live.add(e.id);
      let g = this.enemyViews.get(e.id);
      if (!g) {
        g = makeEnemy(e.type);
        this.enemyViews.set(e.id, g);
        this.scene.add(g);
      }
      g.position.set(e.x, 0.5, e.z);
      if (e.path[0]) {
        const n = this.nodePos.get(e.path[0]);
        if (n) g.rotation.y = Math.atan2(n.x - e.x, n.z - e.z);
      }
      const pulse = e.flash > 0 ? 1.32 : 1;
      g.scale.setScalar(pulse);
      let bar = g.getObjectByName('hp') as THREE.Mesh | undefined;
      if (!bar) {
        const bg = box(0.85, 0.08, 0.08, 0x220808, 1.45);
        bg.name = 'hpbg';
        bar = box(0.82, 0.09, 0.09, PAL.red, 1.46);
        bar.name = 'hp';
        g.add(bg);
        g.add(bar);
      }
      bar.scale.x = Math.max(0.08, e.hp / e.maxHp);
      bar.position.x = (bar.scale.x - 1) * 0.34;
      const barMat = bar.material as THREE.MeshStandardMaterial | undefined;
      if (barMat) {
        if (e.type === 'brute') barMat.color.set(0xff66aa);
        else if (e.type === 'runner') barMat.color.set(0xffaa33);
        else barMat.color.set(PAL.red);
      }
      const bg = g.getObjectByName('hpbg') as THREE.Mesh | undefined;
      const face = g.quaternion.clone().invert().multiply(this.camera.quaternion);
      bar.quaternion.copy(face);
      if (bg) bg.quaternion.copy(face);
    }
    for (const [id, g] of this.enemyViews) {
      if (live.has(id)) continue;
      this.scene.remove(g);
      this.enemyViews.delete(id);
    }
  }

  private spawnVfx(ev: JuiceEvent): void {
    if (ev.kind === 'hit' && ev.nodeId === 'hub') this.kick = 1.6;
    if (ev.kind === 'deposit' || ev.kind === 'death' || ev.kind === 'build' || ev.kind === 'splash' || ev.kind === 'sabotage' || ev.kind === 'brownout' || ev.kind === 'route') {
      const color = ev.kind === 'deposit'
        ? ev.resource === 'food' ? PAL.green : ev.resource === 'power' ? PAL.blue : PAL.amber
        : ev.kind === 'splash' ? 0xffaa55
          : ev.kind === 'sabotage' || ev.kind === 'brownout' ? PAL.sabo
            : ev.kind === 'route' ? PAL.rail
              : PAL.teal;
      const ring = new THREE.Mesh(
        new THREE.TorusGeometry(ev.kind === 'deposit' ? 0.85 : ev.kind === 'sabotage' ? 0.7 : 0.45, 0.07, 8, 18),
        mat(color, { emissive: color, emissiveIntensity: 1.2, transparent: true, opacity: 0.95 }),
      );
      ring.rotation.x = Math.PI / 2;
      ring.position.set(ev.x ?? 0, 0.75, ev.z ?? 0);
      this.scene.add(ring);
      this.vfx.push({ obj: ring, life: ev.kind === 'deposit' || ev.kind === 'sabotage' ? 0.85 : 0.5, max: ev.kind === 'deposit' || ev.kind === 'sabotage' ? 0.85 : 0.5 });
    }
    if (ev.kind === 'shot' && ev.fromX !== undefined && ev.toX !== undefined) {
      const dx = ev.toX - ev.fromX;
      const dz = (ev.toZ ?? 0) - (ev.fromZ ?? 0);
      const len = Math.hypot(dx, dz);
      const bolt = new THREE.Mesh(
        new THREE.BoxGeometry(Math.max(0.4, len), 0.16, 0.16),
        mat(PAL.teal, { emissive: PAL.teal, emissiveIntensity: 2.6, transparent: true, opacity: 1 }),
      );
      bolt.position.set((ev.fromX + ev.toX) / 2, 1.25, ((ev.fromZ ?? 0) + (ev.toZ ?? 0)) / 2);
      bolt.rotation.y = -Math.atan2(dz, dx);
      this.scene.add(bolt);
      this.vfx.push({ obj: bolt, life: 0.32, max: 0.32 });
      const muzzle = new THREE.Mesh(
        new THREE.SphereGeometry(0.28, 8, 8),
        mat(PAL.teal, { emissive: PAL.teal, emissiveIntensity: 2.4, transparent: true, opacity: 1 }),
      );
      muzzle.position.set(ev.fromX, 1.15, ev.fromZ ?? 0);
      this.scene.add(muzzle);
      this.vfx.push({ obj: muzzle, life: 0.18, max: 0.18 });
      const disc = new THREE.Mesh(
        new THREE.TorusGeometry(0.55, 0.05, 8, 16),
        mat(PAL.teal, { emissive: PAL.teal, emissiveIntensity: 1.4, transparent: true, opacity: 0.85 }),
      );
      disc.rotation.x = Math.PI / 2;
      disc.position.set(ev.fromX, 0.55, ev.fromZ ?? 0);
      this.scene.add(disc);
      this.vfx.push({ obj: disc, life: 0.2, max: 0.2 });
    }
    if (ev.kind === 'hit' && ev.x !== undefined) {
      const flash = new THREE.Mesh(
        new THREE.SphereGeometry(0.32, 6, 6),
        mat(0xffffff, { emissive: 0xffffff, emissiveIntensity: 1.6, transparent: true, opacity: 0.95 }),
      );
      flash.position.set(ev.x, 0.95, ev.z ?? 0);
      this.scene.add(flash);
      this.vfx.push({ obj: flash, life: 0.16, max: 0.16 });
    }
  }

  render(): void {
    if (this.kick > 0.02) {
      this.camera.position.set(18 + this.kick * 0.18, 20, 18 + this.kick * 0.1);
      this.kick *= 0.78;
    } else {
      this.kick = 0;
      this.camera.position.set(18, 20, 18);
    }
    this.renderer.render(this.scene, this.camera);
  }
}

function tagSprite(text: string, color: number): THREE.Sprite {
  const c = document.createElement('canvas');
  c.width = 256;
  c.height = 64;
  const ctx = c.getContext('2d')!;
  ctx.clearRect(0, 0, 256, 64);
  ctx.font = '700 36px Trebuchet MS, sans-serif';
  ctx.textAlign = 'center';
  ctx.fillStyle = `#${color.toString(16).padStart(6, '0')}`;
  ctx.strokeStyle = '#0c2432';
  ctx.lineWidth = 8;
  ctx.strokeText(text, 128, 46);
  ctx.fillText(text, 128, 46);
  const tex = new THREE.CanvasTexture(c);
  const spr = new THREE.Sprite(new THREE.SpriteMaterial({ map: tex, transparent: true, depthTest: false }));
  spr.position.set(0, 2.4, 0);
  spr.scale.set(2.4, 0.6, 1);
  return spr;
}

function makeBuilding(type: string): THREE.Group {
  const g = new THREE.Group();
  switch (type) {
    case 'hub': {
      g.add(cyl(1.22, 0.58, PAL.cream, 0, 8));
      g.add(cyl(0.78, 1.05, PAL.hub, 0.58, 8));
      g.add(cyl(0.14, 1.7, PAL.teal, 1.5, 5));
      const dish = new THREE.Mesh(new THREE.ConeGeometry(0.6, 0.28, 8), mat(PAL.teal, { emissive: PAL.teal, emissiveIntensity: 0.7 }));
      dish.position.y = 3.35;
      g.add(dish);
      break;
    }
    case 'depot': {
      g.add(box(1.7, 0.72, 1.15, 0x2a5560));
      g.add(box(0.95, 0.22, 1.45, PAL.teal, 0.72));
      break;
    }
    case 'mine': {
      g.add(box(1.15, 0.28, 1.15, 0x6a3a18));
      const crystal = new THREE.Mesh(new THREE.ConeGeometry(0.4, 1.2, 5), mat(PAL.amber, { emissive: PAL.amber, emissiveIntensity: 0.75 }));
      crystal.position.y = 0.9;
      g.add(crystal);
      break;
    }
    case 'farm': {
      g.add(box(1.38, 0.14, 1.38, 0x3a6a28));
      g.add(box(0.38, 0.52, 0.38, PAL.green, 0.14));
      g.add(box(0.38, 0.4, 0.38, 0x98e070, 0.14)).position.x = 0.42;
      break;
    }
    case 'power': {
      g.add(cyl(0.38, 1.5, 0x1a3a58, 0, 6));
      const orb = new THREE.Mesh(new THREE.SphereGeometry(0.32, 8, 8), mat(PAL.blue, { emissive: PAL.blue, emissiveIntensity: 1.1 }));
      orb.position.y = 1.68;
      g.add(orb);
      break;
    }
    case 'kinetic': {
      g.add(cyl(0.62, 0.55, 0x1a3844, 0, 6));
      const collar = cyl(0.38, 0.28, PAL.teal, 0.55, 6);
      g.add(collar);
      const barrel = box(0.28, 0.28, 1.55, PAL.cream, 0.62);
      barrel.position.z = 0.28;
      g.add(barrel);
      const tip = box(0.16, 0.16, 0.28, PAL.teal, 0.68);
      tip.position.z = 1.05;
      g.add(tip);
      break;
    }
    case 'splash': {
      g.add(cyl(0.7, 0.48, 0x1a3844, 0, 6));
      const dish = new THREE.Mesh(new THREE.SphereGeometry(0.82, 8, 6, 0, Math.PI * 2, 0, Math.PI / 2), mat(0xffb060, { emissive: 0xffb060, emissiveIntensity: 0.7 }));
      dish.position.y = 0.72;
      g.add(dish);
      break;
    }
    default:
      g.add(box(0.8, 0.5, 0.8, 0x888888));
  }
  return g;
}

function makeEnemy(type: string): THREE.Group {
  const g = new THREE.Group();
  if (type === 'brute') {
    g.add(box(1.85, 1.22, 1.35, 0x3a0814));
    const shoulders = box(2.35, 0.48, 0.82, 0x1a0408, 0.82);
    g.add(shoulders);
    g.add(box(1.05, 0.58, 0.88, 0xff2255, 1.22));
    const hornL = box(0.22, 1.05, 0.22, 0xfff0c8, 1.55);
    hornL.position.x = -0.55;
    hornL.rotation.z = 0.42;
    g.add(hornL);
    const hornR = box(0.22, 1.05, 0.22, 0xfff0c8, 1.55);
    hornR.position.x = 0.55;
    hornR.rotation.z = -0.42;
    g.add(hornR);
    g.add(box(0.48, 0.52, 0.48, 0x2a0810, 0)).position.x = -0.55;
    g.add(box(0.48, 0.52, 0.48, 0x2a0810, 0)).position.x = 0.55;
  } else if (type === 'runner') {
    const body = new THREE.Mesh(
      new THREE.ConeGeometry(0.34, 2.55, 3),
      mat(0xff9900, { emissive: 0xff7700, emissiveIntensity: 1.35 }),
    );
    body.rotation.x = Math.PI / 2;
    body.position.set(0, 0.42, 0.22);
    g.add(body);
    const finL = box(0.1, 0.72, 0.72, 0xffee66, 0.18);
    finL.position.x = -0.38;
    finL.rotation.z = 0.62;
    g.add(finL);
    const finR = box(0.1, 0.72, 0.72, 0xffee66, 0.18);
    finR.position.x = 0.38;
    finR.rotation.z = -0.62;
    g.add(finR);
    const stripe = box(0.16, 0.16, 0.9, 0xfff6c0, 0.7);
    stripe.position.z = -0.2;
    g.add(stripe);
    const tail = new THREE.Mesh(
      new THREE.SphereGeometry(0.16, 6, 6),
      mat(0xffcc44, { emissive: 0xffaa22, emissiveIntensity: 1.4 }),
    );
    tail.position.set(0, 0.42, -1.05);
    g.add(tail);
  } else {
    g.add(cyl(0.38, 0.62, 0xb82028, 0, 6));
    g.add(cyl(0.28, 0.42, 0xff4a3a, 0.62, 6));
    const visor = box(0.44, 0.14, 0.2, 0xfff0a0, 0.92);
    visor.position.z = 0.2;
    g.add(visor);
    g.add(box(0.14, 0.34, 0.14, 0x7a1818, 0.22)).position.x = -0.36;
    g.add(box(0.14, 0.34, 0.14, 0x7a1818, 0.22)).position.x = 0.36;
    const rifle = box(0.11, 0.11, 0.85, 0x2a2828, 0.62);
    rifle.position.set(0.42, 0, 0.28);
    g.add(rifle);
  }
  return g;
}
