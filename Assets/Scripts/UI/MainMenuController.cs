using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace MultiplayerTemplate.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class MainMenuController : MonoBehaviour
    {
        [Tooltip("Nombre de la escena de Lobby (Sala de espera).")]
        [SerializeField] private string lobbySceneName = "02_Lobby";
        
        [Tooltip("Referencia al controlador del Modal de Opciones.")]
        [SerializeField] private OptionsModalController optionsModalController;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            var btnPlay = root.Q<Button>("BtnPlay");
            var btnOptions = root.Q<Button>("BtnOptions");
            var btnQuit = root.Q<Button>("BtnQuit");

            btnPlay?.RegisterCallback<ClickEvent>(ev => OnPlayClicked());
            btnOptions?.RegisterCallback<ClickEvent>(ev => OnOptionsClicked());
            btnQuit?.RegisterCallback<ClickEvent>(ev => OnQuitClicked());
        }

        private void OnPlayClicked()
        {
            SceneManager.LoadScene(lobbySceneName);
        }

        private void OnOptionsClicked()
        {
            if (optionsModalController != null)
            {
                optionsModalController.ShowModal();
            }
            else
            {
                Debug.LogWarning("[MainMenu] OptionsModalController no está asignado en el Inspector.");
            }
        }

        private void OnQuitClicked()
        {
            Debug.Log("[MainMenu] Saliendo del juego...");
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
