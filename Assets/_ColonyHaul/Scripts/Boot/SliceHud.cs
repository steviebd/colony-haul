using System;
using UnityEngine;

namespace ColonyHaul
{
    public sealed class SliceHud
    {
        struct ToolBtn
        {
            public Tool Tool;
            public string Label;
            public string Hint;
            public KeyCode Key;
        }

        static readonly ToolBtn[] Tools =
        {
            new ToolBtn { Tool = Tool.Farm, Label = "Farm", Hint = "10 ore · mesa pad · food", Key = KeyCode.Alpha1 },
            new ToolBtn { Tool = Tool.Mine, Label = "Mine", Hint = "12 ore · mesa pad · ore", Key = KeyCode.Alpha2 },
            new ToolBtn { Tool = Tool.Power, Label = "Power", Hint = "13 ore · mesa pad · power", Key = KeyCode.Alpha3 },
            new ToolBtn { Tool = Tool.Route, Label = "Route", Hint = "3 ore · pad then Hub", Key = KeyCode.Alpha4 },
            new ToolBtn { Tool = Tool.Kinetic, Label = "Kinetic", Hint = "16 ore + 3 pwr · choke", Key = KeyCode.Alpha5 },
            new ToolBtn { Tool = Tool.Splash, Label = "Splash", Hint = "Hub L2 · 28 ore + 6 pwr", Key = KeyCode.Alpha6 },
            new ToolBtn { Tool = Tool.Barrier, Label = "Barrier", Hint = "5 ore · slows grunts/brutes", Key = KeyCode.Alpha7 },
            new ToolBtn { Tool = Tool.Upgrade, Label = "Hub L2", Hint = "64 ore + 30 food + 22 pwr", Key = KeyCode.U },
        };

        string _banner;
        float _bannerUntil;
        Color _bannerColor = new Color(0.82f, 0.28f, 0.32f, 0.95f);
        string _wave;
        float _waveUntil;
        bool _boot = true;
        public bool ShowBoot = true;

        public bool ConsumeBootPlay { get; private set; }
        public bool ConsumeBootDemo { get; private set; }
        public bool ConsumeRestart { get; private set; }
        public bool ConsumeHold { get; private set; }
        public Tool? ClickedTool { get; private set; }

        public void Flash(string text, float seconds)
        {
            Flash(text, seconds, new Color(0.82f, 0.28f, 0.32f, 0.95f));
        }

        public void Flash(string text, float seconds, Color color)
        {
            _banner = text;
            _bannerUntil = Time.unscaledTime + seconds;
            _bannerColor = color;
        }

        public void WaveCall(string copy, float seconds)
        {
            _wave = copy;
            _waveUntil = Time.unscaledTime + seconds;
        }

        public void Draw(GameSim game)
        {
            ConsumeBootPlay = ConsumeBootDemo = ConsumeRestart = ConsumeHold = false;
            ClickedTool = null;
            var e = Event.current;

            DrawTop(game);
            DrawLogistics(game);
            DrawCoach(game);
            DrawTray(game);
            DrawHint(game);
            DrawWave();
            DrawBanner();
            if (game.StarveTimer > 0.2f && game.Phase == Phase.Playing)
            {
                GUI.color = new Color(1f, 0.45f, 0.35f);
                GUI.Box(new Rect(Screen.width / 2 - 180, 152, 360, 28), "FOOD STORES EMPTY · colony starving");
                GUI.color = Color.white;
            }
            if (ShowBoot && _boot && game.Phase == Phase.Playing)
                DrawBoot();
            if (game.Phase != Phase.Playing) DrawEnd(game);

            if (e != null && e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape) _boot = false;
        }

