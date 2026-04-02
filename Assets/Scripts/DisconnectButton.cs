using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DisconnectButton : MonoBehaviour
{
    public void OnClickToDisconnect()
    {
        if(LobbyManager.instance != null)
            LobbyManager.instance.LeaveLobby();
        
        if(NetworkManager.Singleton != null)
            NetworkManager.Singleton.Shutdown();
        
        SceneManager.LoadScene(1);
    }
}
