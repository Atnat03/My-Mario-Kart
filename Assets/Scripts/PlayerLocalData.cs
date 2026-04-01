using UnityEngine;

public class PlayerLocalData : MonoBehaviour
{
    public static PlayerLocalData Instance { get; private set; }

    public int LocalPlayerSkinId { get; private set; } = 0;
    public string LocalPlayerName { get; private set; } = "";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        Debug.Log("PlayerLocalData created and persisted");
    }

    public void SetPlayerData(int skinId, string playerName)
    {
        LocalPlayerSkinId = skinId;
        LocalPlayerName = playerName;
        
        Debug.Log($"PlayerLocalData saved: Skin={skinId}, Name={playerName}");
    }

    public void ClearData()
    {
        LocalPlayerSkinId = 0;
        LocalPlayerName = "";
    }
}