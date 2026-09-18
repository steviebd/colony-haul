# FM-GAME-003 — Unity Editor Linux install + Play Mode attempt

**FEATURE FREEZE = Yes.** No game/code churn. This is an install-and-prove tape, not a slice change.

GitHub: **https://github.com/steviebd/colony-haul**

| Field | Value |
| --- | --- |
| Repo tip used | `d81c5b4ee8460b05c51d0510217c5f7ef6cafc1f` (`d81c5b4 Drop freeze SHA import workflow`) on `main` |
| Project pin | `ProjectSettings/ProjectVersion.txt` → **2022.3.50f1** (`c3db7f8c9b10` in-file) |
| Official changeset | **`c3db7f8bf9b1`** (Unity archive / tarball). The in-file revision string does not fetch. |
| Editor installed | **Yes** — Unity **2022.3.50f1** Linux (`c3db7f8bf9b1`), reports `2022.3.50f1` |
| Hub installed | **Yes** — `unityhub` **3.21.3** via official apt repo |
| Play Mode | **FAIL** — license, before project import / Play |
| Video | **None** — Play Mode never started. Not replaced by Vite / TS sim. |
| GPU | None. Mesa **llvmpipe** OpenGL 4.5 software (untested for Play; license blocked first) |
| Display | **Yes** — TigerVNC `:1` 1920×1200 XFCE |

## Verdict (captain)

Editor **downloads and launches on this Linux VM**. Play Mode does **not**. Exact blocker:

```
No valid Unity Editor license found. Please activate your license.
```

LicensingClient extras: `No ULF license found`, `Token not found in cache`, `Access token is unavailable`, `Found 0 entitlement groups and 0 free entitlements`, `com.unity.editor.ui` / `com.unity.editor.headless` not found, `Pro License: NO`.

Hub then requires **Sign in**. No Unity ID / serial / ULF was present in the environment. Did not invent credentials.

**This is not Unity-verified Play Mode.** Spare `playable/` Vite / headless TS sim were not used as a substitute.

## Disk (before → after)

| | Size | Used | Avail |
| --- | --- | --- | --- |
| Start | 252G overlay | 9.8G (5%) | **230G** |
| After Hub + Editor | 252G overlay | 22G (10%) | **218G** |

No Unity bits existed at start (`which unityhub` empty; no `/opt/Unity`, no `~/.local/share/unity3d` license).

| Artifact | Size |
| --- | --- |
| Official tarball `Unity-2022.3.50f1.tar.xz` | **3.9G** (4,125,472,828 bytes) |
| Tarball sha256 | `62b4e41fb49830645b3403c3b917ac507b6094e1cb0ad95d63a79fb6d045ba84` |
| Extracted Editor | **7.2G** |
| Hub deb install | 166 MB download → ~553 MB on disk (`/usr/lib/unityhub` ~528M) |
| Unity tree total | **~12G** under `/home/ubuntu/Unity` |

Disk was **not** the blocker.

## Commands (what actually ran)

Hub (official Ubuntu repo; AppImage CDN 404):

```bash
sudo install -d /etc/apt/keyrings
curl -fsSL https://hub.unity3d.com/linux/keys/public | sudo gpg --dearmor -o /etc/apt/keyrings/unityhub.gpg
echo "deb [arch=amd64 signed-by=/etc/apt/keyrings/unityhub.gpg] https://hub.unity3d.com/linux/repos/deb stable main" | sudo tee /etc/apt/sources.list.d/unityhub.list
sudo apt-get update
sudo apt-get install -y unityhub mesa-utils libglu1-mesa
# unityhub 3.21.3  /usr/bin/unityhub -> /usr/lib/unityhub/unityhub
```

Editor tarball (project changeset `c3db7f8c9b10` → **404**; official `c3db7f8bf9b1` → **200**):

```bash
curl -L -o /home/ubuntu/Unity/downloads/Unity-2022.3.50f1.tar.xz \
  https://download.unity3d.com/download_unity/c3db7f8bf9b1/LinuxEditorInstaller/Unity.tar.xz
# 07:52:35 → 07:58:47 UTC, CURL_EXIT=0
tar -xJf /home/ubuntu/Unity/downloads/Unity-2022.3.50f1.tar.xz \
  -C /home/ubuntu/Unity/Hub/Editor/2022.3.50f1
/home/ubuntu/Unity/Hub/Editor/2022.3.50f1/Editor/Unity -version
# → 2022.3.50f1
```

Hub already listed that path as installed:

