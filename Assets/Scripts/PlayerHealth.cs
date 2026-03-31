using System;
using System.Collections;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    public NetworkVariable<int> Health = new NetworkVariable<int>(2);
    [SerializeField] private GameObject[] _balloon;
    
    [Header("Dead")]
    [SerializeField] private ParticleSystem _deathParticle;
    [SerializeField] private int _looseScore = 25;
    
    public void TakeDamage()
    {
        if (!IsServer) return;
        
        Debug.Log("Take Damage");
        
        Health.Value--;

        if (Health.Value >= 0)
        {
            UpdateVisualClientRpc(Health.Value);

            if (Health.Value == 0)
            {
                Debug.Log("Player dead");

                GameManager.instance.AddScore(OwnerClientId, _looseScore * -1);
                
                DeadPlayerClientRpc();
            }
        }
    }

    [ClientRpc]
    void UpdateVisualClientRpc(int index)
    {
        _balloon[index].SetActive(false);
    }

    [Rpc(SendTo.Everyone)]
    void DeadPlayerClientRpc()
    {
        Instantiate(_deathParticle, transform.position, Quaternion.identity);

        StartCoroutine(WaitBeforeRespawn());
    }

    IEnumerator WaitBeforeRespawn()
    {
        yield return new WaitForSeconds(2);

        (Vector3, Quaternion) coord;

        if(GameManager.instance.SpawnPlayer != null)
        {
            Transform t = GameManager.instance.SpawnPlayer.GetSpawnPos();
            coord.Item1 = t.position;
            coord.Item2 = t.rotation;
        }
        else
        {
            coord.Item1 = Vector3.zero;
            coord.Item2 = Quaternion.identity;
        }

        
        transform.position = coord.Item1;
        transform.rotation = coord.Item2;

        for (int i = 0; i < _balloon.Length; i++)
        {
            _balloon[i].SetActive(true);
        }

        if(IsServer)
            Health.Value = 2;
    }
}
