# Colony Haul

Sci-fi frontier colony + mag-rail logistics + tower defense. Hold Mesa 7 through six raider waves with Hub Level 2 online.

**Product is the Unity project.** Open this folder in Cursor, then open the same folder in Unity Hub.

GitHub (Cursor-friendly): **https://github.com/steviebd/colony-haul**

## Open in Cursor

```bash
git clone https://github.com/steviebd/colony-haul.git
cd colony-haul
```

File → Open Folder on that clone. Work under `Assets/_ColonyHaul`.

## Open in Unity Hub

Unity **2022.3 LTS** (project version `2022.3.50f1`) or **Unity 6 ≤ 6.3** with URP.

1. Unity Hub → **Open** → this repo folder (the folder that contains `Assets/`, `Packages/`, `ProjectSettings/`).
2. If prompted, add **URP** (`com.unity.render-pipelines.universal` 14.x on 2022.3). It is already in `Packages/manifest.json`.
3. Open `Assets/_ColonyHaul/Scenes/Boot.unity` (loads the slice after a short beat) **or** `Assets/_ColonyHaul/Scenes/Game_VerticalSlice.unity`.
4. Press Play, or use the menus:
   - **Colony Haul → Play Vertical Slice** — you take the watch (Farm first).
   - **Colony Haul → Play Demo (autopilot)** — seed-7 demo pilot.
   - **Colony Haul → Run Headless Sim** — ticks C# rules with no rendering; expect `win=True` on seed 7.

Scenes are thin on purpose. `VerticalSliceBootstrap` builds the dusk mesa, pads, rails, HUD, and juice in Play Mode.

### Play in the Editor

- Place a **Farm** on the south mesa pad (it pulses), then click the **Hub** to lay mag-rail. Haulers only move on live rails.
- Keys `1–7`: Farm, Mine, Power, Route, Kinetic, Splash, Barrier. `U` Hub L2. `D` demo. `R` restart.
- Left tray is clickable. Splash stays locked until Hub L2.
- Top-right logistics strip: loaded vs idle haulers, live/inbound raiders, next-wave lane copy, and a gun-fire fuse. HAUL CUT shows splice countdown.
- Opening coach: 1 Farm (south pad) → 2 rail to Hub → 3 first hauls. Farm/Route tray buttons pulse until that beat is done.
- Guns draw range rings. Route mode shows a ghost rail. Enemies show HP. Deposits float scrap/ore pips. Pads stack a buffer pillar as ore/food/power sits uncollected.
- Win: survive 6 waves **and** Hub Level 2. Lose: Hub HP 0, or food stays at 0 for 14s.
- Runners cut rails. Route is auto-armed — click the glowing pad to splice.
- Brownout: guns try to fire with a dry pylon. Keep Power on live rails.

Folder layout: `Assets/_ColonyHaul/{Scripts,Art,Prefabs,Scenes,UI,Audio}`.

## Unity Semantic Bridge (USB)

Vendored from https://github.com/Programalyst/unity-semantic-bridge at commit `41e8bca`. Do not move the package path.

- Unity package: `ThirdParty/unity-semantic-bridge/com.gamenami.unity-semantic-bridge` (manifest id `com.gamenami.unity-semantic-bridge` via `file:`). Upstream `package.json` still spells `com.gamenami.unity-scemantic-bridge`. Do not "fix" that in place unless you are updating the vendor.
- Python MCP: `ThirdParty/unity-semantic-bridge/mcp-editor-bridge`

### Listener

1. Open Unity.
2. **Tools → Unity Semantic Bridge**.
3. Start the HTTP listener on port **1073**.
4. Health: `GET http://127.0.0.1:1073/health`.

The Unity `/rpc` URL is the Python server's downstream connection. It is not an MCP server URL.

### uv MCP

Install [uv](https://docs.astral.sh/uv/getting-started/installation/). Point Cursor at the vendored Python server:

```json
{
  "mcpServers": {
    "unity-semantic-bridge": {
      "command": "uv",
      "args": [
        "--directory",
        "/absolute/path/to/this-repo/ThirdParty/unity-semantic-bridge/mcp-editor-bridge",
        "run",
        "main.py"
      ]
    }
  }
}
```

Python talks JSON-RPC to Unity at `http://127.0.0.1:1073/rpc`. After C# changes, instance IDs are invalid until you call `get_scene_hierarchy` again.

## Spare harness (not the product)

A lockstep TypeScript sim lives in `playable/` for agent VMs that have no Unity Editor.

```bash
cd playable
npm install
npm run sim      # headless seed 7
npm run playtest # nested Farm→Route→Hub plus stress
npm run dev      # http://127.0.0.1:43147  (?play=1, ?demo=1&rate=2)
```

Do not treat the Vite client as the ship target. Keep it only when kernel parity helps.

## Gaps

- This cloud VM has no Unity Editor, so Editor Play is documented, not screenshot-verified here. Headless C# is what the menu item runs.
- Runtime art is procedural low-poly, not authored FBX.
- Origin `tmp-*` remote cannot be renamed with the current CLI; GitHub is the repo to open in Cursor.
- GitHub clone is Hub/Cursor complete: `Assets/`, `Packages/manifest.json`, `ProjectSettings/`, USB C# package, `playable/`, artifacts tape including `colony-haul-run.webm`.
- Out of scope: multiplayer, cloud meta, deep tech tree, campaign.
