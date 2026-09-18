# FM-GAME-003 — Unity Linux + USB harness

**Captain verdict: B) Built wrong / harness mismatch**

Colony Haul does **not** open in Unity 2022.3.50f1 because the vendored Unity Semantic Bridge package is wired under the wrong UPM id. USB’s documented Editor listener / MCP / `set_play_mode` path never starts. License is active. Interactive Play Mode was **not** reached. This is **not** Unity-verified. Vite `playable/` is not a substitute.

| Field | Value |
| --- | --- |
| Verdict | **B — built wrong / harness mismatch** |
| Unity | **2022.3.50f1** (`c3db7f8bf9b1`) Linux Editor, Hub 3.21.3 |
| License | **Unity Personal** (`16767881234125-UnityPersXXXX`, Pro: NO). Hub signed in. |
| Repo SHA used | `9872c1a6cd93b47e477ca3be0979aaad4c997224` (branch); game tip behind that is `d81c5b4ee8460b05c51d0510217c5f7ef6cafc1f` |
| USB upstream read | `https://github.com/Programalyst/unity-semantic-bridge` clone `6cfbee6` |
| USB vendored | `ThirdParty/unity-semantic-bridge` (README claims `41e8bca`) |
| Interactive Play Mode | **Not reached** |
| Video | **None** |
| HTTPS gameplay | **None** |

## USB source of truth (what the harness actually says)

From Programalyst/unity-semantic-bridge README (same in the vendored copy):

1. Unity **2022.3 LTS–6.3** + **uv**.
2. Add `/com.gamenami.unity-semantic-bridge` **from disk**. Unity uses `package.json` `"name"`.
3. Register Python MCP: `uv --directory …/mcp-editor-bridge run main.py` (stdio). Python POSTs JSON-RPC to `http://127.0.0.1:1073/rpc`. That URL is **not** an MCP server.
4. In the **live Editor**: **Tools → Unity Semantic Bridge**, start HTTP listener **1073**. Health: `GET http://127.0.0.1:1073/health`.
5. Then agent tools: `get_scene_hierarchy`, `set_play_mode`, `get_screenshot`, etc.

USB is a **live Editor MCP**, not a batchmode test runner. Listener auto-starts only if EditorPrefs `UnitySemanticBridge_AutoConnect` is true (default **false**). `docs/UNITY_SEMANTIC_BRIDGE_NOTES.md` in the vendor tree is **stale** (old `ws://127.0.0.1:8765` / `Server/` layout). Current code is HTTP JSON-RPC on **1073** (`EditorBridge.cs`, `mcp-editor-bridge/`).

## Exact gap vs USB (fatal)

Unity Package Manager:

```
An error occurred while resolving packages:
  Project has invalid dependencies:
    com.gamenami.unity-semantic-bridge: The requested dependency 'com.gamenami.unity-semantic-bridge'
    does not match the `name` 'com.gamenami.unity-scemantic-bridge' specified in the package
    manifest of [com.gamenami.unity-semantic-bridge@file:../ThirdParty/unity-semantic-bridge/com.gamenami.unity-semantic-bridge]
```

| Piece | Value |
| --- | --- |
| USB `package.json` `"name"` (upstream **and** vendor) | `com.gamenami.unity-scemantic-bridge` (typo, shipped) |
| Colony Haul `Packages/manifest.json` key | `com.gamenami.unity-semantic-bridge` (correct spelling) |
| `Packages/packages-lock.json` | **No USB entry at all** |
| Baked by | `tools/generate_unity_project.py` |
| Project README | Documents the mismatch and says **do not fix vendor in place** unless updating the vendor |

USB “add from disk” would register the **typo** id. Colony Haul remapped the id in the game manifest. UPM requires they match. The Editor never imports USB, never compiles `EditorBridge`, never offers **Tools → Unity Semantic Bridge**, never binds **:1073**.

Did **not** silently rename the package to paper this over.

## Other USB gaps (would still remain after the id fix)

- **`uv` is not installed** on this VM. USB lists it as a prerequisite.
- **No MCP registration** (no `.cursor/mcp.json` pointing at `ThirdParty/unity-semantic-bridge/mcp-editor-bridge`).
- Listener **not running**: `curl http://127.0.0.1:1073/health` → connection refused.
- Colony Haul CLI `ColonyHaul.EditorTools.ColonyHaulMenus.Headless` is **not** the USB harness. USB Play is `set_play_mode` after the listener is up in a GUI Editor.
- `executeMethod` **Headless never ran** — UPM failed first (`CLI_EXIT=1`).

## Commands run

```bash
# USB docs
git clone --depth 1 https://github.com/Programalyst/unity-semantic-bridge.git /tmp/unity-semantic-bridge
# HEAD 6cfbee6; package.json name = com.gamenami.unity-scemantic-bridge

# License-active Editor CLI (Colony Haul executeMethod — blocked before USB/listener)
/home/ubuntu/Unity/Hub/Editor/2022.3.50f1/Editor/Unity \
  -batchmode -nographics -quit -accept-apiupdate \
  -projectPath /workspace \
  -executeMethod ColonyHaul.EditorTools.ColonyHaulMenus.Headless \
  -logFile /tmp/unity-cli/headless.log
# CLI_START 2026-09-18T08:31:10Z  CLI_EXIT=1  CLI_DONE 2026-09-18T08:31:28Z

curl -sS -m 2 http://127.0.0.1:1073/health
# Failed to connect to 127.0.0.1 port 1073
```

Log excerpt (license OK, then UPM):

```
Unity Editor version:    2022.3.50f1 (c3db7f8bf9b1)
[Licensing::Module] Serial number assigned to: "16767881234125-UnityPersXXXX"
Pro License: NO
Rebuilding Library because the asset database could not be found!
[Package Manager] Done resolving packages in 17.05 seconds
An error occurred while resolving packages:
  Project has invalid dependencies:
    com.gamenami.unity-semantic-bridge: The requested dependency 'com.gamenami.unity-semantic-bridge'
    does not match the `name` 'com.gamenami.unity-scemantic-bridge' ...
```

Full log: `artifacts/fm-game-003-unity-cli-package-fail.txt`

## Proof type (honest labels)

| Claim | Status |
| --- | --- |
| Unity Editor installed on this Linux VM | Yes |
| Unity Personal license active | Yes |
| USB harness (listener 1073 + MCP `set_play_mode` / `get_screenshot`) | **No — project will not resolve USB** |
| Editor Play Mode / vertical slice video | **No** |
| CLI `Run Headless Sim` in Editor | **No — never reached executeMethod** |
| Vite / TS sim | Spare harness only — **not** this proof |

## Remaining captain blocker

**Align the UPM id** with USB `package.json` `"name"` (`com.gamenami.unity-scemantic-bridge`) *or* change the vendor `"name"` to the spelled-correct id (Colony Haul README currently forbids an in-place vendor “fix”). Then: install `uv`, register the Python MCP, open the project in the GUI Editor, **Tools → Unity Semantic Bridge → Connect**, `GET :1073/health`, `set_play_mode`. Until the id matches, USB cannot run.

## Phase A history (superseded)

Earlier this run: Editor installed; first GUI/batchmode died on **no license**. Captain signed into Hub. Personal entitlement landed at 08:29 UTC (`UnityEntitlementLicense.xml`). That wall is cleared. Current wall is the USB package id.

Screenshots from that phase: `fm-game-003-license-error.png`, `fm-game-003-hub-signin.png`, `fm-game-003-signin-ready.png`.
