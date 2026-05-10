using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace MultiplayerTemplate.Networking
{
    public class RelayManager : MonoBehaviour
    {
        public static RelayManager Instance { get; private set; }

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

        /// <summary>
        /// Crea un servidor Relay y hace que el jugador actual sea el Host.
        /// </summary>
        /// <returns>El código de unión (Join Code) alfanumérico.</returns>
        public async Task<string> CreateRelay(int maxPlayers = 4)
        {
            try
            {
                // Se resta 1 porque el Host ya cuenta como un jugador en la asignación del Relay
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

                NetworkManager.Singleton.StartHost();
                return joinCode;
            }
            catch (RelayServiceException e)
            {
                Debug.LogError($"[RelayManager] Error creando Relay: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Une al jugador actual a un servidor Relay existente mediante su código.
        /// </summary>
        public async Task<bool> JoinRelay(string joinCode)
        {
            try
            {
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

                RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

                return NetworkManager.Singleton.StartClient();
            }
            catch (RelayServiceException e)
            {
                Debug.LogError($"[RelayManager] Error uniéndose al Relay: {e.Message}");
                return false;
            }
        }
    }
}
