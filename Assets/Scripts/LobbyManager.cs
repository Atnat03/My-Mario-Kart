using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyPrint;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using Console = MyPrint.Console;
using Random = UnityEngine.Random;

public class LobbyManager : MonoBehaviour
{
  public static    LobbyManager   instance;
  [SerializeField] TMP_InputField lobbyCodeTextField;
  [SerializeField] TMP_InputField playerNameTextField;
  float                           heartBeatTimer;
  [SerializeField] float          _refreshUiTImer = 1f;


  Lobby           hostLobby;
  Lobby           joinedLobby;
  readonly string keyGameMode           = "GameMode";
  readonly string keyMap                = "Map";
  readonly string keyPlayerName         = "PlayerName";
  readonly string keyStartGameRelayCode = "StartGameRelayCode";
  float           lobbyUpdateTimer;
  string          playerName;

  private float elaspedTimeUpdate = 0;
  bool _gameStarting = false;
  float lobbyListTimer;
  
  public Action<List<Player>> OnUpdatePlayerList;
  public Action OnJoinLobby;
  public Action OnAllPlayerReady;
  public Action<List<Lobby>> OnLobbyListChanged;

  private void Awake()
  {
    instance = this;
  }

  async void Start()
  {
    //Sert a t'autentifier, tu t'en servira pour relier le compte steam
    // il faut installer le  Steamworks SDK
    //SignInWithSteamAsync


    await UnityServices.InitializeAsync();


    AuthenticationService.Instance.SignedIn += () => { Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId); };


    await AuthenticationService.Instance.SignInAnonymouslyAsync();


