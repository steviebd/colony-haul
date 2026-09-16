using UnityEngine;

namespace ColonyHaul
{
    /// <summary>Procedural dusk-mesa dressing. No authored FBX required.</summary>
    public static class MesaView
    {
        public static Color Mesa = new Color(0.42f, 0.32f, 0.24f);
        public static Color Rim = new Color(0.22f, 0.28f, 0.30f);
        public static Color PadIdle = new Color(0.82f, 0.68f, 0.48f);
        public static Color PadFarm = new Color(0.55f, 0.86f, 0.48f);
        public static Color PadRoute = new Color(0.95f, 0.82f, 0.32f);
        public static Color Spawn = new Color(0.86f, 0.24f, 0.24f);
        public static Color RailLive = new Color(0.42f, 0.92f, 0.88f);
        public static Color RailCut = new Color(1f, 0.38f, 0.22f);
        public static Color Barrier = new Color(0.95f, 0.28f, 0.32f);

        public static Transform BuildStage(Transform parent, Camera cam)
        {
            var sun = New(parent, "Sun");
            var light = sun.gameObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.72f, 0.48f);
            light.intensity = 1.15f;
            light.shadows = LightShadows.Soft;
            sun.rotation = Quaternion.Euler(38f, -48f, 0f);

            var fill = New(parent, "Fill").gameObject.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.color = new Color(0.25f, 0.45f, 0.55f);
            fill.intensity = 0.35f;
            fill.transform.rotation = Quaternion.Euler(70f, 130f, 0f);

            RenderSettings.ambientLight = new Color(0.28f, 0.38f, 0.42f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.07f, 0.18f, 0.22f);
            RenderSettings.fogStartDistance = 16f;
            RenderSettings.fogEndDistance = 52f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.16f, 0.20f);
            cam.orthographic = true;
            cam.orthographicSize = 13.5f;
            cam.transform.position = new Vector3(18f, 20f, 18f);
            cam.transform.LookAt(new Vector3(0f, 0.4f, 0f));

            var mesa = Prim(parent, PrimitiveType.Cylinder, "Mesa", Vector3.zero, new Vector3(22f, 0.38f, 22f), Mesa);
            mesa.rotation = Quaternion.identity;

            for (var i = 0; i < 8; i++)
            {
                var a = i * Mathf.PI * 2f / 8f;
                var p = new Vector3(Mathf.Cos(a) * 11.6f, 0.2f, Mathf.Sin(a) * 11.6f);
                Prim(parent, PrimitiveType.Cube, "Cliff", p, new Vector3(3.2f, 1.1f, 1.1f), Rim)
                    .rotation = Quaternion.Euler(0f, -a * Mathf.Rad2Deg, 0f);
            }

            Prim(parent, PrimitiveType.Cylinder, "HubRing", new Vector3(0f, 0.42f, 0f), new Vector3(3.2f, 0.04f, 3.2f),
                new Color(0.55f, 0.48f, 0.38f));
            return mesa;
        }

        public static Transform Marker(Transform parent, SimNode n)
        {
            var color = n.Kind == NodeKind.Spawn ? Spawn
                : n.Kind == NodeKind.Hub ? new Color(0.9f, 0.78f, 0.58f)
                : n.Kind == NodeKind.Choke ? new Color(0.62f, 0.52f, 0.4f)
                : PadIdle;
            var scale = n.Kind == NodeKind.Hub ? new Vector3(1.6f, 0.06f, 1.6f)
                : n.Kind == NodeKind.Spawn ? new Vector3(1.1f, 0.05f, 1.1f)
                : new Vector3(0.95f, 0.05f, 0.95f);
            var t = Prim(parent, PrimitiveType.Cylinder, n.Id, new Vector3(n.X, 0.41f, n.Z), scale, color);
            var label = new GameObject("label").AddComponent<TextMesh>();
            label.transform.SetParent(t, false);
            label.transform.localPosition = new Vector3(0f, 18f, 0f);
            label.transform.localRotation = Quaternion.Euler(90f, 45f, 0f);
            label.transform.localScale = Vector3.one * 0.12f;
            label.characterSize = 0.35f;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.fontSize = 48;
            label.color = new Color(0.92f, 0.9f, 0.82f, 0.85f);
            label.text = n.Kind == NodeKind.Pad ? "PAD" : n.Kind == NodeKind.Spawn ? "RAID" : n.Kind.ToString().ToUpperInvariant();
            if (n.Kind == NodeKind.Hub) label.text = "HUB";
            if (n.Kind == NodeKind.Depot) label.text = "YARD";
            if (n.Kind == NodeKind.Choke) label.text = "GUN";
            if (n.Kind == NodeKind.Tower) label.text = "GUN";
            return t;
        }

        public static void SetLabel(Transform marker, string text)
        {
            var tm = marker.GetComponentInChildren<TextMesh>();
            if (tm != null) tm.text = text;
        }

        public static Transform Prim(Transform parent, PrimitiveType type, string name, Vector3 pos, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = scale;
            Tint(go, color);
            return go.transform;
        }

        public static void Tint(GameObject go, Color color)
        {
            var r = go.GetComponent<Renderer>();
            if (r == null) return;
            r.material.color = color;
        }

        public static Color BuildingColor(BuildingType type)
        {
            switch (type)
            {
                case BuildingType.Mine: return new Color(0.94f, 0.64f, 0.23f);
                case BuildingType.Farm: return new Color(0.44f, 0.78f, 0.42f);
                case BuildingType.Power: return new Color(0.32f, 0.68f, 0.94f);
                case BuildingType.Kinetic: return new Color(0.86f, 0.86f, 0.8f);
                case BuildingType.Splash: return new Color(0.94f, 0.63f, 0.38f);
                case BuildingType.Hub: return new Color(0.84f, 0.69f, 0.54f);
                case BuildingType.Depot: return new Color(0.23f, 0.33f, 0.38f);
                default: throw new System.ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public static Color EnemyColor(EnemyType type)
        {
            switch (type)
            {
                case EnemyType.Grunt: return new Color(0.9f, 0.32f, 0.3f);
                case EnemyType.Brute: return new Color(0.55f, 0.12f, 0.16f);
                case EnemyType.Runner: return new Color(0.95f, 0.42f, 0.78f);
                default: throw new System.ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        static Transform New(Transform parent, string name)
        {
            var t = new GameObject(name).transform;
            t.SetParent(parent, false);
            return t;
        }
    }
}
