using System.Collections.Generic;
using MyPrint;
using Unity.Netcode;
using UnityEngine;

public class SpawnPlayer : NetworkBehaviour
{
    [SerializeField] private GameObject[] playerPrefabList;
    [SerializeField] private Transform[] spawnPos;

    // Stocke les données de chaque client
    private Dictionary<ulong, PlayerData> clientPlayerData = new();

    private struct PlayerData
    {
        public int skinId;
        public string playerName;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

            // L'host envoie ses données directement
            if (IsHost)
            {
                int mySkin = PlayerLocalData.Instance.LocalPlayerSkinId;
                string myName = PlayerLocalData.Instance.LocalPlayerName;
                
                if (string.IsNullOrEmpty(myName))
                    myName = "Player " + Random.Range(0, 100);

                clientPlayerData[NetworkManager.Singleton.LocalClientId] = new PlayerData
                {
                    skinId = mySkin,
                    playerName = myName
                };

                SpawnPlayerForClient(NetworkManager.Singleton.LocalClientId, mySkin, myName);
            }
        }

        // Les clients (host inclus) envoient leurs données au serveur
        if (IsClient && !IsHost)
        {
            int mySkin = PlayerLocalData.Instance.LocalPlayerSkinId;
            string myName = PlayerLocalData.Instance.LocalPlayerName;
            
            if (string.IsNullOrEmpty(myName))
                myName = "Player " + Random.Range(0, 100);

            SendPlayerDataToServerRpc(mySkin, myName);
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null && IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} connected, waiting for player data...");
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendPlayerDataToServerRpc(int skinId, string playerName, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        Console.Print($"Received data from Client {clientId}: Skin={skinId}, Name={playerName}", ColorConsole.Blue);

        clientPlayerData[clientId] = new PlayerData
        {
            skinId = skinId,
            playerName = playerName
        };

        SpawnPlayerForClient(clientId, skinId, playerName);
    }

    private void SpawnPlayerForClient(ulong clientId, int skinId, string playerName)
    {
        Transform pos = GetSpawnPos();

        if (skinId < 0 || skinId >= playerPrefabList.Length)
        {
            Debug.LogWarning($"Invalid skinId {skinId} for client {clientId}, using skin 0");
            skinId = 0;
        }

        GameObject prefab = playerPrefabList[skinId];
        GameObject player = Instantiate(prefab, pos.position, pos.rotation);

        NetworkObject networkObject = player.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(clientId, true);
        
        var kart = networkObject.GetComponent<KartController>();
        kart.InitServerSide(playerName);
        
        Debug.Log($"Spawned player for client {clientId} - Skin: {skinId}, Name: {playerName}");
    }
    
    public Transform GetSpawnPos() => spawnPos[Random.Range(0, spawnPos.Length)];
}