        void DrawTop(GameSim game)
        {
            GUI.backgroundColor = new Color(0.06f, 0.1f, 0.12f, 0.92f);
            GUI.Box(new Rect(12, 10, Screen.width - 24, 72), "");
            GUI.backgroundColor = Color.white;
            GUI.Label(new Rect(24, 16, 280, 22), "COLONY HAUL");
            var sub = GUI.contentColor;
            var cut = game.ActiveCut();
            if (cut != null) GUI.contentColor = new Color(1f, 0.55f, 0.32f);
            else if (game.HubChewers() > 0) GUI.contentColor = new Color(1f, 0.38f, 0.28f);
            else if (game.HottestRailThreat() != null) GUI.contentColor = new Color(0.95f, 0.42f, 0.78f);
            else if (game.HubClosers() > 0) GUI.contentColor = new Color(1f, 0.32f, 0.18f);
            else if (game.GunsDry()) GUI.contentColor = new Color(1f, 0.48f, 0.22f);
            else if (game.WaveClearLive()) GUI.contentColor = new Color(0.55f, 0.9f, 0.5f);
            else if (game.GunsLow()) GUI.contentColor = new Color(1f, 0.62f, 0.28f);
            else if (game.OfflinePad() != null) GUI.contentColor = new Color(0.92f, 0.62f, 0.28f);
            else if (game.SittingStock() != null) GUI.contentColor = new Color(0.95f, 0.78f, 0.32f);
            else if (game.L2Ready()) GUI.contentColor = new Color(1f, 0.86f, 0.42f);
            else if (game.OpenChokeId() != null) GUI.contentColor = new Color(1f, 0.42f, 0.32f);
            else if (game.Surging) GUI.contentColor = new Color(0.45f, 0.9f, 1f);
            else if (game.BraceInbound() != null) GUI.contentColor = new Color(0.45f, 0.9f, 1f);
            else if (game.HoldOrder == HoldOrder.Power) GUI.contentColor = new Color(0.4f, 0.75f, 1f);
            else if (game.HoldOrder == HoldOrder.Food) GUI.contentColor = new Color(0.5f, 0.85f, 0.48f);
            else if (game.WaveIndex >= 5) GUI.contentColor = new Color(1f, 0.42f, 0.38f);
            string subCopy;
            if (cut != null)
                subCopy = game.CutStakeTitle();
            else if (game.HubChewers() > 0)
                subCopy = game.HubChewTitle();
            else if (game.HottestRailThreat() != null)
                subCopy = game.RailThreatTitle();
            else if (game.HubClosers() > 0)
                subCopy = game.CoreBoundTitle();
            else if (game.GunsDry())
                subCopy = game.GunsDryTitle();
            else if (game.WaveClearLive())
                subCopy = game.WaveClearTitle();
            else if (game.GunsLow())
                subCopy = game.GunsLowTitle();
            else if (game.OfflinePad() != null)
                subCopy = game.OfflineTitle();
            else if (game.SittingStock() != null)
                subCopy = game.SittingTitle();
            else if (game.L2Ready())
                subCopy = game.L2ReadyTitle();
            else if (game.OpenChokeId() != null)
                subCopy = game.OpenChokeTitle();
            else if (game.Surging)
                subCopy = "BRACE · Hub shrugs hits · " + GameSim.CeilSecs(game.SurgeLeft) + "s";
            else if (game.BraceInbound() != null)
                subCopy = game.BraceInboundCopy();
            else if (game.HoldOrder == HoldOrder.Power)
                subCopy = "GUNS ORDER · haulers rush Power · H flips";
            else if (game.HoldOrder == HoldOrder.Food)
                subCopy = "CREW ORDER · haulers rush Food · H flips";
            else if (game.WaveIndex >= 5)
                subCopy = "LAST RAIDS · hold the mesa";
            else
                subCopy = "Mesa 7 · dusk cycle · Unity slice";
            GUI.Label(new Rect(24, 36, 340, 18), subCopy);
            GUI.contentColor = sub;
            Chip(320, 18, "ORE", Mathf.FloorToInt(game.Ore).ToString(), new Color(0.94f, 0.64f, 0.23f));
            Chip(430, 18, "FOOD", Mathf.FloorToInt(game.Food).ToString(), new Color(0.5f, 0.85f, 0.48f));
            var pwrColor = game.PowerBrownout ? new Color(1f, 0.35f, 0.32f) : new Color(0.4f, 0.75f, 1f);
            Chip(540, 18, "PWR", Mathf.FloorToInt(game.Power).ToString(), pwrColor);
            Chip(650, 18, "STAFF", game.StaffedProducers() + "/" + game.WorkersTotal,
                game.CrewStretched() ? new Color(1f, 0.62f, 0.32f) : new Color(0.75f, 0.8f, 0.85f));
            GUI.Label(new Rect(Screen.width - 280, 18, 260, 20),
                game.WaveIndex == 0
                    ? $"WAVE 1 / {Balance.WavesToWin} IN {Mathf.CeilToInt(game.NextWaveIn)}S"
                    : game.WaveClearLive()
                        ? $"WAVE {game.WaveIndex} / {Balance.WavesToWin} · CLEAR · {GameSim.CeilSecs(game.NextWaveIn)}S"
                        : $"WAVE {game.WaveIndex} / {Balance.WavesToWin} · {game.Enemies.Count} LIVE");
            var hubHpColor = game.CoreThin ? new Color(1f, 0.42f, 0.32f) : Color.white;
            var oldHp = GUI.contentColor;
            GUI.contentColor = hubHpColor;
            GUI.Label(new Rect(Screen.width - 280, 40, 260, 20),
                $"Hub HP {Mathf.CeilToInt(game.HubHp)} · L{game.HubLevel} · {game.Haulers.Count} haul");
            GUI.contentColor = oldHp;
        }

