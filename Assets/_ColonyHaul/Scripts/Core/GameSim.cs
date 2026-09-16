using System;
using System.Collections.Generic;

namespace ColonyHaul
{
    public sealed class GameSim
    {
        public float T;
        public Phase Phase = Phase.Playing;
        public float Ore = Balance.StartOre;
        public float Food = Balance.StartFood;
        public float Power = Balance.StartPower;
        public float HubHp = Balance.HubMaxHp;
        public int HubLevel = 1;
        public float HubUpgradeLeft;
        public int WorkersTotal = Balance.StartWorkers;
        public float StarveTimer;
        public int WaveIndex;
        public float NextWaveIn = 42f;
        public Tool SelectedTool = Tool.None;
        public HoldOrder HoldOrder = HoldOrder.Auto;
        public string RouteFrom;
        public string Hint = "Food is already draining. Place a Farm on a mesa pad, then click the Hub.";
        public readonly Dictionary<string, SimNode> Nodes = new Dictionary<string, SimNode>();
        public readonly Dictionary<string, SimEdge> Edges = new Dictionary<string, SimEdge>();
        public readonly Dictionary<string, Building> Buildings = new Dictionary<string, Building>();
        public readonly List<Hauler> Haulers = new List<Hauler>();
        public readonly List<Enemy> Enemies = new List<Enemy>();
        readonly List<(float at, EnemyType type, string spawn)> _pending = new List<(float, EnemyType, string)>();
        readonly List<SimEvent> _events = new List<SimEvent>();
        readonly Random _rng;
        int _seq = 1;
        float _brownoutAt = -99f;
        float _hubGunCd;
        float _hubL2At = -99f;
        float _surgeUntil;
        readonly float[] _waveGap = { 42f, 28f, 28f, 27f, 26f, 26f };

        public float LastBrownoutAt => _brownoutAt;
        public bool PowerBrownout => Power < Balance.KineticPowerShot;

        public List<SimEvent> DrainEvents()
        {
            var copy = new List<SimEvent>(_events);
            _events.Clear();
            return copy;
        }

        public SimEdge ActiveCut()
        {
            foreach (var e in Edges.Values)
                if (e.Routed && e.SabotagedUntil > T) return e;
            return null;
        }

        public int StaffedProducers()
        {
            var n = 0;
            foreach (var b in Buildings.Values)
                if (b.Staffed && (b.Type == BuildingType.Mine || b.Type == BuildingType.Farm || b.Type == BuildingType.Power)) n++;
            return n;
        }

        public bool CoreThin => HubHp < 72f;

        public bool CoreThinLive()
        {
            return Phase == Phase.Playing && CoreThin && RaidLive;
        }

        public string CoreThinTitle()
        {
            if (!CoreThinLive()) return null;
            if (Surging) return "CORE THIN · BRACE shrugs · " + CeilSecs(SurgeLeft) + "s";
            var h = BraceInbound();
            if (h != null)
            {
                var eta = HaulEtaToHub(h);
                if (eta <= 0.35f) return "CORE THIN · BRACE NOW";
                return "CORE THIN · BRACE IN " + CeilSecs(eta) + "s";
            }
            if (HoldReady && HoldOrder != HoldOrder.Power)
                return "CORE THIN · H for GUNS · haul braces";
            return "CORE THIN · haul braces the Hub";
        }

        public string CoreThinCopy()
        {
            if (!CoreThinLive()) return null;
            if (Surging) return "CORE THIN — BRACE is up · keep hauls coming";
            var h = BraceInbound();
            if (h != null)
            {
                var eta = HaulEtaToHub(h);
                if (eta <= 0.35f) return "CORE THIN — BRACE at the Hub";
                return "CORE THIN — BRACE in " + CeilSecs(eta) + "s";
            }
            return "CORE THIN — Hub HP " + (int)Math.Ceiling(HubHp) + " · a haul braces it";
        }

        public string CoreThinChip()
        {
            if (!CoreThinLive()) return null;
            return "THIN";
        }

        public bool CrewStretched()
        {
            return ProducerCount() > WorkersTotal;
        }

        public int CountType(BuildingType type)
        {
            var n = 0;
            foreach (var b in Buildings.Values)
                if (b.Type == type) n++;
            return n;
        }

        public int ProducerCount()
        {
            var n = 0;
            foreach (var b in Buildings.Values)
                if (b.Type == BuildingType.Mine || b.Type == BuildingType.Farm || b.Type == BuildingType.Power) n++;
            return n;
        }

        public string UnstaffedPad()
        {
            foreach (var b in Buildings.Values)
            {
                if (b.Type != BuildingType.Mine && b.Type != BuildingType.Farm && b.Type != BuildingType.Power) continue;
                if (!b.Staffed) return b.NodeId;
            }
            return null;
        }

        public string HoldCopy()
        {
            if (HubLevel < 2) return "Need Hub L2 to win — Splash rides on it";
            if (WaveIndex >= Balance.WavesToWin) return "Last raiders — splice cuts, haul Power";
            var left = Balance.WavesToWin - WaveIndex;
            return left + (left == 1 ? " wave left · Hub L2 online" : " waves left · Hub L2 online");
        }

        public bool HoldReady => HubLevel >= 2 && WaveIndex >= 4;

        public bool HoldReadyLive()
        {
            return Phase == Phase.Playing
                && HoldReady
                && HoldOrder == HoldOrder.Auto
                && WaveIndex == 4;
        }

        public bool HoldReadyWorld()
        {
            return HoldReadyLive()
                && !WaveClearLive()
                && !GunsLow()
                && !GunsDry()
                && !CoreThinLive()
                && HubChewers() == 0
                && HubClosers() == 0
                && ActiveCut() == null
                && HottestRailThreat() == null;
        }

        public string HoldReadyTitle()
        {
            if (!HoldReadyLive()) return null;
            if (GunsHungry()) return "HOLD READY · H for GUNS";
            if (FoodSecondsLeft() < 28f) return "HOLD READY · H for CREW";
            return "HOLD READY · H locks GUNS or CREW";
        }

        public string HoldReadyCopy()
        {
            if (!HoldReadyLive()) return null;
            if (GunsHungry()) return "HOLD READY — H sends haulers to Power";
            if (FoodSecondsLeft() < 28f) return "HOLD READY — H sends haulers to Food";
            return "HOLD READY — H locks haulers on Power or Food";
        }

        public string HoldReadyChip()
        {
            if (!HoldReadyLive()) return null;
            return "HOLD";
        }

        public string HoldReadyFlash()
        {
            return "HOLD READY — H locks haulers on Power or Food";
        }

        public string HoldOrderCopy()
        {
            if (!HoldReady) return HoldCopy();
            switch (HoldOrder)
            {
                case HoldOrder.Auto: return "HOLD auto · H guns / crew";
                case HoldOrder.Power: return "GUNS ORDER · haulers rush Power · H flips";
                case HoldOrder.Food: return "CREW ORDER · haulers rush Food · H flips";
                default: throw new ArgumentOutOfRangeException(nameof(HoldOrder), HoldOrder, null);
            }
        }

        public bool CycleHold(out string why)
        {
            why = null;
            if (Phase != Phase.Playing)
            {
                why = "match over";
                return false;
            }
            if (!HoldReady)
            {
                why = "Hold orders unlock on wave 4 with Hub L2";
                Hint = why;
                return false;
            }
            switch (HoldOrder)
            {
                case HoldOrder.Auto: HoldOrder = HoldOrder.Power; break;
                case HoldOrder.Power: HoldOrder = HoldOrder.Food; break;
                case HoldOrder.Food: HoldOrder = HoldOrder.Auto; break;
                default: throw new ArgumentOutOfRangeException(nameof(HoldOrder), HoldOrder, null);
            }
            ReplanIdleHaulers();
            string reason;
            switch (HoldOrder)
            {
                case HoldOrder.Power:
                    reason = "power";
                    Hint = "GUNS ORDER. Haulers rush Power. Deposits still BRACE the Hub.";
                    break;
                case HoldOrder.Food:
                    reason = "food";
                    Hint = "CREW ORDER. Haulers rush Food. Splice still beats this.";
                    break;
                case HoldOrder.Auto:
                    reason = "auto";
                    Hint = "Hold auto. Haulers pick the hungriest stock.";
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(HoldOrder), HoldOrder, null);
            }
            Emit(SimEventKind.Hold, "hub", reason: reason);
            return true;
        }

        void ReplanIdleHaulers()
        {
            foreach (var h in Haulers)
            {
                if (h.Wait > 0 || h.CargoAmount > 0) continue;
                h.Path.Clear();
                PlanHauler(h);
            }
        }

        public int IncomingRaiders => _pending.Count;

        public bool RaidLive => Enemies.Count > 0 || IncomingRaiders > 0;

        public bool HubRaising => HubUpgradeLeft > 0;

        public bool Surging => _surgeUntil > T;

        public float SurgeLeft => Math.Max(0f, _surgeUntil - T);

        public bool GunsHungry()
        {
            return LiveTowers() > 0 && (PowerBrownout || GunSecondsLeft() < 9f);
        }

        public bool SplashFresh()
        {
            return HubLevel >= 2 && !HasType(BuildingType.Splash) && _hubL2At > 0f && T - _hubL2At < 16f;
        }

        public string WaveBannerCopy()
        {
            switch (WaveIndex)
            {
                case 1: return "WAVE 1 — 4 grunts  ·  EAST choke";
                case 2: return "WAVE 2 — 5 grunts NORTH  ·  runner EAST";
                case 3: return "WAVE 3 — 4 grunts WEST  ·  2 brutes EAST";
                case 4: return "WAVE 4 — mixed three lanes  ·  runners + brute";
                case 5: return "LAST RAIDS — WAVE 5 — brutes EAST · runners WEST";
                case 6: return "LAST RAID — WAVE 6 — FULL RAID · all spawns";
                default: throw new ArgumentOutOfRangeException(nameof(WaveIndex), WaveIndex, null);
            }
        }

        public int HaulersLoaded
        {
            get
            {
                var n = 0;
                foreach (var h in Haulers) if (h.CargoAmount > 0) n++;
                return n;
            }
        }

        public int HaulersBlocked()
        {
            if (ActiveCut() == null) return 0;
            var n = 0;
            foreach (var h in Haulers)
                if (h.Path.Count == 0 && h.Wait <= 0) n++;
            return n;
        }

        public bool HaulerBlocked(Hauler h)
        {
            return ActiveCut() != null && h.Path.Count == 0 && h.Wait <= 0;
        }

        public string NextWaveCopy()
        {
            if (WaveIndex >= Balance.WavesToWin) return "No more waves — hold the mesa.";
            var next = WaveIndex + 1;
            var eta = CeilSecs(NextWaveIn);
            switch (next)
            {
                case 1: return "Wave 1 in " + eta + "s — 4 grunts east";
                case 2: return "Wave 2 in " + eta + "s — 5 grunts north + runner east";
                case 3: return "Wave 3 in " + eta + "s — 4 grunts west + 2 brutes east";
                case 4: return "Wave 4 in " + eta + "s — grunts, runners, a brute";
                case 5: return "Wave 5 in " + eta + "s — mixed three lanes";
                case 6: return "Wave 6 in " + eta + "s — full raid, all spawns";
                default: throw new ArgumentOutOfRangeException(nameof(next), next, null);
            }
        }

        public static int CeilSecs(float v)
        {
            var i = (int)v;
            if (v > i) i++;
            return i < 0 ? 0 : i;
        }

        public int IncomingAt(string spawnId)
        {
            var n = 0;
            foreach (var p in _pending)
                if (p.spawn == spawnId) n++;
            return n;
        }

        public string[] NextWaveSpawns()
        {
            if (WaveIndex >= Balance.WavesToWin) return new string[0];
            switch (WaveIndex + 1)
            {
                case 1: return new[] { "spawn_e" };
                case 2: return new[] { "spawn_n", "spawn_e" };
                case 3: return new[] { "spawn_w", "spawn_e" };
                case 4: return new[] { "spawn_e", "spawn_n", "spawn_w" };
                case 5: return new[] { "spawn_n", "spawn_e", "spawn_w" };
                case 6: return new[] { "spawn_e", "spawn_n", "spawn_w" };
                default: throw new ArgumentOutOfRangeException(nameof(WaveIndex), WaveIndex, null);
            }
        }

