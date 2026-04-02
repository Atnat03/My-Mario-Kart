using System;
using ScriptableObjectsDefinitions;
using UnityEngine;

public class ButtonClickSound : MonoBehaviour
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField] AudioSource _audioSource;
	[SerializeField] private SoundsDataSO _data;
	[SerializeField] private string _name = "";
	
	#endregion


	#region Fonctions

	public void PlayClickSound()
	{
		Debug.Log("Play Click Sound");
		
		if (_audioSource != null)
		{
			if (_name.Equals(""))
				name = "Click";
			
			SoundManager.PlaySound(_data, _name, _audioSource);
		}
	}
	
	#endregion
}
