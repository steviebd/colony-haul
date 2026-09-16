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

        readonly List<Tracer> _tracers = new List<Tracer>();
        readonly List<LineRenderer> _pool = new List<LineRenderer>();
        readonly Transform _root;
        readonly AudioSource _audio;
        readonly AudioClip _deposit;
        readonly AudioClip _shot;
        readonly AudioClip _cut;
        readonly AudioClip _wave;
        readonly AudioClip _win;
        readonly AudioClip _lose;
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
            _win = Beep(660f, 0.35f);
            _lose = Beep(110f, 0.4f);
        }

        public void Punch(float amount) => _shake = Mathf.Max(_shake, amount);

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
            for (var i = _tracers.Count - 1; i >= 0; i--)
                if (Time.time > _tracers[i].Until) _tracers.RemoveAt(i);
            while (_pool.Count < _tracers.Count)
            {
                var go = new GameObject("tracer");
                go.transform.SetParent(_root, false);
                var lr = go.AddComponent<LineRenderer>();
                lr.positionCount = 2;
                lr.startWidth = 0.08f;
                lr.endWidth = 0.02f;
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
                    AddTracer(ev, new Color(0.55f, 0.95f, 0.9f));
                    _audio.PlayOneShot(_shot, 0.35f);
                    break;
                case SimEventKind.Splash:
                    AddTracer(ev, new Color(0.95f, 0.7f, 0.35f));
                    _audio.PlayOneShot(_shot, 0.45f);
                    Punch(0.4f);
                    break;
                case SimEventKind.Deposit:
                    _audio.PlayOneShot(_deposit, 0.4f);
                    break;
                case SimEventKind.Sabotage:
                    _audio.PlayOneShot(_cut, 0.6f);
                    Punch(0.9f);
                    break;
                case SimEventKind.Wave:
                    _audio.PlayOneShot(_wave, 0.5f);
                    Punch(0.55f);
                    break;
                case SimEventKind.Brownout:
                    Punch(0.25f);
                    break;
                case SimEventKind.Barrier:
                    Punch(0.2f);
                    break;
                case SimEventKind.Hit:
                    break;
                case SimEventKind.Death:
                    break;
                case SimEventKind.WarnFood:
                    break;
                case SimEventKind.Build:
                    _audio.PlayOneShot(_deposit, 0.2f);
                    break;
                case SimEventKind.Upgrade:
                    _audio.PlayOneShot(_wave, 0.4f);
                    Punch(0.35f);
                    break;
                case SimEventKind.Win:
                    _audio.PlayOneShot(_win, 0.8f);
                    break;
                case SimEventKind.Lose:
                    _audio.PlayOneShot(_lose, 0.8f);
                    Punch(1.1f);
                    break;
                case SimEventKind.Route:
                    _audio.PlayOneShot(_deposit, 0.25f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ev.Kind), ev.Kind, null);
            }
        }

        void AddTracer(SimEvent ev, Color color)
        {
            _tracers.Add(new Tracer
            {
                A = new Vector3(ev.FromX, 1.2f, ev.FromZ),
                B = new Vector3(ev.ToX, 0.7f, ev.ToZ),
                Color = color,
                Until = Time.time + 0.09f
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
    }
}
