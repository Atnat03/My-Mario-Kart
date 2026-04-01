using System;
using System.Collections.Generic;
using MyPrint;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using Console = MyPrint.Console;

[Serializable]
public struct PlayerData
{
	public string Id;
	public string Name;
	public int SkinId;
	public bool IsReady;
}

public class LobbyUI : MonoBehaviour
{
	#region Properties
	public bool IsLobbyCanva {get; private set;}
	
	#endregion


	#region Variables

	[SerializeField] private LobbyManager _lobby;
	
	[SerializeField] private GameObject _createLobbyCanva;
	[SerializeField] private GameObject _lobbyCanva;
	[SerializeField] private GameObject _startingScreen;
	[SerializeField] private GameObject _selectionPerso;
	
	[Header("LobbyList")]
	[SerializeField] private Transform _lobbyListParent;
	[SerializeField] private ElementLobbyList _lobbyListElementUI;
	private List<ElementLobbyList> _currentLobbies = new();
	
	[Header("Lobby Creation")]
	[SerializeField] private GameObject _creationLobbyUI;
	[SerializeField] private TMP_Dropdown _logoDropDown;
	[SerializeField] private TMP_InputField _nameNewLobby;
	[SerializeField] private Transform _infoMissingParent;
	[SerializeField] private GameObject _infoMissing;
	[SerializeField] private Sprite[] _lobbyLogoSprite;
	
	[Header("PlayerList")]
	[SerializeField] private Transform _playerListParent;
	[SerializeField] private PlayerIconLobby _playerPrefab;
	[SerializeField] private List<PlayerData> _playerList;
	[SerializeField] private List<PlayerIconLobby> _playerUIList;
	
	[Header("PlayerName")]
	[SerializeField] private TMP_InputField _playerNameInputField;
	
	[Header("ReadyButton")]
	[SerializeField] private Button _readyButton;
	[SerializeField] private Color[] _readyButtonColors;
	[SerializeField] private string[] _readyButtonTexts;
	
	[Header("3d model")]
	[SerializeField] private UpdateModelPerso _modelPerso;
	
	#endregion
	
	public void Start()
	{
		IsLobbyCanva = false;
		
		_selectionPerso.SetActive(false);
		
		_createLobbyCanva.SetActive(true);
		_lobbyCanva.SetActive(false);
		_startingScreen.SetActive(false);
		
		_playerList = new List<PlayerData>();
		_playerUIList = new List<PlayerIconLobby>();

		CloseLobbyCreation();
	}

	#region LobbyMenu

	public void OpenLobbyCreation() => _creationLobbyUI.gameObject.SetActive(true);
	public void CloseLobbyCreation() => _creationLobbyUI.gameObject.SetActive(false);


	public async void CreateNewLobby()
	{
		try
		{
			if (_nameNewLobby.text.Equals(""))
			{
				Instantiate(_infoMissing, _infoMissingParent);
				return;
			}

			string lobbyName = _nameNewLobby.text;
			int logoIndex = _logoDropDown.value;

			await _lobby.CreateLobby(lobbyName, logoIndex);

			CloseLobbyCreation();
		}
		catch (Exception e)
		{
			Debug.Log(e);
		}
	}
	
	private void UpdateLobbyListUI(List<Lobby> lobbies)
	{
		foreach (ElementLobbyList lobby in _currentLobbies)
		{
			Destroy(lobby.gameObject);
		}
		_currentLobbies.Clear();

		foreach (Lobby lobby in lobbies)
		{
			ElementLobbyList element = Instantiate(_lobbyListElementUI, _lobbyListParent);

			string lobbyName = lobby.Name;
			int logoIndex = 0;

			if (lobby.Data.ContainsKey("LobbyName"))
			{
				lobbyName = lobby.Data["LobbyName"].Value;
			}

			if (lobby.Data.ContainsKey("LobbyLogo"))
			{
				int.TryParse(lobby.Data["LobbyLogo"].Value, out logoIndex);
			}

			Sprite logo = _lobbyLogoSprite[logoIndex];
			
			Console.Print("Lobby created id : " + lobby.Id, ColorConsole.Orange);
			
			element.CreateNewLobby(
				lobbyName,
				logo,
				lobby.Id
			);

			_currentLobbies.Add(element);
		}
	}
	

	#endregion
	

	#region InLobby
	
	private void LobbyIsJoined()
	{
		IsLobbyCanva = true;
		
		_selectionPerso.SetActive(true);
		
		_createLobbyCanva.SetActive(false);
		_lobbyCanva.SetActive(true);
	}

	private void UpdateUI(List<Player> list)
	{
		if (list.Count > _playerList.Count)
		{
			AddPlayer(list.Count - _playerList.Count);
		}

		if (list.Count < _playerList.Count)
		{
			for (int i = _playerList.Count - 1; i >= list.Count; i--)
			{
				Destroy(_playerUIList[i].gameObject);
				_playerUIList.RemoveAt(i);
				_playerList.RemoveAt(i);
			}
		}

		for (int i = 0; i < list.Count; i++)
		{
			Player player = list[i];

			string nom = "Unknown";

			if (player.Data.TryGetValue("PlayerName", out PlayerDataObject valueName))
				nom = valueName.Value;
			
			int skin = 0;
			
			if(player.Data.TryGetValue("SkinId", out PlayerDataObject valueSkin))
				int.TryParse(valueSkin.Value, out skin);
			
			bool isReady = false;
			
			if(player.Data.TryGetValue("IsReady", out PlayerDataObject valueReady))
				isReady = valueReady.Value == "1";
			
			if (player.Id == AuthenticationService.Instance.PlayerId)
			{
				SetReady(isReady);
			}
			
			if (_playerList[i].Name != nom || _playerList[i].SkinId != skin || _playerList[i].IsReady != isReady)
			{
				_playerList[i] = new PlayerData
				{
					Id = player.Id,
					Name = nom,
					SkinId = skin,
					IsReady = isReady
				};

				_playerUIList[i].ChangeLogo(skin);
				_playerUIList[i].IsReady(isReady);
			}
		}
	}

	private void AddPlayer(int delta)
	{
		for (int i = 0; i < delta; i++)
		{
			PlayerIconLobby ui = Instantiate(_playerPrefab, _playerListParent);

			_playerUIList.Add(ui);

			_playerList.Add(new PlayerData
			{
				Id = "0",
				Name = "",
				SkinId = 0
			});
		}
	}
	
	private void SetReady(bool state)
	{
		Color c = state ? _readyButtonColors[1] : _readyButtonColors[0];
		string s = state ? _readyButtonTexts[1] : _readyButtonTexts[0];
		
		_readyButton.GetComponent<Image>().color = c;
		_readyButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = s;
	}

	void SetStartingScreen()
	{
		_startingScreen.SetActive(true);
	}
	
	#endregion

	private void OnEnable()
	{
		_lobby.OnUpdatePlayerList += UpdateUI;
		_lobby.OnJoinLobby += LobbyIsJoined;
		_lobby.OnAllPlayerReady += SetStartingScreen;

		_lobby.OnLobbyListChanged += UpdateLobbyListUI;
	}
	
	private void OnDisable()
	{
		_lobby.OnUpdatePlayerList -= UpdateUI;
		_lobby.OnJoinLobby -= LobbyIsJoined;
		_lobby.OnAllPlayerReady -= SetStartingScreen;
		
		_lobby.OnLobbyListChanged -= UpdateLobbyListUI;
	}
}
