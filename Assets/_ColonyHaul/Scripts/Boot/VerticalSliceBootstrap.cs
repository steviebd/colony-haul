using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColonyHaul
{
    public sealed class VerticalSliceBootstrap : MonoBehaviour
    {
        public bool AutoDemo;
        public static bool ForceDemo;

        GameSim _game;
        DemoPilot _demo;
        SliceHud _hud;
        SliceJuice _juice;
        readonly Dictionary<string, Transform> _nodes = new Dictionary<string, Transform>();
        readonly Dictionary<string, Transform> _buildings = new Dictionary<string, Transform>();
        readonly Dictionary<string, Vector3> _buildingScale = new Dictionary<string, Vector3>();
        readonly Dictionary<string, Transform> _haulers = new Dictionary<string, Transform>();
        readonly Dictionary<string, Transform> _enemies = new Dictionary<string, Transform>();
        readonly Dictionary<string, Transform> _rails = new Dictionary<string, Transform>();
        readonly Dictionary<string, Transform> _barriers = new Dictionary<string, Transform>();
        readonly Dictionary<string, Transform> _rings = new Dictionary<string, Transform>();
        readonly Dictionary<string, Transform> _ghosts = new Dictionary<string, Transform>();
        readonly Dictionary<string, Transform> _buffers = new Dictionary<string, Transform>();
        readonly Dictionary<string, LineRenderer> _trails = new Dictionary<string, LineRenderer>();
        Transform _root;
        Camera _cam;
        float _acc;
        float _hubFlash;
        bool _pendingRestart;
        bool _pendingDemo;
        bool _pendingManual;
        const float Step = 1f / 20f;
        int _seed = 7;

        void Awake()
        {
            if (ForceDemo)
            {
                AutoDemo = true;
                ForceDemo = false;
            }
            EnsureCamera();
            _cam = Camera.main;
            _root = new GameObject("MesaRoot").transform;
            MesaView.BuildStage(_root, _cam);
            _hud = new SliceHud { ShowBoot = !AutoDemo && !Application.isBatchMode };
            _juice = new SliceJuice(_root, _cam);
            BootMatch();
        }

        static void EnsureCamera()
        {
            if (Camera.main != null) return;
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
        }

        void BootMatch()
        {
            _game = new GameSim(_seed);
            _demo = AutoDemo || Application.isBatchMode ? new DemoPilot() : null;
            foreach (var n in _game.Nodes.Values)
                _nodes[n.Id] = MesaView.Marker(_root, n);
            SyncView(true);
        }

        void Restart(bool demo)
        {
            AutoDemo = demo;
            foreach (Transform child in _root)
            {
                if (child.name == "Sun" || child.name == "Fill" || child.name == "Mesa" || child.name == "Cliff" || child.name == "HubRing")
                    continue;
                if (child.name == "tracer" || child.name == "pip" || child.name == "ring" || child.name == "ghost" || child.name == "buffer" || child.name == "trail") continue;
            }
            ClearMap(_buildings);
            ClearMap(_haulers);
            ClearMap(_enemies);
            ClearMap(_rails);
            ClearMap(_barriers);
            ClearMap(_rings);
            ClearMap(_ghosts);
            ClearMap(_buffers);
            ClearMap(_nodes);
            ClearTrails();
            _hubFlash = 0f;
            _buildingScale.Clear();
            _acc = 0f;
            BootMatch();
        }

        static void ClearMap(Dictionary<string, Transform> map)
        {
            foreach (var t in map.Values)
                if (t != null) Destroy(t.gameObject);
            map.Clear();
        }

        void Update()
        {
            if (_pendingRestart)
            {
                _pendingRestart = false;
                Restart(false);
            }
            if (_pendingDemo)
            {
                _pendingDemo = false;
                _demo = new DemoPilot();
            }
            if (_pendingManual)
            {
                _pendingManual = false;
                _demo = null;
            }
            if (_game.Phase == Phase.Playing)
            {
                _acc += Time.deltaTime;
                while (_acc >= Step)
                {
                    _demo?.Step(_game, Step);
                    _game.Tick(Step);
                    _acc -= Step;
                }
            }
            HandleInput();
            var events = _game.DrainEvents();
            foreach (var ev in events) Banner(ev);
            _juice.Tick(_cam, events);
            _hubFlash = Mathf.Max(0f, _hubFlash - Time.deltaTime);
            SyncView(false);
        }

        void Banner(SimEvent ev)
        {
            switch (ev.Kind)
            {
                case SimEventKind.Brownout:
                    _hud.Flash("Brownout — towers dry", 2.2f);
                    break;
                case SimEventKind.Sabotage:
                    _hud.Flash("HAUL CUT — runner sparked a mag-rail", 1.8f);
                    break;
                case SimEventKind.Barrier:
                    _hud.Flash("Barrier up — spawn approach slowed", 1.8f);
                    break;
                case SimEventKind.Wave:
                    _hud.Flash("Wave " + ev.Wave + " inbound", 1.6f);
                    break;
                case SimEventKind.Upgrade:
                    if (_game.HubLevel >= 2) _hud.Flash("Hub Level 2 — Splash unlocked", 2f);
                    break;
                case SimEventKind.Shot:
                case SimEventKind.Splash:
                case SimEventKind.Deposit:
                    break;
                case SimEventKind.Hit:
                    if (ev.NodeId == "hub") _hubFlash = 0.4f;
                    break;
                case SimEventKind.Death:
                case SimEventKind.WarnFood:
                case SimEventKind.Build:
                case SimEventKind.Win:
                case SimEventKind.Lose:
                case SimEventKind.Route:
                    break;
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(ev.Kind), ev.Kind, null);
            }
        }

        void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) _game.SetTool(Tool.Farm);
            if (Input.GetKeyDown(KeyCode.Alpha2)) _game.SetTool(Tool.Mine);
            if (Input.GetKeyDown(KeyCode.Alpha3)) _game.SetTool(Tool.Power);
            if (Input.GetKeyDown(KeyCode.Alpha4)) _game.SetTool(Tool.Route);
            if (Input.GetKeyDown(KeyCode.Alpha5)) _game.SetTool(Tool.Kinetic);
            if (Input.GetKeyDown(KeyCode.Alpha6)) _game.SetTool(Tool.Splash);
            if (Input.GetKeyDown(KeyCode.Alpha7)) _game.SetTool(Tool.Barrier);
            if (Input.GetKeyDown(KeyCode.U)) _game.TryUpgrade(out _);
            if (Input.GetKeyDown(KeyCode.D)) _demo = new DemoPilot();
            if (Input.GetKeyDown(KeyCode.R)) Restart(false);
            if (Input.mousePosition.x < 236f) return;
            if (Input.mousePosition.y > Screen.height - 86f) return;
            if (!Input.GetMouseButtonDown(0) || _cam == null) return;
            var ray = _cam.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit, 80f)) return;
            var id = Nearest(hit.point);
            if (id != null) _game.ClickNode(id, out _);
        }

        string Nearest(Vector3 p)
        {
            string best = null;
            var bestD = 1.45f;
            foreach (var n in _game.Nodes.Values)
            {
                var d = Vector2.Distance(new Vector2(p.x, p.z), new Vector2(n.X, n.Z));
                if (d < bestD) { bestD = d; best = n.Id; }
            }
            return best;
        }

        void SyncView(bool _)
        {
            var gate = _game.OpeningGate();
            var pulse = 0.92f + 0.08f * Mathf.Sin(Time.time * 5f);
            var cut = _game.ActiveCut();
            foreach (var n in _game.Nodes.Values)
            {
                if (!_nodes.TryGetValue(n.Id, out var mark)) continue;
                var farmGlow = gate == "farm" && n.Kind == NodeKind.Pad && n.Id == "pad_s";
                var hubGlow = gate == "route" && n.Id == "hub";
                var routeGlow = _game.RouteFrom == n.Id || (cut != null && (cut.A == n.Id || cut.B == n.Id));
                var inbound = n.Kind == NodeKind.Spawn &&
                    (_game.IncomingAt(n.Id) > 0 ||
                     (Array.IndexOf(_game.NextWaveSpawns(), n.Id) >= 0 && _game.NextWaveIn < 10f));
                var hotLane = _game.HottestLane();
                var chokeHot = n.Kind == NodeKind.Choke &&
                    ((n.Id == "choke_e" && hotLane == "east") ||
                     (n.Id == "choke_n" && hotLane == "north") ||
                     (n.Id == "choke_w" && hotLane == "west"));
                var near = n.Kind == NodeKind.Choke ? _game.EnemiesNear(n.Id, 4.8f) : 0;
                Color c;
                if (inbound) c = MesaView.Spawn * pulse;
                else if (n.Kind == NodeKind.Spawn) c = MesaView.Spawn;
                else if (farmGlow) c = MesaView.PadFarm * pulse;
                else if (hubGlow || routeGlow) c = MesaView.PadRoute * pulse;
                else if (n.Kind == NodeKind.Choke && (chokeHot || near > 0))
                    c = Color.Lerp(new Color(0.62f, 0.52f, 0.4f), MesaView.Spawn * pulse,
                        Mathf.Clamp01(near / 4f + (chokeHot ? 0.35f : 0f)));
                else if (n.Kind == NodeKind.Hub) c = new Color(0.9f, 0.78f, 0.58f);
                else c = MesaView.PadIdle;
                MesaView.Tint(mark.gameObject, c);
                if (n.Id == "pad_s")
                    MesaView.SetLabel(mark, farmGlow ? "1 FARM" : "PAD");
                else if (n.Id == "hub")
                    MesaView.SetLabel(mark, hubGlow ? "2 HUB" : "HUB");
                else if (n.Kind == NodeKind.Spawn)
                    MesaView.SetLabel(mark, inbound ? "IN " + _game.IncomingAt(n.Id) : "RAID");
                else if (n.Kind == NodeKind.Choke)
                    MesaView.SetLabel(mark, ChokeLabel(n.Id, near, chokeHot));
            }

            foreach (var b in _game.Buildings.Values)
            {
                if (!_buildings.TryGetValue(b.Id, out var tr))
                {
                    var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    go.name = b.Type.ToString();
                    go.transform.SetParent(_root, false);
                    var n = _game.Nodes[b.NodeId];
                    go.transform.position = new Vector3(n.X, 0.95f, n.Z);
                    var scale = b.Type == BuildingType.Hub ? new Vector3(1.7f, 1.7f, 1.7f)
                        : b.Type == BuildingType.Depot ? new Vector3(1.1f, 0.7f, 1.1f)
                        : new Vector3(0.9f, 1.15f, 0.9f);
                    go.transform.localScale = scale;
                    MesaView.Tint(go, MesaView.BuildingColor(b.Type));
                    tr = go.transform;
                    _buildings[b.Id] = tr;
                    _buildingScale[b.Id] = scale;
                }
                var tint = MesaView.BuildingColor(b.Type);
                var producer = b.Type == BuildingType.Mine || b.Type == BuildingType.Farm || b.Type == BuildingType.Power;
                if (producer && !b.Staffed) tint *= 0.42f;
                if ((b.Type == BuildingType.Kinetic || b.Type == BuildingType.Splash) && _game.PowerBrownout)
                    tint = Color.Lerp(tint, new Color(1f, 0.28f, 0.22f), 0.55f);
                if (b.Type == BuildingType.Hub && _hubFlash > 0f)
                    tint = Color.Lerp(tint, new Color(1f, 0.22f, 0.18f), Mathf.Clamp01(_hubFlash * 2.4f));
                MesaView.Tint(tr.gameObject, tint);
                if (b.Type == BuildingType.Hub && _game.HubLevel >= 2)
                    tr.localScale = _buildingScale[b.Id] * 1.18f;
            }

            SyncRings();
            SyncGhostRails();
            SyncBuffers();

            foreach (var e in _game.Edges.Values)
            {
                if (!e.Routed) continue;
                if (!_rails.TryGetValue(e.Id, out var rail))
                {
                    var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    go.name = "rail";
                    go.transform.SetParent(_root, false);
                    rail = go.transform;
                    _rails[e.Id] = rail;
                }
                var a = _game.Nodes[e.A];
                var b = _game.Nodes[e.B];
                var pa = new Vector3(a.X, 0.48f, a.Z);
                var pb = new Vector3(b.X, 0.48f, b.Z);
                rail.position = (pa + pb) * 0.5f;
                rail.localScale = new Vector3(0.2f, 0.08f, Vector3.Distance(pa, pb));
                rail.rotation = Quaternion.LookRotation(pb - pa);
                MesaView.Tint(rail.gameObject, e.SabotagedUntil > _game.T ? MesaView.RailCut : MesaView.RailLive);
            }

            foreach (var e in _game.Edges.Values)
            {
                if (!e.Barrier) continue;
                if (!_barriers.TryGetValue(e.Id, out var beam))
                {
                    var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    go.name = "barrier";
                    go.transform.SetParent(_root, false);
                    beam = go.transform;
                    _barriers[e.Id] = beam;
                }
                var a = _game.Nodes[e.A];
                var b = _game.Nodes[e.B];
                var pa = new Vector3(a.X, 0.85f, a.Z);
                var pb = new Vector3(b.X, 0.85f, b.Z);
                beam.position = (pa + pb) * 0.5f;
                beam.localScale = new Vector3(0.42f, 0.55f, Vector3.Distance(pa, pb) * 0.92f);
                beam.rotation = Quaternion.LookRotation(pb - pa);
                MesaView.Tint(beam.gameObject, MesaView.Barrier);
            }

            SyncHaulers();
            SyncTrails();
            SyncEnemies();
        }

        static string ChokeLabel(string id, int near, bool hot)
        {
            string tag;
            switch (id)
            {
                case "choke_e": tag = "EAST"; break;
                case "choke_n": tag = "NORTH"; break;
                case "choke_w": tag = "WEST"; break;
                default: tag = "GUN"; break;
            }
            if (near > 0) return (hot ? "*" : "") + tag + " " + near;
            return hot ? "*" + tag : tag;
        }

        void SyncHaulers()
        {
            var live = new HashSet<string>();
            foreach (var h in _game.Haulers)
            {
                live.Add(h.Id);
                if (!_haulers.TryGetValue(h.Id, out var tr))
                {
                    var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    go.name = h.Id;
                    go.transform.SetParent(_root, false);
                    tr = go.transform;
                    tr.localScale = Vector3.one * 0.38f;
                    _haulers[h.Id] = tr;
                }
                tr.position = new Vector3(h.X, 0.58f, h.Z);
                tr.localScale = Vector3.one * (h.CargoAmount > 0 ? 0.5f : 0.38f);
                var cargo = h.CargoAmount <= 0 ? new Color(0.31f, 0.8f, 0.77f)
                    : h.CargoKind == Resource.Food ? new Color(0.5f, 0.85f, 0.45f)
                    : h.CargoKind == Resource.Power ? new Color(0.35f, 0.7f, 1f)
                    : new Color(0.94f, 0.64f, 0.23f);
                MesaView.Tint(tr.gameObject, cargo);
            }
            Prune(_haulers, live);
        }

        void SyncEnemies()
        {
            var live = new HashSet<string>();
            foreach (var e in _game.Enemies)
            {
                live.Add(e.Id);
                if (!_enemies.TryGetValue(e.Id, out var tr))
                {
                    var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    go.name = e.Type.ToString();
                    go.transform.SetParent(_root, false);
                    tr = go.transform;
                    tr.localScale = e.Type == EnemyType.Brute ? new Vector3(0.95f, 0.65f, 0.95f)
                        : e.Type == EnemyType.Runner ? new Vector3(0.38f, 0.5f, 0.38f)
                        : new Vector3(0.48f, 0.48f, 0.48f);
                    _enemies[e.Id] = tr;
                }
                tr.position = new Vector3(e.X, 0.62f, e.Z);
                var c = MesaView.EnemyColor(e.Type);
                if (e.SlowUntil > _game.T)
                    c = Color.Lerp(c, new Color(0.35f, 0.88f, 1f), 0.62f);
                if (e.Flash > 0f) c = Color.white;
                MesaView.Tint(tr.gameObject, c);
            }
            Prune(_enemies, live);
        }

        void SyncRings()
        {
            var live = new HashSet<string>();
            foreach (var b in _game.Buildings.Values)
            {
                var range = GameSim.RangeOf(b.Type);
                if (range <= 0f) continue;
                var node = _game.Nodes[b.NodeId];
                live.Add(b.Id);
                EnsureRing(b.Id, node, range, b.Type == BuildingType.Splash
                    ? new Color(0.94f, 0.63f, 0.38f, 0.35f)
                    : b.Type == BuildingType.Hub
                        ? new Color(0.9f, 0.78f, 0.58f, 0.28f)
                        : new Color(0.55f, 0.9f, 0.88f, 0.32f));
            }
            if (_game.SelectedTool == Tool.Kinetic || _game.SelectedTool == Tool.Splash)
            {
                var range = _game.SelectedTool == Tool.Kinetic ? Balance.KineticRange : Balance.SplashRange;
                var color = _game.SelectedTool == Tool.Splash
                    ? new Color(0.94f, 0.63f, 0.38f, 0.22f)
                    : new Color(0.55f, 0.9f, 0.88f, 0.2f);
                foreach (var n in _game.Nodes.Values)
                {
                    if (n.Kind != NodeKind.Choke && n.Kind != NodeKind.Tower) continue;
                    if (_game.Buildings.ContainsKey(n.Id)) continue;
                    var id = "place-" + n.Id;
                    live.Add(id);
                    EnsureRing(id, n, range, color);
                }
            }
            Prune(_rings, live);
        }

        void EnsureRing(string id, SimNode node, float range, Color color)
        {
            if (!_rings.TryGetValue(id, out var ring))
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                go.name = "ring";
                go.transform.SetParent(_root, false);
                var col = go.GetComponent<Collider>();
                if (col != null) UnityEngine.Object.Destroy(col);
                ring = go.transform;
                _rings[id] = ring;
            }
            ring.position = new Vector3(node.X, 0.46f, node.Z);
            ring.localScale = new Vector3(range * 2f, 0.015f, range * 2f);
            MesaView.Tint(ring.gameObject, color);
        }

        void SyncGhostRails()
        {
            var live = new HashSet<string>();
            if (_game.SelectedTool == Tool.Route && _game.RouteFrom != null)
            {
                foreach (var end in _game.RouteEnds())
                {
                    var edge = _game.EdgeBetween(_game.RouteFrom, end);
                    if (edge == null) continue;
                    if (edge.Routed && edge.SabotagedUntil <= _game.T) continue;
                    live.Add(edge.Id);
                    if (!_ghosts.TryGetValue(edge.Id, out var rail))
                    {
                        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        go.name = "ghost";
                        go.transform.SetParent(_root, false);
                        var col = go.GetComponent<Collider>();
                        if (col != null) UnityEngine.Object.Destroy(col);
                        rail = go.transform;
                        _ghosts[edge.Id] = rail;
                    }
                    var a = _game.Nodes[edge.A];
                    var b = _game.Nodes[edge.B];
                    var pa = new Vector3(a.X, 0.52f, a.Z);
                    var pb = new Vector3(b.X, 0.52f, b.Z);
                    rail.position = (pa + pb) * 0.5f;
                    rail.localScale = new Vector3(0.12f, 0.05f, Vector3.Distance(pa, pb));
                    rail.rotation = Quaternion.LookRotation(pb - pa);
                    MesaView.Tint(rail.gameObject, MesaView.PadRoute);
                }
            }
            Prune(_ghosts, live);
        }

        void SyncBuffers()
        {
            var live = new HashSet<string>();
            foreach (var b in _game.Buildings.Values)
            {
                if (b.Type != BuildingType.Mine && b.Type != BuildingType.Farm && b.Type != BuildingType.Power)
                    continue;
                live.Add(b.Id);
                if (!_buffers.TryGetValue(b.Id, out var pillar))
                {
                    var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    go.name = "buffer";
                    go.transform.SetParent(_root, false);
                    var col = go.GetComponent<Collider>();
                    if (col != null) UnityEngine.Object.Destroy(col);
                    pillar = go.transform;
                    _buffers[b.Id] = pillar;
                }
                var node = _game.Nodes[b.NodeId];
                var fill = Mathf.Clamp01(_game.ProducerFill(b));
                pillar.localScale = new Vector3(0.28f, 0.12f + fill * 1.35f, 0.28f);
                pillar.position = new Vector3(node.X + 0.55f, 0.5f + fill * 0.7f, node.Z);
                MesaView.Tint(pillar.gameObject, MesaView.BuildingColor(b.Type));
            }
            Prune(_buffers, live);
        }

        void SyncTrails()
        {
            var live = new HashSet<string>();
            foreach (var h in _game.Haulers)
            {
                if (h.CargoAmount <= 0 || h.Path.Count == 0) continue;
                live.Add(h.Id);
                if (!_trails.TryGetValue(h.Id, out var lr))
                {
                    var go = new GameObject("trail");
                    go.transform.SetParent(_root, false);
                    lr = go.AddComponent<LineRenderer>();
                    lr.useWorldSpace = true;
                    lr.startWidth = 0.11f;
                    lr.endWidth = 0.03f;
                    var shader = Shader.Find("Hidden/Internal-Colored")
                        ?? Shader.Find("Sprites/Default")
                        ?? Shader.Find("Unlit/Color")
                        ?? Shader.Find("Standard");
                    if (shader != null) lr.material = new Material(shader);
                    _trails[h.Id] = lr;
                }
                lr.positionCount = 1 + h.Path.Count;
                lr.SetPosition(0, new Vector3(h.X, 0.56f, h.Z));
                for (var i = 0; i < h.Path.Count; i++)
                {
                    if (!_game.Nodes.TryGetValue(h.Path[i], out var node)) continue;
                    lr.SetPosition(i + 1, new Vector3(node.X, 0.56f, node.Z));
                }
                var cargo = h.CargoKind == Resource.Food ? new Color(0.5f, 0.85f, 0.45f, 0.85f)
                    : h.CargoKind == Resource.Power ? new Color(0.35f, 0.7f, 1f, 0.85f)
                    : new Color(0.94f, 0.64f, 0.23f, 0.85f);
                lr.startColor = cargo;
                lr.endColor = cargo;
                lr.enabled = true;
            }
            var dead = new List<string>();
            foreach (var kv in _trails)
                if (!live.Contains(kv.Key)) dead.Add(kv.Key);
            foreach (var id in dead)
            {
                if (_trails[id] != null) Destroy(_trails[id].gameObject);
                _trails.Remove(id);
            }
        }

        void ClearTrails()
        {
            foreach (var lr in _trails.Values)
                if (lr != null) Destroy(lr.gameObject);
            _trails.Clear();
        }

        void DrawWorldBars()
        {
            if (_cam == null) return;
            foreach (var e in _game.Enemies)
            {
                var sp = _cam.WorldToScreenPoint(new Vector3(e.X, 1.35f, e.Z));
                if (sp.z <= 0f) continue;
                var x = sp.x;
                var y = Screen.height - sp.y;
                var w = e.Type == EnemyType.Brute ? 48f : 34f;
                var pct = Mathf.Clamp01(e.Hp / Mathf.Max(1f, e.MaxHp));
                GUI.backgroundColor = new Color(0f, 0f, 0f, 0.65f);
                GUI.Box(new Rect(x - w * 0.5f, y, w, 7f), "");
                GUI.backgroundColor = e.SlowUntil > _game.T
                    ? Color.Lerp(new Color(0.2f, 0.55f, 0.85f), new Color(0.45f, 0.9f, 1f), pct)
                    : Color.Lerp(new Color(0.85f, 0.18f, 0.16f), new Color(0.45f, 0.85f, 0.32f), pct);
                GUI.Box(new Rect(x - w * 0.5f, y, w * pct, 7f), "");
                GUI.backgroundColor = Color.white;
            }
            if (!_game.Nodes.TryGetValue("hub", out var hub)) return;
            var hubSp = _cam.WorldToScreenPoint(new Vector3(hub.X, 2.35f, hub.Z));
            if (hubSp.z <= 0f) return;
            var hx = hubSp.x;
            var hy = Screen.height - hubSp.y;
            var hp = Mathf.Clamp01(_game.HubHp / Balance.HubMaxHp);
            GUI.backgroundColor = new Color(0f, 0f, 0f, 0.7f);
            GUI.Box(new Rect(hx - 42f, hy, 84f, 9f), "");
            GUI.backgroundColor = Color.Lerp(new Color(0.85f, 0.18f, 0.16f), new Color(0.9f, 0.78f, 0.5f), hp);
            GUI.Box(new Rect(hx - 42f, hy, 84f * hp, 9f), "");
            GUI.backgroundColor = Color.white;
        }

        static void Prune(Dictionary<string, Transform> map, HashSet<string> live)
        {
            var dead = new List<string>();
            foreach (var kv in map) if (!live.Contains(kv.Key)) dead.Add(kv.Key);
            foreach (var id in dead)
            {
                if (map[id] != null) Destroy(map[id].gameObject);
                map.Remove(id);
            }
        }

        void OnGUI()
        {
            if (_hud == null || _game == null) return;
            _hud.Draw(_game);
            DrawWorldBars();
            if (_hud.ClickedTool == Tool.Upgrade) _game.TryUpgrade(out _);
            else if (_hud.ClickedTool.HasValue) _game.SetTool(_hud.ClickedTool.Value);
            if (_hud.ConsumeBootDemo) _pendingDemo = true;
            if (_hud.ConsumeBootPlay) _pendingManual = true;
            if (_hud.ConsumeRestart) _pendingRestart = true;
        }
    }
}
