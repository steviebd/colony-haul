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
        }

        struct Burst
        {
            public Transform T;
            public float Until;
            public float Start;
            public float Size;
            public Color Color;
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
        readonly AudioClip _alarm;
        readonly AudioSource _alarmSrc;
        bool _alarmOn;
        float _shake;
        Vector3 _camHome;

        public SliceJuice(Transform root, Camera cam)
        {
            _root = root;
            _camHome = cam.transform.position;
            _audio = cam.gameObject.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
            _audio.spatialBlend = 0f;
            _deposit = Beep(880f, 0.09f);
            _shot = Beep(420f, 0.05f);
            _cut = Beep(180f, 0.16f);
            _wave = Beep(240f, 0.22f);
            _win = Beep(660f, 0.45f);
            _lose = Beep(95f, 0.55f);
            _surge = Beep(990f, 0.12f);
            _dry = Beep(140f, 0.2f);
            _splice = Beep(620f, 0.16f);
            _alarm = Drone(92f, 0.42f);
            _alarmSrc = cam.gameObject.AddComponent<AudioSource>();
            _alarmSrc.playOnAwake = false;
            _alarmSrc.loop = true;
            _alarmSrc.spatialBlend = 0f;
            _alarmSrc.clip = _alarm;
            _alarmSrc.volume = 0.16f;
        }

        public void Punch(float amount) => _shake = Mathf.Max(_shake, amount);

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
            SpawnPip(x, z, "ROLLING", new Color(0.42f, 0.92f, 0.88f));
            SpawnBurst(x, z, new Color(0.42f, 0.92f, 0.88f, 0.45f), 1.8f);
        }

        public void Inbound(float x, float z, int count)
        {
            SpawnPip(x, z, "IN " + count, new Color(0.86f, 0.24f, 0.24f));
            SpawnBurst(x, z, new Color(0.86f, 0.24f, 0.24f, 0.5f), 4.4f);
            Spokes(x, z, 1.8f, new Color(0.86f, 0.24f, 0.24f));
            Punch(0.45f);
        }

        public void BraceComing(float x, float z)
        {
            SpawnPip(x, z, "INBOUND", new Color(0.45f, 0.9f, 1f));
            Spokes(x, z, 1.4f, new Color(0.45f, 0.9f, 1f));
            SpawnBurst(x, z, new Color(0.45f, 0.9f, 1f, 0.4f), 2.6f);
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

        public void HubHit(bool braced)
        {
            if (braced)
            {
                SpawnPip(0f, 0f, "SHRUG", new Color(0.45f, 0.9f, 1f));
                Punch(0.18f);
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
            if (cam != null)
            {
                var j = _shake * 0.18f;
                cam.transform.position = _camHome + new Vector3(
                    Mathf.Sin(Time.time * 70f) * j,
                    0f,
                    Mathf.Cos(Time.time * 63f) * j);
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
                    AddTracer(ev, new Color(0.95f, 0.7f, 0.35f), 0.18f);
                    Spokes(ev.ToX, ev.ToZ, 1.6f, new Color(0.95f, 0.7f, 0.35f));
                    SpawnBurst(ev.ToX, ev.ToZ, new Color(0.94f, 0.63f, 0.38f, 0.55f), 3.2f);
                    _audio.PlayOneShot(_shot, 0.45f);
                    Punch(0.4f);
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
                    Punch(0.45f);
                    _audio.PlayOneShot(_dry, 0.55f);
                    SpawnPip(ev.X, ev.Z, "DRY", new Color(1f, 0.45f, 0.22f));
                    Spokes(ev.X, ev.Z, 1.6f, new Color(1f, 0.5f, 0.2f));
                    SpawnBurst(ev.X, ev.Z, new Color(1f, 0.4f, 0.18f, 0.5f), 3.0f);
                    break;
                case SimEventKind.Barrier:
                    Punch(0.28f);
                    _audio.PlayOneShot(_cut, 0.28f);
                    SpawnPip(ev.X, ev.Z, "SLOW", MesaView.Barrier);
                    SpawnBurst(ev.X, ev.Z, new Color(0.95f, 0.28f, 0.32f, 0.5f), 2.8f);
                    break;
                case SimEventKind.Hit:
                    break;
                case SimEventKind.Death:
                    SpawnPip(ev.X, ev.Z, "+" + Mathf.RoundToInt(ev.Amount) + " scrap", new Color(0.94f, 0.64f, 0.23f));
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
                    _audio.PlayOneShot(_wave, 0.4f);
                    Punch(0.35f);
                    SpawnBurst(0f, 0f, new Color(0.9f, 0.78f, 0.5f, 0.55f), 4.2f);
                    break;
                case SimEventKind.Win:
                    _audio.PlayOneShot(_win, 0.9f);
                    Punch(0.85f);
                    SpawnBurst(0f, 0f, new Color(0.5f, 0.85f, 0.48f, 0.55f), 7f);
                    SpawnBurst(0f, 0f, new Color(0.9f, 0.86f, 0.5f, 0.4f), 10f);
                    SpawnPip(0f, 0f, "HOLD", new Color(0.55f, 0.9f, 0.5f));
                    break;
                case SimEventKind.Lose:
                    _audio.PlayOneShot(_lose, 0.9f);
                    Punch(1.35f);
                    SpawnBurst(0f, 0f, new Color(0.85f, 0.16f, 0.14f, 0.55f), 8f);
                    SpawnPip(0f, 0f, ev.Reason == "starve" ? "STARVED" : "DOWN", new Color(1f, 0.35f, 0.3f));
                    break;
                case SimEventKind.Route:
                    if (ev.Reason == "splice")
                    {
                        _audio.PlayOneShot(_splice, 0.8f);
                        Punch(0.55f);
                        SpawnPip(ev.X, ev.Z, "SPLICED", new Color(0.42f, 0.92f, 0.88f));
                        SpawnBurst(ev.X, ev.Z, new Color(0.42f, 0.92f, 0.88f, 0.55f), 3.6f);
                        Spokes(ev.X, ev.Z, 2.1f, new Color(0.42f, 0.92f, 0.88f));
                    }
                    else _audio.PlayOneShot(_deposit, 0.25f);
                    break;
                case SimEventKind.Surge:
                    _audio.PlayOneShot(_surge, 0.45f);
                    Punch(0.22f);
                    SpawnPip(0f, 0f, "BRACE", new Color(0.45f, 0.9f, 1f));
                    Spokes(0f, 0f, 1.5f, new Color(0.45f, 0.9f, 1f));
                    SpawnBurst(0f, 0f, new Color(0.45f, 0.9f, 1f, 0.45f), 3.6f);
                    break;
                case SimEventKind.Hold:
                    if (ev.Reason == "power")
                    {
                        _audio.PlayOneShot(_surge, 0.55f);
                        SpawnPip(0f, 0f, "GUNS", new Color(0.4f, 0.75f, 1f));
                        Spokes(0f, 0f, 1.8f, new Color(0.4f, 0.75f, 1f));
                        SpawnBurst(0f, 0f, new Color(0.4f, 0.75f, 1f, 0.5f), 4.2f);
                    }
                    else if (ev.Reason == "food")
                    {
                        _audio.PlayOneShot(_deposit, 0.55f);
                        SpawnPip(0f, 0f, "CREW", new Color(0.5f, 0.85f, 0.48f));
                        Spokes(0f, 0f, 1.8f, new Color(0.5f, 0.85f, 0.48f));
                        SpawnBurst(0f, 0f, new Color(0.5f, 0.85f, 0.48f, 0.5f), 4.2f);
                    }
                    else
                    {
                        _audio.PlayOneShot(_dry, 0.35f);
                        SpawnPip(0f, 0f, "AUTO", new Color(0.85f, 0.82f, 0.7f));
                    }
                    Punch(0.32f);
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
                    var t = Mathf.Clamp01((p.Until - Time.time) / 0.85f);
                    var c = p.Mesh.color;
                    c.a = t;
                    p.Mesh.color = c;
                }
                _pips[i] = p;
            }
        }

        void SpawnPip(float x, float z, string text, Color color)
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
                Until = Time.time + 0.85f,
                Vel = new Vector3(0f, 1.7f, 0f)
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
                Until = Time.time + 0.38f,
                Start = Time.time,
                Size = size,
                Color = color
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
                var t = Mathf.Clamp01((Time.time - b.Start) / 0.38f);
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