        // Presentation query. Counts must match Pack() in TickWaves.
        public int NextWaveCount(string spawnId, EnemyType type)
        {
            if (WaveIndex >= Balance.WavesToWin) return 0;
            var next = WaveIndex + 1;
            switch (next)
            {
                case 1:
                    return spawnId == "spawn_e" && type == EnemyType.Grunt ? 4 : 0;
                case 2:
                    if (spawnId == "spawn_n" && type == EnemyType.Grunt) return 5;
                    if (spawnId == "spawn_e" && type == EnemyType.Runner) return 1;
                    return 0;
                case 3:
                    if (spawnId == "spawn_w" && type == EnemyType.Grunt) return 4;
                    if (spawnId == "spawn_e" && type == EnemyType.Brute) return 2;
                    return 0;
                case 4:
                    if (spawnId == "spawn_e" && type == EnemyType.Grunt) return 6;
                    if (spawnId == "spawn_n" && type == EnemyType.Runner) return 2;
                    if (spawnId == "spawn_w" && type == EnemyType.Brute) return 1;
                    return 0;
                case 5:
                    if (spawnId == "spawn_n" && type == EnemyType.Grunt) return 5;
                    if (spawnId == "spawn_e" && type == EnemyType.Brute) return 3;
                    if (spawnId == "spawn_w" && type == EnemyType.Runner) return 3;
                    return 0;
                case 6:
                    if (spawnId == "spawn_e" && type == EnemyType.Grunt) return 8;
                    if (spawnId == "spawn_n" && type == EnemyType.Brute) return 4;
                    if (spawnId == "spawn_w" && type == EnemyType.Runner) return 4;
                    return 0;
                default:
                    throw new ArgumentOutOfRangeException(nameof(next), next, null);
            }
        }

        public bool WaveForecastLive =>
            Phase == Phase.Playing && WaveIndex < Balance.WavesToWin && NextWaveIn <= 14f && IncomingRaiders == 0;

        public string SpawnForecastCopy(string spawnId)
        {
            var inbound = IncomingAt(spawnId);
            if (inbound > 0) return "IN " + inbound;
            if (!WaveForecastLive) return null;
            var g = NextWaveCount(spawnId, EnemyType.Grunt);
            var b = NextWaveCount(spawnId, EnemyType.Brute);
            var r = NextWaveCount(spawnId, EnemyType.Runner);
            if (g + b + r <= 0) return null;
            var bits = "";
            if (g > 0) bits += g + "G";
            if (b > 0) bits += (bits.Length > 0 ? " " : "") + b + "B";
            if (r > 0) bits += (bits.Length > 0 ? " " : "") + r + "R";
            return bits + " · " + CeilSecs(NextWaveIn) + "s";
        }

        public Enemy BestTarget(float x, float z, float range, bool hubGun)
        {
            Enemy target = null;
            if (hubGun)
            {
                var best = range;
                foreach (var e in Enemies)
                {
                    var dx = x - e.X;
                    var dz = z - e.Z;
                    var d = (float)Math.Sqrt(dx * dx + dz * dz);
                    if (d <= best)
                    {
                        best = d;
                        target = e;
                    }
                }
                return target;
            }
            var bestScore = float.PositiveInfinity;
            foreach (var e in Enemies)
            {
                var dx = x - e.X;
                var dz = z - e.Z;
                var d = (float)Math.Sqrt(dx * dx + dz * dz);
                if (d > range) continue;
                var pri = e.Type == EnemyType.Brute ? 0 : e.Type == EnemyType.Grunt ? 1 : 2;
                var score = pri * 100f + d;
                if (score < bestScore)
                {
                    bestScore = score;
                    target = e;
                }
            }
            return target;
        }

        public Enemy TowerLock(Building b)
        {
            if (b == null || b.BuildLeft > 0) return null;
            if (b.Type == BuildingType.Hub)
            {
                var hub = Nodes["hub"];
                return BestTarget(hub.X, hub.Z, 3.4f, true);
            }
            if (b.Type != BuildingType.Kinetic && b.Type != BuildingType.Splash) return null;
            var node = Nodes[b.NodeId];
            var range = b.Type == BuildingType.Kinetic ? Balance.KineticRange : Balance.SplashRange;
            return BestTarget(node.X, node.Z, range, false);
        }

        public bool LockedOn(Enemy e)
        {
            if (e == null) return false;
            foreach (var b in Buildings.Values)
            {
                var locked = TowerLock(b);
                if (locked != null && locked.Id == e.Id) return true;
            }
            return false;
        }

        public int GunsLocked()
        {
            var n = 0;
            foreach (var b in Buildings.Values)
                if (TowerLock(b) != null) n++;
            return n;
        }

        public string GunLockCopy()
        {
            var n = GunsLocked();
            if (n <= 0) return PowerBrownout && LiveTowers() > 0 ? "Guns aimed — DRY" : "No lock";
            if (PowerBrownout) return "LOCK · " + n + (n == 1 ? " gun DRY" : " guns DRY");
            return "LOCK · " + n + (n == 1 ? " gun tracking" : " guns tracking");
        }

        public float HaulEtaToHub(Hauler h)
        {
            if (h == null || h.CargoAmount <= 0) return -1f;
            if (h.NodeId == "hub" && h.Path.Count == 0)
                return h.Wait > 0f ? h.Wait : 0f;
            if (h.Path.Count == 0) return -1f;
            var last = h.Path[h.Path.Count - 1];
            if (last != "hub") return -1f;
            var cx = h.X;
            var cz = h.Z;
            var remain = 0f;
            for (var i = 0; i < h.Path.Count; i++)
            {
                if (!Nodes.TryGetValue(h.Path[i], out var n)) return -1f;
                var dx = n.X - cx;
                var dz = n.Z - cz;
                remain += (float)Math.Sqrt(dx * dx + dz * dz);
                cx = n.X;
                cz = n.Z;
            }
            return remain / Balance.HaulerSpeed + Math.Max(0f, h.Wait);
        }

        public Hauler BraceInbound()
        {
            if (Phase != Phase.Playing || !RaidLive || Surging) return null;
            Hauler best = null;
            var bestEta = float.PositiveInfinity;
            foreach (var h in Haulers)
            {
                if (h.CargoAmount <= 0) continue;
                if (HaulerBlocked(h)) continue;
                var eta = HaulEtaToHub(h);
                if (eta < 0f) continue;
                if (eta < bestEta)
                {
                    bestEta = eta;
                    best = h;
                }
            }
            return best;
        }

        public string BraceInboundCopy()
        {
            var h = BraceInbound();
            if (h == null) return null;
            var eta = HaulEtaToHub(h);
            string kind;
            if (h.CargoKind == Resource.Power) kind = "Power";
            else if (h.CargoKind == Resource.Food) kind = "Food";
            else kind = "Ore";
            if (eta <= 0.35f) return "BRACE NOW · " + kind + " at Hub";
            return "BRACE IN " + CeilSecs(eta) + "s · " + kind + " on the rail";
        }

        public bool ChewingHub(Enemy e)
        {
            if (e == null || e.Path.Count > 0) return false;
            var hub = Nodes["hub"];
            var dx = e.X - hub.X;
            var dz = e.Z - hub.Z;
            return Math.Sqrt(dx * dx + dz * dz) <= 0.9;
        }

        public int HubChewers()
        {
            var n = 0;
            foreach (var e in Enemies)
                if (ChewingHub(e)) n++;
            return n;
        }

        public string HubChewCopy()
        {
            var n = HubChewers();
            if (n <= 0) return null;
            var who = n == 1 ? "1 raider on the Hub" : n + " raiders on the Hub";
            if (Surging) return "UNDER FIRE · BRACE shrugs · " + who;
            var brace = BraceInboundCopy();
            if (brace != null) return "UNDER FIRE · " + brace;
            return "UNDER FIRE · " + who;
        }

        public string HubChewTitle()
        {
            var n = HubChewers();
            if (n <= 0) return null;
            if (Surging) return "UNDER FIRE · BRACE shrugs";
            var h = BraceInbound();
            if (h != null)
            {
                var eta = HaulEtaToHub(h);
                if (eta <= 0.35f) return "UNDER FIRE · BRACE NOW";
                return "UNDER FIRE · BRACE IN " + CeilSecs(eta) + "s";
            }
            return "UNDER FIRE · " + n + (n == 1 ? " on Hub" : " on Hub");
        }

        public SimEdge RunnerThreatEdge(Enemy e)
        {
            if (e == null || e.Type != EnemyType.Runner || e.Path.Count == 0) return null;
            var edge = EdgeBetween(e.NodeId, e.Path[0]);
            if (edge == null || !edge.Routed || edge.SabotagedUntil > T) return null;
            return edge;
        }

        public bool RunnerThreatImminent(Enemy e)
        {
            var edge = RunnerThreatEdge(e);
            if (edge == null || e.Path.Count == 0) return false;
            if (!Nodes.TryGetValue(e.Path[0], out var next)) return false;
            var dx = e.X - next.X;
            var dz = e.Z - next.Z;
            return Math.Sqrt(dx * dx + dz * dz) <= 2.2;
        }

        public bool RailThreatened(string edgeId)
        {
            if (string.IsNullOrEmpty(edgeId)) return false;
            foreach (var e in Enemies)
            {
                var t = RunnerThreatEdge(e);
                if (t != null && t.Id == edgeId) return true;
            }
            return false;
        }

        public SimEdge HottestRailThreat()
        {
            SimEdge best = null;
            var bestD = float.PositiveInfinity;
            foreach (var e in Enemies)
            {
                var edge = RunnerThreatEdge(e);
                if (edge == null || e.Path.Count == 0) continue;
                if (!Nodes.TryGetValue(e.Path[0], out var next)) continue;
                var dx = e.X - next.X;
                var dz = e.Z - next.Z;
                var d = (float)Math.Sqrt(dx * dx + dz * dz);
                if (d < bestD)
                {
                    bestD = d;
                    best = edge;
                }
            }
            return best;
        }

        public bool HottestThreatImminent()
        {
            var hot = HottestRailThreat();
            if (hot == null) return false;
            foreach (var e in Enemies)
            {
                var edge = RunnerThreatEdge(e);
                if (edge == null || edge.Id != hot.Id) continue;
                if (RunnerThreatImminent(e)) return true;
            }
            return false;
        }

        public string RailThreatTitle()
        {
            if (HottestRailThreat() == null) return null;
            if (HottestThreatImminent() && BraceInbound() != null)
                return "RAIL THREAT · BRACE haul in danger";
            if (HottestThreatImminent()) return "RAIL THREAT · about to cut";
            if (BraceInbound() != null) return "RAIL THREAT · runner on the brace line";
            return "RAIL THREAT · runner on the rail";
        }

        public string RailThreatCopy()
        {
            var edge = HottestRailThreat();
            if (edge == null) return null;
            var name = RailName(edge);
            if (HottestThreatImminent()) return "RUNNER about to cut " + name;
            return "RUNNER on " + name;
        }

        static string RailName(SimEdge edge)
        {
            var a = edge.A;
            var b = edge.B;
            if (a == "pad_s" || b == "pad_s") return "farm rail";
            if (a == "hub" || b == "hub") return "Hub rail";
            return "mag-rail";
        }

        public bool GunsDry()
        {
            return PowerBrownout && LiveTowers() > 0;
        }

        public Hauler PowerInbound()
        {
            if (Phase != Phase.Playing || !GunsDry()) return null;
            Hauler best = null;
            var bestEta = float.PositiveInfinity;
            foreach (var h in Haulers)
            {
                if (h.CargoAmount <= 0 || h.CargoKind != Resource.Power) continue;
                if (HaulerBlocked(h)) continue;
                var eta = HaulEtaToHub(h);
                if (eta < 0f) continue;
                if (eta < bestEta)
                {
                    bestEta = eta;
                    best = h;
                }
            }
            return best;
        }