        static void DrawLogistics(GameSim game)
        {
            GUI.backgroundColor = new Color(0.05f, 0.09f, 0.11f, 0.88f);
            GUI.Box(new Rect(Screen.width - 292, 88, 280, 172), "");
            GUI.backgroundColor = Color.white;
            var cut = game.ActiveCut();
            string haul;
            if (cut != null)
                haul = game.CutStakeCopy();
            else if (game.OfflinePad() != null)
                haul = game.OfflineCopy();
            else if (game.SittingStock() != null)
                haul = game.SittingCopy();
            else if (game.L2Ready())
                haul = game.L2ReadyCopy();
            else haul = "Haul " + game.HaulersLoaded + " loaded · " + (game.Haulers.Count - game.HaulersLoaded) + " idle";
            GUI.Label(new Rect(Screen.width - 280, 92, 256, 20), haul);
            var lanes = game.Lanes();
            var hot = game.HottestLane();
            GUI.Label(new Rect(Screen.width - 280, 112, 256, 20),
                LaneChip("E", lanes.East, hot == "east") + "  " +
                LaneChip("N", lanes.North, hot == "north") + "  " +
                LaneChip("W", lanes.West, hot == "west") +
                "  · " + game.IncomingRaiders + " inbound");
            GUI.Label(new Rect(Screen.width - 280, 132, 256, 20), game.NextWaveCopy());
            var towers = game.LiveTowers();
            string guns;
            if (towers <= 0) guns = "No guns yet — kinetic on a choke";
            else if (game.PowerBrownout) guns = "GUNS DRY — haul Power now";
            else if (game.GunsHungry()) guns = "Guns hungry — ~" + GameSim.CeilSecs(game.GunSecondsLeft()) + "s · haul Power";
            else guns = "Guns ~" + GameSim.CeilSecs(game.GunSecondsLeft()) + "s of fire · " + towers + " live";
            GUI.Label(new Rect(Screen.width - 280, 152, 256, 20), guns);
            GUI.Label(new Rect(Screen.width - 280, 172, 256, 20), LarderCopy(game));
            GUI.Label(new Rect(Screen.width - 280, 192, 256, 20), game.HoldOrderCopy());
            var chew = game.HubChewCopy();
            var threat = game.RailThreatCopy();
            var bound = game.CoreBoundCopy();
            var dry = game.GunsDryCopy();
            var clear = game.WaveClearCopy();
            var brace = game.BraceInboundCopy();
            string raidRead;
            if (chew != null) raidRead = chew;
            else if (threat != null) raidRead = threat;
            else if (bound != null) raidRead = bound;
            else if (dry != null) raidRead = dry;
            else if (clear != null) raidRead = clear;
            else if (game.GunsLowCopy() != null) raidRead = game.GunsLowCopy();
            else if (game.L2ReadyCopy() != null) raidRead = game.L2ReadyCopy();
            else if (game.OpenChokeCopy() != null) raidRead = game.OpenChokeCopy();
            else if (brace != null) raidRead = brace;
            else if (game.LiveTowers() > 0 || game.RaidLive) raidRead = game.GunLockCopy();
            else raidRead = "Combat haul BRACEs the Hub";
            GUI.Label(new Rect(Screen.width - 280, 212, 256, 20), raidRead);
        }

