using Unity.Multiplayer.PlayMode;
using Unity.Netcode;


public class GameManager : NetworkBehaviour
{
    public static GameManager instance;
    
    TimerManager timer;
    ScoreManager scoreManager;

    public override void OnNetworkSpawn()
    {
        instance = this;
        
        //Score
        scoreManager = GetComponent<ScoreManager>();
        scoreManager.StartGame();
        
        //Timer
        timer = GetComponent<TimerManager>();
        timer.StartTimer();
    }
    
    private void Start()
    {
        if (CurrentPlayer.IsMainEditor)
        {
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            NetworkManager.Singleton.StartClient();
        }
    }

    public void AddScore(ulong id, int score) => scoreManager.AddScore(score, id);
}