    playerName = "Player " + Random.Range(10, 99);
    Debug.Log("Player Name: " + playerName);
  }

  private void OnEnable()
  {
    elaspedTimeUpdate = _refreshUiTImer;
  }

  void Update()
  {
    HandleLobbyHeartBeat();
    HandleLobbyPollForUpdates();
    HandleUpdateUI();
    HandleLobbyListRefresh();
  }

  void HandleLobbyListRefresh()
  {
    if (joinedLobby != null) return;

    lobbyListTimer -= Time.deltaTime;

    if (lobbyListTimer <= 0f)
    {
      lobbyListTimer = 3f;
      ListLobbies();
    }
  }

  private void HandleUpdateUI()
  {
    if (elaspedTimeUpdate > 0)
    {
      elaspedTimeUpdate -= Time.deltaTime;

      if (elaspedTimeUpdate <= 0)
      {
        elaspedTimeUpdate = _refreshUiTImer;

        PrintPlayers();
          
        bool allReady = CheckAllPlayerReady();
        
        Debug.Log("=== READY CHECK ===");

        foreach (var p in joinedLobby.Players)
        {
          string ready = (p.Data != null && p.Data.ContainsKey("IsReady"))
            ? p.Data["IsReady"].Value
            : "NULL";

          Debug.Log(p.Id + " ready = " + ready);
        }

        if (allReady && !_gameStarting)
        {
          Console.Print("Start game...",  ColorConsole.Green);
    
          OnAllPlayerReady?.Invoke();
          _gameStarting = true; 
          StartGame();
        }
      }
    }
  }


  async void HandleLobbyHeartBeat()
  {
    if (hostLobby != null)
    {
      heartBeatTimer -= Time.deltaTime;


      if (heartBeatTimer < 0f)
      {
        float heartbeatTimerMax = 15;
        heartBeatTimer = heartbeatTimerMax;


        await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
      }
    }
  }


  async void HandleLobbyPollForUpdates()
{
    if (joinedLobby != null)
    {
        lobbyUpdateTimer -= Time.deltaTime;

        if (lobbyUpdateTimer < 0f)
        {
            float lobbyUpdateTimerMax = 3f;
            lobbyUpdateTimer = lobbyUpdateTimerMax;

            Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
            joinedLobby = lobby;

            OnUpdatePlayerList?.Invoke(joinedLobby.Players);

            if (joinedLobby.Data.ContainsKey(keyStartGameRelayCode) &&
                joinedLobby.Data[keyStartGameRelayCode].Value != "0")
            {
                if (!IsLobbyHost())
                {
                    string myPlayerId = AuthenticationService.Instance.PlayerId;
                    int mySkin = 0;
                    string myName = playerName;

                    Player me = joinedLobby.Players.Find(p => p.Id == myPlayerId);

                    if (me != null && me.Data != null)
                    {
                        if (me.Data.ContainsKey(keyPlayerName))
                        {
                            myName = me.Data[keyPlayerName].Value;
                        }

                        if (me.Data.ContainsKey("SkinId"))
                        {
                            int.TryParse(me.Data["SkinId"].Value, out mySkin);
                        }
                    }

                    PlayerLocalData localData = PlayerLocalData.Instance;
                    if (localData == null)
                    {
                        GameObject dataHolder = new GameObject("PlayerLocalData");
                        localData = dataHolder.AddComponent<PlayerLocalData>();
                    }

                    localData.SetPlayerData(mySkin, myName, joinedLobby.Players.Count);

                    Debug.Log($"[CLIENT] Saved local data before joining relay: Skin={mySkin}, Name={myName}");

                    RelayManager.instance.JoinRelay(joinedLobby.Data[keyStartGameRelayCode].Value);
                    Debug.Log("Joining Relay");
                }

                joinedLobby = null;
            }
        }
    }
}
  
  public async Task<Lobby> CreateLobby(string lobbyName, int logoIndex)  {
    try
    {
      int    maxPlayers = 12;


      CreateLobbyOptions createLobbyOptions = new()
      {
        IsPrivate = false,
        Player = GetPlayer(),

        Data = new Dictionary<string, DataObject>
        {
          {keyGameMode, new DataObject(DataObject.VisibilityOptions.Public, "CaptureTheFlag")},
          {keyMap, new DataObject(DataObject.VisibilityOptions.Public, "Dust1")},
          {keyStartGameRelayCode, new DataObject(DataObject.VisibilityOptions.Member, "0")},

          {"LobbyName", new DataObject(DataObject.VisibilityOptions.Public, lobbyName)},
          {"LobbyLogo", new DataObject(DataObject.VisibilityOptions.Public, logoIndex.ToString())},
          { "LobbyCode", new DataObject(DataObject.VisibilityOptions.Public, "0") }
        }
      };


      Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);


      hostLobby   = lobby;
      joinedLobby = hostLobby;
      
      Debug.Log("Lobby Created      Lobby Name: " + lobby.Name + "      Max Player: " + maxPlayers + "      Lobby Id: " + lobby.Id + "      LobbyCode: " + lobby.LobbyCode + "      Game Mode: " + lobby.Data[keyGameMode].Value);
      PrintPlayers(hostLobby);
      
      OnJoinLobby?.Invoke();

      return joinedLobby;
    }
    catch (LobbyServiceException e)
    {
      Debug.Log(e);
      throw;
    }
  }


  [ContextMenu("List Lobbies")]
  public void ListLobbiesButton()
  {
    ListLobbies();
  }


  async void ListLobbies()
  {
    try
    {
      QueryLobbiesOptions queryLobbiesOptions = new()
      {
        Count = 25,
        Filters = new List<QueryFilter>
        {
          new(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
        },
        Order = new List<QueryOrder>
        {
          new(false, QueryOrder.FieldOptions.Created)
        }
      };

      QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);

      Debug.Log("Lobbies found: " + queryResponse.Results.Count);

      OnLobbyListChanged?.Invoke(queryResponse.Results);
    }
    catch (LobbyServiceException e)
    {
      Debug.Log(e);
    }
  }
  
  
  public async void JoinLobbyById(string lobbyId)
  {
    try
    {
      JoinLobbyByIdOptions options = new()
      {
        Player = GetPlayer()
      };

      Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, options);
      joinedLobby = lobby;

      Debug.Log("Joined Lobby by Id: " + lobbyId);

      OnJoinLobby?.Invoke();
      PrintPlayers(lobby);
    }
    catch (LobbyServiceException e)
    {
      Debug.Log(e);
    }
  }
  
  public async void JoinLobbyByCode(string lobbyCode)
  {
    try
    {
      JoinLobbyByCodeOptions joinLobbyByCodeOptions = new()
      {
        Player = GetPlayer()
      };


      Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
      joinedLobby = lobby;


      Debug.Log("Joined Lobby with code: " + lobbyCode);

      OnJoinLobby?.Invoke();
      
      PrintPlayers(lobby);
    }
    catch (LobbyServiceException e)
    {
      Debug.Log(e);
    }
  }


  [ContextMenu("Quick Join Lobby")]
  //[Button("Quick Join Lobby")]
  public async void QuickJoinLobby()
  {
    try
    {
      QuickJoinLobbyOptions quickJoinLobbyOptions = new()
      {
        Player = GetPlayer()
      };


      Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);
      joinedLobby = lobby;
      
      Debug.Log("Quick Joined Lobby");

      OnJoinLobby?.Invoke();

      PrintPlayers(lobby);
    }
    catch (LobbyServiceException e)
    {
      Debug.Log(e);
    }
  }


  Player GetPlayer()
  {
    return new Player
    {
      Data = new Dictionary<string, PlayerDataObject>
      {
        {keyPlayerName, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName)},
        {"SkinId", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "0")},
        {"IsReady", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "0")}
      }
    };
  }


  [ContextMenu("Print Players")]
  public bool PrintPlayers()
  {
    return PrintPlayers(joinedLobby);
  }
  
  bool PrintPlayers(Lobby lobby)
  {
    if (lobby.Players.Count <= 0) return false;
    if (lobby.Players == null) return false;
    
    foreach (Player player in lobby.Players)
    {
      if (player == null) return false;
      
      Debug.Log("Payer Id: "       + player.Id +
            "   Player Name: " + player.Data[keyPlayerName].Value);
    }
    
    OnUpdatePlayerList?.Invoke(lobby.Players);
    
    return true;
  }

  public string GetPlayerName(int playerId)
  {
    return joinedLobby.Players[playerId].Data[keyPlayerName].Value;
  }


  //[Button("Update Lobby Game Mode To Hide And Seek")]
  public void UpdateLobbyGameModeToHideAndSeek()
  {
    UpdateLobbyGameMode("HideAndSeek");
  }


  async void UpdateLobbyGameMode(string gameMode)
  {
    try
    {
      hostLobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
      {
        Data = new Dictionary<string, DataObject>
        {
          {keyGameMode, new DataObject(DataObject.VisibilityOptions.Public, gameMode)}
        }
      });


      joinedLobby = hostLobby;


      PrintPlayers(hostLobby);
    }
    catch (LobbyServiceException e)
    {
      Debug.Log(e);
    }
  }


  [ContextMenu("Update Player Name")]
  public void UpdatePlayerNameButton()
  {
    UpdatePlayerName(playerNameTextField.text);
  }


  async void UpdatePlayerName(string newPlayerName)
  {
    try
    {
      playerName = newPlayerName;
      await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
      {
        Data = new Dictionary<string, PlayerDataObject>
        {
          {keyPlayerName, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, newPlayerName)}
        }
      });
    }
    catch (LobbyServiceException e)
    {
      Debug.Log(e);
    }
  }


  [ContextMenu("Leave Lobby")]
  public async void LeaveLobby()
  {
    try
    {
      await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);


      Debug.Log("Leave Lobby");
    }
    catch (LobbyServiceException e)
    {
      Debug.Log(e);
    }
  }


  [ContextMenu("Kick Player")]
  public async void KickPlayer()
  {
    try
    {
      await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, joinedLobby.Players[1].Id);


      Debug.Log("Kick Player");
    }
    catch (LobbyServiceException e)
    {
      Debug.Log(e);
    }
  }


  async void MigrateLobbyHost()
  {
    try
    {
      hostLobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
      {
        HostId = joinedLobby.Players[1].Id
      });


      joinedLobby = hostLobby;


      PrintPlayers(hostLobby);
    }
    catch (LobbyServiceException e)
    {
      Debug.Log(e);
    }
  }


  async void DeleteLobby()
  {
    try
    {
      await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
      Debug.Log("Delete Lobby");
    }
    catch (LobbyServiceException e)
    {
      Debug.Log(e);
    }
  }