        public string GunsDryTitle()
        {
            if (!GunsDry()) return null;
            var h = PowerInbound();
            if (h != null)
            {
                var eta = HaulEtaToHub(h);
                if (eta <= 0.35f) return "GUNS DRY · POWER NOW";
                return "GUNS DRY · POWER IN " + CeilSecs(eta) + "s";
            }
            if (HoldOrder == HoldOrder.Power) return "GUNS DRY · crew rushing Power";
            if (HoldReady) return "GUNS DRY · H for GUNS";
            return "GUNS DRY · haul Power";
        }

        public string GunsDryCopy()
        {
            if (!GunsDry()) return null;
            var h = PowerInbound();
            if (h != null)
            {
                var eta = HaulEtaToHub(h);
                if (eta <= 0.35f) return "POWER at Hub — guns about to live";
                return "POWER inbound · " + CeilSecs(eta) + "s to feed guns";
            }
            return "GUNS DRY — haul Power now";
        }

        public bool GunsLow()
        {
            return GunsHungry() && !GunsDry();
        }

        public Hauler GunsLowInbound()
        {
            if (Phase != Phase.Playing || !GunsLow()) return null;
            Hauler best = null;
            var bestEta = float.PositiveInfinity;
            foreach (var h in Haulers)
            {
                if (h.CargoAmount <= 0 || h.CargoKind != Resource.Power) continue;
                if (HaulerBlocked(h)) continue;
                var eta = HaulEtaToHub(h);
                if (eta < 0f) continue;
                if (eta < bestEta)
                {
                    bestEta = eta;
                    best = h;
                }
            }
            return best;
        }

        public string GunsLowTitle()
        {
            if (!GunsLow()) return null;
            var h = GunsLowInbound();
            if (h != null)
            {
                var eta = HaulEtaToHub(h);
                if (eta <= 0.35f) return "GUNS LOW · POWER NOW";
                return "GUNS LOW · POWER IN " + CeilSecs(eta) + "s";
            }
            if (HoldOrder == HoldOrder.Power) return "GUNS LOW · crew rushing Power";
            if (HoldReady) return "GUNS LOW · H for GUNS";
            return "GUNS LOW · ~" + CeilSecs(GunSecondsLeft()) + "s of fire";
        }

        public string GunsLowCopy()
        {
            if (!GunsLow()) return null;
            var h = GunsLowInbound();
            if (h != null)
            {
                var eta = HaulEtaToHub(h);
                if (eta <= 0.35f) return "POWER at Hub — fuse still live";
                return "GUNS LOW · POWER in " + CeilSecs(eta) + "s";
            }
            return "GUNS LOW — ~" + CeilSecs(GunSecondsLeft()) + "s · haul Power";
        }

        public bool ClosingOnHub(Enemy e)
        {
            if (e == null || ChewingHub(e)) return false;
            if (e.Path.Count == 0) return false;
            return e.Path[0] == "hub";
        }

        public float DistToHub(Enemy e)
        {
            if (e == null || !Nodes.TryGetValue("hub", out var hub)) return float.PositiveInfinity;
            var dx = e.X - hub.X;
            var dz = e.Z - hub.Z;
            return (float)Math.Sqrt(dx * dx + dz * dz);
        }

        public bool ClosingImminent(Enemy e)
        {
            return ClosingOnHub(e) && DistToHub(e) <= 2.4f;
        }

        public int HubClosers()
        {
            var n = 0;
            foreach (var en in Enemies)
                if (ClosingOnHub(en)) n++;
            return n;
        }

        public Enemy HottestCloser()
        {
            Enemy best = null;
            var bestD = float.PositiveInfinity;
            foreach (var en in Enemies)
            {
                if (!ClosingOnHub(en)) continue;
                var d = DistToHub(en);
                if (d < bestD)
                {
                    bestD = d;
                    best = en;
                }
            }
            return best;
        }

        public bool AnyCloseImminent()
        {
            foreach (var en in Enemies)
                if (ClosingImminent(en)) return true;
            return false;
        }

        public string CoreBoundTitle()
        {
            var n = HubClosers();
            if (n <= 0) return null;
            if (AnyCloseImminent() && BraceInbound() != null)
                return "CORE BOUND · BRACE haul racing them";
            if (AnyCloseImminent()) return "CORE BOUND · at the pad";
            var h = BraceInbound();
            if (h != null)
            {
                var eta = HaulEtaToHub(h);
                if (eta <= 0.35f) return "CORE BOUND · BRACE NOW";
                return "CORE BOUND · BRACE IN " + CeilSecs(eta) + "s";
            }
            return "CORE BOUND · " + n + " on the last rail";
        }

        public string CoreBoundCopy()
        {
            var n = HubClosers();
            if (n <= 0) return null;
            if (AnyCloseImminent()) return "RAID at the Hub pad · " + n + " closing";
            return n + (n == 1 ? " raider" : " raiders") + " on the last hop to Hub";
        }

        public bool WaveClearLive()
        {
            return Phase == Phase.Playing
                && WaveIndex > 0
                && WaveIndex < Balance.WavesToWin
                && !RaidLive
                && NextWaveIn > 0f;
        }

        public string WaveClearTitle()
        {
            if (!WaveClearLive()) return null;
            var eta = CeilSecs(NextWaveIn);
            if (HubLevel < 2) return "CLEAR · raise Hub L2 · next " + eta + "s";
            if (!HasType(BuildingType.Splash)) return "CLEAR · Splash WEST · next " + eta + "s";
            if (GunsHungry() && !GunsDry()) return "CLEAR · haul Power · next " + eta + "s";
            return "CLEAR · next raid in " + eta + "s";
        }

        public string WaveClearCopy()
        {
            if (!WaveClearLive()) return null;
            return "CLEAR · food ~" + CeilSecs(FoodSecondsLeft()) + "s · guns ~" + CeilSecs(GunSecondsLeft()) + "s";
        }

        public bool L2Ready()
        {
            return Phase == Phase.Playing && CanAfford(Tool.Upgrade);
        }

        public bool L2ReadyWorld()
        {
            return L2Ready()
                && !WaveClearLive()
                && !GunsLow()
                && !GunsDry()
                && HubChewers() == 0
                && HubClosers() == 0
                && ActiveCut() == null;
        }

        public string L2ReadyTitle()
        {
            if (!L2Ready()) return null;
            return "L2 READY · U raises Hub · Splash next";
        }

        public string L2ReadyCopy()
        {
            if (!L2Ready()) return null;
            return "L2 READY — U spends ore / food / pwr · Splash unlocks";
        }

        public string L2ReadyFlash()
        {
            return "L2 READY — press U · Splash next";
        }

        public string L2ReadyChip()
        {
            if (!L2Ready()) return null;
            return "L2 READY";
        }

        public string OpenChokeId()
        {
            if (Phase != Phase.Playing) return null;
            if (OpeningStep() != 0) return null;
            var hot = HottestLane();
            string choke;
            switch (hot)
            {
                case "east": choke = "choke_e"; break;
                case "west": choke = "choke_w"; break;
                case "north": choke = "choke_n"; break;
                default: throw new ArgumentOutOfRangeException(nameof(hot), hot, null);
            }
            if (NodeArmed(choke)) return null;
            if (choke == "choke_w" && SplashFresh()) return null;
            var lanes = Lanes();
            int pressure;
            switch (hot)
            {
                case "east": pressure = lanes.East; break;
                case "west": pressure = lanes.West; break;
                case "north": pressure = lanes.North; break;
                default: throw new ArgumentOutOfRangeException(nameof(hot), hot, null);
            }
            if (pressure <= 0) return null;
            return choke;
        }

        public string OpenChokeLane()
        {
            var id = OpenChokeId();
            if (id == null) return null;
            switch (id)
            {
                case "choke_e": return "EAST";
                case "choke_n": return "NORTH";
                case "choke_w": return "WEST";
                default: throw new ArgumentOutOfRangeException(nameof(id), id, null);
            }
        }

        public string OpenChokeTitle()
        {
            var lane = OpenChokeLane();
            if (lane == null) return null;
            var id = OpenChokeId();
            if (id == "choke_w" && HubLevel >= 2 && !HasType(BuildingType.Splash))
            {
                if (CanAfford(Tool.Splash)) return "OPEN " + lane + " · Splash";
                return "OPEN " + lane + " · stock Splash";
            }
            if (CanAfford(Tool.Kinetic)) return "OPEN " + lane + " · Kinetic";
            return "OPEN " + lane + " · ore short";
        }

        public string OpenChokeCopy()
        {
            var id = OpenChokeId();
            var lane = OpenChokeLane();
            if (id == null || lane == null) return null;
            var near = EnemiesNear(id, 4.8f);
            if (near > 0) return "OPEN " + lane + " · " + near + " on the choke";
            return "OPEN " + lane + " — plant a gun before they pad";
        }

        public string OpenChokeChip()
        {
            var lane = OpenChokeLane();
            if (lane == null) return null;
            return "OPEN " + lane;
        }

        public string OpenChokeFlash()
        {
            var lane = OpenChokeLane();
            if (lane == null) return "OPEN CHOKE — plant a gun";
            return "OPEN " + lane + " — plant a gun";
        }

        public string SlowChokeId()
        {
            if (Phase != Phase.Playing) return null;
            if (OpeningStep() != 0) return null;
            var hot = HottestLane();
            string choke;
            switch (hot)
            {
                case "east": choke = "choke_e"; break;
                case "west": choke = "choke_w"; break;
                case "north": choke = "choke_n"; break;
                default: throw new ArgumentOutOfRangeException(nameof(hot), hot, null);
            }
            if (!NodeArmed(choke)) return null;
            if (ChokeBarred(choke)) return null;
            if (choke == "choke_w" && SplashFresh()) return null;
            var lanes = Lanes();
            int pressure;
            switch (hot)
            {
                case "east": pressure = lanes.East; break;
                case "west": pressure = lanes.West; break;
                case "north": pressure = lanes.North; break;
                default: throw new ArgumentOutOfRangeException(nameof(hot), hot, null);
            }
            if (pressure <= 0) return null;
            return choke;
        }

        public string SlowChokeLane()
        {
            var id = SlowChokeId();
            if (id == null) return null;
            switch (id)
            {
                case "choke_e": return "EAST";
                case "choke_n": return "NORTH";
                case "choke_w": return "WEST";
                default: throw new ArgumentOutOfRangeException(nameof(id), id, null);
            }
        }

        public string SlowChokeTitle()
        {
            var lane = SlowChokeLane();
            if (lane == null) return null;
            if (CanAfford(Tool.Barrier)) return "SLOW " + lane + " · Barrier";
            return "SLOW " + lane + " · ore short";
        }

        public string SlowChokeCopy()
        {
            var lane = SlowChokeLane();
            if (lane == null) return null;
            if (RaidLive) return "SLOW " + lane + " — Barrier buys guns / BRACE time";
            return "SLOW " + lane + " — Barrier before they pad";
        }

        public string SlowChokeChip()
        {
            var lane = SlowChokeLane();
            if (lane == null) return null;
            return "SLOW " + lane;
        }

        public string SlowChokeFlash()
        {
            var lane = SlowChokeLane();
            if (lane == null) return "SLOW — Barrier the choke";
            return "SLOW " + lane + " — Barrier the approach";
        }

        public string StretchPad()
        {
            if (Phase != Phase.Playing) return null;
            if (OpeningStep() != 0) return null;
            if (!CrewStretched()) return null;
            var id = UnstaffedPad();
            if (id == null) return null;
            if (OfflinePad() == id) return null;
            return id;
        }

        public string StretchTitle()
        {
            var id = StretchPad();
            if (id == null) return null;
            if (HubLevel < 2 && CanAfford(Tool.Upgrade))
                return "CREW STRETCH · U adds crew";
            return "CREW STRETCH · " + PadCall(id) + " idle";
        }

        public string StretchCopy()
        {
            var id = StretchPad();
            if (id == null) return null;
            return "CREW STRETCH — " + PadCall(id) + " is idle · rail beats a new pad";
        }

        public string StretchChip()
        {
            if (StretchPad() == null) return null;
            return "IDLE";
        }

