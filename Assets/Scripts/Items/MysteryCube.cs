using System;
using MyPrint;
using Unity.Netcode;
using UnityEngine;

public class MysteryCube : NetworkBehaviour
{
    private void Update()
    {
        transform.Rotate(0, 120 * Time.deltaTime, 0, Space.World);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        if (other.TryGetComponent(out PlayerItem player))
        {
            player.PickUpNewItem();
            GetComponent<NetworkObject>().Despawn(true);
        }
    }
}
