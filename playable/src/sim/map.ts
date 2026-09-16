import type { SimEdge, SimNode } from './types.ts';

function n(id: string, x: number, z: number, kind: SimNode['kind']): SimNode {
  return { id, x, z, kind };
}

function e(a: string, b: string): SimEdge {
  const id = a < b ? `${a}~${b}` : `${b}~${a}`;
  return { id, a, b, routed: false, sabotagedUntil: 0, barrier: false };
}

export function createMesa(): { nodes: SimNode[]; edges: SimEdge[] } {
  const nodes: SimNode[] = [
    n('hub', 0, 0, 'hub'),
    n('depot', 2.5, -2.3, 'depot'),
    n('pad_n', 0, 6.1, 'pad'),
    n('pad_ne', 5.3, 3.05, 'pad'),
    n('pad_se', 5.3, -3.05, 'pad'),
    n('pad_s', 0, -6.1, 'pad'),
    n('pad_sw', -5.3, -3.05, 'pad'),
    n('pad_nw', -5.3, 3.05, 'pad'),
    n('choke_n', 0, 11.4, 'choke'),
    n('choke_e', 11.1, 0.2, 'choke'),
    n('choke_w', -11.1, 0.2, 'choke'),
    n('tower_ne', 8.2, 8.0, 'tower'),
    n('tower_sw', -8.2, -7.2, 'tower'),
    n('spawn_n', 0, 16.6, 'spawn'),
    n('spawn_e', 16.4, 1.0, 'spawn'),
    n('spawn_w', -16.4, 1.0, 'spawn'),
  ];

  const links: [string, string][] = [
    ['hub', 'depot'],
    ['hub', 'pad_n'],
    ['hub', 'pad_ne'],
    ['hub', 'pad_se'],
    ['hub', 'pad_s'],
    ['hub', 'pad_sw'],
    ['hub', 'pad_nw'],
    ['pad_n', 'pad_ne'],
    ['pad_ne', 'pad_se'],
    ['pad_se', 'pad_s'],
    ['pad_s', 'pad_sw'],
    ['pad_sw', 'pad_nw'],
    ['pad_nw', 'pad_n'],
    ['pad_n', 'choke_n'],
    ['pad_ne', 'choke_n'],
    ['pad_nw', 'choke_n'],
    ['choke_n', 'spawn_n'],
    ['pad_ne', 'choke_e'],
    ['pad_se', 'choke_e'],
    ['choke_e', 'spawn_e'],
    ['pad_nw', 'choke_w'],
    ['pad_sw', 'choke_w'],
    ['choke_w', 'spawn_w'],
    ['tower_ne', 'pad_ne'],
    ['tower_ne', 'choke_n'],
    ['tower_ne', 'choke_e'],
    ['tower_sw', 'pad_sw'],
    ['tower_sw', 'pad_s'],
    ['tower_sw', 'choke_w'],
  ];

  const edges = links.map(([a, b]) => e(a, b));
  const hubDepot = edges.find((edge) => edge.id === 'depot~hub' || edge.id === 'hub~depot');
  if (hubDepot) hubDepot.routed = true;

  return { nodes, edges };
}

export function otherEnd(edge: SimEdge, nodeId: string): string {
  return edge.a === nodeId ? edge.b : edge.a;
}
