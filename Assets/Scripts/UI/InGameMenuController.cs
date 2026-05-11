using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using MultiplayerTemplate.Networking;

namespace MultiplayerTemplate.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class InGameMenuController : MonoBehaviour
    {
        [Tooltip("Referencia al controlador del modal de opciones en la misma escena.")]
        [SerializeField] private OptionsModalController optionsModalController;

        private VisualElement menuOverlay;
        private bool isMenuOpen = false;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            menuOverlay = root.Q<VisualElement>("InGameMenuOverlay");

            // Asegurar que inicie oculto
            if (menuOverlay != null && !menuOverlay.ClassListContains("hidden"))
            {
                menuOverlay.AddToClassList("hidden");
            }

            // Registrar botones
            var btnResume = root.Q<Button>("BtnResume");
            var btnOptions = root.Q<Button>("BtnOptions");
            var btnDisconnect = root.Q<Button>("BtnDisconnect");

            btnResume?.RegisterCallback<ClickEvent>(ev => CloseMenu());
            btnOptions?.RegisterCallback<ClickEvent>(ev => OpenOptions());
            btnDisconnect?.RegisterCallback<ClickEvent>(ev => Disconnect());
        }

        private void Update()
        {
            // Detectar la tecla Escape usando el New Input System
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                ToggleMenu();
            }
        }

        private void ToggleMenu()
        {
            isMenuOpen = !isMenuOpen;

            if (isMenuOpen)
            {
                menuOverlay?.RemoveFromClassList("hidden");
            }
            else
            {
                CloseMenu();
            }
        }

        private void CloseMenu()
        {
            isMenuOpen = false;
            menuOverlay?.AddToClassList("hidden");
        }

        private void OpenOptions()
        {
            if (optionsModalController != null)
            {
                optionsModalController.ShowModal();
            }
            else
            {
                Debug.LogWarning("[InGameMenu] No hay OptionsModalController asignado.");
            }
        }

        private void Disconnect()
        {
            // Usamos el ConnectionManager para cerrar la sesión de red limpiamente
            if (ConnectionManager.Instance != null)
            {
                ConnectionManager.Instance.Disconnect();
            }
            else
            {
                Debug.LogError("[InGameMenu] ConnectionManager no encontrado.");
            }
        }
    }
}
