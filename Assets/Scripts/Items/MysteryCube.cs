using System;
using System.Collections;
using MyPrint;
using ScriptableObjectsDefinitions;
using Unity.Netcode;
using UnityEngine;
using Console = MyPrint.Console;
using Random = UnityEngine.Random;

public class MysteryCube : NetworkBehaviour
{
    [SerializeField] private GameObject _model;
    [SerializeField] private Collider _collider;
    
    [SerializeField] private Vector2 timeBeforeRespawn;
    [SerializeField] private GameObject _destroyParticle;

    [Header("Sound")] 
    [SerializeField] private SoundsDataSO _soundData;
    [SerializeField] private AudioSource _audioSource;
    
    private float elapsedTimeRespawn;
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
        
        SoundManager.PlaySound(_soundData, "Appear", _audioSource);
        
        _model.transform.localScale = Vector3.zero;
        _model.SetActive(true);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            
            _model.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, elapsed / duration);
            
            yield return null;
        }
        
        _model.transform.localScale = Vector3.one;
        
        hasCube = true;
        _collider.enabled = true;
    }
    
    [Rpc(SendTo.Everyone)]
    private void DestroyCubeRpc()
    {
        Console.Print("Pick up Cube", ColorConsole.Pink);
        
        SoundManager.PlaySound(_soundData, "Destroy", _audioSource);
        
        Instantiate(_destroyParticle, transform.position, Quaternion.identity);
        
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
            int itemIndex = Random.Range(0, player.dataItem.itemList.Count);
            player.GiveItemServerSide(itemIndex);
            DestroyCubeRpc();
        }
    }
}
