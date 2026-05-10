using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MultiplayerTemplate.Networking
{
    public class ConnectionManager : MonoBehaviour
    {
        public static ConnectionManager Instance { get; private set; }
        
        [SerializeField] private string gameSceneName = "03_Game";
        [SerializeField] private string mainMenuSceneName = "01_MainMenu";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (NetworkManager.Singleton != null)
            {
                // OnClientStopped se dispara SÓLO cuando el cliente local pierde la conexión
                NetworkManager.Singleton.OnClientStopped += HandleLocalClientStopped;
            }
            else
            {
                Debug.LogWarning("[ConnectionManager] NetworkManager no encontrado en la escena. Asegúrate de tener el prefab NetworkManager.");
            }
        }

        private void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientStopped -= HandleLocalClientStopped;
            }
        }

        public void StartGame()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost)
            {
                // NetworkSceneManager carga la escena para todos los clientes conectados de forma sincronizada
                NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
            }
        }

        /// <summary>
        /// Se llama cuando el cliente local se desconecta (ya sea por kick, pérdida de conexión
        /// o porque el Host cerró la sala). Redirige al menú principal.
        /// </summary>
        private void HandleLocalClientStopped(bool isHost)
        {
            Debug.Log($"[ConnectionManager] Conexión terminada (wasHost: {isHost}). Volviendo al menú principal.");
            SceneManager.LoadScene(mainMenuSceneName);
        }

        /// <summary>
        /// Desconecta limpiamente al jugador local y lo redirige al menú.
        /// Úsalo desde el botón 'Volver' del Lobby o la pausa.
        /// </summary>
        public void Disconnect()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
            }
            else
            {
                SceneManager.LoadScene(mainMenuSceneName);
            }
        }
    }
}
