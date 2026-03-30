using TMPro;
using UnityEngine;

public class ScoreUIElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerIdText;
    [SerializeField] private TextMeshProUGUI playerScoreText;
    [SerializeField] private TextMeshProUGUI playerRankText;

    public void UpdateScore(ulong id, int score, int rank)
    {
        playerIdText.text = id.ToString();
        playerScoreText.text = score.ToString();
        playerRankText.text = rank.ToString();
    }
}
