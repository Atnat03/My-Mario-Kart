using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreUIElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerIdText;
    [SerializeField] private TextMeshProUGUI playerScoreText;
    [SerializeField] private Image playerRankImage;
    
    [SerializeField] private Image background;
    [SerializeField] private Color isLocalColor;
    
    [SerializeField] private Sprite[] rankSprites;

    public void UpdateScore(string nom, int score, int rank)
    {
        playerIdText.text = nom;
        playerScoreText.text = score.ToString();
        playerRankImage.sprite = rankSprites[rank];
    }

    public void UpdateLocalScore()
    {
        background.color = isLocalColor;
    }
}
