using System;
using System.Collections;
using MyPrint;
using Unity.Netcode;
using UnityEngine;
using Console = MyPrint.Console;
using Random = UnityEngine.Random;

public class MysteryCube : NetworkBehaviour
{
    [SerializeField] private GameObject _model;
    [SerializeField] private Collider _collider;
    
    [SerializeField] private Vector2 timeBeforeRespawn;
    
    [Header("DEBUG")]
    [SerializeField] private float elapsedTimeRespawn;
    private bool hasCube = true;
    
    private void Update()
    {
        transform.Rotate(0, 120 * Time.deltaTime, 0, Space.World);

        if (!IsServer) return;
        
        if (elapsedTimeRespawn > 0)
        {
            elapsedTimeRespawn -= Time.deltaTime;

            if (elapsedTimeRespawn <= 0)
            {
                RepawnCubeRpc();
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    private void RepawnCubeRpc()
    {
        StartCoroutine(CubeAppearAnimation());
    }

    IEnumerator CubeAppearAnimation()
    {
        float duration = 1f;
        float elapsed = 0;
        
        _model.transform.localScale = Vector3.zero;
        _model.SetActive(true);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            
            _model.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, elapsed / duration);
            
            yield return null;
        }
        
        _model.transform.localScale = Vector3.one;
        
        Console.Print("Spawn Cube", ColorConsole.Pink);
        
        hasCube = true;
        _collider.enabled = true;
    }
    
    [Rpc(SendTo.Everyone)]
    private void DestroyCubeRpc()
    {
        Console.Print("Pick up Cube", ColorConsole.Pink);
        
        hasCube = false;
        _model.SetActive(false);
        _collider.enabled = false;
        
        elapsedTimeRespawn = Random.Range(timeBeforeRespawn.x, timeBeforeRespawn.y);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        if (!hasCube) return;

        if (other.TryGetComponent(out PlayerItem player))
        {
            player.PickUpNewItem();
            DestroyCubeRpc();
        }
    }
}
