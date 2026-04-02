using System;
using System.Collections;
using UnityEngine;

public class MusiqueManager : MonoBehaviour
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField] private GameManager _gameManager;
	
	[SerializeField] AudioSource _audioSource;
	[SerializeField] AudioSource _audioSourceSFX;
	
	[Header("Musique")]
	[SerializeField] private AudioClip _musiqueCinematique;
	[SerializeField] private AudioClip _musiqueCountDown;
	[SerializeField] private AudioClip _musiqueGame;
	[SerializeField] private AudioClip _musiqueEndGame;
	[SerializeField] private AudioClip _musiqueEndGameUI;

	[Header("Settings")] 
	[SerializeField] private float fadeOutDuration = 1;
	
	private float _startVolume;
	
	#endregion


	#region Fonctions

	private void Start()
	{
		_startVolume = _audioSource.volume;
	}

	private void EndGameUIMusic()
	{
		SetNewMusique(_musiqueEndGameUI);
	}

	private void EndGameMusic()
	{
		SetNewMusique(_musiqueEndGame);
	}

	private void CountDownMusic()
	{
		_audioSource.Stop();
		_audioSourceSFX.PlayOneShot(_musiqueCountDown);
	}

	private void CinematiqueMusic()
	{
		SetNewMusique(_musiqueCinematique);

		StartCoroutine(FadeOutMusique(5.5f));
	}

	private void StartGame()
	{
		SetNewMusique(_musiqueGame);
	}
	
	IEnumerator FadeOutMusique(float waitBefore = 0)
	{
		yield return new WaitForSeconds(waitBefore);
		
		float elapsedTime = 0;

		while (elapsedTime < fadeOutDuration)
		{
			elapsedTime += Time.deltaTime;

			_audioSource.volume = Mathf.Lerp(_audioSource.volume, 0,elapsedTime / fadeOutDuration);
			
			yield return null;
		}
	}

	void SetNewMusique(AudioClip audioClip)
	{
		_audioSource.volume = _startVolume;
		_audioSource.clip = audioClip;
		_audioSource.Play();
	}
	
	private void OnEnable()
	{
		_gameManager.OnStartCinematique += CinematiqueMusic;
		_gameManager.OnStartCountDown += CountDownMusic;
		_gameManager.OnEndGame += EndGameMusic;
		_gameManager.OnEndGameUI += EndGameUIMusic;
		_gameManager.OnStartGame += StartGame;
	}


	private void OnDisable()
	{
		_gameManager.OnStartCinematique -= CinematiqueMusic;
		_gameManager.OnStartCountDown = CountDownMusic;
		_gameManager.OnEndGame -= EndGameMusic;
		_gameManager.OnEndGameUI -= EndGameUIMusic;
		_gameManager.OnStartGame -= StartGame;
	}

	#endregion
}
