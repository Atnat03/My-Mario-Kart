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
	[SerializeField] private GameObject _title;
	[SerializeField] private GameObject _buttons;
	[SerializeField] private GameObject _inputUI;

	#endregion

	#region Fonctions

	private void Awake()
	{
		_title.SetActive(false);
		_buttons.SetActive(false);
		_inputUI.SetActive(false);
	}

	public void Start()
	{
		StartCoroutine(WaitBfrSetActiveButton());
	}

	IEnumerator WaitBfrSetActiveButton()
	{
		yield return new WaitForSecondsRealtime(0.5f);
		
		_title.SetActive(true);
		SoundManager.PlaySound(_soundData,"Start",_audioSource);
		
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
	
	public void SetInputUI(bool value) => _inputUI.SetActive(value);
	
	public void Quit() => Application.Quit();

	#endregion
}