[ContextMenu("Start Game")]
public async void StartGame()
{
    if (IsLobbyHost())
    {
        try
        {
            Debug.Log("Start Game");

            string myPlayerId = AuthenticationService.Instance.PlayerId;
            int mySkin = 0;
            string myName = playerName;
            
            Player me = joinedLobby.Players.Find(p => p.Id == AuthenticationService.Instance.PlayerId);
            
            if(me != null)
            {
              myName = me.Data[keyPlayerName].Value;
              mySkin = int.Parse(me.Data["SkinId"].Value);
            }

            PlayerLocalData localData = PlayerLocalData.Instance;
            localData.SetPlayerData(mySkin, myName, joinedLobby.Players.Count);

            string relayCode = await RelayManager.instance.CreateRelay();
            
            Lobby lobby = await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {keyStartGameRelayCode, new DataObject(DataObject.VisibilityOptions.Member, relayCode)}
                }
            });

            NetworkManager.Singleton.SceneManager.LoadScene("Karting", LoadSceneMode.Single);

            joinedLobby = lobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
}

  bool IsLobbyHost()
  {
    if (joinedLobby != null)
    {
      return joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    return false;
  }
  
  public async void SetSkin(int newSkinId)
  {
    try
    {
      await LobbyService.Instance.UpdatePlayerAsync(
        joinedLobby.Id,
        AuthenticationService.Instance.PlayerId,
        new UpdatePlayerOptions
        {
          Data = new Dictionary<string, PlayerDataObject>
          {
            {"SkinId", new PlayerDataObject(
              PlayerDataObject.VisibilityOptions.Member,
              newSkinId.ToString())}
          }
        });
      
      joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
      OnUpdatePlayerList?.Invoke(joinedLobby.Players);
      
      Debug.Log("New player skin : " +  newSkinId);
    }
    catch (LobbyServiceException e)
    {
      Debug.Log(e);
    }
  }
  
  public async void SetReady()
  {
    try
    {
      Player me = joinedLobby.Players.Find(p => p.Id == AuthenticationService.Instance.PlayerId);

      if (me == null)
      {
        Debug.LogError("Local player not found in lobby");
        return;
      }

      string current = "0";
      if (me.Data.ContainsKey("IsReady"))
        current = me.Data["IsReady"].Value;

      string newState = current == "0" ? "1" : "0";

      await LobbyService.Instance.UpdatePlayerAsync(
        joinedLobby.Id,
        AuthenticationService.Instance.PlayerId,
        new UpdatePlayerOptions
        {
          Data = new Dictionary<string, PlayerDataObject>
          {
            {
              "IsReady",
              new PlayerDataObject(
                PlayerDataObject.VisibilityOptions.Member,
                newState)
            }
          }
        });

      joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
      OnUpdatePlayerList?.Invoke(joinedLobby.Players);
      
      Debug.Log("Is Player Ready : " + newState);
    }
    catch (LobbyServiceException e)
    {
      Debug.Log(e);
    }
  }

  bool CheckAllPlayerReady()
  {
    foreach (var player in joinedLobby.Players)
    {
      if (player.Data == null ||
          !player.Data.ContainsKey("IsReady") ||
          player.Data["IsReady"].Value == "0")
      {
        return false;
      }
    }
    return true;
  }
}