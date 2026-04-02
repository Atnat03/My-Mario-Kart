using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndGameUI : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform _parentList;
    [SerializeField] private ScoreEndGameElement _uiPLayerElement;
    [SerializeField] private GameObject _uiEndGame;
    [SerializeField] private Button _quitButton;
    [SerializeField] private Sprite[] _ranks;
    [SerializeField] private Sprite[] _logoPersoList;

    public override void OnNetworkSpawn()
    {
        _uiEndGame.SetActive(false);
    }
    
    public void ActivateEndGameUI(List<PlayerDataScore> players)
    {
        UpdateUIClientRpc(players.ToArray());
    }

    [ClientRpc]
    void UpdateUIClientRpc(PlayerDataScore[] players)
    {
        List<PlayerDataScore> sortedList = players.OrderByDescending(p => p.score).ToList();

        for (int i = 0; i < sortedList.Count; i++)
        {
            ScoreEndGameElement element = Instantiate(_uiPLayerElement, _parentList);
            element.SetData(_ranks[i], _logoPersoList[sortedList[i].indexSkin], sortedList[i].playerName, sortedList[i].score);
        
            if (NetworkManager.LocalClientId == sortedList[i].playerID)
            {
                element.UpdateLocalScore();
            }
        }
    
        _quitButton.onClick.RemoveAllListeners();
        _quitButton.onClick.AddListener(QuitButton);
    }

    void QuitButton()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        SceneManager.LoadScene("Lobby");
    }
}
