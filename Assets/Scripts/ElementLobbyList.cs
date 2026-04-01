using System;
using MyPrint;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Console = MyPrint.Console;

public class ElementLobbyList : MonoBehaviour
{
	#region Properties

	#endregion

	#region Variables

	[SerializeField] private TextMeshProUGUI _lobbyNameText;
	[SerializeField] private Image _lobbyLogo;
	[SerializeField] private Button _joinButton;
	private string lobbyId;
	
	#endregion


	#region Fonctions

	public void CreateNewLobby(string lobbyName, Sprite lobbyLogo, string idLobby)
	{
		_lobbyNameText.text = lobbyName;
		_lobbyLogo.sprite = lobbyLogo;
		lobbyId = idLobby;
		
		_joinButton.onClick.RemoveAllListeners();
		_joinButton.onClick.AddListener(JoinLobby);
	}

	public void JoinLobby()
	{
		Console.Print("Lobby joined", ConsoleStyle.Bold);
		LobbyManager.instance.JoinLobbyById(lobbyId);
	}
	
	#endregion
}
