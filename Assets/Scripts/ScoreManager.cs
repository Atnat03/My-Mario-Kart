using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class ScoreManager : NetworkBehaviour
{
    [System.Serializable]
    public struct PlayerData : INetworkSerializable
    {
        public ulong playerID;
        public int score;

        public void AddScore(int s)
        {
            score += s;
            if(score < 0) score = 0;
        }
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref playerID);
            serializer.SerializeValue(ref score);
        }
    }

    public List<PlayerData> playerList = new List<PlayerData>();

    [Header("UI")]
    [SerializeField] private Transform scoreParentUI;
    [SerializeField] private GameObject prefabScore;

    private List<GameObject> _uiElements = new List<GameObject>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += AddClient;

            if (playerList.Count == 0)
            {
                playerList.Add(new PlayerData 
                { 
                    playerID = NetworkManager.Singleton.LocalClientId, 
                    score = 0 
                });
            }
        }

        Invoke(nameof(SyncAndUpdateUI), 0.5f);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (IsServer && NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= AddClient;
    }

    private void AddClient(ulong id)
    {
        if (!IsServer) return;
        if (playerList.Exists(p => p.playerID == id)) return;

        playerList.Add(new PlayerData { playerID = id, score = 0 });
        SyncAndUpdateUI();
    }

    public void AddScore(int score, ulong playerID)
    {
        if (!IsServer)
        {
            AddScoreServerRpc(score, playerID);
            return;
        }
        for (int i = 0; i < playerList.Count; i++)
        {
            if (playerList[i].playerID == playerID)
            {
                PlayerData updated = playerList[i];
                updated.AddScore(score);
                playerList[i] = updated;
                break;
            }
        }

        SyncAndUpdateUI();
    }

    [Rpc(SendTo.Server)]
    private void AddScoreServerRpc(int score, ulong playerID)
    {
        AddScore(score, playerID);
    }

    private void SyncAndUpdateUI()
    {
        if (IsServer)
        {
            SendFullListClientRpc(playerList.ToArray());
        }
    }

    [Rpc(SendTo.Everyone)]
    private void SendFullListClientRpc(PlayerData[] players)
    {
        playerList = players.ToList();
        UpdateUIRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void UpdateUIRpc()
    {
        List<PlayerData> sortedList = playerList.OrderByDescending(p => p.score).ToList();

        while (_uiElements.Count < sortedList.Count)
        {
            GameObject newUI = Instantiate(prefabScore, scoreParentUI);
            _uiElements.Add(newUI);
        }

        for (int i = 0; i < sortedList.Count; i++)
        {
            GameObject uiElement = _uiElements[i];
            PlayerData data = sortedList[i];

            ScoreUIElement scoreUI = uiElement.GetComponent<ScoreUIElement>();
            if (scoreUI != null)
            {
                scoreUI.UpdateScore(data.playerID, data.score, i + 1);
            }

            uiElement.SetActive(true);
        }

        for (int i = sortedList.Count; i < _uiElements.Count; i++)
        {
            _uiElements[i].SetActive(false);
        }
    }

    public void StartGame()
    {
        SyncAndUpdateUI();
    }
}