```text
[{ "version": "2022.3.50f1", "architecture": "x86_64",
   "location": "/home/ubuntu/Unity/Hub/Editor/2022.3.50f1/Editor/Unity" }]
```

Batchmode (no GUI) and GUI Play both died on license before `-projectPath` imported `Assets/_ColonyHaul`:

```bash
DISPLAY=:1 /home/ubuntu/Unity/Hub/Editor/2022.3.50f1/Editor/Unity \
  -batchmode -nographics -quit -logFile /tmp/unity-batch.log -projectPath /workspace
# BATCH_EXIT=1

DISPLAY=:1 LIBGL_ALWAYS_SOFTWARE=1 \
  /home/ubuntu/Unity/Hub/Editor/2022.3.50f1/Editor/Unity \
  -projectPath /workspace -logFile /tmp/unity-gui.log -force-glcore
# GUI: License error dialog, then handed off to Hub
```

Manual activation request file **was** generated (not committed; machine-local `.alf`):

```bash
Unity -batchmode -nographics -quit -createManualActivationFile
# → /tmp/Unity_v2022.3.50f1.alf
```

That file still has to be exchanged at license.unity3d.com with a Unity account. Not done.

Hub CLI (`unityhub --no-sandbox -- --headless help`) works; it can list/install editors. It cannot skip sign-in.

## Play Mode

**Pass / fail:** **FAIL**

**Exact error:** `No valid Unity Editor license found. Please activate your license.`

Did **not** reach:

- Package resolve (URP 14.0.11 / USB file: package)
- `Assets/_ColonyHaul/Scenes/Game_VerticalSlice.unity` or `Boot.unity`
- Menu **Colony Haul → Play Vertical Slice** / **Play Demo (autopilot)**
- Any mesa / Farm→rail loop

Display existed. Editor process drew windows. Failure is **license / Hub auth**, not “no display”.

## Proof (Unity-side, not Vite)

- `artifacts/fm-game-003-license-error.png` — Editor **License error** over Hub EULA: *No valid Unity Editor license found. Please activate your license.* Open Hub / Exit.
- `artifacts/fm-game-003-hub-signin.png` — After EULA Agree: **Welcome to Unity Hub** / **Sign in** / Create account. Stopped here.
- Batch log (full):

```
Unity Editor version:    2022.3.50f1 (c3db7f8bf9b1)
Batch mode:              YES
[Licensing::Module] Error: Access token is unavailable; failed to update
[Licensing::Client] Error: Code 500 while processing request (status: Unable to update licenses. Errors: No ULF license found.,Token not found in cache)
[Licensing::Client] Error: Code 404 while processing request (status: Found 0 entitlement groups and 0 free entitlements matching requested entitlement ids)
[Licensing::Module] Error: 'com.unity.editor.headless' was not found.
Pro License: NO
No valid Unity Editor license found. Please activate your license.
```

GUI log same error with `com.unity.editor.ui` and `Desktop is 1920 x 1200 @ 60 Hz`, then Hub spawn:

```
'/usr/bin/unityhub' '--' '--silent' '--' '-projectPath' '/workspace' '-logFile' '/tmp/unity-gui.log' '-force-glcore'
```

## Video

**None.** Play Mode did not run, so there is no Editor gameplay mp4/webm. Existing `artifacts/colony-haul-run.webm` is the **Vite spare harness** (FM-GAME-002) and must not be cited as Unity Play Mode.

Durable HTTPS: n/a.

## Honest critique

Install is the easy part on this VM: 230G free, Hub apt repo live, official Linux tarball live, Editor binary runs, X11 + llvmpipe are present. Unity still will not open the project without a signed-in Hub / Personal (or Pro) ULF. That is an account entitlement, not a missing download.

If a license lands later, the **next** unproven risks are still real: software GL (no GPU), 15G RAM / 4 CPU, first-import package resolve from `packages.unity.com`, and whether URP Play Mode is stable under llvmpipe. None of those were reached.

`ProjectVersion.txt` ships a changeset (`c3db7f8c9b10`) that 404s; the matching public 2022.3.50f1 Linux editor is `c3db7f8bf9b1`. Harmless for Hub-on-a-dev-machine; painful for a scripted tarball.

## One remaining captain blocker

**Unity Hub login (or a Personal/Pro `.ulf` / serial) on this VM.** Until that exists, Editor Play Mode cannot start, and the vertical slice cannot be Unity-verified here.

Do not treat Vite `playable/` or `Colony Haul → Run Headless Sim` as a stand-in for that watch.
