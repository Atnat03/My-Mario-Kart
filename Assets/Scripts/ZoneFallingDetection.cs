using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class ZoneFallingDetection : NetworkBehaviour
{
    [SerializeField] private GameObject _message;
    [SerializeField] private Transform _messageTransform;

    public void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.GetComponentInParent<NetworkObject>() is NetworkObject netObj)
        {
            ulong clientId = netObj.OwnerClientId;

            TeleportClientRpc(
                netObj.NetworkObjectId,
                GameManager.instance.SpawnPlayer.GetSpawnPos().position,
                new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { clientId }
                    }
                }
            );

            EffectServerRpc(clientId);
        }
    }
    
    [ClientRpc]
    void TeleportClientRpc(ulong networkObjectId, Vector3 position, ClientRpcParams rpcParams = default)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject netObj))
        {
            CharacterController cc = netObj.GetComponent<CharacterController>();

            if (cc != null) cc.enabled = false;

            netObj.transform.position = position;

            if (cc != null) cc.enabled = true;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void EffectServerRpc(ulong clientId)
    {
        GameManager.instance.AddScore(clientId, -100);

        ShowMessageClientRpc(
            new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { clientId }
                }
            }
        );
    }

    [ClientRpc]
    private void ShowMessageClientRpc(ClientRpcParams rpcParams = default)
    {
        Instantiate(_message, _messageTransform);
    }
}