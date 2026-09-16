using UnityEngine;
using UnityEngine.SceneManagement;

namespace ColonyHaul
{
    public sealed class BootLoader : MonoBehaviour
    {
        [SerializeField] float delay = 0.6f;

        void Start()
        {
            Invoke(nameof(Go), delay);
        }

        void Go()
        {
            SceneManager.LoadScene("Game_VerticalSlice");
        }
    }
}