        static string LaneChip(string tag, int n, bool hot)
        {
            return (hot ? tag + "*" : tag) + n;
        }

        static string LarderCopy(GameSim game)
        {
            if (game.StarveTimer > 0.2f)
                return "STARVING · " + GameSim.CeilSecs(game.StarveSecondsLeft()) + "s to fail";
            var secs = GameSim.CeilSecs(game.FoodSecondsLeft());
            if (game.Food < 11f) return "LARDER THIN · ~" + secs + "s of food";
            return "Larder ~" + secs + "s of food";
        }

        static void DrawCoach(GameSim game)
        {
            if (game.Phase != Phase.Playing) return;
            var opening = game.OpeningCoach();
            if (!string.IsNullOrEmpty(opening))
            {
                var step = game.OpeningStep();
                GUI.backgroundColor = new Color(0.07f, 0.16f, 0.12f, 0.92f);
                GUI.Box(new Rect(Screen.width / 2 - 230, 88, 460, 58), "");
                GUI.backgroundColor = Color.white;
                GUI.Label(new Rect(Screen.width / 2 - 214, 92, 428, 22), opening);
                GUI.Label(new Rect(Screen.width / 2 - 214, 114, 428, 22),
                    (step >= 1 ? "✓" : "·") + " Farm   " +
                    (step >= 2 ? "✓" : "·") + " Rail to Hub   " +
                    (step >= 3 ? "✓" : "·") + " First hauls");
                return;
            }
            if (game.HubRaising)
            {
                GUI.backgroundColor = new Color(0.22f, 0.16f, 0.08f, 0.94f);
                GUI.Box(new Rect(Screen.width / 2 - 250, 88, 500, 58), "");
                GUI.backgroundColor = Color.white;
                GUI.Label(new Rect(Screen.width / 2 - 234, 92, 468, 22),
                    "HUB L2 RAISING — Splash unlocks in " + GameSim.CeilSecs(game.HubUpgradeLeft) + "s");
                GUI.Label(new Rect(Screen.width / 2 - 234, 114, 468, 22),
                    "Keep the farm rail live. West choke is next.");
                return;
            }
            var watch = game.MidWatch();
            if (watch == null || string.IsNullOrEmpty(watch.Copy)) return;
            GUI.backgroundColor = game.ActiveCut() != null
                ? new Color(0.32f, 0.1f, 0.06f, 0.95f)
                : game.CoreThin
                    ? new Color(0.22f, 0.08f, 0.08f, 0.94f)
                    : new Color(0.07f, 0.14f, 0.18f, 0.92f);
            GUI.Box(new Rect(Screen.width / 2 - 250, 88, 500, 58), "");
            GUI.backgroundColor = Color.white;
            GUI.Label(new Rect(Screen.width / 2 - 234, 92, 468, 22),
                (game.ActiveCut() != null ? "CUT  " : game.CoreThin ? "CORE  " : "WATCH  ") + watch.Copy);
            var lanes = game.Lanes();
            GUI.Label(new Rect(Screen.width / 2 - 234, 114, 468, 22),
                game.HoldCopy() +
                "  ·  E" + lanes.East + " N" + lanes.North + " W" + lanes.West);
        }

