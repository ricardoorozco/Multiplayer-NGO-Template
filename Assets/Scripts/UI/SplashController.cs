using UnityEngine;
using UnityEngine.UIElements;

namespace MultiplayerTemplate.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class SplashController : MonoBehaviour
    {
        private ProgressBar prgLoading;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            prgLoading = root.Q<ProgressBar>("PrgLoading");
        }

        public void UpdateProgress(float value, string message = null)
        {
            if (prgLoading != null)
            {
                prgLoading.value = value;
                if (!string.IsNullOrEmpty(message))
                {
                    prgLoading.title = message;
                }
            }
        }
    }
}