        public string StretchFlash()
        {
            var id = StretchPad();
            if (id == null) return "CREW STRETCH — rail beats a new pad";
            return "CREW STRETCH — " + PadCall(id) + " idle";
        }

        public Hauler BlockedLoadedHauler()
        {
            foreach (var h in Haulers)
            {
                if (!HaulerBlocked(h)) continue;
                if (h.CargoAmount > 0) return h;
            }
            return null;
        }

        bool CutTouchesType(SimEdge cut, BuildingType type)
        {
            if (cut == null) return false;
            return NodeIsType(cut.A, type) || NodeIsType(cut.B, type);
        }

        bool NodeIsType(string nodeId, BuildingType type)
        {
            Building b;
            return Buildings.TryGetValue(nodeId, out b) && b.Type == type;
        }

        public CutStake CutStakeOf()
        {
            var cut = ActiveCut();
            if (cut == null) return CutStake.Generic;
            if (RaidLive && BlockedLoadedHauler() != null)
                return CutStake.Brace;
            if (CutTouchesType(cut, BuildingType.Power))
                return CutStake.Power;
            if (CutTouchesType(cut, BuildingType.Farm))
                return CutStake.Farm;
            if (CutTouchesType(cut, BuildingType.Mine))
                return CutStake.Ore;
            if (RaidLive && HaulersBlocked() > 0)
                return CutStake.Brace;
            return CutStake.Generic;
        }

        public string CutStakeTitle()
        {
            var cut = ActiveCut();
            if (cut == null) return null;
            var left = CeilSecs(cut.SabotagedUntil - T);
            switch (CutStakeOf())
            {
                case CutStake.Brace:
                    return "HAUL CUT · BRACE stuck · splice " + left + "s";
                case CutStake.Power:
                    return GunsHungry() || GunsDry()
                        ? "HAUL CUT · Power rail down · splice " + left + "s"
                        : "HAUL CUT · Power rail · splice " + left + "s";
                case CutStake.Farm:
                    return "HAUL CUT · farm rail down · splice " + left + "s";
                case CutStake.Ore:
                    return "HAUL CUT · ore rail down · splice " + left + "s";
                case CutStake.Generic:
                    return "HAUL CUT · splice the orange rail · " + left + "s";
                default:
                    throw new ArgumentOutOfRangeException(nameof(CutStake), CutStakeOf(), null);
            }
        }

        public string CutStakeCopy()
        {
            var cut = ActiveCut();
            if (cut == null) return null;
            var left = CeilSecs(cut.SabotagedUntil - T);
            var stuck = HaulersBlocked();
            switch (CutStakeOf())
            {
                case CutStake.Brace:
                    return stuck > 0
                        ? "BRACE stuck — splice or Hub takes hits"
                        : "BRACE rail down — splice " + left + "s";
                case CutStake.Power:
                    return GunsHungry() || GunsDry()
                        ? "Power rail down — guns starve · splice " + left + "s"
                        : "Power rail down — splice " + left + "s";
                case CutStake.Farm:
                    return "Farm rail down — larder ~" + CeilSecs(FoodSecondsLeft()) + "s";
                case CutStake.Ore:
                    return "Ore rail down — splice " + left + "s";
                case CutStake.Generic:
                    return stuck > 0
                        ? "HAUL CUT — " + stuck + " stuck · splice " + left + "s"
                        : "HAUL CUT — splice · " + left + "s left";
                default:
                    throw new ArgumentOutOfRangeException(nameof(CutStake), CutStakeOf(), null);
            }
        }

        public string CutStakeChip()
        {
            var cut = ActiveCut();
            if (cut == null) return null;
            var left = CeilSecs(cut.SabotagedUntil - T);
            switch (CutStakeOf())
            {
                case CutStake.Brace: return "BRACE " + left + "s";
                case CutStake.Power: return "PWR " + left + "s";
                case CutStake.Farm: return "FARM " + left + "s";
                case CutStake.Ore: return "ORE " + left + "s";
                case CutStake.Generic: return "SPLICE " + left + "s";
                default:
                    throw new ArgumentOutOfRangeException(nameof(CutStake), CutStakeOf(), null);
            }
        }

        public string CutStakeFlash()
        {
            if (ActiveCut() == null) return null;
            switch (CutStakeOf())
            {
                case CutStake.Brace: return "HAUL CUT — BRACE haul stuck";
                case CutStake.Power: return "HAUL CUT — Power rail down";
                case CutStake.Farm: return "HAUL CUT — farm rail down";
                case CutStake.Ore: return "HAUL CUT — ore rail down";
                case CutStake.Generic: return "HAUL CUT — splice the orange rail";
                default:
                    throw new ArgumentOutOfRangeException(nameof(CutStake), CutStakeOf(), null);
            }
        }

        public string OfflinePad()
        {
            if (Phase != Phase.Playing) return null;
            if (ActiveCut() != null) return null;
            return UnroutedProducer();
        }

        Building OfflineBuilding()
        {
            var id = OfflinePad();
            if (id == null) return null;
            Building b;
            return Buildings.TryGetValue(id, out b) ? b : null;
        }

