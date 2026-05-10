using System;
using System.Threading.Tasks;
using MultiplayerTemplate.UI;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MultiplayerTemplate.Core
{
    /// <summary>
    /// Se encarga de inicializar los servicios básicos de Unity (Relay, Auth)
    /// antes de pasar al menú principal. Idealmente se adjunta a un GameObject
    /// en la escena 00_Splash.
    /// </summary>
    public class Bootstrapper : MonoBehaviour
    {
        [Tooltip("Nombre de la escena del Menú Principal a cargar después del Splash.")]
        [SerializeField] private string mainMenuSceneName = "01_MainMenu";

        [Tooltip("Referencia al SplashController para actualizar la barra de progreso.")]
        [SerializeField] private SplashController splashController;

        private async void Start()
        {
            Application.targetFrameRate = 60;

            await InitializeServicesAsync();
            LoadMainMenu();
        }

        private async Task InitializeServicesAsync()
        {
            try
            {
                splashController?.UpdateProgress(10f, "Iniciando servicios...");
                await UnityServices.InitializeAsync();
                splashController?.UpdateProgress(50f, "Autenticando...");
                Debug.Log("[Bootstrapper] Unity Services Initialized.");

                // Autenticación anónima necesaria para Unity Relay
                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    Debug.Log($"[Bootstrapper] Player signed in anonymously: {AuthenticationService.Instance.PlayerId}");
                }

                splashController?.UpdateProgress(100f, "Listo!");
                // Pequeña pausa para que el usuario vea el 100% antes de cambiar de escena
                await Task.Delay(300);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Bootstrapper] Error initializing services: {e.Message}");
                splashController?.UpdateProgress(0f, "Error al iniciar. Reintentando...");
            }
        }

        private void LoadMainMenu()
        {
            Debug.Log($"[Bootstrapper] Loading scene: {mainMenuSceneName}");
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