        static void Chip(float x, float y, string label, string value, Color color)
        {
            GUI.backgroundColor = new Color(color.r, color.g, color.b, 0.25f);
            GUI.Box(new Rect(x, y, 100, 48), "");
            GUI.backgroundColor = Color.white;
            var old = GUI.contentColor;
            GUI.contentColor = color;
            GUI.Label(new Rect(x + 8, y + 4, 90, 16), label);
            GUI.contentColor = Color.white;
            GUI.Label(new Rect(x + 8, y + 22, 90, 20), value);
            GUI.contentColor = old;
        }

        void DrawTray(GameSim game)
        {
            GUI.backgroundColor = new Color(0.05f, 0.09f, 0.11f, 0.9f);
            GUI.Box(new Rect(12, 92, 220, 8 * 44 + 72), "");
            GUI.backgroundColor = Color.white;
            var y = 100f;
            var gate = game.OpeningGate();
            var watch = game.MidWatch();
            var pulseTool = watch != null ? watch.Pulse : Tool.None;
            var pulse = 0.55f + 0.45f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * 4f));
            foreach (var tool in Tools)
            {
                var locked = tool.Tool == Tool.Splash && game.HubLevel < 2;
                var selected = game.SelectedTool == tool.Tool;
                var coachFarm = gate == "farm" && tool.Tool == Tool.Farm;
                var coachRoute = gate == "route" && tool.Tool == Tool.Route;
                var midPulse = pulseTool == tool.Tool && pulseTool != Tool.None && gate == null;
                var splashFresh = game.SplashFresh() && tool.Tool == Tool.Splash && !locked;
                var shortStock = !locked && !game.CanAfford(tool.Tool);
                GUI.backgroundColor = selected ? new Color(0.25f, 0.85f, 0.8f) : new Color(0.12f, 0.18f, 0.2f);
                if (shortStock) GUI.backgroundColor = new Color(0.1f, 0.11f, 0.12f);
                if (coachFarm) GUI.backgroundColor = new Color(0.2f * pulse, 0.85f * pulse, 0.38f);
                if (coachRoute) GUI.backgroundColor = new Color(0.95f * pulse, 0.82f * pulse, 0.28f);
                if (midPulse) GUI.backgroundColor = PulseColor(tool.Tool, pulse);
                var l2Ready = game.L2Ready() && tool.Tool == Tool.Upgrade && gate == null
                    && game.ActiveCut() == null && !game.GunsLow() && !game.GunsDry()
                    && game.HubChewers() == 0 && game.HubClosers() == 0;
                if (l2Ready) GUI.backgroundColor = PulseColor(Tool.Upgrade, pulse);
                var openGun = game.OpenChokeId() != null && gate == null && game.ActiveCut() == null
                    && !game.L2Ready() && !game.GunsLow() && !game.GunsDry();
                var openSplash = openGun && game.OpenChokeId() == "choke_w"
                    && game.HubLevel >= 2 && !game.HasType(BuildingType.Splash) && tool.Tool == Tool.Splash;
                var openKinetic = openGun && !openSplash && tool.Tool == Tool.Kinetic
                    && !(game.OpenChokeId() == "choke_w" && game.HubLevel >= 2 && !game.HasType(BuildingType.Splash));
                if (openSplash) GUI.backgroundColor = PulseColor(Tool.Splash, pulse);
                if (openKinetic) GUI.backgroundColor = PulseColor(Tool.Kinetic, pulse);
                if (splashFresh) GUI.backgroundColor = PulseColor(Tool.Splash, pulse);
                if (locked) GUI.backgroundColor = new Color(0.12f, 0.12f, 0.12f);
                var label = (selected ? "▶ " : "") + tool.Label;
                if (coachFarm) label = "▶ 1 Farm — south pad";
                if (coachRoute) label = "▶ 2 Route — click Hub";
                if (midPulse) label = "▶ " + tool.Label;
                if (l2Ready) label = "▶ Hub L2 — Splash next";
                if (openKinetic) label = "▶ Kinetic — OPEN " + (game.OpenChokeLane() ?? "");
                if (openSplash) label = "▶ Splash — OPEN WEST";
                if (splashFresh) label = "▶ Splash — WEST choke";
                if (shortStock && !midPulse && !l2Ready && !openKinetic && !openSplash && !splashFresh)
                    label = tool.Label + "  · short";
                if (locked) label = "Splash  · locked Hub L2";
                if (GUI.Button(new Rect(20, y, 204, 40), label + "\n" + tool.Hint))
                {
                    if (tool.Tool == Tool.Upgrade) ClickedTool = Tool.Upgrade;
                    else ClickedTool = tool.Tool;
                }
                GUI.backgroundColor = Color.white;
                y += 44;
            }
            GUI.Label(new Rect(20, y + 8, 200, 36), "1–7 tools · U Hub L2\nD demo · R restart · H hold");
            var holdY = y + 48;
            if (game.HoldReady)
            {
                var holdPulse = watch != null && !string.IsNullOrEmpty(watch.Copy) &&
                    (watch.Copy.IndexOf("H for", StringComparison.Ordinal) >= 0 ||
                     watch.Copy.IndexOf("HOLD", StringComparison.Ordinal) >= 0 ||
                     watch.Copy.IndexOf("GUNS ORDER", StringComparison.Ordinal) >= 0 ||
                     watch.Copy.IndexOf("CREW ORDER", StringComparison.Ordinal) >= 0);
                string holdLabel;
                Color holdColor;
                switch (game.HoldOrder)
                {
                    case HoldOrder.Auto:
                        holdLabel = "H Hold — auto";
                        holdColor = holdPulse ? new Color(0.9f * pulse, 0.78f * pulse, 0.5f) : new Color(0.16f, 0.22f, 0.24f);
                        break;
                    case HoldOrder.Power:
                        holdLabel = "H GUNS — rush Power";
                        holdColor = new Color(0.28f, 0.55f, 0.85f);
                        break;
                    case HoldOrder.Food:
                        holdLabel = "H CREW — rush Food";
                        holdColor = new Color(0.22f, 0.62f, 0.32f);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(game.HoldOrder), game.HoldOrder, null);
                }
                GUI.backgroundColor = holdColor;
                if (GUI.Button(new Rect(20, holdY, 204, 40), holdLabel + "\nlocks idle haulers · H cycles"))
                    ConsumeHold = true;
                GUI.backgroundColor = Color.white;
            }
            else
            {
                GUI.backgroundColor = new Color(0.1f, 0.11f, 0.12f);
                GUI.Box(new Rect(20, holdY, 204, 40), "H Hold · locked\nwave 4 + Hub L2");
                GUI.backgroundColor = Color.white;
            }
        }

        static Color PulseColor(Tool tool, float pulse)
        {
            switch (tool)
            {
                case Tool.Farm: return new Color(0.2f * pulse, 0.85f * pulse, 0.38f);
                case Tool.Mine: return new Color(0.94f * pulse, 0.64f * pulse, 0.23f);
                case Tool.Power: return new Color(0.32f * pulse, 0.68f * pulse, 0.94f);
                case Tool.Route: return new Color(0.95f * pulse, 0.82f * pulse, 0.28f);
                case Tool.Kinetic: return new Color(0.45f * pulse, 0.9f * pulse, 0.88f);
                case Tool.Splash: return new Color(0.94f * pulse, 0.63f * pulse, 0.38f);
                case Tool.Barrier: return new Color(0.95f * pulse, 0.32f * pulse, 0.34f);
                case Tool.Upgrade: return new Color(0.9f * pulse, 0.78f * pulse, 0.5f);
                case Tool.None: return new Color(0.12f, 0.18f, 0.2f);
                default: throw new ArgumentOutOfRangeException(nameof(tool), tool, null);
            }
        }

        void DrawHint(GameSim game)
        {
            GUI.backgroundColor = new Color(0.05f, 0.08f, 0.1f, 0.85f);
            GUI.Box(new Rect(Screen.width / 2 - 280, Screen.height - 52, 560, 36), game.Hint ?? "");
            GUI.backgroundColor = Color.white;
        }

        void DrawWave()
        {
            if (string.IsNullOrEmpty(_wave) || Time.unscaledTime > _waveUntil) return;
            GUI.backgroundColor = new Color(0.72f, 0.16f, 0.18f, 0.94f);
            GUI.Box(new Rect(Screen.width / 2 - 280, Screen.height - 96, 560, 36), _wave.ToUpperInvariant());
            GUI.backgroundColor = Color.white;
        }

        void DrawBanner()
        {
            if (string.IsNullOrEmpty(_banner) || Time.unscaledTime > _bannerUntil) return;
            GUI.backgroundColor = _bannerColor;
            GUI.Box(new Rect(Screen.width / 2 - 240, Screen.height - 140, 480, 36), _banner.ToUpperInvariant());
            GUI.backgroundColor = Color.white;
        }

        void DrawBoot()
        {
            GUI.backgroundColor = new Color(0.04f, 0.07f, 0.08f, 0.92f);
            GUI.Box(new Rect(Screen.width / 2 - 240, Screen.height / 2 - 130, 480, 240), "");
            GUI.backgroundColor = Color.white;
            GUI.Label(new Rect(Screen.width / 2 - 220, Screen.height / 2 - 112, 440, 24), "VERTICAL SLICE");
            GUI.Label(new Rect(Screen.width / 2 - 220, Screen.height / 2 - 86, 440, 28), "Hold Mesa 7");
            GUI.Label(new Rect(Screen.width / 2 - 220, Screen.height / 2 - 52, 440, 70),
                "Raise a Farm, string mag-rail to the Hub, and keep the crew fed through six raider waves. Win with Hub Level 2 still standing.");
            if (GUI.Button(new Rect(Screen.width / 2 - 210, Screen.height / 2 + 40, 200, 40), "Take the watch"))
            {
                ConsumeBootPlay = true;
                _boot = false;
            }
            if (GUI.Button(new Rect(Screen.width / 2 + 10, Screen.height / 2 + 40, 200, 40), "Watch a run"))
            {
                ConsumeBootDemo = true;
                _boot = false;
            }
        }

        void DrawEnd(GameSim game)
        {
            string title;
            string copy;
            switch (game.Phase)
            {
                case Phase.Won:
                    title = "MESA HOLDS";
                    copy = "Six waves down and Hub Level 2 online. The mag-rail still sings.";
                    break;
                case Phase.LostHub:
                    title = "HUB DOWN";
                    copy = "Raiders cracked the core. Splice faster, or feed the guns.";
                    break;
                case Phase.LostStarve:
                    title = "STARVED OUT";
                    copy = "The crew emptied the larder. Farm rail has to stay live.";
                    break;
                case Phase.Playing:
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(game.Phase), game.Phase, null);
            }
            GUI.backgroundColor = game.Phase == Phase.Won
                ? new Color(0.06f, 0.16f, 0.1f, 0.95f)
                : new Color(0.14f, 0.05f, 0.05f, 0.95f);
            GUI.Box(new Rect(Screen.width / 2 - 240, Screen.height / 2 - 110, 480, 220), "");
            GUI.backgroundColor = Color.white;
            GUI.Label(new Rect(Screen.width / 2 - 220, Screen.height / 2 - 92, 440, 28), title);
            GUI.Label(new Rect(Screen.width / 2 - 220, Screen.height / 2 - 56, 440, 50), copy);
            GUI.Label(new Rect(Screen.width / 2 - 220, Screen.height / 2 + 4, 440, 40),
                GameSim.CeilSecs(game.T) + "s  ·  Hub " + Mathf.CeilToInt(game.HubHp) + " HP  ·  L" + game.HubLevel +
                "  ·  wave " + game.WaveIndex + "/" + Balance.WavesToWin);
            if (GUI.Button(new Rect(Screen.width / 2 - 90, Screen.height / 2 + 52, 180, 36), "Run it back"))
                ConsumeRestart = true;
        }
    }
}
