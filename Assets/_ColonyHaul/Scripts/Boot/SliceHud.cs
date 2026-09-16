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
        bool _boot = true;
        public bool ShowBoot = true;

        public bool ConsumeBootPlay { get; private set; }
        public bool ConsumeBootDemo { get; private set; }
        public bool ConsumeRestart { get; private set; }
        public Tool? ClickedTool { get; private set; }

        public void Flash(string text, float seconds)
        {
            _banner = text;
            _bannerUntil = Time.unscaledTime + seconds;
        }

        public void Draw(GameSim game)
        {
            ConsumeBootPlay = ConsumeBootDemo = ConsumeRestart = false;
            ClickedTool = null;
            var e = Event.current;

            DrawTop(game);
            DrawTray(game);
            DrawHint(game);
            DrawBanner();
            if (game.StarveTimer > 0.2f && game.Phase == Phase.Playing)
            {
                GUI.color = new Color(1f, 0.45f, 0.35f);
                GUI.Box(new Rect(Screen.width / 2 - 180, 92, 360, 28), "FOOD STORES EMPTY · colony starving");
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
            GUI.Label(new Rect(24, 36, 280, 18), "Mesa 7 · dusk cycle · Unity slice");
            Chip(320, 18, "ORE", Mathf.FloorToInt(game.Ore).ToString(), new Color(0.94f, 0.64f, 0.23f));
            Chip(430, 18, "FOOD", Mathf.FloorToInt(game.Food).ToString(), new Color(0.5f, 0.85f, 0.48f));
            var pwrColor = game.PowerBrownout ? new Color(1f, 0.35f, 0.32f) : new Color(0.4f, 0.75f, 1f);
            Chip(540, 18, "PWR", Mathf.FloorToInt(game.Power).ToString(), pwrColor);
            Chip(650, 18, "STAFF", game.StaffedProducers() + "/" + game.WorkersTotal, new Color(0.75f, 0.8f, 0.85f));
            GUI.Label(new Rect(Screen.width - 280, 18, 260, 20),
                game.WaveIndex == 0
                    ? $"WAVE 1 / {Balance.WavesToWin} IN {Mathf.CeilToInt(game.NextWaveIn)}S"
                    : $"WAVE {game.WaveIndex} / {Balance.WavesToWin} · {game.Enemies.Count} LIVE");
            GUI.Label(new Rect(Screen.width - 280, 40, 260, 20),
                $"Hub HP {Mathf.CeilToInt(game.HubHp)} · L{game.HubLevel} · {game.Haulers.Count} haul");
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
            GUI.Box(new Rect(12, 92, 220, 8 * 44 + 16), "");
            GUI.backgroundColor = Color.white;
            var y = 100f;
            var gate = game.OpeningGate();
            foreach (var tool in Tools)
            {
                var locked = tool.Tool == Tool.Splash && game.HubLevel < 2;
                var selected = game.SelectedTool == tool.Tool;
                GUI.backgroundColor = selected ? new Color(0.25f, 0.85f, 0.8f) : new Color(0.12f, 0.18f, 0.2f);
                if (locked) GUI.backgroundColor = new Color(0.12f, 0.12f, 0.12f);
                var label = (selected ? "▶ " : "") + tool.Label;
                if (locked) label = "Splash  · locked Hub L2";
                if (GUI.Button(new Rect(20, y, 204, 40), label + "\n" + tool.Hint))
                {
                    if (tool.Tool == Tool.Upgrade) ClickedTool = Tool.Upgrade;
                    else ClickedTool = tool.Tool;
                }
                GUI.backgroundColor = Color.white;
                y += 44;
            }
            GUI.Label(new Rect(20, y + 8, 200, 40), "1–7 tools · U Hub L2\nD demo · R restart");
            _ = gate;
        }

        void DrawHint(GameSim game)
        {
            GUI.backgroundColor = new Color(0.05f, 0.08f, 0.1f, 0.85f);
            GUI.Box(new Rect(Screen.width / 2 - 280, Screen.height - 52, 560, 36), game.Hint ?? "");
            GUI.backgroundColor = Color.white;
        }

        void DrawBanner()
        {
            if (string.IsNullOrEmpty(_banner) || Time.unscaledTime > _bannerUntil) return;
            GUI.backgroundColor = new Color(0.82f, 0.28f, 0.32f, 0.95f);
            GUI.Box(new Rect(Screen.width / 2 - 220, 88, 440, 36), _banner.ToUpperInvariant());
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
            GUI.backgroundColor = new Color(0.04f, 0.06f, 0.07f, 0.94f);
            GUI.Box(new Rect(Screen.width / 2 - 220, Screen.height / 2 - 90, 440, 180), "");
            GUI.backgroundColor = Color.white;
            GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 70, 400, 28), title);
            GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 36, 400, 50), copy);
            if (GUI.Button(new Rect(Screen.width / 2 - 90, Screen.height / 2 + 30, 180, 36), "Run it back"))
                ConsumeRestart = true;
        }
    }
}