        public string OfflineTitle()
        {
            var b = OfflineBuilding();
            if (b == null) return null;
            var pad = PadCall(b.NodeId);
            switch (b.Type)
            {
                case BuildingType.Power:
                    return GunsHungry() || GunsDry()
                        ? "PAD OFFLINE · Power has no rail"
                        : "PAD OFFLINE · rail " + pad + " Power home";
                case BuildingType.Farm:
                    return "PAD OFFLINE · farm has no rail · larder ~" + CeilSecs(FoodSecondsLeft()) + "s";
                case BuildingType.Mine:
                    return "PAD OFFLINE · rail " + pad + " ore home";
                case BuildingType.Hub:
                case BuildingType.Depot:
                case BuildingType.Kinetic:
                case BuildingType.Splash:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(b.Type), b.Type, null);
            }
        }

        public string OfflineCopy()
        {
            var b = OfflineBuilding();
            if (b == null) return null;
            var pad = PadCall(b.NodeId);
            switch (b.Type)
            {
                case BuildingType.Power:
                    return GunsHungry() || GunsDry()
                        ? "OFFLINE " + pad + " Power — guns wait on a rail"
                        : "OFFLINE " + pad + " Power — rail home";
                case BuildingType.Farm:
                    return "OFFLINE " + pad + " farm — larder ~" + CeilSecs(FoodSecondsLeft()) + "s";
                case BuildingType.Mine:
                    return "OFFLINE " + pad + " ore — rail home";
                case BuildingType.Hub:
                case BuildingType.Depot:
                case BuildingType.Kinetic:
                case BuildingType.Splash:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(b.Type), b.Type, null);
            }
        }

        public string OfflineFlash()
        {
            var b = OfflineBuilding();
            if (b == null) return null;
            var pad = PadCall(b.NodeId);
            switch (b.Type)
            {
                case BuildingType.Power: return "PAD OFFLINE — rail " + pad + " Power home";
                case BuildingType.Farm: return "PAD OFFLINE — rail " + pad + " farm home";
                case BuildingType.Mine: return "PAD OFFLINE — rail " + pad + " ore home";
                case BuildingType.Hub:
                case BuildingType.Depot:
                case BuildingType.Kinetic:
                case BuildingType.Splash:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(b.Type), b.Type, null);
            }
        }

        bool HaulerBoundFor(string nodeId)
        {
            foreach (var h in Haulers)
            {
                if (h.NodeId == nodeId) return true;
                if (h.Path.Contains(nodeId)) return true;
            }
            return false;
        }

        public Hauler IdleHauler()
        {
            foreach (var h in Haulers)
            {
                if (HaulerBlocked(h)) continue;
                if (h.CargoAmount > 0 || h.Wait > 0 || h.Path.Count > 0) continue;
                return h;
            }
            return null;
        }

        public Building SittingStock()
        {
            if (Phase != Phase.Playing) return null;
            if (ActiveCut() != null) return null;
            if (OfflinePad() != null) return null;
            Building best = null;
            var bestScore = 5.5f;
            foreach (var b in Buildings.Values)
            {
                if (b.Type != BuildingType.Mine && b.Type != BuildingType.Farm && b.Type != BuildingType.Power) continue;
                if (b.BuildLeft > 0 || !b.Staffed) continue;
                if (HaulerBoundFor(b.NodeId)) continue;
                if (Pathfind(b.NodeId, "hub", true, false, false) == null) continue;
                var res = ResourceOf(b.Type);
                var amt = b.Buffer[res];
                if (amt < 6f) continue;
                var score = amt;
                if (res == Resource.Power && GunsHungry()) score += 40f;
                else if (res == Resource.Food && Food < 12f) score += 24f;
                if (score > bestScore)
                {
                    bestScore = score;
                    best = b;
                }
            }
            return best;
        }

        public string SittingTitle()
        {
            var b = SittingStock();
            if (b == null) return null;
            var pad = PadCall(b.NodeId);
            switch (b.Type)
            {
                case BuildingType.Power:
                    return GunsHungry() || GunsDry()
                        ? "SIT · Power piled · haul " + pad
                        : "SIT · Power piled on " + pad;
                case BuildingType.Farm:
                    return "SIT · farm piled · larder ~" + CeilSecs(FoodSecondsLeft()) + "s";
                case BuildingType.Mine:
                    return "SIT · ore piled on " + pad;
                case BuildingType.Hub:
                case BuildingType.Depot:
                case BuildingType.Kinetic:
                case BuildingType.Splash:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(b.Type), b.Type, null);
            }
        }

        public string SittingCopy()
        {
            var b = SittingStock();
            if (b == null) return null;
            var pad = PadCall(b.NodeId);
            var amt = (int)b.Buffer[ResourceOf(b.Type)];
            switch (b.Type)
            {
                case BuildingType.Power:
                    return GunsHungry() || GunsDry()
                        ? "SIT " + pad + " Power " + amt + " — guns wait on a haul"
                        : "SIT " + pad + " Power " + amt + " — haul it";
                case BuildingType.Farm:
                    return "SIT " + pad + " farm " + amt + " — haul Food";
                case BuildingType.Mine:
                    return "SIT " + pad + " ore " + amt + " — haul it";
                case BuildingType.Hub:
                case BuildingType.Depot:
                case BuildingType.Kinetic:
                case BuildingType.Splash:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(b.Type), b.Type, null);
            }
        }

        public string SittingChip()
        {
            var b = SittingStock();
            if (b == null) return null;
            switch (b.Type)
            {
                case BuildingType.Power: return "HAUL PWR";
                case BuildingType.Farm: return "HAUL FOOD";
                case BuildingType.Mine: return "HAUL ORE";
                case BuildingType.Hub:
                case BuildingType.Depot:
                case BuildingType.Kinetic:
                case BuildingType.Splash:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(b.Type), b.Type, null);
            }
        }

        public string SittingFlash()
        {
            var b = SittingStock();
            if (b == null) return null;
            var pad = PadCall(b.NodeId);
            switch (b.Type)
            {
                case BuildingType.Power: return "SIT — haul " + pad + " Power";
                case BuildingType.Farm: return "SIT — haul " + pad + " Food";
                case BuildingType.Mine: return "SIT — haul " + pad + " ore";
                case BuildingType.Hub:
                case BuildingType.Depot:
                case BuildingType.Kinetic:
                case BuildingType.Splash:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(b.Type), b.Type, null);
            }
        }

        public static float RangeOf(BuildingType type)
        {
            switch (type)
            {
                case BuildingType.Kinetic: return Balance.KineticRange;
                case BuildingType.Splash: return Balance.SplashRange;
                case BuildingType.Hub: return 3.4f;
                case BuildingType.Depot:
                case BuildingType.Mine:
                case BuildingType.Farm:
                case BuildingType.Power:
                    return 0f;
                default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public int OpeningStep()
        {
            var gate = OpeningGate();
            if (gate == "farm") return 1;
            if (gate == "route") return 2;
            if (T < 28f) return 3;
            return 0;
        }

        public string OpeningCoach()
        {
            switch (OpeningStep())
            {
                case 1: return "1 / 3  Farm — click the glowing south pad";
                case 2: return "2 / 3  Mag-rail — click the Hub to finish the line";
                case 3: return "3 / 3  Haulers are rolling — keep that rail spliced";
                case 0: return null;
                default: throw new ArgumentOutOfRangeException(nameof(OpeningStep), OpeningStep(), null);
            }
        }

        public int LiveTowers()
        {
            var n = 0;
            foreach (var b in Buildings.Values)
                if ((b.Type == BuildingType.Kinetic || b.Type == BuildingType.Splash) && b.BuildLeft <= 0) n++;
            return n;
        }

        public float GunSecondsLeft()
        {
            var towers = LiveTowers();
            var drain = 0.44f;
            if (towers > 0)
                drain += towers * (Balance.TowerIdlePower + Balance.KineticPowerShot / Balance.KineticCooldown);
            return Power / drain;
        }

        public float ProducerFill(Building b)
        {
            if (b.Type == BuildingType.Mine) return b.Buffer[Resource.Ore] / 22f;
            if (b.Type == BuildingType.Farm) return b.Buffer[Resource.Food] / 22f;
            if (b.Type == BuildingType.Power) return b.Buffer[Resource.Power] / 22f;
            return 0f;
        }

        public float FoodSecondsLeft()
        {
            var n = 0;
            foreach (var b in Buildings.Values)
                if (b.Type != BuildingType.Hub && b.Type != BuildingType.Depot) n++;
            var drain = Balance.FoodDrain + n * Balance.FoodDrainPerBuilding;
            if (drain < 0.01f) return 99f;
            return Food / drain;
        }

        public float StarveSecondsLeft() => Math.Max(0f, Balance.StarveFail - StarveTimer);

        public bool HasType(BuildingType type)
        {
            foreach (var b in Buildings.Values)
                if (b.Type == type) return true;
            return false;
        }

        public bool NodeArmed(string nodeId)
        {
            if (!Buildings.TryGetValue(nodeId, out var b)) return false;
            return b.Type == BuildingType.Kinetic || b.Type == BuildingType.Splash;
        }

        public bool ChokeBarred(string chokeId)
        {
            foreach (var e in Neighbors(chokeId))
                if (e.Barrier) return true;
            return false;
        }

        public int EnemiesNear(string nodeId, float range)
        {
            if (!Nodes.TryGetValue(nodeId, out var n)) return 0;
            var c = 0;
            var r2 = range * range;
            foreach (var e in Enemies)
            {
                var dx = n.X - e.X;
                var dz = n.Z - e.Z;
                if (dx * dx + dz * dz <= r2) c++;
            }
            return c;
        }

        public LaneThreat Lanes()
        {
            var t = new LaneThreat();
            foreach (var e in Enemies)
            {
                if (e.X > 5.5f) t.East++;
                else if (e.X < -5.5f) t.West++;
                else t.North++;
            }
            t.East += IncomingAt("spawn_e");
            t.North += IncomingAt("spawn_n");
            t.West += IncomingAt("spawn_w");
            return t;
        }

        public string HottestLane()
        {
            var l = Lanes();
            if (l.East >= l.North && l.East >= l.West) return "east";
            if (l.West >= l.North) return "west";
            return "north";
        }

        public bool CanAfford(Tool tool)
        {
            switch (tool)
            {
                case Tool.None: return true;
                case Tool.Farm: return Ore >= Balance.FarmOre;
                case Tool.Mine: return Ore >= Balance.MineOre;
                case Tool.Power: return Ore >= Balance.PowerOre;
                case Tool.Route: return Ore >= Balance.RouteCost;
                case Tool.Kinetic: return Ore >= Balance.KineticOre && Power >= Balance.KineticPower;
                case Tool.Splash: return HubLevel >= 2 && Ore >= Balance.SplashOre && Power >= Balance.SplashPower;
                case Tool.Barrier: return Ore >= Balance.BarrierCost;
                case Tool.Upgrade:
                    return HubLevel < 2 && HubUpgradeLeft <= 0
                        && Ore >= Balance.HubL2Ore && Food >= Balance.HubL2Food && Power >= Balance.HubL2Power;
                default: throw new ArgumentOutOfRangeException(nameof(tool), tool, null);
            }
        }

        public WatchCall MidWatch()
        {
            if (Phase != Phase.Playing) return null;
            if (OpeningStep() != 0) return null;
            if (ActiveCut() != null)
            {
                var stuck = HaulersBlocked();
                switch (CutStakeOf())
                {
                    case CutStake.Brace:
                        return Call(stuck > 0
                            ? "SPLICE — BRACE haul stuck · Hub is eating hits"
                            : "SPLICE — BRACE rail is down · click the glowing pad", Tool.Route);
                    case CutStake.Power:
                        return Call(GunsHungry() || GunsDry()
                            ? "SPLICE — Power rail down · guns need that haul"
                            : "SPLICE — Power rail is down · click the glowing pad", Tool.Route);
                    case CutStake.Farm:
                        return Call("SPLICE — farm rail down · larder ~" + CeilSecs(FoodSecondsLeft()) + "s", Tool.Route);
                    case CutStake.Ore:
                        return Call("SPLICE — ore rail down · click the glowing pad", Tool.Route);
                    case CutStake.Generic:
                        return Call(stuck > 0
                            ? "SPLICE the orange rail — " + stuck + (stuck == 1 ? " hauler stuck" : " haulers stuck")
                            : "SPLICE the orange rail — click the glowing pad", Tool.Route);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(CutStake), CutStakeOf(), null);
                }
            }
            var dead = UnroutedProducer();
            if (dead != null)
                return Call("Pad " + PadCall(dead) + " is offline — rail it home", Tool.Route);
            if (LiveTowers() == 0 && CanAfford(Tool.Kinetic) && (WaveIndex >= 1 || NextWaveIn <= 22f))
                return Call("Wave on the clock — Kinetic on the EAST choke", Tool.Kinetic);
            if (PowerBrownout || (LiveTowers() > 0 && GunSecondsLeft() < 9f))
            {
                if (!HasType(BuildingType.Power) && CanAfford(Tool.Power))
                    return Call("Guns will brown out — plant Power", Tool.Power);
                if (HoldReady && HoldOrder != HoldOrder.Power)
                    return Call("Guns hungry — H for GUNS so haulers rush Power", Tool.None);
                return Call("Haul POWER — towers are on the last of the pylon", Tool.Route);
            }
            if (HubLevel < 2 && HubUpgradeLeft <= 0 && CanAfford(Tool.Upgrade))
                return Call("Hub L2 is in stock — Splash unlocks after this", Tool.Upgrade);
            if (Food < 8f)
            {
                if (HoldReady && HoldOrder != HoldOrder.Food)
                    return Call("Larder ~" + CeilSecs(FoodSecondsLeft()) + "s — H for CREW so haulers rush Food", Tool.Farm);
                return Call("Larder ~" + CeilSecs(FoodSecondsLeft()) + "s — farm rail must stay live", Tool.Farm);
            }
            if (SplashFresh())
            {
                if (CanAfford(Tool.Splash))
                    return Call("Splash is live — plant it on the WEST choke", Tool.Splash);
                return Call("Splash unlocked — stock 28 ore + 6 pwr for west", Tool.Splash);
            }
            if (Food < 11f)
                return Call("Larder ~" + CeilSecs(FoodSecondsLeft()) + "s — farm rail must stay live", Tool.Farm);
            var hot = HottestLane();
            var choke = hot == "east" ? "choke_e" : hot == "west" ? "choke_w" : "choke_n";
            var lanes = Lanes();
            var pressure = lanes.East + lanes.North + lanes.West;
            if (!NodeArmed(choke) && CanAfford(Tool.Kinetic) && pressure > 0)
                return Call("Heaviest lane is " + hot.ToUpperInvariant() + " — gun that choke", Tool.Kinetic);
            if (!ChokeBarred(choke) && CanAfford(Tool.Barrier) && pressure > 3)
                return Call("Slow the " + hot + " approach — Barrier on the choke", Tool.Barrier);
            if (HubLevel >= 2 && !HasType(BuildingType.Splash) && CanAfford(Tool.Splash))
                return Call("Splash on the west choke — brutes bunch there", Tool.Splash);
            if (CoreThin && RaidLive)
            {
                if (!NodeArmed(choke) && CanAfford(Tool.Kinetic))
                    return Call("CORE THIN — Kinetic on the " + hot.ToUpperInvariant() + " choke", Tool.Kinetic);
                return Call("CORE THIN — keep " + hot + " guns fed, splice any cut", Tool.Route);
            }
            if (CrewStretched())
            {
                var idle = UnstaffedPad();
                if (idle != null)
                    return Call("Crew stretched — " + PadCall(idle) + " is idle. Rail beats a new pad", Tool.Route);
            }
            if (WaveIndex >= 3 && CountType(BuildingType.Farm) < 2 && CanAfford(Tool.Farm) && FoodSecondsLeft() < 28f)
                return Call("Second Farm — late waves chew the larder", Tool.Farm);
            if (WaveIndex >= 4 && HasType(BuildingType.Splash) && !NodeArmed("choke_n") && CanAfford(Tool.Kinetic) && lanes.North > 0)
                return Call("North is open — Kinetic on the north choke", Tool.Kinetic);
            if (HoldReady && HoldOrder == HoldOrder.Auto && WaveIndex >= 4 && WaveIndex < 5)
                return Call("HOLD ORDER — H locks haulers on Power or Food", Tool.None);
            if (WaveIndex >= 5)
            {
                switch (HoldOrder)
                {
                    case HoldOrder.Auto:
                        return Call(HoldReady
                            ? "HOLD — H sends Power to guns or Food to crew"
                            : "HOLD THE MESA — splice every cut, haul Power", Tool.None);
                    case HoldOrder.Power:
                        return Call("GUNS ORDER — Power hauls BRACE the Hub. H to flip", Tool.None);
                    case HoldOrder.Food:
                        return Call("CREW ORDER — Food hauls keep the larder. H to flip", Tool.None);
                    default: throw new ArgumentOutOfRangeException(nameof(HoldOrder), HoldOrder, null);
                }
            }
            return Call(NextWaveCopy(), Tool.None);
        }

        static WatchCall Call(string copy, Tool pulse)
        {
            return new WatchCall { Copy = copy, Pulse = pulse };
        }

        static string PadCall(string id)
        {
            switch (id)
            {
                case "pad_s": return "south";
                case "pad_n": return "north";
                case "pad_se": return "south-east";
                case "pad_ne": return "north-east";
                case "pad_sw": return "south-west";
                case "pad_nw": return "north-west";
                default: return id;
            }
        }

        void Emit(SimEventKind kind, string nodeId = null, float x = 0, float z = 0,
            float fromX = 0, float fromZ = 0, float toX = 0, float toZ = 0,
            float amount = 0, Resource? resource = null, int wave = 0,
            string edgeId = null, string enemyId = null, string reason = null)
        {
            _events.Add(new SimEvent
            {
                Kind = kind,
                T = T,
                NodeId = nodeId,
                X = x,
                Z = z,
                FromX = fromX,
                FromZ = fromZ,
                ToX = toX,
                ToZ = toZ,
                Amount = amount,
                Resource = resource,
                Wave = wave,
                EdgeId = edgeId,
                EnemyId = enemyId,
                Reason = reason
            });
        }

        public GameSim(int seed = 7)
        {
            _rng = new Random(seed);
            BuildMesa();
            PlaceFixed(BuildingType.Hub, "hub");
            PlaceFixed(BuildingType.Depot, "depot");
            var depot = Nodes["depot"];
            for (var i = 0; i < Balance.StartHaulers; i++)
            {
                Haulers.Add(new Hauler
                {
                    Id = Nid("haul"),
                    X = depot.X + (i - 0.5f) * 0.45f,
                    Z = depot.Z,
                    NodeId = "depot"
                });
            }
        }

        string Nid(string prefix) => prefix + "_" + (++_seq);

        static string EdgeId(string a, string b) => string.CompareOrdinal(a, b) < 0 ? a + "~" + b : b + "~" + a;

        void PlaceFixed(BuildingType type, string nodeId)
        {
            Buildings[nodeId] = new Building { Id = Nid(type.ToString()), Type = type, NodeId = nodeId, Staffed = true };
        }

        void BuildMesa()
        {
            void N(string id, float x, float z, NodeKind kind) =>
                Nodes[id] = new SimNode { Id = id, X = x, Z = z, Kind = kind };

            N("hub", 0, 0, NodeKind.Hub);
            N("depot", 2.5f, -2.3f, NodeKind.Depot);
            N("pad_n", 0, 6.1f, NodeKind.Pad);
            N("pad_ne", 5.3f, 3.05f, NodeKind.Pad);
            N("pad_se", 5.3f, -3.05f, NodeKind.Pad);
            N("pad_s", 0, -6.1f, NodeKind.Pad);
            N("pad_sw", -5.3f, -3.05f, NodeKind.Pad);
            N("pad_nw", -5.3f, 3.05f, NodeKind.Pad);
            N("choke_n", 0, 11.4f, NodeKind.Choke);
            N("choke_e", 11.1f, 0.2f, NodeKind.Choke);
            N("choke_w", -11.1f, 0.2f, NodeKind.Choke);
            N("tower_ne", 8.2f, 8f, NodeKind.Tower);
            N("tower_sw", -8.2f, -7.2f, NodeKind.Tower);
            N("spawn_n", 0, 16.6f, NodeKind.Spawn);
            N("spawn_e", 16.4f, 1f, NodeKind.Spawn);
            N("spawn_w", -16.4f, 1f, NodeKind.Spawn);

            void E(string a, string b)
            {
                var id = EdgeId(a, b);
                Edges[id] = new SimEdge { Id = id, A = a, B = b };
            }

            E("hub", "depot"); E("hub", "pad_n"); E("hub", "pad_ne"); E("hub", "pad_se");
            E("hub", "pad_s"); E("hub", "pad_sw"); E("hub", "pad_nw");
            E("pad_n", "pad_ne"); E("pad_ne", "pad_se"); E("pad_se", "pad_s");
            E("pad_s", "pad_sw"); E("pad_sw", "pad_nw"); E("pad_nw", "pad_n");
            E("pad_n", "choke_n"); E("pad_ne", "choke_n"); E("pad_nw", "choke_n"); E("choke_n", "spawn_n");
            E("pad_ne", "choke_e"); E("pad_se", "choke_e"); E("choke_e", "spawn_e");
            E("pad_nw", "choke_w"); E("pad_sw", "choke_w"); E("choke_w", "spawn_w");
            E("tower_ne", "pad_ne"); E("tower_ne", "choke_n"); E("tower_ne", "choke_e");
            E("tower_sw", "pad_sw"); E("tower_sw", "pad_s"); E("tower_sw", "choke_w");
            Edges[EdgeId("hub", "depot")].Routed = true;
        }

        public SimEdge EdgeBetween(string a, string b)
        {
            Edges.TryGetValue(EdgeId(a, b), out var e);
            return e;
        }

        public List<SimEdge> Neighbors(string nodeId)
        {
            var list = new List<SimEdge>();
            foreach (var e in Edges.Values)
                if (e.A == nodeId || e.B == nodeId) list.Add(e);
            return list;
        }

        static string Other(SimEdge e, string id) => e.A == id ? e.B : e.A;

        public string OpeningGate()
        {
            if (T > Balance.OpeningLock) return null;
            Building farm = null;
            foreach (var b in Buildings.Values)
                if (b.Type == BuildingType.Farm) farm = b;
            if (farm == null) return "farm";
            if (Pathfind(farm.NodeId, "hub", true, false, false) == null) return "route";
            return null;
        }

        public string UnroutedProducer()
        {
            string found = null;
            foreach (var b in Buildings.Values)
            {
                if (b.Type != BuildingType.Mine && b.Type != BuildingType.Farm && b.Type != BuildingType.Power) continue;
                if (Pathfind(b.NodeId, "hub", true, false, false) != null) continue;
                found = b.NodeId;
            }
            return found;
        }

        public void SetTool(Tool tool)
        {
            var gate = OpeningGate();
            if (gate == "farm" && tool != Tool.Farm && tool != Tool.None)
            {
                SelectedTool = Tool.Farm;
                Hint = "Farm first — the crew is already chewing stores.";
                return;
            }
            if (gate == "route" && tool != Tool.Route && tool != Tool.None)
            {
                SelectedTool = Tool.Route;
                if (RouteFrom == null) RouteFrom = UnroutedProducer();
                Hint = "Click the Hub to finish the mag-rail from the Farm.";
                return;
            }
            if (tool == SelectedTool && tool != Tool.Route && gate == null)
            {
                SelectedTool = Tool.None;
                RouteFrom = null;
                return;
            }
            SelectedTool = tool;
            if (tool != Tool.Route) RouteFrom = null;
        }

        public bool ClickNode(string nodeId, out string why)
        {
            why = null;
            if (Phase != Phase.Playing) { why = "match over"; return false; }
            if (!Nodes.TryGetValue(nodeId, out var node)) { why = "missing"; return false; }
            switch (SelectedTool)
            {
                case Tool.Mine:
                case Tool.Farm:
                case Tool.Power:
                    if (TryProducer(ToBuilding(SelectedTool), node, out why)) return true;
                    if (node.Kind == NodeKind.Hub || node.Kind == NodeKind.Depot || node.Kind == NodeKind.Pad)
                    {
                        var source = UnroutedProducer();
                        if (source != null)
                        {
                            SelectedTool = Tool.Route;
                            RouteFrom = source;
                            return TryRoute(node, out why);
                        }
                    }
                    return false;
                case Tool.Kinetic: case Tool.Splash: return TryTower(ToBuilding(SelectedTool), node, out why);
                case Tool.Route: return TryRoute(node, out why);
                case Tool.Barrier: return TryBarrier(node, out why);
                case Tool.Upgrade: return TryUpgrade(out why);
                default: why = "select a build tool"; return false;
            }
        }

        static BuildingType ToBuilding(Tool tool)
        {
            switch (tool)
            {
                case Tool.Mine: return BuildingType.Mine;
                case Tool.Farm: return BuildingType.Farm;
                case Tool.Power: return BuildingType.Power;
                case Tool.Kinetic: return BuildingType.Kinetic;
                case Tool.Splash: return BuildingType.Splash;
                default: throw new ArgumentOutOfRangeException(nameof(tool));
            }
        }

        public bool TryUpgrade(out string why)
        {
            why = null;
            if (HubLevel >= 2) { why = "already L2"; return false; }
            if (HubUpgradeLeft > 0) { why = "upgrading"; return false; }
            if (Ore < Balance.HubL2Ore || Food < Balance.HubL2Food || Power < Balance.HubL2Power)
            {
                why = "need more stockpile";
                return false;
            }
            Ore -= Balance.HubL2Ore; Food -= Balance.HubL2Food; Power -= Balance.HubL2Power;
            HubUpgradeLeft = Balance.HubL2Time;
            Emit(SimEventKind.Upgrade, "hub");
            return true;
        }

        bool TryProducer(BuildingType type, SimNode node, out string why)
        {
            why = null;
            if (node.Kind != NodeKind.Pad) { why = "producers go on mesa pads"; return false; }
            if (Buildings.ContainsKey(node.Id)) { why = "occupied"; return false; }
            var cost = type == BuildingType.Mine ? Balance.MineOre : type == BuildingType.Farm ? Balance.FarmOre : Balance.PowerOre;
            if (Ore < cost) { why = "need ore"; return false; }
            Ore -= cost;
            Buildings[node.Id] = new Building { Id = Nid(type.ToString()), Type = type, NodeId = node.Id, BuildLeft = 1.2f };
            AssignWorkers();
            SelectedTool = Tool.Route;
            RouteFrom = node.Id;
            Hint = "Click the Hub to lay mag-rail from this pad.";
            Emit(SimEventKind.Build, node.Id, node.X, node.Z);
            return true;
        }

        bool TryTower(BuildingType type, SimNode node, out string why)
        {
            why = null;
            if (node.Kind != NodeKind.Choke && node.Kind != NodeKind.Tower) { why = "towers go on choke platforms"; return false; }
            if (Buildings.ContainsKey(node.Id)) { why = "occupied"; return false; }
            if (type == BuildingType.Splash && HubLevel < 2) { why = "Splash needs Hub L2"; return false; }
            var ore = type == BuildingType.Kinetic ? Balance.KineticOre : Balance.SplashOre;
            var pwr = type == BuildingType.Kinetic ? Balance.KineticPower : Balance.SplashPower;
            if (Ore < ore || Power < pwr) { why = "need ore/power"; return false; }
            Ore -= ore; Power -= pwr;
            Buildings[node.Id] = new Building { Id = Nid(type.ToString()), Type = type, NodeId = node.Id, BuildLeft = 1f, Staffed = true };
            Emit(SimEventKind.Build, node.Id, node.X, node.Z);
            return true;
        }

        bool TryRoute(SimNode node, out string why)
        {
            why = null;
            if (RouteFrom == null) { RouteFrom = node.Id; return true; }
            if (RouteFrom == node.Id) { RouteFrom = null; Hint = "Route cancelled."; return true; }
            var edge = EdgeBetween(RouteFrom, node.Id);
            if (edge == null) { why = "no corridor"; Hint = "No mag-rail corridor between those nodes."; return false; }
            if (edge.Routed && edge.SabotagedUntil <= T) { why = "already routed"; return false; }
            if (Ore < Balance.RouteCost) { why = "need ore"; return false; }
            var splice = edge.Routed && edge.SabotagedUntil > T;
            Ore -= Balance.RouteCost;
            edge.Routed = true;
            edge.SabotagedUntil = 0;
            var a = Nodes[edge.A];
            var b = Nodes[edge.B];
            RouteFrom = null;
            SelectedTool = Tool.Route;
            if (splice) Hint = "Rail live. Haulers are rolling again.";
            Emit(SimEventKind.Route, edgeId: edge.Id, x: (a.X + b.X) * 0.5f, z: (a.Z + b.Z) * 0.5f,
                fromX: a.X, fromZ: a.Z, toX: b.X, toZ: b.Z, reason: splice ? "splice" : null);
            return true;
        }

        public List<string> RouteEnds(string from = null)
        {
            from = from ?? RouteFrom;
            var cut = new List<SimEdge>();
            foreach (var e in Edges.Values)
                if (e.Routed && e.SabotagedUntil > T) cut.Add(e);
            if (cut.Count > 0 && (SelectedTool == Tool.Route || from == null))
            {
                var ids = new HashSet<string>();
                foreach (var e in cut)
                {
                    ids.Add(e.A);
                    ids.Add(e.B);
                }
                if (from != null && ids.Contains(from))
                {
                    var local = new List<string>();
                    foreach (var e in cut)
                    {
                        if (e.A == from) local.Add(e.B);
                        else if (e.B == from) local.Add(e.A);
                    }
                    if (local.Count > 0) return local;
                }
                if (from == null) return new List<string>(ids);
            }
            var ends = new List<string>();
            if (from == null) return ends;
            foreach (var edge in Neighbors(from))
            {
                if (Other(edge, from) == "hub") return new List<string> { "hub" };
            }
            foreach (var edge in Neighbors(from))
            {
                var other = Other(edge, from);
                var kind = Nodes[other].Kind;
                if (kind == NodeKind.Hub || kind == NodeKind.Depot || kind == NodeKind.Pad)
                    ends.Add(other);
            }
            return ends;
        }

        bool TryBarrier(SimNode node, out string why)
        {
            why = null;
            if (node.Kind != NodeKind.Choke) { why = "barriers on chokes"; return false; }
            if (Ore < Balance.BarrierCost) { why = "need ore"; return false; }
            Ore -= Balance.BarrierCost;
            foreach (var e in Neighbors(node.Id))
            {
                var other = Other(e, node.Id);
                if (Nodes[other].Kind == NodeKind.Spawn) e.Barrier = true;
            }
            Emit(SimEventKind.Barrier, node.Id, node.X, node.Z);
            return true;
        }

        public void AssignWorkers()
        {
            var producers = new List<Building>();
            foreach (var b in Buildings.Values)
                if (b.Type == BuildingType.Mine || b.Type == BuildingType.Farm || b.Type == BuildingType.Power)
                    producers.Add(b);
            producers.Sort((a, b) => Diversity(a, producers).CompareTo(Diversity(b, producers)));
            var free = WorkersTotal;
            foreach (var b in producers)
            {
                b.Staffed = free > 0;
                if (free > 0) free--;
            }
        }

        static int Diversity(Building b, List<Building> all)
        {
            Building first = null;
            foreach (var x in all) if (x.Type == b.Type) { first = x; break; }
            var isFirst = first != null && first.Id == b.Id;
            if (b.Type == BuildingType.Farm) return isFirst ? 0 : 5;
            if (b.Type == BuildingType.Power) return isFirst ? 1 : 6;
            if (b.Type == BuildingType.Mine) return isFirst ? 2 : 4;
            return 9;
        }

        public void Tick(float dt)
        {
            if (Phase != Phase.Playing) return;
            dt = Math.Min(dt, 0.05f);
            T += dt;
            TickUpgrade(dt);
            TickBuildings(dt);
            TickFood(dt);
            TickHaulers(dt);
            TickWaves(dt);
            TickEnemies(dt);
            TickTowers(dt);
            TickHubGun(dt);
            RefreshHint();
            CheckEnd();
        }

        void RefreshHint()
        {
            var gate = OpeningGate();
            if (gate == "farm")
            {
                Hint = "Food is already draining. Place a Farm on a mesa pad, then click the Hub.";
                return;
            }
            if (gate == "route")
            {
                Hint = "Click the Hub to finish the mag-rail from the Farm.";
                return;
            }
            if (ActiveCut() != null)
            {
                Hint = "HAUL CUT. Route is armed — click the glowing pad to splice the rail.";
                return;
            }
            if (HoldOrder == HoldOrder.Power)
            {
                Hint = (PowerBrownout ? "GUNS DRY. " : "") + "GUNS ORDER — haulers rush Power. H to flip.";
                return;
            }
            if (HoldOrder == HoldOrder.Food)
            {
                Hint = (Food < 8f ? "LARDER THIN. " : "") + "CREW ORDER — haulers rush Food. H to flip.";
                return;
            }
            var towers = 0;
            foreach (var b in Buildings.Values)
                if (b.Type == BuildingType.Kinetic || b.Type == BuildingType.Splash) towers++;
            if (towers > 0 && Power < 4f)
            {
                Hint = "Brownout. Towers are dry. Keep the Power pylon on live rails.";
                return;
            }
            var watch = MidWatch();
            if (watch != null && !string.IsNullOrEmpty(watch.Copy))
                Hint = watch.Copy;
        }

        void TickUpgrade(float dt)
        {
            if (HubUpgradeLeft <= 0) return;
            HubUpgradeLeft -= dt;
            if (HubUpgradeLeft > 0) return;
            HubUpgradeLeft = 0;
            HubLevel = 2;
            _hubL2At = T;
            Emit(SimEventKind.Upgrade, "hub");
            WorkersTotal += Balance.ExtraWorkers;
            var depot = Nodes["depot"];
            for (var i = 0; i < Balance.ExtraHaulers; i++)
            {
                Haulers.Add(new Hauler { Id = Nid("haul"), X = depot.X, Z = depot.Z, NodeId = "depot" });
            }
            AssignWorkers();
        }

        static Resource ResourceOf(BuildingType type)
        {
            switch (type)
            {
                case BuildingType.Mine: return Resource.Ore;
                case BuildingType.Farm: return Resource.Food;
                case BuildingType.Power: return Resource.Power;
                default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        void TickBuildings(float dt)
        {
            foreach (var b in Buildings.Values)
            {
                if (b.BuildLeft > 0) { b.BuildLeft = Math.Max(0, b.BuildLeft - dt); continue; }
                if (b.Type != BuildingType.Mine && b.Type != BuildingType.Farm && b.Type != BuildingType.Power) continue;
                if (!b.Staffed) continue;
                if (b.Type != BuildingType.Power && Power <= 0) continue;
                if (b.Type != BuildingType.Power) Power = Math.Max(0, Power - Balance.PowerMineFarm * dt);
                var res = ResourceOf(b.Type);
                var rate = b.Type == BuildingType.Mine ? 1.35f : b.Type == BuildingType.Farm ? 1.15f : 1.05f;
                b.Buffer[res] = Math.Min(22f, b.Buffer[res] + rate * dt);
            }
        }

        void TickFood(float dt)
        {
            var n = 0;
            foreach (var b in Buildings.Values)
                if (b.Type != BuildingType.Hub && b.Type != BuildingType.Depot) n++;
            Food = Math.Max(0, Food - (Balance.FoodDrain + n * Balance.FoodDrainPerBuilding) * dt);
            if (Food <= 0.01f)
            {
                if (StarveTimer <= 0f) Emit(SimEventKind.WarnFood);
                StarveTimer += dt;
            }
            else StarveTimer = 0;
        }

        void TickHaulers(float dt)
        {
            foreach (var h in Haulers)
            {
                if (h.Wait > 0) { h.Wait -= dt; if (h.Wait > 0) continue; h.BusyAt = null; }
                if (h.Path.Count == 0) PlanHauler(h);
                Advance(h, dt, Balance.HaulerSpeed, true);
                if (h.Path.Count == 0) OnArrive(h);
            }
        }

        void PlanHauler(Hauler h)
        {
            if (h.CargoAmount > 0)
            {
                var path = Pathfind(h.NodeId, "hub", true, false, false);
                h.Path.Clear();
                if (path != null) for (var i = 1; i < path.Count; i++) h.Path.Add(path[i]);
                return;
            }
            string best = null;
            var bestScore = float.NegativeInfinity;
            var foodNeed = Food < 12f ? 90f : Food < 20f ? 28f : 0f;
            var pwrNeed = Power < 6f ? 110f : Power < 14f ? 36f : 0f;
            var oreNeed = Ore < 18f ? 18f : 0f;
            switch (HoldOrder)
            {
                case HoldOrder.Auto:
                    break;
                case HoldOrder.Power:
                    pwrNeed += 240f;
                    foodNeed *= 0.2f;
                    oreNeed *= 0.2f;
                    break;
                case HoldOrder.Food:
                    foodNeed += 240f;
                    pwrNeed *= 0.2f;
                    oreNeed *= 0.2f;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(HoldOrder), HoldOrder, null);
            }
            foreach (var b in Buildings.Values)
            {
                if (b.Type != BuildingType.Mine && b.Type != BuildingType.Farm && b.Type != BuildingType.Power) continue;
                if (b.BuildLeft > 0) continue;
                var res = ResourceOf(b.Type);
                if (b.Buffer[res] < 1) continue;
                var path = Pathfind(h.NodeId, b.NodeId, true, false, false);
                if (path == null) continue;
                var hunger = res == Resource.Food ? foodNeed : res == Resource.Power ? pwrNeed : oreNeed;
                var score = b.Buffer[res] * 4 - path.Count + hunger;
                if (score > bestScore) { bestScore = score; best = b.NodeId; }
            }
            if (best == null) return;
            var p2 = Pathfind(h.NodeId, best, true, false, false);
            h.Path.Clear();
            if (p2 != null) for (var i = 1; i < p2.Count; i++) h.Path.Add(p2[i]);
        }

        void OnArrive(Hauler h)
        {
            if (h.CargoAmount > 0 && h.NodeId == "hub")
            {
                var kind = h.CargoKind ?? Resource.Ore;
                var amount = h.CargoAmount;
                if (kind == Resource.Ore) Ore += amount;
                else if (kind == Resource.Food) Food += amount;
                else Power += amount;
                h.CargoAmount = 0;
                h.Wait = Balance.DepositBusy;
                h.BusyAt = "hub";
                Emit(SimEventKind.Deposit, "hub", amount: amount, resource: kind);
                    if (RaidLive)
                    {
                        _surgeUntil = T + (HubHp < 72f ? 0.85f : 0.55f);
                        Emit(SimEventKind.Surge, "hub", amount: amount, resource: kind);
                    }
                return;
            }
            if (h.CargoAmount == 0 && Buildings.TryGetValue(h.NodeId, out var b) &&
                (b.Type == BuildingType.Mine || b.Type == BuildingType.Farm || b.Type == BuildingType.Power))
            {
                var res = ResourceOf(b.Type);
                var take = Math.Min(Balance.HaulerCapacity, (int)Math.Floor(b.Buffer[res]));
                if (take >= 1)
                {
                    b.Buffer[res] -= take;
                    h.CargoKind = res;
                    h.CargoAmount = take;
                    h.Wait = Balance.PickupBusy;
                    h.BusyAt = h.NodeId;
                }
            }
        }

        void Advance(Hauler h, float dt, float speed, bool congest)
        {
            AdvanceAgent(ref h.X, ref h.Z, ref h.NodeId, h.Path, dt, speed, congest, h);
        }

        void AdvanceAgent(ref float x, ref float z, ref string nodeId, List<string> path, float dt, float speed, bool congest, Hauler self)
        {
            if (path.Count == 0) return;
            var nextId = path[0];
            if (!Nodes.TryGetValue(nextId, out var next)) { path.Clear(); return; }
            if (congest && NodeBusy(nextId, self)) return;
            var dx = next.X - x;
            var dz = next.Z - z;
            var d = (float)Math.Sqrt(dx * dx + dz * dz);
            var step = speed * dt;
            if (d <= step + 0.08f)
            {
                x = next.X; z = next.Z; nodeId = nextId; path.RemoveAt(0);
                return;
            }
            x += dx / d * step;
            z += dz / d * step;
        }

        bool NodeBusy(string nodeId, Hauler self)
        {
            foreach (var h in Haulers)
                if (!ReferenceEquals(h, self) && h.BusyAt == nodeId && h.Wait > 0) return true;
            return false;
        }

        void TickWaves(float dt)
        {
            for (var i = _pending.Count - 1; i >= 0; i--)
            {
                if (_pending[i].at > T) continue;
                Spawn(_pending[i].type, _pending[i].spawn);
                _pending.RemoveAt(i);
            }
            if (WaveIndex >= Balance.WavesToWin) return;
            NextWaveIn = Math.Max(0, NextWaveIn - dt);
            if (NextWaveIn > 0) return;
            WaveIndex++;
            QueueWave(WaveIndex);
            NextWaveIn = WaveIndex < Balance.WavesToWin ? _waveGap[WaveIndex] : 0;
            Emit(SimEventKind.Wave, wave: WaveIndex);
        }

        void QueueWave(int index)
        {
            var t = T;
            void Pack(EnemyType type, int count, string spawn, float stagger)
            {
                for (var i = 0; i < count; i++)
                {
                    _pending.Add((t, type, spawn));
                    t += stagger;
                }
            }
            switch (index)
            {
                case 1: Pack(EnemyType.Grunt, 4, "spawn_e", 0.55f); break;
                case 2: Pack(EnemyType.Grunt, 5, "spawn_n", 0.45f); Pack(EnemyType.Runner, 1, "spawn_e", 0.4f); break;
                case 3: Pack(EnemyType.Grunt, 4, "spawn_w", 0.4f); Pack(EnemyType.Brute, 2, "spawn_e", 0.7f); break;
                case 4: Pack(EnemyType.Grunt, 6, "spawn_e", 0.35f); Pack(EnemyType.Runner, 2, "spawn_n", 0.4f); Pack(EnemyType.Brute, 1, "spawn_w", 0.6f); break;
                case 5: Pack(EnemyType.Grunt, 5, "spawn_n", 0.32f); Pack(EnemyType.Brute, 3, "spawn_e", 0.55f); Pack(EnemyType.Runner, 3, "spawn_w", 0.35f); break;
                case 6: Pack(EnemyType.Grunt, 8, "spawn_e", 0.28f); Pack(EnemyType.Brute, 4, "spawn_n", 0.5f); Pack(EnemyType.Runner, 4, "spawn_w", 0.3f); break;
                default: throw new ArgumentOutOfRangeException(nameof(index), index, null);
            }
        }

        void Spawn(EnemyType type, string spawnId)
        {
            var spawn = Nodes[spawnId];
            float hp = type == EnemyType.Brute ? 120f : type == EnemyType.Runner ? 52f : 36f;
            var e = new Enemy
            {
                Id = Nid("e"),
                Type = type,
                X = spawn.X,
                Z = spawn.Z,
                Hp = hp,
                MaxHp = hp,
                NodeId = spawnId
            };
            var path = Pathfind(spawnId, "hub", false, type == EnemyType.Runner, true);
            if (path != null) for (var i = 1; i < path.Count; i++) e.Path.Add(path[i]);
            Enemies.Add(e);
        }

        void TickEnemies(float dt)
        {
            var hub = Nodes["hub"];
            foreach (var e in Enemies)
            {
                e.Flash = Math.Max(0, e.Flash - dt);
                var speed = e.Type == EnemyType.Brute ? 1.05f : e.Type == EnemyType.Runner ? 4.35f : 2.2f;
                if (e.SlowUntil > T) speed *= 1f - Balance.SplashSlow;
                    var dmg = e.Type == EnemyType.Brute ? 18f : e.Type == EnemyType.Runner ? 5f : 8f;
                if (e.Path.Count == 0)
                {
                    var dx = e.X - hub.X; var dz = e.Z - hub.Z;
                    if (Math.Sqrt(dx * dx + dz * dz) > 0.9)
                    {
                        var path = Pathfind(e.NodeId, "hub", false, e.Type == EnemyType.Runner, true);
                        e.Path.Clear();
                        if (path != null) for (var i = 1; i < path.Count; i++) e.Path.Add(path[i]);
                    }
                    else
                    {
                        e.AttackCd -= dt;
                        if (e.AttackCd <= 0)
                        {
                            if (_surgeUntil > T) dmg *= 0.72f;
                            HubHp = Math.Max(0, HubHp - dmg);
                            e.AttackCd = 0.85f;
                            Emit(SimEventKind.Hit, "hub", amount: dmg);
                        }
                    }
                }
                else
                {
                    var nextId = e.Path[0];
                    var edge = EdgeBetween(e.NodeId, nextId);
                    var stepSpeed = (edge != null && edge.Barrier && e.Type != EnemyType.Runner) ? speed * 0.32f : speed;
                    var before = e.NodeId;
                    AdvanceAgent(ref e.X, ref e.Z, ref e.NodeId, e.Path, dt, stepSpeed, false, null);
                    if (e.Type == EnemyType.Runner && e.NodeId != before)
                    {
                        var crossed = EdgeBetween(before, e.NodeId);
                        if (crossed != null && crossed.Routed && crossed.SabotagedUntil <= T && _rng.NextDouble() < Balance.RunnerSabotageChance)
                        {
                            crossed.SabotagedUntil = T + Balance.RunnerSabotage;
                            ReplanIfCut(crossed.Id);
                            var midX = (Nodes[crossed.A].X + Nodes[crossed.B].X) * 0.5f;
                            var midZ = (Nodes[crossed.A].Z + Nodes[crossed.B].Z) * 0.5f;
                            Emit(SimEventKind.Sabotage, edgeId: crossed.Id, x: midX, z: midZ);
                            if (OpeningGate() == null)
                            {
                                SelectedTool = Tool.Route;
                                RouteFrom = crossed.A;
                                Hint = "HAUL CUT. Route is armed — click the glowing pad to splice the rail.";
                            }
                        }
                    }
                }
            }
        }

        void TickTowers(float dt)
        {
            foreach (var b in Buildings.Values)
            {
                if (b.Type != BuildingType.Kinetic && b.Type != BuildingType.Splash) continue;
                if (b.BuildLeft > 0) continue;
                Power = Math.Max(0, Power - Balance.TowerIdlePower * dt);
                b.Cooldown = Math.Max(0, b.Cooldown - dt);
                if (b.Cooldown > 0) continue;
                var node = Nodes[b.NodeId];
                var range = b.Type == BuildingType.Kinetic ? Balance.KineticRange : Balance.SplashRange;
                Enemy target = null;
                var best = float.PositiveInfinity;
                foreach (var e in Enemies)
                {
                    var dx = node.X - e.X; var dz = node.Z - e.Z;
                    var d = (float)Math.Sqrt(dx * dx + dz * dz);
                    if (d > range) continue;
                    var pri = e.Type == EnemyType.Brute ? 0 : e.Type == EnemyType.Grunt ? 1 : 2;
                    var score = pri * 100 + d;
                    if (score < best) { best = score; target = e; }
                }
                if (target == null) continue;
                var shotCost = b.Type == BuildingType.Kinetic ? Balance.KineticPowerShot : Balance.SplashPowerShot;
                if (Power < shotCost)
                {
                    b.Cooldown = 0.22f;
                    _brownoutAt = T;
                    Emit(SimEventKind.Brownout, b.NodeId, node.X, node.Z);
                    continue;
                }
                Power = Math.Max(0, Power - shotCost);
                if (b.Type == BuildingType.Kinetic)
                {
                    Hurt(target, Balance.KineticDamage);
                    b.Cooldown = Balance.KineticCooldown;
                    Emit(SimEventKind.Shot, b.NodeId, target.X, target.Z, node.X, node.Z, target.X, target.Z, Balance.KineticDamage, enemyId: target.Id);
                }
                else
                {
                    foreach (var e in Enemies)
                    {
                        var dx = target.X - e.X; var dz = target.Z - e.Z;
                        if (Math.Sqrt(dx * dx + dz * dz) <= Balance.SplashRadius)
                        {
                            Hurt(e, Balance.SplashDamage);
                            e.SlowUntil = Math.Max(e.SlowUntil, T + Balance.SplashSlowTime);
                        }
                    }
                    b.Cooldown = Balance.SplashCooldown;
                    Emit(SimEventKind.Splash, b.NodeId, target.X, target.Z, node.X, node.Z, target.X, target.Z, Balance.SplashDamage, enemyId: target.Id);
                }
            }
            Enemies.RemoveAll(e => e.Hp <= 0);
        }

        void TickHubGun(float dt)
        {
            _hubGunCd = Math.Max(0, _hubGunCd - dt);
            if (_hubGunCd > 0) return;
            var hub = Nodes["hub"];
            Enemy target = null;
            var best = 3.4f;
            foreach (var e in Enemies)
            {
                var dx = hub.X - e.X; var dz = hub.Z - e.Z;
                var d = (float)Math.Sqrt(dx * dx + dz * dz);
                if (d <= best) { best = d; target = e; }
            }
            if (target == null) return;
            if (Power < 0.22f) { _brownoutAt = T; Emit(SimEventKind.Brownout, "hub"); return; }
            Power -= 0.22f;
            Hurt(target, 12f);
            _hubGunCd = 0.5f;
            Emit(SimEventKind.Shot, "hub", target.X, target.Z, 0, 0, target.X, target.Z, 12f, enemyId: target.Id);
            Enemies.RemoveAll(e => e.Hp <= 0);
        }

        void ReplanIfCut(string edgeId)
        {
            foreach (var h in Haulers)
            {
                if (h.Path.Count == 0) continue;
                var chain = new List<string> { h.NodeId };
                chain.AddRange(h.Path);
                for (var i = 0; i < chain.Count - 1; i++)
                {
                    var edge = EdgeBetween(chain[i], chain[i + 1]);
                    if (edge != null && edge.Id == edgeId)
                    {
                        h.Path.Clear();
                        break;
                    }
                }
            }
        }

        void Hurt(Enemy e, float amount)
        {
            e.Hp -= amount;
            e.Flash = 0.12f;
            Emit(SimEventKind.Hit, e.NodeId, e.X, e.Z, amount: amount, enemyId: e.Id);
            if (e.Hp <= 0)
            {
                var scrap = e.Type == EnemyType.Brute ? 8 : e.Type == EnemyType.Runner ? 4 : 3;
                Ore += scrap;
                Emit(SimEventKind.Death, e.NodeId, e.X, e.Z, amount: scrap, resource: Resource.Ore, enemyId: e.Id);
            }
        }

        public List<string> Pathfind(string from, string to, bool routedOnly, bool ignoreBarriers, bool preferRouted)
        {
            if (from == to) return new List<string> { from };
            var dist = new Dictionary<string, float> { [from] = 0 };
            var prev = new Dictionary<string, string>();
            var pq = new List<(string id, float d)> { (from, 0) };
            while (pq.Count > 0)
            {
                pq.Sort((a, b) => a.d.CompareTo(b.d));
                var cur = pq[0];
                pq.RemoveAt(0);
                if (cur.id == to) break;
                if (Math.Abs(cur.d - dist[cur.id]) > 0.0001f) continue;
                foreach (var edge in Neighbors(cur.id))
                {
                    var live = edge.Routed && edge.SabotagedUntil <= T;
                    if (routedOnly && !live) continue;
                    var nxt = Other(edge, cur.id);
                    var a = Nodes[cur.id]; var b = Nodes[nxt];
                    var dx = a.X - b.X; var dz = a.Z - b.Z;
                    var w = (float)Math.Sqrt(dx * dx + dz * dz);
                    if (edge.Barrier && !ignoreBarriers) w *= 3.2f;
                    if (preferRouted && live) w *= 0.45f;
                    if (preferRouted && !live) w *= 1.7f;
                    var nd = cur.d + w;
                    if (!dist.ContainsKey(nxt) || nd < dist[nxt])
                    {
                        dist[nxt] = nd;
                        prev[nxt] = cur.id;
                        pq.Add((nxt, nd));
                    }
                }
            }
            if (!dist.ContainsKey(to)) return null;
            var path = new List<string> { to };
            while (path[0] != from)
            {
                if (!prev.ContainsKey(path[0])) return null;
                path.Insert(0, prev[path[0]]);
            }
            return path;
        }

        void CheckEnd()
        {
            if (HubHp <= 0)
            {
                Phase = Phase.LostHub;
                Emit(SimEventKind.Lose, "hub", reason: "hub");
                return;
            }
            if (StarveTimer >= Balance.StarveFail)
            {
                Phase = Phase.LostStarve;
                Emit(SimEventKind.Lose, reason: "starve");
                return;
            }
            if (WaveIndex >= Balance.WavesToWin && Enemies.Count == 0 && _pending.Count == 0 && HubLevel >= 2)
            {
                Phase = Phase.Won;
                Emit(SimEventKind.Win, "hub");
            }
        }

        public static HeadlessResult RunHeadless(int seed, float seconds)
        {
            var game = new GameSim(seed);
            var demo = new DemoPilot();
            var deposits = 0;
            var kills = 0;
            var brownouts = 0;
            var sabotages = 0;
            var minPower = game.Power;
            const float step = 1f / 20f;
            for (var t = 0f; t < seconds && game.Phase == Phase.Playing; t += step)
            {
                demo.Step(game, step);
                game.Tick(step);
                minPower = Math.Min(minPower, game.Power);
                foreach (var ev in game.DrainEvents())
                {
                    switch (ev.Kind)
                    {
                        case SimEventKind.Deposit: deposits++; break;
                        case SimEventKind.Death: kills++; break;
                        case SimEventKind.Brownout: brownouts++; break;
                        case SimEventKind.Sabotage: sabotages++; break;
                        case SimEventKind.Shot:
                        case SimEventKind.Splash:
                        case SimEventKind.Hit:
                        case SimEventKind.Wave:
                        case SimEventKind.WarnFood:
                        case SimEventKind.Build:
                        case SimEventKind.Upgrade:
                        case SimEventKind.Win:
                        case SimEventKind.Lose:
                        case SimEventKind.Route:
                        case SimEventKind.Barrier:
                        case SimEventKind.Surge:
                        case SimEventKind.Hold:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(ev.Kind), ev.Kind, null);
                    }
                }
            }
            var routes = 0;
            foreach (var e in game.Edges.Values) if (e.Routed) routes++;
            return new HeadlessResult
            {
                Phase = game.Phase,
                T = game.T,
                WaveIndex = game.WaveIndex,
                HubLevel = game.HubLevel,
                Deposits = deposits,
                Buildings = game.Buildings.Count,
                Routes = routes,
                Kills = kills,
                Win = game.Phase == Phase.Won,
                Brownouts = brownouts,
                MinPower = minPower,
                Sabotages = sabotages
            };
        }
    }
}
