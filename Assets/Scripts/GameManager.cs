using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.PlayMode;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;


[DefaultExecutionOrder(-1)]
public class GameManager : NetworkBehaviour
{
    public static GameManager instance;

    #region Properties

    public SpawnPlayer SpawnPlayer => _spawnPlayer;

    #endregion

    [SerializeField] private SpawnPlayer _spawnPlayer;
    
    TimerManager timer;
    ScoreManager scoreManager;
    
    public List<KartController> _playerKartList = new List<KartController>();
    
    public NetworkVariable<bool> isStarting = new(false);

    public bool isTest = false;

    public override void OnNetworkSpawn()
    {
        instance = this;

        if (!isTest)
        {
            StartTheGameplay();
            return;
        }

        if (!IsServer) return;
        
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoaded;
    }

    private void OnSceneLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        int expectedPlayers = NetworkManager.Singleton.ConnectedClientsList.Count;

        if (clientsCompleted.Count < expectedPlayers)
            return;

        isStarting.Value = false;

        StartCoroutine(CinematiqueWait());
    }

    IEnumerator CinematiqueWait()
    {
        yield return new WaitForSeconds(1);

        foreach (KartController kc in _playerKartList)
        {
            Transform t = _spawnPlayer.GetSpawnPos();
            
            kc.SetTransform(t.position, t.rotation);
        }
        
        yield return new WaitForSeconds(2);
        
        StartTheGameplay();
    }

    void StartTheGameplay()
    {
        if(!IsServer) return;
        
        //Score
        scoreManager = GetComponent<ScoreManager>();
        scoreManager.StartGame();
        
        //Timer
        timer = GetComponent<TimerManager>();
        timer.StartTimer();
        
        isStarting.Value = true;
    }

    public void AddScore(ulong id, int score) => scoreManager.AddScore(score, id);
    
    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoaded;
    }
}
