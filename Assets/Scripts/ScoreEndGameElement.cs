using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScoreEndGameElement : MonoBehaviour
{
    [SerializeField] private Image _logoPerso;
    [SerializeField] private Image _rank;
    [SerializeField] private Image _mine;
    [SerializeField] private TextMeshProUGUI _playerName;
    [SerializeField] private TextMeshProUGUI _playerScore;

    public void SetData(Sprite rank, Sprite logo, FixedString32Bytes playerName, int playerScore)
    {
        _rank.sprite = rank;
        _logoPerso.sprite = logo;
        _playerName.text = playerName.ToString();
        _playerScore.text = playerScore.ToString();
    }
    
    public void UpdateLocalScore()
    {
        _mine.gameObject.SetActive(true);
    }
}
