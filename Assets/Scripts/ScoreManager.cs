using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public struct PlayerDataScore : INetworkSerializable, IEquatable<PlayerDataScore>
{
    public ulong playerID;
    public int score;
    public FixedString32Bytes playerName;
    public int indexSkin;

    public void AddScore(int s)
    {
        score += s;
        if (score < 0) score = 0;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref playerID);
        serializer.SerializeValue(ref score);
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref indexSkin);
    }

    public bool Equals(PlayerDataScore other)
    {
        return playerID == other.playerID && score == other.score && playerName.Equals(other.playerName) && indexSkin == other.indexSkin;
    }

    public override bool Equals(object obj)
    {
        return obj is PlayerDataScore other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(playerID, score, playerName, indexSkin);
    }
}

public class ScoreManager : NetworkBehaviour
{
    public NetworkList<PlayerDataScore> playerList;

    [Header("UI")]
    [SerializeField] private Transform scoreParentUI;
    [SerializeField] private GameObject prefabScore;

    private List<GameObject> _uiElements = new List<GameObject>();
    
    public NetworkVariable<bool> scoreUIVisible = new NetworkVariable<bool>();

    private void Awake()
    {
        if (scoreParentUI != null)
        {
            scoreParentUI.gameObject.SetActive(false);
        }
    }

    public override void OnNetworkSpawn()
    {
        scoreUIVisible.OnValueChanged += OnScoreUIChanged;
        playerList.OnListChanged += OnPlayerListChanged;

        scoreParentUI.gameObject.SetActive(scoreUIVisible.Value);
        
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += AddClient;

            foreach (ulong id in NetworkManager.Singleton.ConnectedClientsIds)
                AddClient(id);
        }
    }

    public override void OnNetworkDespawn()
    {
        scoreUIVisible.OnValueChanged -= OnScoreUIChanged;
        playerList.OnListChanged -= OnPlayerListChanged;
        
        if (IsServer && NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= AddClient;
    }

    private void AddClient(ulong id)
    {
        if (!IsServer) return;

        StartCoroutine(AddClientWhenReady(id));
    }

    private IEnumerator AddClientWhenReady(ulong id)
    {
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(id, out var clientData))
            yield break;

        while (clientData.PlayerObject == null)
            yield return null;

        KartController kart = clientData.PlayerObject.GetComponent<KartController>();
        
        if (ContainsPlayer(id))
            yield break;

        playerList.Add(new PlayerDataScore
        {
            playerID = id,
            score = 0,
            playerName = kart.PlayerName.Value,
            indexSkin = kart.SkinID.Value
        });
    }
    
    public void AddScore(int score, ulong playerID)
    {
        if (!IsServer)
        {
            AddScoreServerRpc(score, playerID);
            return;
        }

        for (int i = 0; i < playerList.Count; i++)
        {
            if (playerList[i].playerID == playerID)
            {
                PlayerDataScore data = playerList[i];
                data.AddScore(score);
                playerList[i] = data;
                break;
            }
        }
    }

    [Rpc(SendTo.Server)]
    private void AddScoreServerRpc(int score, ulong playerID)
    {
        AddScore(score, playerID);
    }

    private void OnPlayerListChanged(NetworkListEvent<PlayerDataScore> changeEvent)
    {
        // Mettre à jour l'UI seulement si elle est visible
        if (scoreUIVisible.Value && scoreParentUI.gameObject.activeSelf)
        {
            UpdateUI();
        }
    }
    
    private void UpdateUI()
    {
        List<PlayerDataScore> sorted = GetSortedList();

        // Créer les éléments UI manquants
        while (_uiElements.Count < sorted.Count)
        {
            GameObject ui = Instantiate(prefabScore, scoreParentUI);
            _uiElements.Add(ui);
        }
        
        ulong localClientId = NetworkManager.Singleton.LocalClientId;

        // Mettre à jour tous les éléments visibles
        for (int i = 0; i < sorted.Count; i++)
        {
            PlayerDataScore data = sorted[i];
            GameObject ui = _uiElements[i];

            ui.SetActive(true);

            ScoreUIElement scoreUI = ui.GetComponent<ScoreUIElement>();
            if (scoreUI != null)
            {
                scoreUI.UpdateScore(
                    data.playerName.ToString(),
                    data.score,
                    i
                );

                if (localClientId == data.playerID)
                {
                    scoreUI.UpdateLocalScore();
                }
            }
        }

        // Cacher les éléments inutilisés
        for (int i = sorted.Count; i < _uiElements.Count; i++)
        {
            _uiElements[i].SetActive(false);
        }
    }
    
    private bool ContainsPlayer(ulong id)
    {
        for (int i = 0; i < playerList.Count; i++)
        {
            if (playerList[i].playerID == id)
                return true;
        }
        return false;
    }

    private List<PlayerDataScore> GetSortedList()
    {
        List<PlayerDataScore> sorted = new List<PlayerDataScore>();

        for (int i = 0; i < playerList.Count; i++)
        {
            PlayerDataScore current = playerList[i];

            int insertIndex = 0;

            for (int j = 0; j < sorted.Count; j++)
            {
                if (current.score < sorted[j].score)
                {
                    insertIndex = j + 1;
                }
            }

            sorted.Insert(insertIndex, current);
        }

        return sorted;
    }
    
    public void StartGame()
    {
        if (!IsServer) return;

        scoreUIVisible.Value = true;
    }

    public void EndGame()
    {
        if (!IsServer) return;

        scoreUIVisible.Value = false;
    }
    
    private void OnScoreUIChanged(bool previous, bool current)
    {
        scoreParentUI.gameObject.SetActive(current);
        
        if (current)
        {
            UpdateUI();
        }
    }
}