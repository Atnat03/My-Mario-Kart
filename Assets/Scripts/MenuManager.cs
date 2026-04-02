using System;
using System.Collections;
using ScriptableObjectsDefinitions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
	#region Variables

	[Header("Sound")]
	[SerializeField] private AudioSource _audioSource;
	[SerializeField] private SoundsDataSO _soundData;
	
	[Header("UI")]
	[SerializeField] private GameObject _buttons;

	#endregion


	#region Fonctions

	private void Awake()
	{
		_buttons.SetActive(false);
	}

	public void PlayStartSound()
	{
		SoundManager.PlaySound(_soundData,"Start",_audioSource);
		StartCoroutine(WaitBfrSetActiveButton());
	}

	IEnumerator WaitBfrSetActiveButton()
	{
		yield return new WaitForSeconds(2f);
		
		_buttons.SetActive(true);
	}
	
	public void PlaySound(string SoundName)
	{
		SoundManager.PlaySound(_soundData,SoundName,_audioSource);
	}

	public void Play()
	{
		SceneManager.LoadScene(1);
	}
	
	public void Quit() => Application.Quit();

	#endregion
}
