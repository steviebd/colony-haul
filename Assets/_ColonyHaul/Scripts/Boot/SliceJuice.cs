using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColonyHaul
{
    public sealed class SliceJuice
    {
        struct Tracer
        {
            public Vector3 A;
            public Vector3 B;
            public Color Color;
            public float Until;
        }

        struct Pip
        {
            public Transform T;
            public TextMesh Mesh;
            public float Until;
            public Vector3 Vel;
            public float Life;
        }

        struct Burst
        {
            public Transform T;
            public float Until;
            public float Start;
            public float Size;
            public Color Color;
            public float Life;
        }

        readonly List<Tracer> _tracers = new List<Tracer>();
        readonly List<LineRenderer> _pool = new List<LineRenderer>();
        readonly List<Pip> _pips = new List<Pip>();
        readonly List<Burst> _bursts = new List<Burst>();
        readonly Transform _root;
        readonly AudioSource _audio;
        readonly AudioClip _deposit;
        readonly AudioClip _shot;
        readonly AudioClip _cut;
        readonly AudioClip _wave;
        readonly AudioClip _win;
        readonly AudioClip _lose;
        readonly AudioClip _surge;
        readonly AudioClip _dry;
        readonly AudioClip _splice;
        readonly AudioClip _back;
        readonly AudioClip _rail;
        readonly AudioClip _raise;
        readonly AudioClip _west;
        readonly AudioClip _slam;
        readonly AudioClip _gate;
        readonly AudioClip _crew;
        readonly AudioClip _alarm;
        readonly AudioSource _alarmSrc;
        bool _alarmOn;
        float _shake;
        float _close;
        float _closeTarget;
        float _homeSize;
        Vector3 _camHome;

        public SliceJuice(Transform root, Camera cam)
        {
            _root = root;
            _camHome = cam.transform.position;
            _homeSize = cam.orthographic ? cam.orthographicSize : 13.5f;
            _audio = cam.gameObject.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
            _audio.spatialBlend = 0f;
            _deposit = Beep(880f, 0.09f);
            _shot = Beep(420f, 0.05f);
            _cut = Beep(180f, 0.16f);
            _wave = Beep(240f, 0.22f);
            _win = Beep(660f, 0.62f);
            _lose = Beep(95f, 0.62f);
            _surge = Beep(990f, 0.22f);
            _dry = Beep(140f, 0.2f);
            _splice = Beep(620f, 0.28f);
            _back = Beep(760f, 0.28f);
            _rail = Beep(540f, 0.22f);
            _raise = Beep(470f, 0.26f);
            _west = Beep(390f, 0.3f);
            _slam = Beep(310f, 0.24f);
            _gate = Beep(200f, 0.26f);
            _crew = Beep(580f, 0.28f);
            _alarm = Drone(92f, 0.42f);
            _alarmSrc = cam.gameObject.AddComponent<AudioSource>();
            _alarmSrc.playOnAwake = false;
            _alarmSrc.loop = true;
            _alarmSrc.spatialBlend = 0f;
            _alarmSrc.clip = _alarm;
            _alarmSrc.volume = 0.16f;
        }

        public void Punch(float amount) => _shake = Mathf.Max(_shake, amount);

        public void JuiceLine(float fromX, float fromZ, float toX, float toZ, Color color)
        {
            AddTracer(fromX, fromZ, 0.9f, toX, toZ, 0.7f, color, 0.66f);
        }

        public void SetClose(float amount) => _closeTarget = Mathf.Clamp01(amount);

        public void ResetClose()
        {
            _close = 0f;
            _closeTarget = 0f;
            _shake = 0f;
        }

        public void Reset()
        {
            CutAlarm(false);
            ResetClose();
            for (var i = 0; i < _pips.Count; i++)
                if (_pips[i].T != null) UnityEngine.Object.Destroy(_pips[i].T.gameObject);
            _pips.Clear();
            for (var i = 0; i < _bursts.Count; i++)
                if (_bursts[i].T != null) UnityEngine.Object.Destroy(_bursts[i].T.gameObject);
            _bursts.Clear();
            _tracers.Clear();
            for (var i = 0; i < _pool.Count; i++)
                if (_pool[i] != null) _pool[i].enabled = false;
        }

        public void LastHeat()
        {
            SpawnPip(0f, 0f, "LAST", new Color(1f, 0.38f, 0.22f), 1.4f);
            Spokes(0f, 0f, 2.6f, new Color(0.86f, 0.24f, 0.24f));
            SpawnBurst(0f, 0f, new Color(0.86f, 0.24f, 0.24f, 0.55f), 8.2f, 0.62f);
            SpawnBurst(0f, 0f, new Color(1f, 0.48f, 0.22f, 0.32f), 4.8f, 0.4f);
            Punch(0.62f);
            _audio.PlayOneShot(_wave, 0.78f);
        }

        public void CutAlarm(bool on)
        {
            if (on == _alarmOn) return;
            _alarmOn = on;
            if (on) _alarmSrc.Play();
            else _alarmSrc.Stop();
        }

        public void Stuck(float x, float z)
        {
            SpawnPip(x, z, "STUCK", new Color(1f, 0.55f, 0.28f));
        }

        public void Rolling(float x, float z)
        {
            SpawnPip(x, z, "ROLLING", new Color(0.42f, 0.92f, 0.88f), 1.1f);
            SpawnBurst(x, z, new Color(0.42f, 0.92f, 0.88f, 0.5f), 3.4f, 0.48f);
            Spokes(x, z, 1.5f, new Color(0.42f, 0.92f, 0.88f));
            Punch(0.32f);
        }

        public void RailLive(float x, float z, string tag)
        {
            SpawnPip(x, z, string.IsNullOrEmpty(tag) ? "LIVE" : tag, new Color(0.42f, 0.92f, 0.88f), 1.25f);
            Spokes(x, z, 2.3f, new Color(0.42f, 0.92f, 0.88f));
            SpawnBurst(x, z, new Color(0.42f, 0.92f, 0.88f, 0.55f), 6.4f, 0.58f);
            SpawnBurst(x, z, new Color(0.75f, 0.98f, 0.95f, 0.32f), 3.8f, 0.4f);
            Punch(0.45f);
            _audio.PlayOneShot(_splice, 0.55f);
        }

        public void Inbound(float x, float z, int count)
        {
            SpawnPip(x, z, "IN " + count, new Color(0.86f, 0.24f, 0.24f));
            SpawnBurst(x, z, new Color(0.86f, 0.24f, 0.24f, 0.5f), 4.4f);
            Spokes(x, z, 1.8f, new Color(0.86f, 0.24f, 0.24f));
            Punch(0.45f);
        }

        public void PackIn(float x, float z, int count)
        {
            SpawnPip(x, z, count > 0 ? "PACK " + count : "PACK", new Color(0.86f, 0.28f, 0.24f));
            Spokes(x, z, 1.7f, new Color(0.86f, 0.28f, 0.24f));
            SpawnBurst(x, z, new Color(0.86f, 0.24f, 0.24f, 0.42f), 3.6f);
            Punch(0.28f);
        }

        public void GunsUp(float x, float z, string lane)
        {
            SpawnPip(x, z, string.IsNullOrEmpty(lane) ? "GUNS" : "UP " + lane, new Color(0.45f, 0.9f, 0.88f));
            Spokes(x, z, 1.6f, new Color(0.45f, 0.9f, 0.88f));
            SpawnBurst(x, z, new Color(0.45f, 0.9f, 0.88f, 0.42f), 3.4f);
            Punch(0.22f);
            _audio.PlayOneShot(_deposit, 0.4f);
        }

        public void GunsClick(float x, float z)
        {
            SpawnPip(x, z, "DRY", new Color(1f, 0.45f, 0.22f), 1.1f);
            SpawnBurst(x, z, new Color(1f, 0.4f, 0.18f, 0.5f), 3.2f, 0.42f);
            Spokes(x, z, 1.4f, new Color(1f, 0.5f, 0.2f));
        }

        public void GunsBack(float x, float z)
        {
            SpawnPip(x, z, "BACK", new Color(0.48f, 0.95f, 0.62f), 1.3f);
            Spokes(x, z, 2.3f, new Color(0.48f, 0.95f, 0.62f));
            SpawnBurst(x, z, new Color(0.48f, 0.95f, 0.62f, 0.55f), 6.4f, 0.58f);
            SpawnBurst(x, z, new Color(0.82f, 1f, 0.78f, 0.32f), 3.8f, 0.4f);
            Punch(0.45f);
            _audio.PlayOneShot(_back, 0.62f);
        }

        public void GunsFire(float x, float z)
        {
            SpawnPip(x, z, "FIRE", new Color(0.48f, 0.95f, 0.62f), 1.15f);
            SpawnBurst(x, z, new Color(0.48f, 0.95f, 0.62f, 0.5f), 3.4f, 0.48f);
            Spokes(x, z, 1.5f, new Color(0.48f, 0.95f, 0.62f));
        }

        public void Planted(float x, float z)
        {
            SpawnPip(x, z, "FARM", new Color(0.5f, 0.85f, 0.48f), 1.25f);
            Spokes(x, z, 1.8f, new Color(0.5f, 0.85f, 0.48f));
            SpawnBurst(x, z, new Color(0.5f, 0.85f, 0.48f, 0.52f), 4.4f, 0.5f);
            SpawnBurst(x, z, new Color(0.82f, 0.95f, 0.55f, 0.3f), 2.8f, 0.34f);
            Punch(0.36f);
            _audio.PlayOneShot(_deposit, 0.62f);
        }

        public void RailDrop(float fromX, float fromZ, float toX, float toZ)
        {
            var mx = (fromX + toX) * 0.5f;
            var mz = (fromZ + toZ) * 0.5f;
            SpawnPip(mx, mz, "RAIL", new Color(0.42f, 0.92f, 0.88f), 1.3f);
            Spokes(mx, mz, 2.2f, new Color(0.42f, 0.92f, 0.88f));
            SpawnBurst(mx, mz, new Color(0.42f, 0.92f, 0.88f, 0.55f), 5.6f, 0.55f);
            SpawnBurst(mx, mz, new Color(0.75f, 0.98f, 0.95f, 0.32f), 3.2f, 0.36f);
            JuiceLine(fromX, fromZ, toX, toZ, new Color(0.42f, 0.92f, 0.88f));
            Punch(0.42f);
            _audio.PlayOneShot(_rail, 0.7f);
        }

        public void FirstHaul(float x, float z)
        {
            SpawnPip(x, z, "HAUL", new Color(0.5f, 0.85f, 0.48f), 1.2f);
            SpawnBurst(x, z, new Color(0.5f, 0.85f, 0.48f, 0.5f), 3.4f, 0.45f);
            Spokes(x, z, 1.5f, new Color(0.5f, 0.85f, 0.48f));
            Punch(0.28f);
            _audio.PlayOneShot(_rail, 0.5f);
        }

        public void FirstDrop()
        {
            SpawnPip(0f, 0f, "HOME", new Color(0.5f, 0.85f, 0.48f), 1.25f);
            Spokes(0f, 0f, 2.0f, new Color(0.5f, 0.85f, 0.48f));
            SpawnBurst(0f, 0f, new Color(0.5f, 0.85f, 0.48f, 0.5f), 5.2f, 0.5f);
            SpawnBurst(0f, 0f, new Color(0.82f, 0.95f, 0.55f, 0.3f), 3.0f, 0.34f);
            Punch(0.32f);
            _audio.PlayOneShot(_deposit, 0.55f);
        }

        public void HubRaise()
        {
            SpawnPip(0f, 0f, "RAISE", new Color(1f, 0.86f, 0.42f), 1.2f);
            Spokes(0f, 0f, 2.0f, new Color(1f, 0.86f, 0.42f));
            SpawnBurst(0f, 0f, new Color(1f, 0.82f, 0.38f, 0.52f), 5.2f, 0.5f);
            SpawnBurst(0f, 0f, new Color(1f, 0.92f, 0.62f, 0.3f), 3.0f, 0.34f);
            Punch(0.4f);
            _audio.PlayOneShot(_raise, 0.65f);
        }

        public void HubLand()
        {
            SpawnPip(0f, 0f, "L2", new Color(1f, 0.86f, 0.42f), 1.3f);
            Spokes(0f, 0f, 2.4f, new Color(1f, 0.86f, 0.42f));
            SpawnBurst(0f, 0f, new Color(1f, 0.82f, 0.38f, 0.55f), 6.8f, 0.58f);
            SpawnBurst(0f, 0f, new Color(1f, 0.94f, 0.7f, 0.32f), 4.0f, 0.4f);
            Punch(0.52f);
            _audio.PlayOneShot(_raise, 0.82f);
        }

        public void SplashWest(float x, float z)
        {
            SpawnPip(x, z, "WEST", new Color(0.94f, 0.63f, 0.38f), 1.3f);
            Spokes(x, z, 2.4f, new Color(0.94f, 0.63f, 0.38f));
            SpawnBurst(x, z, new Color(0.94f, 0.63f, 0.38f, 0.55f), 6.4f, 0.58f);
            SpawnBurst(x, z, new Color(1f, 0.82f, 0.5f, 0.32f), 3.8f, 0.4f);
            JuiceLine(0f, 0f, x, z, new Color(0.94f, 0.63f, 0.38f));
            Punch(0.42f);
            _audio.PlayOneShot(_west, 0.7f);
        }

        public void SplashSlam(float x, float z)
        {
            SpawnPip(x, z, "SLAM", new Color(0.94f, 0.63f, 0.38f), 0.85f);
            Spokes(x, z, 2.2f, new Color(0.94f, 0.63f, 0.38f));
            SpawnBurst(x, z, new Color(0.94f, 0.63f, 0.38f, 0.55f), 5.4f, 0.5f);
            SpawnBurst(x, z, new Color(1f, 0.82f, 0.5f, 0.32f), 3.0f, 0.34f);
            Punch(0.48f);
            _audio.PlayOneShot(_slam, 0.7f);
        }

        public void GateDrop(float x, float z)
        {
            SpawnPip(x, z, "GATE", new Color(0.95f, 0.28f, 0.32f), 1.2f);
            Spokes(x, z, 2.2f, MesaView.Barrier);
            SpawnBurst(x, z, new Color(0.95f, 0.28f, 0.32f, 0.55f), 5.2f, 0.5f);
            SpawnBurst(x, z, new Color(1f, 0.55f, 0.42f, 0.32f), 3.0f, 0.34f);
            Punch(0.42f);
            _audio.PlayOneShot(_gate, 0.72f);
        }

        public void GateLine(float fromX, float fromZ, float toX, float toZ)
        {
            JuiceLine(fromX, fromZ, toX, toZ, MesaView.Barrier);
        }

        public void MesaHold()
        {
            SpawnPip(0f, 0f, "HOLD", new Color(0.55f, 0.9f, 0.5f), 1.45f);
            Spokes(0f, 0f, 2.8f, new Color(0.55f, 0.9f, 0.5f));
            SpawnBurst(0f, 0f, new Color(0.5f, 0.85f, 0.48f, 0.55f), 9.2f, 0.7f);
            SpawnBurst(0f, 0f, new Color(0.9f, 0.86f, 0.5f, 0.4f), 6.4f, 0.52f);
            SpawnBurst(0f, 0f, new Color(0.82f, 0.95f, 0.7f, 0.28f), 3.8f, 0.36f);
            Punch(0.72f);
            _audio.PlayOneShot(_win, 0.95f);
            _audio.PlayOneShot(_raise, 0.5f);
        }

        public void HubDown()
        {
            SpawnPip(0f, 0f, "DOWN", new Color(1f, 0.32f, 0.22f), 1.45f);
            Spokes(0f, 0f, 2.8f, new Color(1f, 0.28f, 0.16f));
            SpawnBurst(0f, 0f, new Color(0.85f, 0.16f, 0.14f, 0.58f), 8.6f, 0.68f);
            SpawnBurst(0f, 0f, new Color(1f, 0.42f, 0.22f, 0.35f), 5.2f, 0.45f);
            Punch(1.15f);
            _audio.PlayOneShot(_lose, 0.95f);
            _audio.PlayOneShot(_cut, 0.55f);
        }

        public void StarvedOut()
        {
            SpawnPip(0f, 0f, "STARVED", new Color(0.72f, 0.55f, 0.28f), 1.45f);
            Spokes(0f, 0f, 2.4f, new Color(0.72f, 0.52f, 0.22f));
            SpawnBurst(0f, 0f, new Color(0.55f, 0.38f, 0.14f, 0.52f), 7.4f, 0.62f);
            SpawnBurst(0f, 0f, new Color(0.82f, 0.62f, 0.28f, 0.3f), 4.2f, 0.4f);
            Punch(0.78f);
            _audio.PlayOneShot(_dry, 0.82f);
            _audio.PlayOneShot(_lose, 0.55f);
        }

        public void BraceComing(float x, float z)
        {
            SpawnPip(x, z, "INBOUND", new Color(0.45f, 0.9f, 1f));
            Spokes(x, z, 1.4f, new Color(0.45f, 0.9f, 1f));
            SpawnBurst(x, z, new Color(0.45f, 0.9f, 1f, 0.4f), 2.6f);
        }

        public void HaulHome(float x, float z, string tag)
        {
            SpawnPip(x, z, string.IsNullOrEmpty(tag) ? "HOME" : tag, new Color(0.86f, 0.72f, 0.38f));
            Spokes(x, z, 1.3f, new Color(0.86f, 0.72f, 0.38f));
            Punch(0.14f);
        }

        public void Chew(float x, float z)
        {
            SpawnPip(x, z, "CHEW", new Color(1f, 0.32f, 0.22f));
            Spokes(x, z, 1.6f, new Color(1f, 0.32f, 0.22f));
            SpawnBurst(x, z, new Color(1f, 0.28f, 0.18f, 0.5f), 3.4f);
            Punch(0.55f);
        }

        public void RailThreat(float x, float z, bool imminent)
        {
            SpawnPip(x, z, imminent ? "CUT?" : "THREAT", new Color(0.95f, 0.42f, 0.78f));
            Spokes(x, z, imminent ? 1.8f : 1.3f, new Color(0.95f, 0.42f, 0.78f));
            if (imminent)
            {
                SpawnBurst(x, z, new Color(0.95f, 0.42f, 0.78f, 0.5f), 2.8f);
                Punch(0.4f);
            }
        }

        public void PowerComing(float x, float z)
        {
            SpawnPip(x, z, "POWER", new Color(0.4f, 0.75f, 1f));
            Spokes(x, z, 1.4f, new Color(1f, 0.55f, 0.22f));
            SpawnBurst(x, z, new Color(1f, 0.48f, 0.18f, 0.4f), 2.6f);
        }

        public void CoreBound(float x, float z, bool imminent)
        {
            SpawnPip(x, z, imminent ? "PAD" : "BOUND", new Color(1f, 0.32f, 0.18f));
            Spokes(x, z, imminent ? 1.7f : 1.3f, new Color(1f, 0.32f, 0.18f));
            if (imminent)
            {
                SpawnBurst(x, z, new Color(1f, 0.28f, 0.16f, 0.5f), 2.8f);
                Punch(0.42f);
            }
        }

        public void WaveClear()
        {
            SpawnPip(0f, 0f, "CLEAR", new Color(0.55f, 0.9f, 0.5f));
            Spokes(0f, 0f, 2.0f, new Color(0.55f, 0.9f, 0.5f));
            SpawnBurst(0f, 0f, new Color(0.5f, 0.85f, 0.48f, 0.45f), 4.4f);
            Punch(0.32f);
            _audio.PlayOneShot(_deposit, 0.55f);
        }

        public void CutStakeBrace()
        {
            SpawnPip(0f, 0f, "BRACE", new Color(0.45f, 0.9f, 1f));
            Spokes(0f, 0f, 1.7f, new Color(1f, 0.45f, 0.22f));
            SpawnBurst(0f, 0f, new Color(1f, 0.4f, 0.2f, 0.4f), 3.2f);
            Punch(0.28f);
        }

        public void PadOffline(float x, float z)
        {
            SpawnPip(x, z, "OFF", new Color(0.92f, 0.62f, 0.28f));
            Spokes(x, z, 1.5f, new Color(0.92f, 0.62f, 0.28f));
            SpawnBurst(x, z, new Color(0.9f, 0.55f, 0.22f, 0.4f), 2.8f);
            Punch(0.22f);
        }

        public void SittingStock(float x, float z, string tag)
        {
            SpawnPip(x, z, tag, new Color(0.95f, 0.78f, 0.32f));
            Spokes(x, z, 1.4f, new Color(0.95f, 0.78f, 0.32f));
            Punch(0.18f);
        }

        public void L2Ready()
        {
            SpawnPip(0f, 0f, "L2", new Color(1f, 0.86f, 0.42f));
            Spokes(0f, 0f, 1.8f, new Color(1f, 0.86f, 0.42f));
            SpawnBurst(0f, 0f, new Color(1f, 0.82f, 0.38f, 0.45f), 3.6f);
            Punch(0.24f);
            _audio.PlayOneShot(_deposit, 0.5f);
        }

        public void OpenChoke(float x, float z, string lane)
        {
            SpawnPip(x, z, string.IsNullOrEmpty(lane) ? "OPEN" : lane, new Color(1f, 0.42f, 0.32f));
            Spokes(x, z, 1.6f, new Color(1f, 0.42f, 0.32f));
            SpawnBurst(x, z, new Color(1f, 0.38f, 0.22f, 0.45f), 3.2f);
            Punch(0.22f);
        }

        public void SlowChoke(float x, float z, string lane)
        {
            SpawnPip(x, z, string.IsNullOrEmpty(lane) ? "SLOW" : lane, new Color(0.95f, 0.32f, 0.34f));
            Spokes(x, z, 1.5f, new Color(0.95f, 0.32f, 0.34f));
            SpawnBurst(x, z, new Color(0.95f, 0.28f, 0.32f, 0.42f), 2.8f);
            Punch(0.2f);
        }

        public void CrewStretch(float x, float z)
        {
            SpawnPip(x, z, "IDLE", new Color(1f, 0.62f, 0.32f));
            Spokes(x, z, 1.3f, new Color(1f, 0.62f, 0.32f));
            Punch(0.16f);
        }

        public void CrewUp(float x, float z)
        {
            CrewArrive(x, z, x, z);
        }

        public void CrewArrive(float fromX, float fromZ, float toX, float toZ)
        {
            SpawnPip(fromX, fromZ, "OUT", new Color(0.58f, 0.9f, 0.48f), 1.25f);
            Spokes(fromX, fromZ, 2.0f, new Color(0.58f, 0.9f, 0.48f));
            SpawnBurst(fromX, fromZ, new Color(0.58f, 0.9f, 0.48f, 0.55f), 5.2f, 0.5f);
            SpawnBurst(fromX, fromZ, new Color(0.82f, 0.95f, 0.7f, 0.32f), 3.0f, 0.34f);
            JuiceLine(fromX, fromZ, toX, toZ, new Color(0.58f, 0.9f, 0.48f));
            Punch(0.42f);
            _audio.PlayOneShot(_crew, 0.72f);
        }

        public void HoldReady()
        {
            SpawnPip(0f, 0f, "HOLD", new Color(0.92f, 0.78f, 0.42f));
            Spokes(0f, 0f, 1.7f, new Color(0.92f, 0.78f, 0.42f));
            SpawnBurst(0f, 0f, new Color(0.9f, 0.74f, 0.38f, 0.42f), 3.4f);
            Punch(0.22f);
            _audio.PlayOneShot(_deposit, 0.45f);
        }

        public void PeelYank(float x, float z, float hopX, float hopZ, string reason)
        {
            var guns = reason == "power";
            var color = guns ? new Color(0.4f, 0.75f, 1f) : new Color(0.5f, 0.85f, 0.48f);
            SpawnPip(x, z, guns ? "GUNS" : "CREW", color, 1.15f);
            SpawnBurst(x, z, new Color(color.r, color.g, color.b, 0.5f), 3.2f, 0.42f);
            Spokes(x, z, 1.4f, color);
            JuiceLine(x, z, hopX, hopZ, color);
        }

        public void CoreThin()
        {
            SpawnPip(0f, 0f, "THIN", new Color(1f, 0.32f, 0.22f));
            Spokes(0f, 0f, 1.9f, new Color(1f, 0.28f, 0.18f));
            SpawnBurst(0f, 0f, new Color(1f, 0.22f, 0.16f, 0.5f), 4.0f);
            Punch(0.7f);
        }

        public void HubHit(bool braced)
        {
            if (braced)
            {
                SpawnPip(0f, 0f, "SHRUG", new Color(0.45f, 0.9f, 1f), 1.25f);
                Spokes(0f, 0f, 2.2f, new Color(0.45f, 0.9f, 1f));
                SpawnBurst(0f, 0f, new Color(0.45f, 0.9f, 1f, 0.55f), 5.8f, 0.55f);
                Punch(0.42f);
                _audio.PlayOneShot(_surge, 0.55f);
            }
            else
            {
                SpawnPip(0f, 0f, "HIT", new Color(1f, 0.35f, 0.28f));
                Punch(0.7f);
            }
        }

        public void Tick(Camera cam, List<SimEvent> events)
        {
            foreach (var ev in events) Handle(ev);
            _shake = Mathf.Max(0f, _shake - Time.deltaTime * 8f);
            _close = Mathf.MoveTowards(_close, _closeTarget, Time.deltaTime * 1.35f);
            if (cam != null)
            {
                var j = _shake * 0.18f + _close * 0.055f;
                var pulled = Vector3.Lerp(_camHome, new Vector3(15.1f, 17.4f, 15.1f), _close);
                cam.transform.position = pulled + new Vector3(
                    Mathf.Sin(Time.time * 70f) * j,
                    _close * 0.12f * Mathf.Sin(Time.time * 9f),
                    Mathf.Cos(Time.time * 63f) * j);
                cam.transform.LookAt(new Vector3(0f, 0.4f, 0f));
                if (cam.orthographic)
                    cam.orthographicSize = Mathf.Lerp(_homeSize, _homeSize * 0.84f, _close);
            }
            TickPips();
            TickBursts();
            for (var i = _tracers.Count - 1; i >= 0; i--)
                if (Time.time > _tracers[i].Until) _tracers.RemoveAt(i);
            while (_pool.Count < _tracers.Count)
            {
                var go = new GameObject("tracer");
                go.transform.SetParent(_root, false);
                var lr = go.AddComponent<LineRenderer>();
                lr.positionCount = 2;
                lr.startWidth = 0.14f;
                lr.endWidth = 0.04f;
                var shader = Shader.Find("Hidden/Internal-Colored")
                    ?? Shader.Find("Sprites/Default")
                    ?? Shader.Find("Unlit/Color")
                    ?? Shader.Find("Standard");
                if (shader == null) break;
                lr.material = new Material(shader);
                _pool.Add(lr);
            }
            for (var i = 0; i < _pool.Count; i++)
            {
                var lr = _pool[i];
                if (i >= _tracers.Count)
                {
                    lr.enabled = false;
                    continue;
                }
                var t = _tracers[i];
                lr.enabled = true;
                lr.startColor = t.Color;
                lr.endColor = t.Color;
                lr.SetPosition(0, t.A);
                lr.SetPosition(1, t.B);
            }
        }

        void Handle(SimEvent ev)
        {
            switch (ev.Kind)
            {
                case SimEventKind.Shot:
                    AddTracer(ev, new Color(0.55f, 0.95f, 0.9f), 0.14f);
                    _audio.PlayOneShot(_shot, 0.35f);
                    break;
                case SimEventKind.Splash:
                    AddTracer(ev, new Color(0.95f, 0.7f, 0.35f), 0.28f);
                    SplashSlam(ev.ToX, ev.ToZ);
                    break;
                case SimEventKind.Deposit:
                    _audio.PlayOneShot(_deposit, 0.4f);
                    SpawnPip(0f, 0f, "+" + Mathf.RoundToInt(ev.Amount) + " " + ResLabel(ev.Resource), ResColor(ev.Resource));
                    Spokes(0f, 0f, 1.1f, ResColor(ev.Resource));
                    Punch(0.16f);
                    break;
                case SimEventKind.Sabotage:
                    _audio.PlayOneShot(_cut, 0.7f);
                    Punch(1.05f);
                    SpawnPip(ev.X, ev.Z, "CUT", new Color(1f, 0.38f, 0.22f));
                    Spokes(ev.X, ev.Z, 2.2f, new Color(1f, 0.55f, 0.22f));
                    SpawnBurst(ev.X, ev.Z, new Color(1f, 0.38f, 0.22f, 0.65f), 3.1f);
                    SpawnBurst(ev.X, ev.Z, new Color(1f, 0.72f, 0.28f, 0.4f), 4.4f);
                    break;
                case SimEventKind.Wave:
                    _audio.PlayOneShot(_wave, 0.5f);
                    Punch(0.55f);
                    SpawnBurst(0f, 0f, new Color(0.86f, 0.24f, 0.24f, 0.4f), 6f);
                    break;
                case SimEventKind.Brownout:
                    Punch(0.7f);
                    _audio.PlayOneShot(_dry, 0.72f);
                    SpawnPip(ev.X, ev.Z, "DRY", new Color(1f, 0.45f, 0.22f), 1.25f);
                    Spokes(ev.X, ev.Z, 2.2f, new Color(1f, 0.5f, 0.2f));
                    SpawnBurst(ev.X, ev.Z, new Color(1f, 0.4f, 0.18f, 0.55f), 5.6f, 0.52f);
                    SpawnBurst(ev.X, ev.Z, new Color(1f, 0.72f, 0.28f, 0.32f), 3.4f, 0.36f);
                    break;
                case SimEventKind.Barrier:
                    GateDrop(ev.X, ev.Z);
                    break;
                case SimEventKind.Hit:
                    break;
                case SimEventKind.Death:
                    SpawnPip(ev.X, ev.Z, "DOWN", new Color(0.92f, 0.28f, 0.22f));
                    SpawnPip(ev.X + 0.45f, ev.Z, "+" + Mathf.RoundToInt(ev.Amount) + " scrap", new Color(0.94f, 0.64f, 0.23f));
                    AddTracer(ev.X, ev.Z + 0.4f, 1.6f, ev.X, ev.Z, 0.55f, new Color(1f, 0.82f, 0.35f), 0.16f);
                    Punch(0.55f);
                    _audio.PlayOneShot(_shot, 0.28f);
                    break;
                case SimEventKind.WarnFood:
                    break;
                case SimEventKind.Build:
                    _audio.PlayOneShot(_deposit, 0.2f);
                    break;
                case SimEventKind.Upgrade:
                    break;
                case SimEventKind.Win:
                    MesaHold();
                    break;
                case SimEventKind.Lose:
                    if (ev.Reason == "starve") StarvedOut();
                    else HubDown();
                    break;
                case SimEventKind.Route:
                    if (ev.Reason == "splice")
                    {
                        _audio.PlayOneShot(_splice, 0.92f);
                        Punch(0.75f);
                        SpawnPip(ev.X, ev.Z, "SPLICED", new Color(0.42f, 0.92f, 0.88f), 1.3f);
                        SpawnBurst(ev.X, ev.Z, new Color(0.42f, 0.92f, 0.88f, 0.58f), 5.6f, 0.55f);
                        SpawnBurst(ev.X, ev.Z, new Color(1f, 0.82f, 0.38f, 0.28f), 3.2f, 0.32f);
                        Spokes(ev.X, ev.Z, 2.6f, new Color(0.42f, 0.92f, 0.88f));
                    }
                    else _audio.PlayOneShot(_deposit, 0.25f);
                    break;
                case SimEventKind.Surge:
                    _audio.PlayOneShot(_surge, 0.72f);
                    Punch(0.48f);
                    SpawnPip(0f, 0f, "BRACE", new Color(0.45f, 0.9f, 1f), 1.35f);
                    Spokes(0f, 0f, 2.4f, new Color(0.45f, 0.9f, 1f));
                    SpawnBurst(0f, 0f, new Color(0.45f, 0.9f, 1f, 0.55f), 7.2f, 0.62f);
                    SpawnBurst(0f, 0f, new Color(0.85f, 0.95f, 1f, 0.35f), 4.4f, 0.4f);
                    break;
                case SimEventKind.Hold:
                    if (ev.Reason == "power")
                    {
                        _audio.PlayOneShot(_surge, 0.62f);
                        SpawnPip(0f, 0f, "PEEL", new Color(0.4f, 0.75f, 1f), 1.3f);
                        Spokes(0f, 0f, 2.2f, new Color(0.4f, 0.75f, 1f));
                        SpawnBurst(0f, 0f, new Color(0.4f, 0.75f, 1f, 0.55f), 5.6f, 0.52f);
                        SpawnBurst(0f, 0f, new Color(0.75f, 0.9f, 1f, 0.32f), 3.4f, 0.36f);
                        Punch(0.48f);
                    }
                    else if (ev.Reason == "food")
                    {
                        _audio.PlayOneShot(_deposit, 0.62f);
                        SpawnPip(0f, 0f, "PEEL", new Color(0.5f, 0.85f, 0.48f), 1.3f);
                        Spokes(0f, 0f, 2.2f, new Color(0.5f, 0.85f, 0.48f));
                        SpawnBurst(0f, 0f, new Color(0.5f, 0.85f, 0.48f, 0.55f), 5.6f, 0.52f);
                        SpawnBurst(0f, 0f, new Color(0.78f, 0.95f, 0.7f, 0.32f), 3.4f, 0.36f);
                        Punch(0.48f);
                    }
                    else
                    {
                        _audio.PlayOneShot(_dry, 0.35f);
                        SpawnPip(0f, 0f, "AUTO", new Color(0.85f, 0.82f, 0.7f));
                        Punch(0.22f);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ev.Kind), ev.Kind, null);
            }
        }

        void TickPips()
        {
            for (var i = _pips.Count - 1; i >= 0; i--)
            {
                var p = _pips[i];
                if (p.T == null || Time.time > p.Until)
                {
                    if (p.T != null) UnityEngine.Object.Destroy(p.T.gameObject);
                    _pips.RemoveAt(i);
                    continue;
                }
                p.T.position += p.Vel * Time.deltaTime;
                if (p.Mesh != null)
                {
                    var life = p.Life > 0.01f ? p.Life : 0.85f;
                    var t = Mathf.Clamp01((p.Until - Time.time) / life);
                    var c = p.Mesh.color;
                    c.a = t;
                    p.Mesh.color = c;
                }
                _pips[i] = p;
            }
        }

        void SpawnPip(float x, float z, string text, Color color)
        {
            SpawnPip(x, z, text, color, 0.85f);
        }

        void SpawnPip(float x, float z, string text, Color color, float life)
        {
            var go = new GameObject("pip");
            go.transform.SetParent(_root, false);
            go.transform.position = new Vector3(x, 1.55f, z);
            go.transform.rotation = Quaternion.Euler(90f, 45f, 0f);
            var tm = go.AddComponent<TextMesh>();
            tm.text = text;
            tm.color = color;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.fontSize = 48;
            tm.characterSize = 0.07f;
            _pips.Add(new Pip
            {
                T = go.transform,
                Mesh = tm,
                Until = Time.time + life,
                Vel = new Vector3(0f, 1.7f, 0f),
                Life = life
            });
        }

        static string ResLabel(Resource? res)
        {
            if (res == Resource.Food) return "FOOD";
            if (res == Resource.Power) return "PWR";
            return "ORE";
        }

        static Color ResColor(Resource? res)
        {
            if (res == Resource.Food) return new Color(0.5f, 0.85f, 0.48f);
            if (res == Resource.Power) return new Color(0.4f, 0.75f, 1f);
            return new Color(0.94f, 0.64f, 0.23f);
        }

        void AddTracer(SimEvent ev, Color color)
        {
            AddTracer(ev, color, 0.14f);
        }

        void AddTracer(SimEvent ev, Color color, float life)
        {
            AddTracer(ev.FromX, ev.FromZ, 1.25f, ev.ToX, ev.ToZ, 0.7f, color, life);
        }

        void Spokes(float x, float z, float reach, Color color)
        {
            AddTracer(x - reach, z, 1.1f, x + reach, z, 1.1f, color, 0.12f);
            AddTracer(x, z - reach, 1.1f, x, z + reach, 1.1f, color, 0.12f);
        }

        void SpawnBurst(float x, float z, Color color, float size)
        {
            SpawnBurst(x, z, color, size, 0.38f);
        }

        void SpawnBurst(float x, float z, Color color, float size, float life)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "burst";
            go.transform.SetParent(_root, false);
            var col = go.GetComponent<Collider>();
            if (col != null) UnityEngine.Object.Destroy(col);
            go.transform.position = new Vector3(x, 0.47f, z);
            go.transform.localScale = new Vector3(0.4f, 0.012f, 0.4f);
            MesaView.Tint(go, color);
            _bursts.Add(new Burst
            {
                T = go.transform,
                Until = Time.time + life,
                Start = Time.time,
                Size = size,
                Color = color,
                Life = life
            });
        }

        void TickBursts()
        {
            for (var i = _bursts.Count - 1; i >= 0; i--)
            {
                var b = _bursts[i];
                if (b.T == null || Time.time > b.Until)
                {
                    if (b.T != null) UnityEngine.Object.Destroy(b.T.gameObject);
                    _bursts.RemoveAt(i);
                    continue;
                }
                var span = b.Life > 0.01f ? b.Life : 0.38f;
                var t = Mathf.Clamp01((Time.time - b.Start) / span);
                var s = Mathf.Lerp(0.4f, b.Size, t);
                b.T.localScale = new Vector3(s, 0.012f, s);
                var c = b.Color;
                c.a *= 1f - t;
                MesaView.Tint(b.T.gameObject, c);
            }
        }

        void AddTracer(float ax, float az, float ay, float bx, float bz, float by, Color color, float life)
        {
            _tracers.Add(new Tracer
            {
                A = new Vector3(ax, ay, az),
                B = new Vector3(bx, by, bz),
                Color = color,
                Until = Time.time + life
            });
        }

        static AudioClip Beep(float freq, float dur)
        {
            const int sr = 22050;
            var n = Mathf.Max(8, (int)(sr * dur));
            var clip = AudioClip.Create("beep", n, 1, sr, false);
            var data = new float[n];
            for (var i = 0; i < n; i++)
            {
                var env = 1f - i / (float)n;
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * i / sr) * env * 0.35f;
            }
            clip.SetData(data, 0);
            return clip;
        }

        static AudioClip Drone(float freq, float dur)
        {
            const int sr = 22050;
            var n = Mathf.Max(8, (int)(sr * dur));
            var clip = AudioClip.Create("drone", n, 1, sr, false);
            var data = new float[n];
            for (var i = 0; i < n; i++)
            {
                var fade = Mathf.Sin(Mathf.PI * i / n);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * i / sr) * fade * 0.22f;
            }
            clip.SetData(data, 0);
            return clip;
        }
    }
}
