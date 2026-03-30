using System;
using Unity.Netcode;
using Unity.Multiplayer.PlayMode;
using UnityEngine;

public class NetworkManagerUI : NetworkBehaviour
{
    private void Start()
    {
        if (CurrentPlayer.IsMainEditor)
        {
            StartHost();
        }
        else
        {
            StartClient();
        }
    }

    public void StartHost()
    {
    }
    
    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }
}
