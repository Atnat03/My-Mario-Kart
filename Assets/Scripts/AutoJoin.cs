
    using Unity.Multiplayer.PlayMode;
    using Unity.Netcode;
    using UnityEngine;

    public class AutoJoin : MonoBehaviour
    {
        private void Start()
        {
            if (CurrentPlayer.IsMainEditor)
            {
                NetworkManager.Singleton.StartHost();
            }
            else
            {
                NetworkManager.Singleton.StartClient();
            }
        }
    }
