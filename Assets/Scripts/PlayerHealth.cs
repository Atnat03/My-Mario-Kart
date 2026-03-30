using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    public NetworkVariable<int> Health = new NetworkVariable<int>(2);
    [SerializeField] private GameObject[] balloon;

    public void TakeDamage()
    {
        if (!IsServer) return;
        
        Debug.Log("Take Damage");
        
        Health.Value--;

        if (Health.Value >= 0)
            UpdateVisualClientRpc(Health.Value);
    }

    [ClientRpc]
    void UpdateVisualClientRpc(int index)
    {
        balloon[index].SetActive(false);
    }
}
