using System.Collections.Generic;
using MyPrint;
using Unity.Netcode;
using UnityEngine;

public class SpawnPlayer : NetworkBehaviour
{
    [SerializeField] private GameObject[] playerPrefabList;
    [SerializeField] private Transform[] spawnPos;

    private Dictionary<ulong, PlayerData> clientPlayerData = new();

    private bool gameStarted = false;
    
    private int expectedPlayers;
    
    private struct PlayerData
    {
        public int skinId;
        public string playerName;
        public bool isReady;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            expectedPlayers = PlayerLocalData.Instance.ExpectedPlayerCount;
            
            if (IsHost)
            {
                int mySkin = PlayerLocalData.Instance.LocalPlayerSkinId;
                string myName = PlayerLocalData.Instance.LocalPlayerName;
                
                if (string.IsNullOrEmpty(myName))
                    myName = "Player " + Random.Range(0, 100);

                clientPlayerData[NetworkManager.Singleton.LocalClientId] = new PlayerData
                {
                    skinId = mySkin,
                    playerName = myName,
                    isReady = true
                };

                SpawnPlayerForClient(NetworkManager.Singleton.LocalClientId, mySkin, myName);
                
                CheckAllPlayersReady();
            }
        }

        if (IsClient && !IsHost)
        {
            int mySkin = PlayerLocalData.Instance.LocalPlayerSkinId;
            string myName = PlayerLocalData.Instance.LocalPlayerName;
            
            if (string.IsNullOrEmpty(myName))
                myName = "Player " + Random.Range(0, 100);

            SendPlayerDataToServerRpc(mySkin, myName);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void SendPlayerDataToServerRpc(int skinId, string playerName, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        
        clientPlayerData[clientId] = new PlayerData
        {
            skinId = skinId,
            playerName = playerName,
            isReady = true
        };
        
        SpawnPlayerForClient(clientId, skinId, playerName);
        
        CheckAllPlayersReady();
    }

    private void SpawnPlayerForClient(ulong clientId, int skinId, string playerName)
    {
        int index = (int)clientId % spawnPos.Length;
        Transform pos = spawnPos[index % spawnPos.Length];
        
        if (skinId < 0 || skinId >= playerPrefabList.Length)
        {
            skinId = 0;
        }

        GameObject prefab = playerPrefabList[skinId];
        GameObject player = Instantiate(prefab, pos.position, pos.rotation);

        NetworkObject networkObject = player.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(clientId, true);
        
        KartController kart = networkObject.GetComponent<KartController>();
        kart.InitServerSide(playerName, skinId);
    }
    
    public Transform GetSpawnPos() => spawnPos[Random.Range(0, spawnPos.Length)];
    
    private void CheckAllPlayersReady()
    {
        if (gameStarted) return;

        int totalClients = NetworkManager.Singleton.ConnectedClientsList.Count;

        if (totalClients < expectedPlayers)
        {
            Debug.Log($"Waiting players {totalClients}/{expectedPlayers}");
            return;
        }

        if (clientPlayerData.Count < expectedPlayers)
            return;

        foreach (PlayerData player in clientPlayerData.Values)
        {
            if (!player.isReady)
                return;
        }

        gameStarted = true;

        Debug.Log("Tous les joueurs sont prêts !");
        OnAllPlayersReady();
    }
    
    private void OnAllPlayersReady()
    {
        GameManager.instance.StartGameFromLobby();
    }
}