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
        readonly Dictionary<string, LineRenderer> _intents = new Dictionary<string, LineRenderer>();
        readonly Dictionary<string, Transform> _shadows = new Dictionary<string, Transform>();
        readonly Dictionary<string, LineRenderer> _locks = new Dictionary<string, LineRenderer>();
        readonly Dictionary<string, LineRenderer> _chews = new Dictionary<string, LineRenderer>();
        readonly Dictionary<string, LineRenderer> _closes = new Dictionary<string, LineRenderer>();
        readonly Dictionary<string, Transform> _forecasts = new Dictionary<string, Transform>();
        readonly Dictionary<string, Transform> _cargoTags = new Dictionary<string, Transform>();
        readonly HashSet<string> _stuckShown = new HashSet<string>();
        readonly HashSet<string> _bracePinged = new HashSet<string>();
        readonly HashSet<string> _threatPinged = new HashSet<string>();
        readonly HashSet<string> _cutSoonPinged = new HashSet<string>();
        readonly HashSet<string> _powerPinged = new HashSet<string>();
        readonly HashSet<string> _lowPinged = new HashSet<string>();
        readonly HashSet<string> _homePinged = new HashSet<string>();
        LineRenderer _braceLine;
        LineRenderer _powerLine;
        LineRenderer _offlineLine;
        LineRenderer _sitLine;
        LineRenderer _homeLine;
        bool _chewPinged;
        bool _closePinged;
        bool _atPadPinged;
        bool _clearPinged;
        bool _braceCutPinged;
        string _offlinePinged;
        string _sitPinged;
        bool _l2ReadyPinged;
        string _openPinged;
        string _slowPinged;
        string _stretchPinged;
        bool _holdReadyPinged;
        bool _homeFlash;
        int _packPinged = -1;
        string _gunsUpPinged;
        string _railLivePinged;
        Transform _root;
        Camera _cam;
        float _acc;
        float _hubFlash;
        bool _hubBraceFlash;
        bool _coreAlarm;
        bool _surgeBannered;
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
                if (child.name == "tracer" || child.name == "pip" || child.name == "ring" || child.name == "ghost" || child.name == "buffer" || child.name == "trail" || child.name == "intent" || child.name == "shadow" || child.name == "burst") continue;
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
            ClearMap(_shadows);
            ClearMap(_forecasts);
            ClearTrails();
            ClearIntents();
            ClearLocks();
            ClearChews();
            ClearCloses();
            ClearForecasts();
            ClearCargoTags();
            ClearBraceLine();
            ClearPowerLine();
            ClearOfflineLine();
            ClearSitLine();
            ClearHomeLine();
            _hubFlash = 0f;
            _hubBraceFlash = false;
            _coreAlarm = false;
            _surgeBannered = false;
            _stuckShown.Clear();
            _bracePinged.Clear();
            _threatPinged.Clear();
            _cutSoonPinged.Clear();
            _powerPinged.Clear();
            _lowPinged.Clear();
            _chewPinged = false;
            _closePinged = false;
            _atPadPinged = false;
            _clearPinged = false;
            _braceCutPinged = false;
            _offlinePinged = null;
            _sitPinged = null;
            _l2ReadyPinged = false;
            _openPinged = null;
            _slowPinged = null;
            _stretchPinged = null;
            _holdReadyPinged = false;
            _homePinged.Clear();
            _homeFlash = false;
            _packPinged = -1;
            _gunsUpPinged = null;
            _railLivePinged = null;
            _juice.CutAlarm(false);
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
            if (_game.CoreThin && !_coreAlarm && _game.Phase == Phase.Playing)
            {
                _coreAlarm = true;
                _hud.Flash("CORE THIN — haul braces the Hub", 2.4f, new Color(0.85f, 0.2f, 0.18f, 0.95f));
                _juice.CoreThin();
            }
            if (!_game.Surging) _surgeBannered = false;
            _juice.CutAlarm(_game.ActiveCut() != null && _game.Phase == Phase.Playing);
            PingBraceInbound();
            PingChew();
            PingRailThreat();
            PingGunsDry();
            PingGunsLow();
            PingCoreBound();
            PingWaveClear();
            PingCutStake();
            PingOffline();
            PingSitting();
            PingL2Ready();
            PingOpenChoke();
            PingSlowChoke();
            PingStretch();
            PingHoldReady();
            PingHomeInbound();
            PingPackIn();
            PingGunsUp();
            PingRailLive();
            _juice.Tick(_cam, events);
            _hubFlash = Mathf.Max(0f, _hubFlash - Time.deltaTime);
            SyncAtmosphere();
            SyncView(false);
        }

        void Banner(SimEvent ev)
        {
            switch (ev.Kind)
            {
                case SimEventKind.Brownout:
                    _hud.Flash("GUNS DRY — haul Power", 2.2f, new Color(0.85f, 0.28f, 0.22f, 0.95f));
                    break;
                case SimEventKind.Sabotage:
                    _hud.Flash(_game.CutStakeFlash() ?? "HAUL CUT — splice the orange rail", 1.6f, new Color(0.95f, 0.38f, 0.18f, 0.95f));
                    break;
                case SimEventKind.Barrier:
                    _hud.Flash("Barrier up — spawn approach slowed", 1.8f, new Color(0.95f, 0.32f, 0.34f, 0.92f));
                    break;
                case SimEventKind.Wave:
                    _hud.WaveCall(_game.WaveBannerCopy(), _game.WaveIndex >= 5 ? 4.2f : 3.2f);
                    foreach (var n in _game.Nodes.Values)
                    {
                        if (n.Kind != NodeKind.Spawn) continue;
                        var incoming = _game.IncomingAt(n.Id);
                        if (incoming <= 0) continue;
                        _juice.Inbound(n.X, n.Z, incoming);
                    }
                    break;
                case SimEventKind.Upgrade:
                    if (_game.HubLevel >= 2) _hud.Flash("Hub Level 2 — Splash unlocked · WEST choke", 2.6f, new Color(0.94f, 0.63f, 0.38f, 0.95f));
                    else _hud.Flash("Hub L2 raising", 1.6f, new Color(0.9f, 0.78f, 0.5f, 0.94f));
                    break;
                case SimEventKind.Shot:
                case SimEventKind.Splash:
                case SimEventKind.Deposit:
                    break;
                case SimEventKind.Hit:
                    if (ev.NodeId == "hub")
                    {
                        _hubBraceFlash = _game.Surging;
                        _hubFlash = _game.Surging ? 0.35f : (_game.CoreThin ? 0.7f : 0.4f);
                        _juice.HubHit(_game.Surging);
                    }
                    break;
                case SimEventKind.Death:
                case SimEventKind.WarnFood:
                case SimEventKind.Build:
                    break;
                case SimEventKind.Win:
                    _hud.Flash("MESA HOLDS", 3.2f, new Color(0.18f, 0.55f, 0.32f, 0.95f));
                    break;
                case SimEventKind.Lose:
                    _hud.Flash(_game.Phase == Phase.LostStarve ? "STARVED OUT" : "HUB DOWN", 3.2f, new Color(0.72f, 0.12f, 0.12f, 0.95f));
                    break;
                case SimEventKind.Route:
                    break;
                case SimEventKind.Surge:
                    if (!_surgeBannered)
                    {
                        _surgeBannered = true;
                        _hud.Flash("BRACE — haul bought the Hub a breath", 1.4f, new Color(0.2f, 0.55f, 0.7f, 0.95f));
                    }
                    break;
                case SimEventKind.Hold:
                    if (ev.Reason == "power")
                        _hud.Flash("GUNS ORDER — haulers rush Power", 1.8f, new Color(0.2f, 0.45f, 0.75f, 0.95f));
                    else if (ev.Reason == "food")
                        _hud.Flash("CREW ORDER — haulers rush Food", 1.8f, new Color(0.18f, 0.5f, 0.28f, 0.95f));
                    else
                        _hud.Flash("Hold auto — hungriest stock", 1.4f, new Color(0.35f, 0.35f, 0.32f, 0.92f));
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
            if (Input.GetKeyDown(KeyCode.H)) _game.CycleHold(out _);
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

        void SyncAtmosphere()
        {
            var dusk = new Color(0.07f, 0.18f, 0.22f);
            var raid = new Color(0.2f, 0.05f, 0.06f);
            float heat = 0f;
            if (_game.Phase == Phase.Playing)
            {
                if (_game.WaveIndex >= 5) heat = 0.5f + 0.18f * Mathf.Abs(Mathf.Sin(Time.time * 1.7f));
                else if (_game.HubChewers() > 0) heat = 0.42f + 0.12f * Mathf.Abs(Mathf.Sin(Time.time * 5f));
                else if (_game.Enemies.Count > 0) heat = 0.16f;
            }
            RenderSettings.fogColor = Color.Lerp(dusk, raid, heat);
            if (_cam != null)
                _cam.backgroundColor = Color.Lerp(new Color(0.05f, 0.16f, 0.20f), raid, heat * 0.7f);
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
                     (Array.IndexOf(_game.NextWaveSpawns(), n.Id) >= 0 &&
                      (_game.NextWaveIn < 10f || _game.WaveForecastLive)));
                var hotLane = _game.HottestLane();
                var chokeHot = n.Kind == NodeKind.Choke &&
                    ((n.Id == "choke_e" && hotLane == "east") ||
                     (n.Id == "choke_n" && hotLane == "north") ||
                     (n.Id == "choke_w" && hotLane == "west"));
                var splashWest = _game.SplashFresh() && n.Id == "choke_w";
                var holdGuns = _game.HoldOrder == HoldOrder.Power &&
                    _game.Buildings.TryGetValue(n.Id, out var holdPwr) && holdPwr.Type == BuildingType.Power;
                var holdCrew = _game.HoldOrder == HoldOrder.Food &&
                    _game.Buildings.TryGetValue(n.Id, out var holdFarm) && holdFarm.Type == BuildingType.Farm;
                var near = n.Kind == NodeKind.Choke ? _game.EnemiesNear(n.Id, 4.8f) : 0;
                Color c;
                if (inbound) c = MesaView.Spawn * pulse;
                else if (n.Kind == NodeKind.Spawn) c = MesaView.Spawn;
                else if (farmGlow || holdCrew) c = MesaView.PadFarm * pulse;
                else if (holdGuns) c = new Color(0.32f * pulse, 0.68f * pulse, 0.94f);
                else if (_game.OfflinePad() == n.Id) c = new Color(0.92f * pulse, 0.55f * pulse, 0.22f);
                else if (_game.SittingStock() != null && _game.SittingStock().NodeId == n.Id)
                    c = new Color(0.95f * pulse, 0.78f * pulse, 0.32f);
                else if (_game.StretchPad() == n.Id)
                    c = new Color(1f * pulse, 0.62f * pulse, 0.32f);
                else if (_game.RailLiveTouches(n.Id) && n.Kind != NodeKind.Hub)
                    c = new Color(0.42f * pulse, 0.92f * pulse, 0.88f);
                else if (hubGlow || routeGlow) c = MesaView.PadRoute * pulse;
                else if (splashWest) c = new Color(0.94f * pulse, 0.63f * pulse, 0.38f);
                else if (_game.OpenChokeId() == n.Id)
                    c = Color.Lerp(new Color(0.62f, 0.52f, 0.4f), new Color(1f * pulse, 0.38f * pulse, 0.22f), 0.7f);
                else if (_game.SlowChokeId() == n.Id)
                    c = Color.Lerp(new Color(0.62f, 0.52f, 0.4f), new Color(0.95f * pulse, 0.32f * pulse, 0.34f), 0.7f);
                else if (_game.GunsUpNodeId() == n.Id)
                    c = Color.Lerp(new Color(0.62f, 0.52f, 0.4f), new Color(0.45f * pulse, 0.9f * pulse, 0.88f), 0.75f);
                else if (n.Kind == NodeKind.Choke && (chokeHot || near > 0))
                    c = Color.Lerp(new Color(0.62f, 0.52f, 0.4f), MesaView.Spawn * pulse,
                        Mathf.Clamp01(near / 4f + (chokeHot ? 0.35f : 0f)));
                else if (n.Kind == NodeKind.Hub)
                    c = _game.HubChewers() > 0
                        ? Color.Lerp(new Color(0.9f, 0.78f, 0.58f), new Color(1f, 0.28f, 0.18f), pulse)
                        : _game.HubClosers() > 0
                        ? Color.Lerp(new Color(0.9f, 0.78f, 0.58f), new Color(1f, 0.32f, 0.18f), pulse)
                        : _game.GunsDry()
                        ? Color.Lerp(new Color(0.9f, 0.78f, 0.58f), new Color(1f, 0.48f, 0.18f), pulse)
                        : _game.RailLiveLive()
                        ? Color.Lerp(new Color(0.9f, 0.78f, 0.58f), new Color(0.42f, 0.92f, 0.88f), pulse)
                        : _game.WaveClearLive()
                        ? Color.Lerp(new Color(0.9f, 0.78f, 0.58f), new Color(0.55f, 0.9f, 0.5f), pulse)
                        : _game.CoreThinLive()
                        ? Color.Lerp(new Color(0.9f, 0.78f, 0.58f), new Color(1f, 0.28f, 0.18f), pulse)
                        : _game.L2ReadyWorld()
                        ? Color.Lerp(new Color(0.9f, 0.78f, 0.58f), new Color(1f, 0.86f, 0.4f), pulse)
                        : _game.HoldReadyWorld()
                        ? Color.Lerp(new Color(0.9f, 0.78f, 0.58f), new Color(0.92f, 0.78f, 0.42f), pulse)
                        : _game.PackInLive()
                        ? Color.Lerp(new Color(0.9f, 0.78f, 0.58f), new Color(0.86f, 0.28f, 0.24f), pulse)
                        : _game.GunsUpWorld()
                        ? Color.Lerp(new Color(0.9f, 0.78f, 0.58f), new Color(0.45f, 0.9f, 0.88f), pulse)
                        : _game.HoldOrder == HoldOrder.Power
                        ? Color.Lerp(new Color(0.9f, 0.78f, 0.58f), new Color(0.4f, 0.75f, 1f), pulse)
                        : _game.HoldOrder == HoldOrder.Food
                            ? Color.Lerp(new Color(0.9f, 0.78f, 0.58f), new Color(0.5f, 0.85f, 0.48f), pulse)
                        : _game.HubRaising
                        ? Color.Lerp(new Color(0.9f, 0.78f, 0.58f), new Color(1f, 0.86f, 0.4f), pulse)
                        : new Color(0.9f, 0.78f, 0.58f);
                else c = MesaView.PadIdle;
                MesaView.Tint(mark.gameObject, c);
                if (n.Id == "hub")
                    MesaView.SetLabel(mark, cut != null && (cut.A == n.Id || cut.B == n.Id)
                        ? "SPLICE"
                        : _game.HubChewers() > 0 ? "CHEW"
                        : _game.HubClosers() > 0 ? (_game.AnyCloseImminent() ? "PAD" : "IN")
                        : _game.GunsDry() ? "DRY"
                        : _game.RailLiveLive() ? "LIVE"
                        : _game.WaveClearLive() ? "CLEAR"
                        : _game.CoreThinLive() ? "THIN"
                        : _game.HubRaising ? "L2"
                        : _game.L2ReadyWorld() ? "READY"
                        : _game.HoldReadyWorld() ? "HOLD"
                        : _game.PackInLive() ? "PACK"
                        : _game.GunsUpWorld() ? "GUNS"
                        : _game.HoldOrder == HoldOrder.Power ? "GUNS"
                        : _game.HoldOrder == HoldOrder.Food ? "CREW"
                        : hubGlow ? "2 HUB" : "HUB");
                else if (n.Kind == NodeKind.Spawn)
                {
                    var forecast = _game.SpawnForecastCopy(n.Id);
                    MesaView.SetLabel(mark, forecast ?? (inbound ? "IN " + _game.IncomingAt(n.Id) : "RAID"));
                }
                else if (n.Kind == NodeKind.Choke)
                    MesaView.SetLabel(mark, splashWest ? "SPLASH"
                        : _game.OpenChokeId() == n.Id ? "OPEN"
                        : _game.SlowChokeId() == n.Id ? "SLOW"
                        : _game.GunsUpNodeId() == n.Id ? "UP"
                        : ChokeLabel(n.Id, near, chokeHot));
                else if (n.Kind == NodeKind.Tower)
                    MesaView.SetLabel(mark, _game.GunsUpNodeId() == n.Id ? "UP" : "GUN");
                else if (n.Kind == NodeKind.Pad)
                {
                    if (cut != null && (cut.A == n.Id || cut.B == n.Id))
                        MesaView.SetLabel(mark, "SPLICE");
                    else if (_game.RailLiveTouches(n.Id))
                        MesaView.SetLabel(mark, "LIVE");
                    else if (_game.HoldOrder == HoldOrder.Power &&
                             _game.Buildings.TryGetValue(n.Id, out var pwr) && pwr.Type == BuildingType.Power)
                        MesaView.SetLabel(mark, "GUNS");
                    else if (_game.HoldOrder == HoldOrder.Food &&
                             _game.Buildings.TryGetValue(n.Id, out var farm) && farm.Type == BuildingType.Farm)
                        MesaView.SetLabel(mark, "CREW");
                    else if (_game.OfflinePad() == n.Id)
                        MesaView.SetLabel(mark, "OFFLINE");
                    else if (_game.SittingStock() != null && _game.SittingStock().NodeId == n.Id)
                        MesaView.SetLabel(mark, _game.SittingChip() ?? "HAUL");
                    else if (_game.StretchPad() == n.Id)
                        MesaView.SetLabel(mark, "IDLE");
                    else if (n.Id == "pad_s")
                        MesaView.SetLabel(mark, farmGlow ? "1 FARM" : "PAD");
                    else if (_game.Buildings.TryGetValue(n.Id, out var pb) && pb.Type == BuildingType.Power
                             && (_game.GunsHungry() || _game.GunsUpLive()))
                        MesaView.SetLabel(mark, "FEED");
                    else MesaView.SetLabel(mark, "PAD");
                }
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
                else if ((b.Type == BuildingType.Kinetic || b.Type == BuildingType.Splash) && _game.GunsHungry())
                    tint = Color.Lerp(tint, new Color(1f, 0.72f, 0.28f), 0.4f * pulse);
                else if ((b.Type == BuildingType.Kinetic || b.Type == BuildingType.Splash)
                         && _game.GunsUpNodeId() == b.NodeId)
                    tint = Color.Lerp(tint, new Color(0.45f, 0.9f, 0.88f), 0.45f * pulse);
                if (b.Type == BuildingType.Power && (_game.GunsHungry() || _game.GunsUpLive() || _game.RailLiveTouches(b.NodeId)))
                    tint = Color.Lerp(tint, new Color(0.45f, 0.9f, 1f), 0.45f * pulse);
                if (b.Type == BuildingType.Hub && _game.HubRaising)
                    tint = Color.Lerp(tint, new Color(1f, 0.86f, 0.42f), 0.55f * pulse);
                if (b.Type == BuildingType.Hub && _game.CoreThin)
                    tint = Color.Lerp(tint, new Color(0.95f, 0.22f, 0.18f), 0.4f + 0.2f * pulse);
                if (b.Type == BuildingType.Hub && _game.HubChewers() > 0)
                    tint = Color.Lerp(tint, new Color(1f, 0.18f, 0.12f), 0.45f + 0.2f * pulse);
                else if (b.Type == BuildingType.Hub && _game.HubClosers() > 0)
                    tint = Color.Lerp(tint, new Color(1f, 0.32f, 0.16f), 0.4f + 0.2f * pulse);
                else if (b.Type == BuildingType.Hub && _game.GunsDry())
                    tint = Color.Lerp(tint, new Color(1f, 0.48f, 0.18f), 0.4f + 0.2f * pulse);
                else if (b.Type == BuildingType.Hub && _game.RailLiveLive())
                    tint = Color.Lerp(tint, new Color(0.42f, 0.92f, 0.88f), 0.4f + 0.18f * pulse);
                else if (b.Type == BuildingType.Hub && _game.WaveClearLive())
                    tint = Color.Lerp(tint, new Color(0.55f, 0.9f, 0.5f), 0.4f + 0.15f * pulse);
                else if (b.Type == BuildingType.Hub && _game.L2ReadyWorld())
                    tint = Color.Lerp(tint, new Color(1f, 0.86f, 0.42f), 0.4f + 0.18f * pulse);
                else if (b.Type == BuildingType.Hub && _game.HoldReadyWorld())
                    tint = Color.Lerp(tint, new Color(0.92f, 0.78f, 0.42f), 0.4f + 0.18f * pulse);
                else if (b.Type == BuildingType.Hub && _game.GunsUpWorld())
                    tint = Color.Lerp(tint, new Color(0.45f, 0.9f, 0.88f), 0.4f + 0.18f * pulse);
                if (b.Type == BuildingType.Hub && _hubFlash > 0f)
                    tint = Color.Lerp(tint,
                        _hubBraceFlash ? new Color(0.4f, 0.9f, 1f) : new Color(1f, 0.22f, 0.18f),
                        Mathf.Clamp01(_hubFlash * 2.4f));
                MesaView.Tint(tr.gameObject, tint);
                if (b.Type == BuildingType.Hub && _game.HubLevel >= 2)
                    tr.localScale = _buildingScale[b.Id] * (1.18f + (_game.HubChewers() > 0 ? 0.05f * pulse : 0f));
                else if (b.Type == BuildingType.Hub && _game.HubRaising)
                    tr.localScale = _buildingScale[b.Id] * (1f + 0.18f * (1f - Mathf.Clamp01(_game.HubUpgradeLeft / Balance.HubL2Time)));
                else if (b.Type == BuildingType.Hub && _game.HubChewers() > 0)
                    tr.localScale = _buildingScale[b.Id] * (1f + 0.08f * pulse);
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
                var cutRail = e.SabotagedUntil > _game.T;
                var threat = !cutRail && _game.RailThreatened(e.Id);
                var imminent = false;
                if (threat)
                {
                    foreach (var en in _game.Enemies)
                    {
                        var te = _game.RunnerThreatEdge(en);
                        if (te != null && te.Id == e.Id && _game.RunnerThreatImminent(en))
                        {
                            imminent = true;
                            break;
                        }
                    }
                }
                var recovering = !cutRail && _game.RailLiveEdgeId() == e.Id;
                var pulseW = cutRail || imminent || recovering ? 0.2f + 0.14f * Mathf.Abs(Mathf.Sin(Time.time * 9f))
                    : threat ? 0.2f + 0.08f * Mathf.Abs(Mathf.Sin(Time.time * 7f))
                    : 0.2f;
                rail.localScale = new Vector3(pulseW, cutRail || imminent || recovering ? 0.12f : 0.08f, Vector3.Distance(pa, pb));
                rail.rotation = Quaternion.LookRotation(pb - pa);
                Color railColor;
                if (cutRail)
                    railColor = Color.Lerp(MesaView.RailCut, new Color(1f, 0.82f, 0.28f), Mathf.Abs(Mathf.Sin(Time.time * 9f)));
                else if (imminent)
                    railColor = Color.Lerp(new Color(0.95f, 0.42f, 0.78f), new Color(1f, 0.72f, 0.28f), Mathf.Abs(Mathf.Sin(Time.time * 11f)));
                else if (threat)
                    railColor = Color.Lerp(MesaView.RailLive, new Color(0.95f, 0.42f, 0.78f), 0.55f + 0.35f * Mathf.Abs(Mathf.Sin(Time.time * 6f)));
                else if (recovering)
                    railColor = Color.Lerp(MesaView.RailLive, new Color(0.42f, 0.92f, 0.88f), 0.55f + 0.4f * Mathf.Abs(Mathf.Sin(Time.time * 8f)));
                else
                    railColor = MesaView.RailLive;
                MesaView.Tint(rail.gameObject, railColor);
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
            SyncCargoTags();
            SyncTrails();
            SyncRunnerIntents();
            SyncGunLocks();
            SyncChews();
            SyncCloses();
            SyncBraceLine();
            SyncPowerLine();
            SyncHomeLine();
            SyncOfflineLine();
            SyncSitLine();
            SyncForecasts();
            SyncEnemies();
            SyncShadows();
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
            var inboundHaul = _game.BraceInbound();
            var powerHaul = _game.PowerInbound();
            if (powerHaul == null)
            {
                powerHaul = _game.GunsLowInbound();
                if (powerHaul != null && inboundHaul != null && powerHaul.Id == inboundHaul.Id)
                    powerHaul = null;
            }
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
                var blocked = _game.HaulerBlocked(h);
                var inbound = inboundHaul != null && inboundHaul.Id == h.Id;
                var feeding = powerHaul != null && powerHaul.Id == h.Id;
                if (blocked)
                {
                    if (_stuckShown.Add(h.Id)) _juice.Stuck(h.X, h.Z);
                }
                else if (_stuckShown.Remove(h.Id) && h.Path.Count > 0)
                    _juice.Rolling(h.X, h.Z);
                var waitPulse = h.Wait > 0 || blocked ? 1f + 0.16f * Mathf.Abs(Mathf.Sin(Time.time * 9f)) : 1f;
                if (inbound || feeding) waitPulse *= 1f + 0.12f * Mathf.Abs(Mathf.Sin(Time.time * 7f));
                tr.localScale = Vector3.one * ((h.CargoAmount > 0 ? 0.5f : 0.38f) * waitPulse);
                var cargo = h.CargoAmount <= 0 ? new Color(0.31f, 0.8f, 0.77f)
                    : h.CargoKind == Resource.Food ? new Color(0.5f, 0.85f, 0.45f)
                    : h.CargoKind == Resource.Power ? new Color(0.35f, 0.7f, 1f)
                    : new Color(0.94f, 0.64f, 0.23f);
                if (blocked) cargo = Color.Lerp(cargo, new Color(1f, 0.5f, 0.22f), 0.62f);
                if (feeding) cargo = Color.Lerp(cargo, new Color(1f, 0.55f, 0.2f), 0.5f);
                else if (inbound) cargo = Color.Lerp(cargo, new Color(0.45f, 0.9f, 1f), 0.4f);
                MesaView.Tint(tr.gameObject, cargo);
            }
            Prune(_haulers, live);
            if (_game.ActiveCut() == null) _stuckShown.Clear();
        }

        void SyncEnemies()
        {
            var live = new HashSet<string>();
            foreach (var e in _game.Enemies)
            {
                live.Add(e.Id);
                if (!_enemies.TryGetValue(e.Id, out var tr))
                {
                    var go = GameObject.CreatePrimitive(MesaView.EnemyPrim(e.Type));
                    go.name = e.Type.ToString();
                    go.transform.SetParent(_root, false);
                    tr = go.transform;
                    tr.localScale = MesaView.EnemyScale(e.Type);
                    if (e.Type == EnemyType.Runner)
                        tr.rotation = Quaternion.Euler(0f, 45f, 0f);
                    _enemies[e.Id] = tr;
                }
                tr.position = new Vector3(e.X, 0.62f, e.Z);
                var c = MesaView.EnemyColor(e.Type);
                if (e.Type == EnemyType.Runner)
                    c = Color.Lerp(c, new Color(1f, 0.82f, 0.45f), 0.35f * Mathf.Abs(Mathf.Sin(Time.time * 14f)));
                if (e.SlowUntil > _game.T)
                    c = Color.Lerp(c, new Color(0.35f, 0.88f, 1f), 0.62f);
                if (_game.LockedOn(e))
                    c = Color.Lerp(c, Color.white, 0.28f + 0.12f * Mathf.Abs(Mathf.Sin(Time.time * 11f)));
                if (_game.ChewingHub(e))
                    c = Color.Lerp(c, new Color(1f, 0.55f, 0.2f), 0.4f + 0.2f * Mathf.Abs(Mathf.Sin(Time.time * 10f)));
                else if (_game.ClosingOnHub(e))
                    c = Color.Lerp(c, new Color(1f, 0.32f, 0.16f), 0.35f + 0.2f * Mathf.Abs(Mathf.Sin(Time.time * 9f)));
                if (e.Flash > 0f) c = Color.white;
                MesaView.Tint(tr.gameObject, c);
                if (e.Type == EnemyType.Runner)
                    tr.localScale = MesaView.EnemyScale(e.Type) * (1f + 0.08f * Mathf.Abs(Mathf.Sin(Time.time * 14f)));
                else if (_game.ChewingHub(e))
                    tr.localScale = MesaView.EnemyScale(e.Type) * (1f + 0.1f * Mathf.Abs(Mathf.Sin(Time.time * 10f)));
                else if (_game.ClosingOnHub(e))
                    tr.localScale = MesaView.EnemyScale(e.Type) * (1f + 0.08f * Mathf.Abs(Mathf.Sin(Time.time * 9f)));
            }
            Prune(_enemies, live);
        }

        void SyncShadows()
        {
            var live = new HashSet<string>();
            foreach (var e in _game.Enemies)
            {
                live.Add(e.Id);
                if (!_shadows.TryGetValue(e.Id, out var sh))
                {
                    var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    go.name = "shadow";
                    go.transform.SetParent(_root, false);
                    var col = go.GetComponent<Collider>();
                    if (col != null) Destroy(col);
                    sh = go.transform;
                    _shadows[e.Id] = sh;
                }
                var wide = e.Type == EnemyType.Brute ? 1.15f : e.Type == EnemyType.Runner ? 0.42f : 0.7f;
                sh.position = new Vector3(e.X, 0.43f, e.Z);
                sh.localScale = new Vector3(wide, 0.012f, wide);
                MesaView.Tint(sh.gameObject, new Color(0.04f, 0.05f, 0.06f, 0.65f));
            }
            Prune(_shadows, live);
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
                var locked = _game.TowerLock(b) != null;
                var dry = _game.PowerBrownout && (b.Type == BuildingType.Kinetic || b.Type == BuildingType.Splash || b.Type == BuildingType.Hub);
                Color ringColor;
                if (b.Type == BuildingType.Splash)
                    ringColor = new Color(0.94f, 0.63f, 0.38f, locked ? 0.55f : 0.35f);
                else if (b.Type == BuildingType.Hub)
                    ringColor = new Color(0.9f, 0.78f, 0.58f, locked ? 0.45f : 0.28f);
                else
                    ringColor = new Color(0.55f, 0.9f, 0.88f, locked ? 0.52f : 0.32f);
                if (dry) ringColor = Color.Lerp(ringColor, new Color(1f, 0.28f, 0.22f, 0.5f), 0.55f);
                EnsureRing(b.Id, node, range, ringColor);
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
            if (_game.SplashFresh() && _game.Nodes.TryGetValue("choke_w", out var west))
            {
                live.Add("teach-splash-w");
                var glow = 0.18f + 0.1f * Mathf.Abs(Mathf.Sin(Time.time * 5f));
                EnsureRing("teach-splash-w", west, Balance.SplashRange, new Color(0.94f, 0.63f, 0.38f, glow));
            }
            var openId = _game.OpenChokeId();
            if (openId != null && _game.Nodes.TryGetValue(openId, out var openNode))
            {
                live.Add("open-choke");
                var openRange = openId == "choke_w" && _game.HubLevel >= 2 && !_game.HasType(BuildingType.Splash)
                    ? Balance.SplashRange
                    : Balance.KineticRange;
                var openGlow = 0.36f + 0.08f * Mathf.Abs(Mathf.Sin(Time.time * 7f));
                EnsureRing("open-choke", openNode, openRange, new Color(1f, 0.38f, 0.22f, openGlow));
            }
            var slowId = _game.SlowChokeId();
            if (slowId != null && _game.Nodes.TryGetValue(slowId, out var slowNode))
            {
                live.Add("slow-choke");
                var slowGlow = 0.34f + 0.08f * Mathf.Abs(Mathf.Sin(Time.time * 8f));
                EnsureRing("slow-choke", slowNode, 3.4f, new Color(0.95f, 0.32f, 0.34f, slowGlow));
            }
            if (_game.HubChewers() > 0 && !_game.Surging && _game.Nodes.TryGetValue("hub", out var chewHub))
            {
                live.Add("chew-ring");
                var chewPulse = 2.4f + 0.35f * Mathf.Abs(Mathf.Sin(Time.time * 9f));
                EnsureRing("chew-ring", chewHub, chewPulse, new Color(1f, 0.28f, 0.16f, 0.4f));
            }
            if (_game.HubClosers() > 0 && _game.HubChewers() <= 0 && _game.Nodes.TryGetValue("hub", out var boundHub))
            {
                live.Add("core-bound");
                var boundPulse = _game.AnyCloseImminent()
                    ? 2.8f + 0.4f * Mathf.Abs(Mathf.Sin(Time.time * 10f))
                    : 3.4f + 0.25f * Mathf.Abs(Mathf.Sin(Time.time * 7f));
                EnsureRing("core-bound", boundHub, boundPulse, new Color(1f, 0.32f, 0.16f, 0.36f));
            }
            if (_game.GunsDry() && _game.Nodes.TryGetValue("hub", out var dryHub))
            {
                live.Add("guns-dry");
                var dryPulse = 3.0f + 0.3f * Mathf.Abs(Mathf.Sin(Time.time * 10f));
                EnsureRing("guns-dry", dryHub, dryPulse, new Color(1f, 0.45f, 0.18f, 0.38f));
            }
            if (_game.CoreThinLive() && _game.HubChewers() <= 0 && _game.HubClosers() <= 0
                && !_game.GunsDry() && !_game.WaveClearLive()
                && _game.Nodes.TryGetValue("hub", out var thinHub))
            {
                live.Add("core-thin");
                var thinPulse = 3.2f + 0.35f * Mathf.Abs(Mathf.Sin(Time.time * 8f));
                EnsureRing("core-thin", thinHub, thinPulse, new Color(1f, 0.22f, 0.16f, 0.36f));
            }
            if (_game.WaveClearLive() && _game.Nodes.TryGetValue("hub", out var clearHub))
            {
                live.Add("wave-clear");
                var clearPulse = 3.6f + 0.3f * Mathf.Abs(Mathf.Sin(Time.time * 5f));
                EnsureRing("wave-clear", clearHub, clearPulse, new Color(0.55f, 0.9f, 0.5f, 0.34f));
            }
            if (_game.L2ReadyWorld() && _game.Nodes.TryGetValue("hub", out var l2Hub))
            {
                live.Add("l2-ready");
                var l2Pulse = 3.2f + 0.35f * Mathf.Abs(Mathf.Sin(Time.time * 6f));
                EnsureRing("l2-ready", l2Hub, l2Pulse, new Color(1f, 0.86f, 0.42f, 0.38f));
            }
            if (_game.HoldReadyWorld() && _game.Nodes.TryGetValue("hub", out var holdReadyHub))
            {
                live.Add("hold-ready");
                var holdPulse = 3.4f + 0.3f * Mathf.Abs(Mathf.Sin(Time.time * 5.5f));
                EnsureRing("hold-ready", holdReadyHub, holdPulse, new Color(0.92f, 0.78f, 0.42f, 0.36f));
            }
            var gunsUpId = _game.GunsUpNodeId();
            if (gunsUpId != null && _game.Nodes.TryGetValue(gunsUpId, out var gunsUpNode))
            {
                live.Add("guns-up");
                var gunsRange = _game.Buildings.TryGetValue(gunsUpId, out var gunsUpB)
                    && gunsUpB.Type == BuildingType.Splash
                    ? Balance.SplashRange
                    : Balance.KineticRange;
                var gunsGlow = 0.38f + 0.1f * Mathf.Abs(Mathf.Sin(Time.time * 7f));
                EnsureRing("guns-up", gunsUpNode, gunsRange, new Color(0.45f, 0.9f, 0.88f, gunsGlow));
            }
            if (_game.Surging && _game.Nodes.TryGetValue("hub", out var hubNode))
            {
                live.Add("surge-shield");
                var pulse = 3.6f + 0.35f * Mathf.Abs(Mathf.Sin(Time.time * 8f));
                EnsureRing("surge-shield", hubNode, pulse, new Color(0.4f, 0.9f, 1f, 0.42f));
            }
            if (!_game.Surging && _game.BraceInbound() != null && _game.Nodes.TryGetValue("hub", out var inboundHub))
            {
                live.Add("brace-inbound");
                var pulse = 3.2f + 0.4f * Mathf.Abs(Mathf.Sin(Time.time * 7f));
                EnsureRing("brace-inbound", inboundHub, pulse, new Color(0.45f, 0.9f, 1f, 0.32f));
            }
            if (!_game.Surging && _game.HoldOrder != HoldOrder.Auto && _game.Nodes.TryGetValue("hub", out var holdHub))
            {
                live.Add("hold-order");
                var glow = 4.4f + 0.25f * Mathf.Abs(Mathf.Sin(Time.time * 5f));
                Color holdColor;
                switch (_game.HoldOrder)
                {
                    case HoldOrder.Power:
                        holdColor = new Color(0.4f, 0.75f, 1f, 0.38f);
                        break;
                    case HoldOrder.Food:
                        holdColor = new Color(0.5f, 0.85f, 0.48f, 0.38f);
                        break;
                    case HoldOrder.Auto:
                        holdColor = new Color(0.9f, 0.78f, 0.58f, 0.2f);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(_game.HoldOrder), _game.HoldOrder, null);
                }
                EnsureRing("hold-order", holdHub, glow, holdColor);
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
            var cut = _game.ActiveCut();
            if (cut != null)
            {
                live.Add(cut.Id);
                PlaceGhost(cut, Color.Lerp(MesaView.RailCut, MesaView.PadRoute, 0.45f + 0.35f * Mathf.Abs(Mathf.Sin(Time.time * 8f))));
            }
            var threat = _game.HottestRailThreat();
            if (threat != null && (cut == null || cut.Id != threat.Id))
            {
                live.Add(threat.Id);
                var mag = Color.Lerp(new Color(0.95f, 0.42f, 0.78f), new Color(1f, 0.72f, 0.35f),
                    0.5f + 0.5f * Mathf.Abs(Mathf.Sin(Time.time * (_game.HottestThreatImminent() ? 11f : 6f))));
                PlaceGhost(threat, mag);
            }
            if (_game.SelectedTool == Tool.Route && _game.RouteFrom != null)
            {
                foreach (var end in _game.RouteEnds())
                {
                    var edge = _game.EdgeBetween(_game.RouteFrom, end);
                    if (edge == null) continue;
                    if (edge.Routed && edge.SabotagedUntil <= _game.T) continue;
                    live.Add(edge.Id);
                    PlaceGhost(edge, MesaView.PadRoute);
                }
            }
            Prune(_ghosts, live);
        }

        void PlaceGhost(SimEdge edge, Color color)
        {
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
            var pa = new Vector3(a.X, 0.54f, a.Z);
            var pb = new Vector3(b.X, 0.54f, b.Z);
            rail.position = (pa + pb) * 0.5f;
            rail.localScale = new Vector3(0.14f, 0.05f, Vector3.Distance(pa, pb));
            rail.rotation = Quaternion.LookRotation(pb - pa);
            MesaView.Tint(rail.gameObject, color);
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

        void ClearIntents()
        {
            foreach (var lr in _intents.Values)
                if (lr != null) Destroy(lr.gameObject);
            _intents.Clear();
        }

        void SyncRunnerIntents()
        {
            var live = new HashSet<string>();
            foreach (var e in _game.Enemies)
            {
                if (e.Type != EnemyType.Runner || e.Path.Count == 0) continue;
                if (!_game.Nodes.TryGetValue(e.Path[0], out var next)) continue;
                live.Add(e.Id);
                if (!_intents.TryGetValue(e.Id, out var lr))
                {
                    var go = new GameObject("intent");
                    go.transform.SetParent(_root, false);
                    lr = go.AddComponent<LineRenderer>();
                    lr.useWorldSpace = true;
                    lr.startWidth = 0.09f;
                    lr.endWidth = 0.02f;
                    var shader = Shader.Find("Hidden/Internal-Colored")
                        ?? Shader.Find("Sprites/Default")
                        ?? Shader.Find("Unlit/Color")
                        ?? Shader.Find("Standard");
                    if (shader != null) lr.material = new Material(shader);
                    _intents[e.Id] = lr;
                }
                lr.positionCount = 2;
                lr.SetPosition(0, new Vector3(e.X, 0.72f, e.Z));
                lr.SetPosition(1, new Vector3(next.X, 0.62f, next.Z));
                var mag = new Color(0.95f, 0.42f, 0.78f, 0.85f);
                lr.startColor = mag;
                lr.endColor = mag;
                lr.enabled = true;
            }
            var dead = new List<string>();
            foreach (var kv in _intents)
                if (!live.Contains(kv.Key)) dead.Add(kv.Key);
            foreach (var id in dead)
            {
                if (_intents[id] != null) Destroy(_intents[id].gameObject);
                _intents.Remove(id);
            }
        }

        void PingBraceInbound()
        {
            var h = _game.BraceInbound();
            if (h == null)
            {
                _bracePinged.Clear();
                return;
            }
            var eta = _game.HaulEtaToHub(h);
            if (eta < 0f || eta > 2.4f) return;
            if (!_bracePinged.Add(h.Id)) return;
            _juice.BraceComing(h.X, h.Z);
        }

        void PingChew()
        {
            var n = _game.HubChewers();
            if (n <= 0)
            {
                _chewPinged = false;
                return;
            }
            if (_chewPinged) return;
            _chewPinged = true;
            _juice.Chew(0f, 0f);
        }

        void PingRailThreat()
        {
            var live = new HashSet<string>();
            foreach (var e in _game.Enemies)
            {
                var edge = _game.RunnerThreatEdge(e);
                if (edge == null) continue;
                live.Add(edge.Id);
                var a = _game.Nodes[edge.A];
                var b = _game.Nodes[edge.B];
                var mx = (a.X + b.X) * 0.5f;
                var mz = (a.Z + b.Z) * 0.5f;
                var imminent = _game.RunnerThreatImminent(e);
                if (imminent)
                {
                    if (_cutSoonPinged.Add(edge.Id))
                        _juice.RailThreat(mx, mz, true);
                }
                else if (_threatPinged.Add(edge.Id))
                    _juice.RailThreat(mx, mz, false);
            }
            if (live.Count == 0)
            {
                _threatPinged.Clear();
                _cutSoonPinged.Clear();
                return;
            }
            var dead = new List<string>();
            foreach (var id in _threatPinged)
                if (!live.Contains(id)) dead.Add(id);
            foreach (var id in dead) _threatPinged.Remove(id);
            dead.Clear();
            foreach (var id in _cutSoonPinged)
                if (!live.Contains(id)) dead.Add(id);
            foreach (var id in dead) _cutSoonPinged.Remove(id);
        }

        void PingGunsDry()
        {
            if (!_game.GunsDry())
            {
                _powerPinged.Clear();
                return;
            }
            var h = _game.PowerInbound();
            if (h == null) return;
            var eta = _game.HaulEtaToHub(h);
            if (eta < 0f || eta > 2.4f) return;
            if (!_powerPinged.Add(h.Id)) return;
            _juice.PowerComing(h.X, h.Z);
        }

        void PingGunsLow()
        {
            if (!_game.GunsLow())
            {
                _lowPinged.Clear();
                return;
            }
            var h = _game.GunsLowInbound();
            if (h == null) return;
            var brace = _game.BraceInbound();
            if (brace != null && brace.Id == h.Id) return;
            var eta = _game.HaulEtaToHub(h);
            if (eta < 0f || eta > 2.4f) return;
            if (!_lowPinged.Add(h.Id)) return;
            _juice.PowerComing(h.X, h.Z);
        }

        void PingCoreBound()
        {
            var n = _game.HubClosers();
            if (n <= 0)
            {
                _closePinged = false;
                _atPadPinged = false;
                return;
            }
            var hot = _game.HottestCloser();
            var hx = hot != null ? hot.X : 0f;
            var hz = hot != null ? hot.Z : 0f;
            if (!_closePinged)
            {
                _closePinged = true;
                _juice.CoreBound(hx, hz, false);
            }
            if (_game.AnyCloseImminent() && !_atPadPinged)
            {
                _atPadPinged = true;
                _juice.CoreBound(hx, hz, true);
            }
        }

        void PingWaveClear()
        {
            if (!_game.WaveClearLive())
            {
                _clearPinged = false;
                return;
            }
            if (_clearPinged) return;
            _clearPinged = true;
            _juice.WaveClear();
            _hud.Flash("WAVE CLEAR — next in " + GameSim.CeilSecs(_game.NextWaveIn) + "s", 2.2f,
                new Color(0.22f, 0.52f, 0.32f, 0.95f));
        }

        void PingCutStake()
        {
            if (_game.ActiveCut() == null)
            {
                _braceCutPinged = false;
                return;
            }
            if (_game.CutStakeOf() != CutStake.Brace)
                return;
            if (_braceCutPinged) return;
            _braceCutPinged = true;
            _juice.CutStakeBrace();
            _hud.Flash("HAUL CUT — BRACE haul stuck", 1.8f, new Color(0.95f, 0.38f, 0.18f, 0.95f));
        }

        void PingOffline()
        {
            var id = _game.OfflinePad();
            if (id == null)
            {
                _offlinePinged = null;
                return;
            }
            if (_offlinePinged == id) return;
            _offlinePinged = id;
            if (!_game.Nodes.TryGetValue(id, out var node)) return;
            _juice.PadOffline(node.X, node.Z);
            _hud.Flash(_game.OfflineFlash() ?? "PAD OFFLINE — rail it home", 1.8f,
                new Color(0.92f, 0.55f, 0.22f, 0.95f));
        }

        void PingSitting()
        {
            var b = _game.SittingStock();
            if (b == null)
            {
                _sitPinged = null;
                return;
            }
            if (_sitPinged == b.NodeId) return;
            _sitPinged = b.NodeId;
            if (!_game.Nodes.TryGetValue(b.NodeId, out var node)) return;
            string tag;
            switch (b.Type)
            {
                case BuildingType.Power: tag = "PWR"; break;
                case BuildingType.Farm: tag = "FOOD"; break;
                case BuildingType.Mine: tag = "ORE"; break;
                case BuildingType.Hub:
                case BuildingType.Depot:
                case BuildingType.Kinetic:
                case BuildingType.Splash:
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(b.Type), b.Type, null);
            }
            _juice.SittingStock(node.X, node.Z, tag);
            _hud.Flash(_game.SittingFlash() ?? "SIT — haul the piled pad", 1.6f,
                new Color(0.95f, 0.72f, 0.28f, 0.95f));
        }

        void PingL2Ready()
        {
            if (!_game.L2Ready())
            {
                _l2ReadyPinged = false;
                return;
            }
            if (_l2ReadyPinged) return;
            _l2ReadyPinged = true;
            _juice.L2Ready();
            _hud.Flash(_game.L2ReadyFlash() ?? "L2 READY — press U · Splash next", 2.0f,
                new Color(0.55f, 0.42f, 0.12f, 0.95f));
        }

        void PingOpenChoke()
        {
            var id = _game.OpenChokeId();
            if (id == null)
            {
                _openPinged = null;
                return;
            }
            if (_openPinged == id) return;
            _openPinged = id;
            if (!_game.Nodes.TryGetValue(id, out var node)) return;
            _juice.OpenChoke(node.X, node.Z, _game.OpenChokeLane() ?? "OPEN");
            _hud.Flash(_game.OpenChokeFlash() ?? "OPEN CHOKE — plant a gun", 1.8f,
                new Color(0.72f, 0.22f, 0.12f, 0.95f));
        }

        void PingSlowChoke()
        {
            var id = _game.SlowChokeId();
            if (id == null)
            {
                _slowPinged = null;
                return;
            }
            if (_slowPinged == id) return;
            _slowPinged = id;
            if (!_game.Nodes.TryGetValue(id, out var node)) return;
            _juice.SlowChoke(node.X, node.Z, _game.SlowChokeLane() ?? "SLOW");
            _hud.Flash(_game.SlowChokeFlash() ?? "SLOW — Barrier the choke", 1.7f,
                new Color(0.72f, 0.18f, 0.16f, 0.95f));
        }

        void PingStretch()
        {
            var id = _game.StretchPad();
            if (id == null)
            {
                _stretchPinged = null;
                return;
            }
            if (_stretchPinged == id) return;
            _stretchPinged = id;
            if (!_game.Nodes.TryGetValue(id, out var node)) return;
            _juice.CrewStretch(node.X, node.Z);
            _hud.Flash(_game.StretchFlash() ?? "CREW STRETCH — rail beats a new pad", 1.7f,
                new Color(0.72f, 0.38f, 0.12f, 0.95f));
        }

        void PingHoldReady()
        {
            if (!_game.HoldReadyLive())
            {
                _holdReadyPinged = false;
                return;
            }
            if (_holdReadyPinged) return;
            _holdReadyPinged = true;
            _juice.HoldReady();
            _hud.Flash(_game.HoldReadyFlash() ?? "HOLD READY — H locks haulers on Power or Food", 2.2f,
                new Color(0.55f, 0.42f, 0.12f, 0.95f));
        }

        void PingHomeInbound()
        {
            var h = _game.HomeInbound();
            if (h == null)
            {
                _homePinged.Clear();
                _homeFlash = false;
                return;
            }
            if (!_homeFlash)
            {
                _homeFlash = true;
                _juice.HaulHome(h.X, h.Z, GameSim.CargoTag(h.CargoKind));
                _hud.Flash(_game.HomeInboundFlash() ?? "HAUL HOME — keep the rail feeding Hub", 1.6f,
                    new Color(0.48f, 0.38f, 0.12f, 0.95f));
            }
            var eta = _game.HaulEtaToHub(h);
            if (eta < 0f || eta > 2.4f) return;
            if (!_homePinged.Add(h.Id)) return;
            _juice.HaulHome(h.X, h.Z, GameSim.CargoTag(h.CargoKind));
        }

        void PingPackIn()
        {
            if (!_game.PackInLive())
            {
                _packPinged = -1;
                return;
            }
            var next = _game.WaveIndex + 1;
            if (_packPinged == next) return;
            _packPinged = next;
            foreach (var spawnId in _game.NextWaveSpawns())
            {
                if (!_game.Nodes.TryGetValue(spawnId, out var spawn)) continue;
                var n = _game.NextWaveCount(spawnId, EnemyType.Grunt)
                    + _game.NextWaveCount(spawnId, EnemyType.Brute)
                    + _game.NextWaveCount(spawnId, EnemyType.Runner);
                if (n <= 0) continue;
                _juice.PackIn(spawn.X, spawn.Z, n);
            }
            _hud.Flash(_game.PackInFlash() ?? "PACK IN — gun the lane / haul Power", 2.0f,
                new Color(0.55f, 0.12f, 0.1f, 0.95f));
        }

        void PingGunsUp()
        {
            var id = _game.GunsUpNodeId();
            if (id == null)
            {
                _gunsUpPinged = null;
                return;
            }
            if (_gunsUpPinged == id) return;
            _gunsUpPinged = id;
            if (!_game.Nodes.TryGetValue(id, out var node)) return;
            _juice.GunsUp(node.X, node.Z, _game.GunsUpLane());
            _hud.Flash(_game.GunsUpFlash() ?? "GUNS UP — haul Power so the choke fires", 2.0f,
                new Color(0.12f, 0.42f, 0.4f, 0.95f));
        }

        void PingRailLive()
        {
            var key = _game.RailLivePingKey();
            if (key == null)
            {
                _railLivePinged = null;
                return;
            }
            if (_railLivePinged == key) return;
            _railLivePinged = key;
            var id = _game.RailLiveEdgeId();
            if (id == null || !_game.Edges.TryGetValue(id, out var edge)) return;
            if (!_game.Nodes.TryGetValue(edge.A, out var a) || !_game.Nodes.TryGetValue(edge.B, out var b))
                return;
            _juice.RailLive((a.X + b.X) * 0.5f, (a.Z + b.Z) * 0.5f, _game.RailLiveChip());
            _hud.Flash(_game.RailLiveFlash() ?? "RAIL LIVE — haulers rolling", 2.0f,
                new Color(0.12f, 0.42f, 0.4f, 0.95f));
        }

        void SyncGunLocks()
        {
            var live = new HashSet<string>();
            foreach (var b in _game.Buildings.Values)
            {
                var target = _game.TowerLock(b);
                if (target == null) continue;
                if (!_game.Nodes.TryGetValue(b.NodeId, out var node)) continue;
                live.Add(b.Id);
                if (!_locks.TryGetValue(b.Id, out var lr))
                {
                    lr = MesaView.MakeLine(_root, "lock", 0.09f, 0.02f);
                    _locks[b.Id] = lr;
                }
                lr.positionCount = 2;
                lr.SetPosition(0, new Vector3(node.X, 1.15f, node.Z));
                lr.SetPosition(1, new Vector3(target.X, 0.85f, target.Z));
                Color color;
                if (_game.PowerBrownout)
                    color = new Color(1f, 0.35f, 0.28f, 0.85f);
                else if (b.Type == BuildingType.Splash)
                    color = new Color(0.94f, 0.63f, 0.38f, 0.88f);
                else if (b.Type == BuildingType.Hub)
                    color = new Color(0.9f, 0.78f, 0.5f, 0.8f);
                else
                    color = new Color(0.45f, 0.95f, 0.9f, 0.88f);
                lr.startColor = color;
                lr.endColor = color;
                lr.enabled = true;
            }
            var dead = new List<string>();
            foreach (var kv in _locks)
                if (!live.Contains(kv.Key)) dead.Add(kv.Key);
            foreach (var id in dead)
            {
                if (_locks[id] != null) Destroy(_locks[id].gameObject);
                _locks.Remove(id);
            }
        }

        void SyncChews()
        {
            var live = new HashSet<string>();
            foreach (var e in _game.Enemies)
            {
                if (!_game.ChewingHub(e)) continue;
                live.Add(e.Id);
                if (!_chews.TryGetValue(e.Id, out var lr))
                {
                    lr = MesaView.MakeLine(_root, "chew", 0.11f, 0.03f);
                    _chews[e.Id] = lr;
                }
                lr.positionCount = 2;
                lr.SetPosition(0, new Vector3(e.X, 0.95f, e.Z));
                lr.SetPosition(1, new Vector3(0f, 1.2f, 0f));
                var a = 0.55f + 0.35f * Mathf.Abs(Mathf.Sin(Time.time * 11f));
                var color = _game.Surging
                    ? new Color(0.45f, 0.9f, 1f, a)
                    : new Color(1f, 0.28f, 0.16f, a);
                lr.startColor = color;
                lr.endColor = color;
                lr.enabled = true;
            }
            var dead = new List<string>();
            foreach (var kv in _chews)
                if (!live.Contains(kv.Key)) dead.Add(kv.Key);
            foreach (var id in dead)
            {
                if (_chews[id] != null) Destroy(_chews[id].gameObject);
                _chews.Remove(id);
            }
        }

        void SyncCloses()
        {
            var live = new HashSet<string>();
            foreach (var e in _game.Enemies)
            {
                if (!_game.ClosingOnHub(e)) continue;
                live.Add(e.Id);
                if (!_closes.TryGetValue(e.Id, out var lr))
                {
                    lr = MesaView.MakeLine(_root, "close", 0.08f, 0.02f);
                    _closes[e.Id] = lr;
                }
                lr.positionCount = 2;
                lr.SetPosition(0, new Vector3(e.X, 0.9f, e.Z));
                lr.SetPosition(1, new Vector3(0f, 1.05f, 0f));
                var a = 0.45f + 0.35f * Mathf.Abs(Mathf.Sin(Time.time * 10f));
                var imminent = _game.ClosingImminent(e);
                var color = imminent
                    ? new Color(1f, 0.28f, 0.14f, a)
                    : new Color(1f, 0.42f, 0.22f, a);
                lr.startColor = color;
                lr.endColor = color;
                lr.enabled = true;
            }
            var dead = new List<string>();
            foreach (var kv in _closes)
                if (!live.Contains(kv.Key)) dead.Add(kv.Key);
            foreach (var id in dead)
            {
                if (_closes[id] != null) Destroy(_closes[id].gameObject);
                _closes.Remove(id);
            }
        }

        void SyncBraceLine()
        {
            var h = _game.BraceInbound();
            var power = _game.PowerInbound();
            if (h != null && power != null && h.Id == power.Id)
                h = null;
            if (h == null)
            {
                if (_braceLine != null) _braceLine.enabled = false;
                return;
            }
            if (_braceLine == null)
                _braceLine = MesaView.MakeLine(_root, "brace-line", 0.12f, 0.04f);
            _braceLine.enabled = true;
            _braceLine.positionCount = 2;
            _braceLine.SetPosition(0, new Vector3(h.X, 0.72f, h.Z));
            _braceLine.SetPosition(1, new Vector3(0f, 0.9f, 0f));
            var pulse = 0.55f + 0.35f * Mathf.Abs(Mathf.Sin(Time.time * 8f));
            var color = new Color(0.45f, 0.9f, 1f, pulse);
            _braceLine.startColor = color;
            _braceLine.endColor = color;
        }

        void SyncPowerLine()
        {
            var h = _game.PowerInbound();
            if (h == null)
            {
                h = _game.GunsLowInbound();
                var brace = _game.BraceInbound();
                if (h != null && brace != null && h.Id == brace.Id)
                    h = null;
            }
            if (h == null)
            {
                if (_powerLine != null) _powerLine.enabled = false;
                return;
            }
            if (_powerLine == null)
                _powerLine = MesaView.MakeLine(_root, "power-line", 0.13f, 0.04f);
            _powerLine.enabled = true;
            _powerLine.positionCount = 2;
            _powerLine.SetPosition(0, new Vector3(h.X, 0.78f, h.Z));
            _powerLine.SetPosition(1, new Vector3(0f, 0.95f, 0f));
            var pulse = 0.6f + 0.35f * Mathf.Abs(Mathf.Sin(Time.time * 9f));
            var color = new Color(1f, 0.5f, 0.2f, pulse);
            _powerLine.startColor = color;
            _powerLine.endColor = color;
        }

        void SyncHomeLine()
        {
            var h = _game.HomeInbound();
            var power = _game.PowerInbound() ?? _game.GunsLowInbound();
            if (h != null && power != null && h.Id == power.Id)
                h = null;
            if (h == null)
            {
                if (_homeLine != null) _homeLine.enabled = false;
                return;
            }
            if (_homeLine == null)
                _homeLine = MesaView.MakeLine(_root, "home-line", 0.11f, 0.035f);
            _homeLine.enabled = true;
            _homeLine.positionCount = 2;
            _homeLine.SetPosition(0, new Vector3(h.X, 0.74f, h.Z));
            _homeLine.SetPosition(1, new Vector3(0f, 0.88f, 0f));
            var pulse = 0.55f + 0.3f * Mathf.Abs(Mathf.Sin(Time.time * 6f));
            var color = HomeCargoColor(h.CargoKind, pulse);
            _homeLine.startColor = color;
            _homeLine.endColor = color;
        }

        static Color HomeCargoColor(Resource? kind, float pulse)
        {
            var k = kind ?? Resource.Ore;
            switch (k)
            {
                case Resource.Ore: return new Color(0.94f, 0.64f, 0.23f, pulse);
                case Resource.Food: return new Color(0.5f, 0.85f, 0.48f, pulse);
                case Resource.Power: return new Color(0.4f, 0.75f, 1f, pulse);
                default: throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }
        }

        void SyncOfflineLine()
        {
            var id = _game.OfflinePad();
            if (id == null || !_game.Nodes.TryGetValue(id, out var node))
            {
                if (_offlineLine != null) _offlineLine.enabled = false;
                return;
            }
            if (_offlineLine == null)
                _offlineLine = MesaView.MakeLine(_root, "offline-line", 0.1f, 0.03f);
            _offlineLine.enabled = true;
            _offlineLine.positionCount = 2;
            _offlineLine.SetPosition(0, new Vector3(node.X, 0.7f, node.Z));
            _offlineLine.SetPosition(1, new Vector3(0f, 0.85f, 0f));
            var pulse = 0.45f + 0.35f * Mathf.Abs(Mathf.Sin(Time.time * 6f));
            var color = new Color(0.92f, 0.58f, 0.22f, pulse);
            _offlineLine.startColor = color;
            _offlineLine.endColor = color;
        }

        void SyncSitLine()
        {
            var b = _game.SittingStock();
            var h = _game.IdleHauler();
            if (b == null || h == null || !_game.Nodes.TryGetValue(b.NodeId, out var node))
            {
                if (_sitLine != null) _sitLine.enabled = false;
                return;
            }
            if (_sitLine == null)
                _sitLine = MesaView.MakeLine(_root, "sit-line", 0.09f, 0.03f);
            _sitLine.enabled = true;
            _sitLine.positionCount = 2;
            _sitLine.SetPosition(0, new Vector3(h.X, 0.62f, h.Z));
            _sitLine.SetPosition(1, new Vector3(node.X, 0.78f, node.Z));
            var pulse = 0.4f + 0.35f * Mathf.Abs(Mathf.Sin(Time.time * 5.5f));
            Color color;
            if (b.Type == BuildingType.Power) color = new Color(0.4f, 0.75f, 1f, pulse);
            else if (b.Type == BuildingType.Farm) color = new Color(0.5f, 0.85f, 0.48f, pulse);
            else color = new Color(0.94f, 0.64f, 0.23f, pulse);
            _sitLine.startColor = color;
            _sitLine.endColor = color;
        }

        void SyncCargoTags()
        {
            var live = new HashSet<string>();
            foreach (var h in _game.Haulers)
            {
                if (h.CargoAmount <= 0) continue;
                live.Add(h.Id);
                if (!_cargoTags.TryGetValue(h.Id, out var tag))
                {
                    var go = new GameObject("cargo");
                    go.transform.SetParent(_root, false);
                    var tm = go.AddComponent<TextMesh>();
                    tm.anchor = TextAnchor.MiddleCenter;
                    tm.alignment = TextAlignment.Center;
                    tm.fontSize = 42;
                    tm.characterSize = 0.055f;
                    tag = go.transform;
                    _cargoTags[h.Id] = tag;
                }
                tag.position = new Vector3(h.X, 1.22f, h.Z);
                tag.rotation = Quaternion.Euler(90f, 45f, 0f);
                var tm2 = tag.GetComponent<TextMesh>();
                if (h.CargoKind == Resource.Food)
                {
                    tm2.text = "FOOD";
                    tm2.color = new Color(0.5f, 0.85f, 0.48f);
                }
                else if (h.CargoKind == Resource.Power)
                {
                    tm2.text = "PWR";
                    tm2.color = new Color(0.4f, 0.75f, 1f);
                }
                else
                {
                    tm2.text = "ORE";
                    tm2.color = new Color(0.94f, 0.64f, 0.23f);
                }
            }
            Prune(_cargoTags, live);
        }

        void SyncForecasts()
        {
            var live = new HashSet<string>();
            if (_game.WaveForecastLive)
            {
                foreach (var spawnId in _game.NextWaveSpawns())
                {
                    if (!_game.Nodes.TryGetValue(spawnId, out var spawn)) continue;
                    var types = new[] { EnemyType.Grunt, EnemyType.Brute, EnemyType.Runner };
                    var slot = 0;
                    foreach (var type in types)
                    {
                        var n = _game.NextWaveCount(spawnId, type);
                        if (n <= 0) continue;
                        var id = spawnId + "-" + type;
                        live.Add(id);
                        if (!_forecasts.TryGetValue(id, out var ghost))
                        {
                            var go = GameObject.CreatePrimitive(MesaView.EnemyPrim(type));
                            go.name = "forecast";
                            go.transform.SetParent(_root, false);
                            var col = go.GetComponent<Collider>();
                            if (col != null) Destroy(col);
                            ghost = go.transform;
                            _forecasts[id] = ghost;
                        }
                        var outward = new Vector3(spawn.X, 0f, spawn.Z);
                        if (outward.sqrMagnitude < 0.01f) outward = Vector3.right;
                        outward.Normalize();
                        var side = Vector3.Cross(Vector3.up, outward);
                        var offset = outward * 1.15f + side * ((slot - 1) * 0.55f);
                        ghost.position = new Vector3(spawn.X + offset.x, 0.7f, spawn.Z + offset.z);
                        var scale = MesaView.EnemyScale(type) * (0.72f + 0.04f * n);
                        var pulse = _game.PackInLive()
                            ? 0.92f + 0.18f * Mathf.Abs(Mathf.Sin(Time.time * 7f + slot))
                            : 0.85f + 0.15f * Mathf.Abs(Mathf.Sin(Time.time * 4f + slot));
                        ghost.localScale = scale * pulse;
                        var c = MesaView.EnemyColor(type);
                        c.a = _game.PackInLive() ? 0.62f : 0.45f;
                        MesaView.Tint(ghost.gameObject, c * (_game.PackInLive() ? 0.75f : 0.55f));
                        slot++;
                    }
                }
            }
            Prune(_forecasts, live);
        }

        void ClearLocks()
        {
            foreach (var lr in _locks.Values)
                if (lr != null) Destroy(lr.gameObject);
            _locks.Clear();
        }

        void ClearChews()
        {
            foreach (var lr in _chews.Values)
                if (lr != null) Destroy(lr.gameObject);
            _chews.Clear();
        }

        void ClearCloses()
        {
            foreach (var lr in _closes.Values)
                if (lr != null) Destroy(lr.gameObject);
            _closes.Clear();
        }

        void ClearForecasts()
        {
            ClearMap(_forecasts);
        }

        void ClearCargoTags()
        {
            ClearMap(_cargoTags);
        }

        void ClearBraceLine()
        {
            if (_braceLine != null)
            {
                Destroy(_braceLine.gameObject);
                _braceLine = null;
            }
        }

        void ClearPowerLine()
        {
            if (_powerLine != null)
            {
                Destroy(_powerLine.gameObject);
                _powerLine = null;
            }
        }

        void ClearOfflineLine()
        {
            if (_offlineLine != null)
            {
                Destroy(_offlineLine.gameObject);
                _offlineLine = null;
            }
        }

        void ClearSitLine()
        {
            if (_sitLine != null)
            {
                Destroy(_sitLine.gameObject);
                _sitLine = null;
            }
        }

        void ClearHomeLine()
        {
            if (_homeLine != null)
            {
                Destroy(_homeLine.gameObject);
                _homeLine = null;
            }
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
            if (_game.Nodes.TryGetValue("hub", out var hub))
            {
                var hubSp = _cam.WorldToScreenPoint(new Vector3(hub.X, 2.35f, hub.Z));
                if (hubSp.z > 0f)
                {
                    var hx = hubSp.x;
                    var hy = Screen.height - hubSp.y;
                    var hp = Mathf.Clamp01(_game.HubHp / Balance.HubMaxHp);
                    GUI.backgroundColor = new Color(0f, 0f, 0f, 0.7f);
                    GUI.Box(new Rect(hx - 42f, hy, 84f, 9f), "");
                    GUI.backgroundColor = _game.CoreThin
                        ? Color.Lerp(new Color(0.55f, 0.08f, 0.08f), new Color(1f, 0.32f, 0.22f), hp)
                        : Color.Lerp(new Color(0.85f, 0.18f, 0.16f), new Color(0.9f, 0.78f, 0.5f), hp);
                    GUI.Box(new Rect(hx - 42f, hy, 84f * hp, 9f), "");
                    GUI.backgroundColor = Color.white;
                    var chewers = _game.HubChewers();
                    if (chewers > 0)
                    {
                        GUI.backgroundColor = _game.Surging
                            ? new Color(0.12f, 0.42f, 0.55f, 0.9f)
                            : new Color(0.72f, 0.12f, 0.1f, 0.92f);
                        GUI.Box(new Rect(hx - 46f, hy - 18f, 92f, 16f),
                            _game.Surging ? "SHRUG " + chewers : "CHEW " + chewers);
                        GUI.backgroundColor = Color.white;
                    }
                    else if (_game.HubClosers() > 0)
                    {
                        GUI.backgroundColor = new Color(0.72f, 0.14f, 0.08f, 0.92f);
                        GUI.Box(new Rect(hx - 46f, hy - 18f, 92f, 16f),
                            _game.AnyCloseImminent() ? "PAD" : "IN " + _game.HubClosers());
                        GUI.backgroundColor = Color.white;
                    }
                    else if (_game.GunsDry())
                    {
                        GUI.backgroundColor = new Color(0.72f, 0.28f, 0.08f, 0.92f);
                        GUI.Box(new Rect(hx - 46f, hy - 18f, 92f, 16f), "DRY");
                        GUI.backgroundColor = Color.white;
                    }
                    else if (_game.RailLiveLive())
                    {
                        GUI.backgroundColor = new Color(0.12f, 0.42f, 0.4f, 0.92f);
                        GUI.Box(new Rect(hx - 46f, hy - 18f, 92f, 16f),
                            _game.RailLiveChip() ?? "LIVE");
                        GUI.backgroundColor = Color.white;
                    }
                    else if (_game.WaveClearLive())
                    {
                        GUI.backgroundColor = new Color(0.12f, 0.42f, 0.22f, 0.92f);
                        GUI.Box(new Rect(hx - 46f, hy - 18f, 92f, 16f),
                            "CLEAR " + GameSim.CeilSecs(_game.NextWaveIn) + "s");
                        GUI.backgroundColor = Color.white;
                    }
                    else if (_game.CoreThinLive())
                    {
                        GUI.backgroundColor = new Color(0.62f, 0.1f, 0.08f, 0.92f);
                        GUI.Box(new Rect(hx - 46f, hy - 18f, 92f, 16f),
                            _game.CoreThinChip() ?? "THIN");
                        GUI.backgroundColor = Color.white;
                    }
                    else if (_game.L2ReadyWorld())
                    {
                        GUI.backgroundColor = new Color(0.48f, 0.36f, 0.08f, 0.92f);
                        GUI.Box(new Rect(hx - 46f, hy - 18f, 92f, 16f),
                            _game.L2ReadyChip() ?? "L2 READY");
                        GUI.backgroundColor = Color.white;
                    }
                    else if (_game.HoldReadyWorld())
                    {
                        GUI.backgroundColor = new Color(0.42f, 0.32f, 0.08f, 0.92f);
                        GUI.Box(new Rect(hx - 46f, hy - 18f, 92f, 16f),
                            _game.HoldReadyChip() ?? "HOLD");
                        GUI.backgroundColor = Color.white;
                    }
                    else if (_game.PackInLive())
                    {
                        GUI.backgroundColor = new Color(0.55f, 0.12f, 0.1f, 0.92f);
                        GUI.Box(new Rect(hx - 46f, hy - 18f, 92f, 16f),
                            _game.PackInChip() ?? "PACK");
                        GUI.backgroundColor = Color.white;
                    }
                    else if (_game.GunsUpWorld())
                    {
                        GUI.backgroundColor = new Color(0.12f, 0.42f, 0.4f, 0.92f);
                        GUI.Box(new Rect(hx - 46f, hy - 18f, 92f, 16f),
                            _game.GunsUpChip() ?? "GUNS");
                        GUI.backgroundColor = Color.white;
                    }
                }
            }
            var inbound = _game.BraceInbound();
            var power = _game.PowerInbound();
            if (power == null)
            {
                power = _game.GunsLowInbound();
                if (power != null && inbound != null && power.Id == inbound.Id)
                    power = null;
            }
            if (inbound != null && (power == null || power.Id != inbound.Id))
            {
                var isp = _cam.WorldToScreenPoint(new Vector3(inbound.X, 1.55f, inbound.Z));
                if (isp.z > 0f)
                {
                    var ix = isp.x;
                    var iy = Screen.height - isp.y;
                    var eta = _game.HaulEtaToHub(inbound);
                    var chip = eta <= 0.35f ? "BRACE NOW" : "BRACE " + GameSim.CeilSecs(eta) + "s";
                    GUI.backgroundColor = new Color(0.12f, 0.42f, 0.55f, 0.9f);
                    GUI.Box(new Rect(ix - 46f, iy - 14f, 92f, 22f), chip);
                    GUI.backgroundColor = Color.white;
                }
            }
            if (power != null)
            {
                var psp = _cam.WorldToScreenPoint(new Vector3(power.X, 1.55f, power.Z));
                if (psp.z > 0f)
                {
                    var eta = _game.HaulEtaToHub(power);
                    var chip = eta <= 0.35f ? "POWER NOW" : "POWER " + GameSim.CeilSecs(eta) + "s";
                    GUI.backgroundColor = new Color(0.72f, 0.28f, 0.08f, 0.9f);
                    GUI.Box(new Rect(psp.x - 46f, Screen.height - psp.y - 14f, 92f, 22f), chip);
                    GUI.backgroundColor = Color.white;
                }
            }
            var home = _game.HomeInbound();
            if (home != null && (power == null || power.Id != home.Id))
            {
                var hsp = _cam.WorldToScreenPoint(new Vector3(home.X, 1.55f, home.Z));
                if (hsp.z > 0f)
                {
                    GUI.backgroundColor = HomeCargoColor(home.CargoKind, 0.92f);
                    GUI.Box(new Rect(hsp.x - 46f, Screen.height - hsp.y - 14f, 92f, 22f),
                        _game.HomeInboundChip() ?? "HOME");
                    GUI.backgroundColor = Color.white;
                }
            }
            var closer = _game.HottestCloser();
            if (closer != null)
            {
                var csp = _cam.WorldToScreenPoint(new Vector3(closer.X, 1.5f, closer.Z));
                if (csp.z > 0f)
                {
                    GUI.backgroundColor = new Color(0.72f, 0.14f, 0.08f, 0.9f);
                    GUI.Box(new Rect(csp.x - 40f, Screen.height - csp.y - 14f, 80f, 22f),
                        _game.ClosingImminent(closer) ? "PAD" : "IN");
                    GUI.backgroundColor = Color.white;
                }
            }
            var threat = _game.HottestRailThreat();
            if (threat != null && _game.Nodes.TryGetValue(threat.A, out var ta) && _game.Nodes.TryGetValue(threat.B, out var tb))
            {
                var tmid = new Vector3((ta.X + tb.X) * 0.5f, 1.2f, (ta.Z + tb.Z) * 0.5f);
                var tsp = _cam.WorldToScreenPoint(tmid);
                if (tsp.z > 0f)
                {
                    GUI.backgroundColor = new Color(0.55f, 0.12f, 0.42f, 0.9f);
                    GUI.Box(new Rect(tsp.x - 46f, Screen.height - tsp.y - 12f, 92f, 22f),
                        _game.HottestThreatImminent() ? "CUT NOW" : "CUT?");
                    GUI.backgroundColor = Color.white;
                }
            }
            var offId = _game.OfflinePad();
            if (offId != null && _game.Nodes.TryGetValue(offId, out var offNode))
            {
                var osp = _cam.WorldToScreenPoint(new Vector3(offNode.X, 1.55f, offNode.Z));
                if (osp.z > 0f)
                {
                    GUI.backgroundColor = new Color(0.62f, 0.32f, 0.08f, 0.92f);
                    GUI.Box(new Rect(osp.x - 46f, Screen.height - osp.y - 12f, 92f, 22f), "OFFLINE");
                    GUI.backgroundColor = Color.white;
                }
            }
            var sit = _game.SittingStock();
            if (sit != null && _game.Nodes.TryGetValue(sit.NodeId, out var sitNode))
            {
                var ssp = _cam.WorldToScreenPoint(new Vector3(sitNode.X, 1.55f, sitNode.Z));
                if (ssp.z > 0f)
                {
                    GUI.backgroundColor = new Color(0.55f, 0.38f, 0.08f, 0.92f);
                    GUI.Box(new Rect(ssp.x - 50f, Screen.height - ssp.y - 12f, 100f, 22f),
                        _game.SittingChip() ?? "HAUL");
                    GUI.backgroundColor = Color.white;
                }
            }
            var openId = _game.OpenChokeId();
            if (openId != null && _game.Nodes.TryGetValue(openId, out var openHud))
            {
                var osp = _cam.WorldToScreenPoint(new Vector3(openHud.X, 1.55f, openHud.Z));
                if (osp.z > 0f)
                {
                    GUI.backgroundColor = new Color(0.62f, 0.18f, 0.1f, 0.92f);
                    GUI.Box(new Rect(osp.x - 50f, Screen.height - osp.y - 12f, 100f, 22f),
                        _game.OpenChokeChip() ?? "OPEN");
                    GUI.backgroundColor = Color.white;
                }
            }
            var slowId = _game.SlowChokeId();
            if (slowId != null && _game.Nodes.TryGetValue(slowId, out var slowHud))
            {
                var slp = _cam.WorldToScreenPoint(new Vector3(slowHud.X, 1.55f, slowHud.Z));
                if (slp.z > 0f)
                {
                    GUI.backgroundColor = new Color(0.62f, 0.14f, 0.16f, 0.92f);
                    GUI.Box(new Rect(slp.x - 50f, Screen.height - slp.y - 12f, 100f, 22f),
                        _game.SlowChokeChip() ?? "SLOW");
                    GUI.backgroundColor = Color.white;
                }
            }
            var gunsUpHudId = _game.GunsUpNodeId();
            if (gunsUpHudId != null && _game.Nodes.TryGetValue(gunsUpHudId, out var gunsUpHud)
                && gunsUpHudId != openId && gunsUpHudId != slowId)
            {
                var gsp = _cam.WorldToScreenPoint(new Vector3(gunsUpHud.X, 1.55f, gunsUpHud.Z));
                if (gsp.z > 0f)
                {
                    GUI.backgroundColor = new Color(0.12f, 0.42f, 0.4f, 0.92f);
                    GUI.Box(new Rect(gsp.x - 50f, Screen.height - gsp.y - 12f, 100f, 22f),
                        _game.GunsUpChip() ?? "UP");
                    GUI.backgroundColor = Color.white;
                }
            }
            var stretchId = _game.StretchPad();
            if (stretchId != null && _game.Nodes.TryGetValue(stretchId, out var stretchNode))
            {
                var tsp = _cam.WorldToScreenPoint(new Vector3(stretchNode.X, 1.55f, stretchNode.Z));
                if (tsp.z > 0f)
                {
                    GUI.backgroundColor = new Color(0.55f, 0.28f, 0.08f, 0.92f);
                    GUI.Box(new Rect(tsp.x - 40f, Screen.height - tsp.y - 12f, 80f, 22f),
                        _game.StretchChip() ?? "IDLE");
                    GUI.backgroundColor = Color.white;
                }
            }
            var cut = _game.ActiveCut();
            if (cut != null && _game.Nodes.TryGetValue(cut.A, out var ca) && _game.Nodes.TryGetValue(cut.B, out var cb))
            {
                var mid = new Vector3((ca.X + cb.X) * 0.5f, 1.15f, (ca.Z + cb.Z) * 0.5f);
                var cutSp = _cam.WorldToScreenPoint(mid);
                if (cutSp.z > 0f)
                {
                    GUI.backgroundColor = CutStakeChipColor(_game.CutStakeOf());
                    GUI.Box(new Rect(cutSp.x - 54f, Screen.height - cutSp.y - 12f, 108f, 22f),
                        _game.CutStakeChip() ?? ("SPLICE " + GameSim.CeilSecs(cut.SabotagedUntil - _game.T) + "s"));
                    GUI.backgroundColor = Color.white;
                }
                return;
            }
            var liveId = _game.RailLiveEdgeId();
            if (liveId == null || !_game.Edges.TryGetValue(liveId, out var liveEdge)) return;
            if (!_game.Nodes.TryGetValue(liveEdge.A, out var la) || !_game.Nodes.TryGetValue(liveEdge.B, out var lb))
                return;
            var liveMid = new Vector3((la.X + lb.X) * 0.5f, 1.15f, (la.Z + lb.Z) * 0.5f);
            var liveSp = _cam.WorldToScreenPoint(liveMid);
            if (liveSp.z <= 0f) return;
            GUI.backgroundColor = new Color(0.12f, 0.42f, 0.4f, 0.92f);
            GUI.Box(new Rect(liveSp.x - 54f, Screen.height - liveSp.y - 12f, 108f, 22f),
                _game.RailLiveChip() ?? "LIVE");
            GUI.backgroundColor = Color.white;
        }

        static Color CutStakeChipColor(CutStake stake)
        {
            switch (stake)
            {
                case CutStake.Brace: return new Color(0.18f, 0.48f, 0.62f, 0.9f);
                case CutStake.Power: return new Color(0.72f, 0.38f, 0.08f, 0.9f);
                case CutStake.Farm: return new Color(0.22f, 0.48f, 0.18f, 0.9f);
                case CutStake.Ore: return new Color(0.72f, 0.42f, 0.12f, 0.9f);
                case CutStake.Generic: return new Color(0.85f, 0.28f, 0.12f, 0.88f);
                default:
                    throw new ArgumentOutOfRangeException(nameof(stake), stake, null);
            }
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
            if (_hud.ConsumeHold) _game.CycleHold(out _);
        }
    }
}
