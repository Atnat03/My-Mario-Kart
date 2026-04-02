using UnityEngine;

public class PlaySound : MonoBehaviour
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField] MenuManager _menuManager;
	
	#endregion


	#region Fonctions

	public void PlayStartSound() => _menuManager.PlayStartSound();

	#endregion
}
