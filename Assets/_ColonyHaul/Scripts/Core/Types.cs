using System;
using System.Collections.Generic;

namespace ColonyHaul
{
    public enum Resource { Ore, Food, Power }
    public enum Phase { Playing, Won, LostHub, LostStarve }
    public enum Tool { None, Mine, Farm, Power, Route, Kinetic, Splash, Barrier, Upgrade }
    public enum HoldOrder { Auto, Power, Food }
    public enum CutStake { Brace, Power, Farm, Ore, Generic }
    public enum NodeKind { Hub, Depot, Pad, Choke, Tower, Spawn }
    public enum BuildingType { Hub, Depot, Mine, Farm, Power, Kinetic, Splash }
    public enum EnemyType { Grunt, Brute, Runner }

    public sealed class WatchCall
    {
        public string Copy;
        public Tool Pulse;
    }

    public struct LaneThreat
    {
        public int East;
        public int North;
        public int West;
    }

    public sealed class SimNode
    {
        public string Id;
        public float X;
        public float Z;
        public NodeKind Kind;
    }

    public sealed class SimEdge
    {
        public string Id;
        public string A;
        public string B;
        public bool Routed;
        public float SabotagedUntil;
        public bool Barrier;
    }

    public sealed class Building
    {
        public string Id;
        public BuildingType Type;
        public string NodeId;
        public float BuildLeft;
        public bool Staffed;
        public readonly Dictionary<Resource, float> Buffer = new Dictionary<Resource, float>
        {
            { Resource.Ore, 0 }, { Resource.Food, 0 }, { Resource.Power, 0 }
        };
        public float Cooldown;
    }

    public sealed class Hauler
    {
        public string Id;
        public float X;
        public float Z;
        public string NodeId;
        public Resource? CargoKind;
        public int CargoAmount;
        public readonly List<string> Path = new List<string>();
        public float Wait;
        public string BusyAt;
    }

    public sealed class Enemy
    {
        public string Id;
        public EnemyType Type;
        public float X;
        public float Z;
        public float Hp;
        public float MaxHp;
        public readonly List<string> Path = new List<string>();
        public string NodeId;
        public float SlowUntil;
        public float AttackCd;
        public float Flash;
    }

    public enum SimEventKind
    {
        Deposit,
        Shot,
        Splash,
        Hit,
        Death,
        Wave,
        Sabotage,
        WarnFood,
        Brownout,
        Build,
        Upgrade,
        Win,
        Lose,
        Route,
        Barrier,
        Surge,
        Hold
    }

    public sealed class SimEvent
    {
        public SimEventKind Kind;
        public float T;
        public float X;
        public float Z;
        public float FromX;
        public float FromZ;
        public float ToX;
        public float ToZ;
        public string NodeId;
        public string EdgeId;
        public string EnemyId;
        public Resource? Resource;
        public float Amount;
        public int Wave;
        public string Reason;
    }

    public sealed class HeadlessResult
    {
        public Phase Phase;
        public float T;
        public int WaveIndex;
        public int HubLevel;
        public int Deposits;
        public int Buildings;
        public int Routes;
        public int Kills;
        public bool Win;
        public int Brownouts;
        public float MinPower;
        public int Sabotages;
    }
}
