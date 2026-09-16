using System;

namespace ColonyHaul
{
    public sealed class DemoPilot
    {
        float _cooldown = 0.15f;
        float _cutHold;
        string _seenCutId;
        static readonly string[] Pads = { "pad_s", "pad_n", "pad_se", "pad_sw", "pad_nw", "pad_ne" };

        public void Step(GameSim game, float dt)
        {
            if (game.Phase != Phase.Playing) return;
            _cooldown -= dt;
            if (_cutHold > 0) _cutHold = Math.Max(0, _cutHold - dt);
            if (_cooldown > 0) return;
            if (Act(game) && _cooldown <= 0) _cooldown = 0.42f;
        }

        bool Act(GameSim game)
        {
            if (RepairCut(game)) return true;
            if (EnsureProducer(game, BuildingType.Farm, "pad_s")) return true;
            if (RouteHome(game, "pad_s")) return true;
            if (EnsureProducer(game, BuildingType.Mine, "pad_n")) return true;
            if (RouteHome(game, "pad_n")) return true;
            if (EnsureProducer(game, BuildingType.Power, "pad_se")) return true;
            if (RouteHome(game, "pad_se")) return true;
            if (EnsureTower(game, BuildingType.Kinetic, "choke_e")) return true;
            if (EnsureBarrier(game, "choke_e")) return true;
            if (EnsureTower(game, BuildingType.Kinetic, "choke_n")) return true;
            if (EnsureBarrier(game, "choke_n")) return true;
            if (Has(game, BuildingType.Kinetic))
            {
                if (EnsureProducer(game, BuildingType.Farm, "pad_sw")) return true;
                if (RouteHome(game, "pad_sw")) return true;
            }
            if (game.HubLevel < 2 && game.T > 90f && game.Ore >= Balance.HubL2Ore && game.Food >= Balance.HubL2Food && game.Power >= Balance.HubL2Power)
            {
                Arm(game, Tool.Upgrade);
                return game.TryUpgrade(out _);
            }
            if (game.HubLevel >= 2)
            {
                if (EnsureTower(game, BuildingType.Splash, "choke_w")) return true;
                if (EnsureBarrier(game, "choke_w")) return true;
                if (EnsureProducer(game, BuildingType.Power, "pad_ne")) return true;
                if (RouteHome(game, "pad_ne")) return true;
                if (EnsureTower(game, BuildingType.Splash, "tower_ne")) return true;
            }
            if (EnsureProducer(game, BuildingType.Mine, "pad_nw")) return true;
            if (RouteHome(game, "pad_nw")) return true;
            return EnsureTower(game, BuildingType.Kinetic, "tower_sw");
        }

        static bool Has(GameSim game, BuildingType type)
        {
            foreach (var b in game.Buildings.Values) if (b.Type == type) return true;
            return false;
        }

        static int Count(GameSim game, BuildingType type)
        {
            var n = 0;
            foreach (var b in game.Buildings.Values) if (b.Type == type) n++;
            return n;
        }

        static void Arm(GameSim game, Tool tool)
        {
            if (game.SelectedTool != tool) game.SetTool(tool);
        }

        static bool EnsureProducer(GameSim game, BuildingType type, string prefer)
        {
            foreach (var b in game.Buildings.Values)
                if (b.Type == type && b.NodeId == prefer) return false;
            var cap = 2;
            if (Count(game, type) >= cap) return false;
            var node = game.Buildings.ContainsKey(prefer) ? FindFree(game) : prefer;
            if (node == null) return false;
            Arm(game, type == BuildingType.Farm ? Tool.Farm : type == BuildingType.Mine ? Tool.Mine : Tool.Power);
            return game.ClickNode(node, out _);
        }

        static string FindFree(GameSim game)
        {
            foreach (var p in Pads) if (!game.Buildings.ContainsKey(p)) return p;
            return null;
        }

        static bool RouteHome(GameSim game, string from)
        {
            if (!game.Buildings.ContainsKey(from)) return false;
            if (game.Pathfind(from, "hub", true, false, false) != null) return false;
            var path = game.Pathfind(from, "hub", false, true, false);
            if (path == null || path.Count < 2) return false;
            for (var i = 0; i < path.Count - 1; i++)
            {
                var edge = game.EdgeBetween(path[i], path[i + 1]);
                if (edge != null && !edge.Routed)
                {
                    game.SetTool(Tool.Route);
                    if (game.RouteFrom != path[i]) game.ClickNode(path[i], out _);
                    return game.ClickNode(path[i + 1], out _);
                }
            }
            return false;
        }

        bool RepairCut(GameSim game)
        {
            SimEdge cut = null;
            foreach (var edge in game.Edges.Values)
            {
                if (!edge.Routed || edge.SabotagedUntil <= game.T) continue;
                cut = edge;
                break;
            }
            if (cut == null)
            {
                _seenCutId = null;
                _cutHold = 0;
                return false;
            }
            game.SetTool(Tool.Route);
            if (game.RouteFrom != cut.A) game.ClickNode(cut.A, out _);
            if (_seenCutId != cut.Id)
            {
                _seenCutId = cut.Id;
                _cutHold = PowerCut(game, cut) ? 5.6f : 2.2f;
                _cooldown = 0.08f;
                return true;
            }
            if (_cutHold > 0)
            {
                if (PowerCut(game, cut) && game.HubLevel < 2 && game.Ore >= Balance.HubL2Ore && game.Food >= Balance.HubL2Food && game.Power >= Balance.HubL2Power)
                {
                    Arm(game, Tool.Upgrade);
                    game.TryUpgrade(out _);
                    Arm(game, Tool.Route);
                    if (game.RouteFrom != cut.A) game.ClickNode(cut.A, out _);
                }
                _cooldown = 0.08f;
                return true;
            }
            var spliced = game.ClickNode(cut.B, out _);
            if (spliced)
            {
                _seenCutId = null;
                _cooldown = 2.8f;
            }
            return spliced;
        }

        static bool PowerCut(GameSim game, SimEdge cut)
        {
            if (game.Buildings.TryGetValue(cut.A, out var ba) && ba.Type == BuildingType.Power) return true;
            if (game.Buildings.TryGetValue(cut.B, out var bb) && bb.Type == BuildingType.Power) return true;
            return false;
        }

        static bool EnsureTower(GameSim game, BuildingType type, string nodeId)
        {
            if (game.Buildings.ContainsKey(nodeId)) return false;
            if (type == BuildingType.Splash && game.HubLevel < 2) return false;
            Arm(game, type == BuildingType.Splash ? Tool.Splash : Tool.Kinetic);
            return game.ClickNode(nodeId, out _);
        }

        static bool EnsureBarrier(GameSim game, string choke)
        {
            var all = true;
            foreach (var e in game.Neighbors(choke))
            {
                var other = e.A == choke ? e.B : e.A;
                if (game.Nodes[other].Kind == NodeKind.Spawn && !e.Barrier) all = false;
            }
            if (all) return false;
            Arm(game, Tool.Barrier);
            return game.ClickNode(choke, out _);
        }
    }
}
