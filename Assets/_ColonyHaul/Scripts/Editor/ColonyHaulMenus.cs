using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ColonyHaul.EditorTools
{
    public static class ColonyHaulMenus
    {
        const string Slice = "Assets/_ColonyHaul/Scenes/Game_VerticalSlice.unity";
        const string Boot = "Assets/_ColonyHaul/Scenes/Boot.unity";

        [MenuItem("Colony Haul/Open Boot Scene")]
        public static void OpenBoot()
        {
            EditorSceneManager.OpenScene(Boot);
        }

        [MenuItem("Colony Haul/Open Vertical Slice Scene")]
        public static void OpenSlice()
        {
            EditorSceneManager.OpenScene(Slice);
        }

        [MenuItem("Colony Haul/Play Vertical Slice")]
        public static void PlaySlice()
        {
            VerticalSliceBootstrap.ForceDemo = false;
            EditorSceneManager.OpenScene(Slice);
            EditorApplication.isPlaying = true;
        }

        [MenuItem("Colony Haul/Play Demo (autopilot)")]
        public static void PlayDemo()
        {
            VerticalSliceBootstrap.ForceDemo = true;
            EditorSceneManager.OpenScene(Slice);
            EditorApplication.isPlaying = true;
        }

        [MenuItem("Colony Haul/Run Headless Sim")]
        public static void Headless()
        {
            var result = GameSim.RunHeadless(7, 480f);
            Debug.Log(
                $"[Colony Haul] phase={result.Phase} t={result.T:0.0}s waves={result.WaveIndex} hubL={result.HubLevel} " +
                $"deposits={result.Deposits} buildings={result.Buildings} routes={result.Routes} kills={result.Kills} " +
                $"brownouts={result.Brownouts} sabotages={result.Sabotages} minPwr={result.MinPower:0.0} win={result.Win}");
            if (!result.Win)
                Debug.LogWarning("[Colony Haul] Headless seed 7 did not win. Check DemoPilot / balance lockstep.");
        }
    }
}
