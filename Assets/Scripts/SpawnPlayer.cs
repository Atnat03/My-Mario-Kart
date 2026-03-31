using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class SpawnPlayer : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    
    [SerializeField] private Transform[] spawnPos;


    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        SpawnPlayerObject(NetworkManager.Singleton.LocalClientId);

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        SpawnPlayerObject(clientId);
    }

    private void SpawnPlayerObject(ulong clientId)
    {
        Transform pos = GetSpawnPos();

        GameObject player = Instantiate(playerPrefab, pos.position, pos.rotation);

        var networkTransform = player.GetComponent<NetworkTransform>();
        if (networkTransform != null)
        {
            networkTransform.enabled = false;
        }

        player.GetComponent<NetworkObject>()
            .SpawnAsPlayerObject(clientId, true);
    
        if (networkTransform != null && IsServer)
        {
            StartCoroutine(ReenableNetworkTransform(networkTransform));
        }
    }

    private System.Collections.IEnumerator ReenableNetworkTransform(NetworkTransform networkTransform)
    {
        yield return new WaitForSeconds(0.1f);
        networkTransform.enabled = true;
    }
    
    public Transform GetSpawnPos() => spawnPos[Random.Range(0, spawnPos.Length)];
    public Transform GetSpawnPos(int id) => spawnPos[id % spawnPos.Length];
}