using UnityEngine;
using UnityEngine.UIElements;
using MultiplayerTemplate.Networking;
using UnityEngine.SceneManagement;

namespace MultiplayerTemplate.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class LobbyController : MonoBehaviour
    {
        private Label lblJoinCode;
        private TextField txtJoinCode;
        private Button btnStartGame;
        private Button btnHost;
        private Button btnClient;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            lblJoinCode = root.Q<Label>("LblJoinCode");
            txtJoinCode = root.Q<TextField>("TxtJoinCode");

            btnHost = root.Q<Button>("BtnHost");
            btnClient = root.Q<Button>("BtnClient");
            btnStartGame = root.Q<Button>("BtnStartGame");
            var btnBack = root.Q<Button>("BtnBack");

            btnHost?.RegisterCallback<ClickEvent>(ev => OnHostClicked());
            btnClient?.RegisterCallback<ClickEvent>(ev => OnClientClicked());
            btnStartGame?.RegisterCallback<ClickEvent>(ev => OnStartGameClicked());
            btnBack?.RegisterCallback<ClickEvent>(ev => OnBackClicked());
        }

        private async void OnHostClicked()
        {
            if (RelayManager.Instance == null)
            {
                Debug.LogError("[Lobby] RelayManager no encontrado.");
                return;
            }

            btnHost.SetEnabled(false);
            btnClient.SetEnabled(false);
            lblJoinCode.text = "Creando sala...";

            string joinCode = await RelayManager.Instance.CreateRelay();
            
            if (!string.IsNullOrEmpty(joinCode))
            {
                lblJoinCode.text = $"Código de Sala: {joinCode}";
                btnStartGame.style.display = DisplayStyle.Flex; // Mostrar botón de iniciar para el host
            }
            else
            {
                lblJoinCode.text = "Error al crear sala";
                btnHost.SetEnabled(true);
                btnClient.SetEnabled(true);
            }
        }

        private async void OnClientClicked()
        {
            if (RelayManager.Instance == null) return;

            if (txtJoinCode != null && !string.IsNullOrEmpty(txtJoinCode.value))
            {
                btnHost.SetEnabled(false);
                btnClient.SetEnabled(false);
                // Normalizamos el código a mayúsculas por si el jugador lo escribe en minúsculas
                string code = txtJoinCode.value.Trim().ToUpper();
                lblJoinCode.text = "Conectando...";

                bool success = await RelayManager.Instance.JoinRelay(code);
                
                if (success)
                {
                    lblJoinCode.text = "✓ Conectado. Esperando al Host...";
                }
                else
                {
                    lblJoinCode.text = "✗ Código inválido o sala llena.";
                    btnHost.SetEnabled(true);
                    btnClient.SetEnabled(true);
                }
            }
            else
            {
                lblJoinCode.text = "⚠ Ingresa un código de sala.";
            }
        }

        private void OnStartGameClicked()
        {
            if (ConnectionManager.Instance != null)
            {
                ConnectionManager.Instance.StartGame();
            }
        }

        private void OnBackClicked()
        {
            // Usa ConnectionManager para cerrar limpiamente la sesión NGO antes de cambiar de escena.
            // Si no hay sesión activa, hace el SceneManager.LoadScene directamente.
            if (ConnectionManager.Instance != null)
            {
                ConnectionManager.Instance.Disconnect();
            }
            else
            {
                SceneManager.LoadScene("01_MainMenu");
            }
        }
    }
}
