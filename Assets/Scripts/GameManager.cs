using System;
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
    public NetworkVariable<bool> isEnd = new(false);
    private bool gameStarting = false;
    
    public bool isTest = false;

    [Header("Cinematique")]
    [SerializeField] private GameObject _cinematique;
    
    [Header("UI")]
    [SerializeField] private GameObject _waitingUI;
    [SerializeField] private GameObject _countDown;

    [Header("End game")]
    [SerializeField] private GameObject _gameFinish;
    [SerializeField] private EndGameUI _endGameManager;
    [SerializeField] private GameObject _endGameUI;
    
    //Action
    public Action OnStartCinematique;
    public Action OnStartCountDown;
    public Action OnEndGame;
    public Action OnEndGameUI;
    public Action OnStartGame;
    
    private void Awake()
    {
        instance = this;
    }

    public override void OnNetworkSpawn()
    {
        isEnd.OnValueChanged += OnEndChanged;
        isStarting.OnValueChanged += OnStartChange;

        if (IsServer)
        {
            isEnd.Value = false;
        }
        
        if (IsClient)
            _waitingUI.SetActive(true);
    }

    private void OnStartChange(bool previousValue, bool newValue)
    {
        if(newValue)
            OnStartGame?.Invoke();
    }

    public override void OnNetworkDespawn()
    {
        isEnd.OnValueChanged -= OnEndChanged;
    }

    private void OnEndChanged(bool previousValue, bool newValue)
    {
        _endGameUI.gameObject.SetActive(newValue);
    }

    public void StartGameFromLobby()
    {
        if (!IsServer) return;
        if (gameStarting) return;

        gameStarting = true;

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

        StartCinematiqueClientRpc();

        StartCoroutine(ServerStartAfterCountdown());
    }

    [ClientRpc]
    void StartCinematiqueClientRpc()
    {
        _cinematique.SetActive(true);
        _waitingUI.SetActive(false);
        
        StartCoroutine(CountDownVisual());
    }

    IEnumerator CountDownVisual()
    {
        OnStartCinematique?.Invoke();
        
        yield return new WaitForSeconds(6.5f);

        _countDown.SetActive(true);
        _cinematique.SetActive(false);
        
        Destroy(_countDown, 7f);
        
        yield return new WaitForSeconds(2.2f);
        
        OnStartCountDown?.Invoke();

        yield return new WaitForSeconds(3f);
    }

    IEnumerator ServerStartAfterCountdown()
    {
        yield return new WaitForSeconds(11.5f);

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
    
    public void EndGame()
    {
        if (!IsServer) return;
        
        isStarting.Value = false;

        scoreManager.EndGame();
        timer.EndGame();

        List<PlayerDataScore> data = new();
        foreach (PlayerDataScore p in scoreManager.playerList)
        {
            data.Add(p);
        }

        EndGameClientRpc(data.ToArray());
    }

    [Rpc(SendTo.Everyone)]
    void EndGameClientRpc(PlayerDataScore[] playerScores)
    {
        StopAllCoroutines();
        StartCoroutine(FinishGame(playerScores));
    }

    IEnumerator FinishGame(PlayerDataScore[] playerScores)
    {
        _gameFinish.SetActive(true);
        
        OnEndGame?.Invoke();

        yield return new WaitForSeconds(2f);

        _gameFinish.SetActive(false);

        if (IsServer)
        {
            isEnd.Value = true;
        }

        List<PlayerDataScore> data = new List<PlayerDataScore>(playerScores);
        
        OnEndGameUI?.Invoke();
        
        _endGameManager.ActivateEndGameUI(data);
    }
}