using System;
using MyPrint;
using ScriptableObjectsDefinitions;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Console = MyPrint.Console;

public class PlayerSound : MonoBehaviour
{
	#region Variables

	[SerializeField] private AudioSource _audioSource;
	[SerializeField] private SoundsDataSO _soundData;
	
	[Header("References")]
	[SerializeField] private KartController _kartController;
	[SerializeField] private PlayerHealth _playerHealth;
	[SerializeField] private PlayerItem _playerItem;
	
	[Header("Sound Kart Engine")]
	[SerializeField] private AudioSource _engineKartSound;
	
	#endregion

	private void OnEnable()
	{
		GameManager.instance.OnStartGame += () => _engineKartSound.Play();
		GameManager.instance.OnEndGame += () => _engineKartSound.Stop();
		
		_kartController.OnMove += ApplyEngineSound;
		_kartController.OnDriftStart += StartDriftSound;
		_kartController.OnDriftEnd += EndDriftSound;
		_kartController.OnBoost += BoostSound;
		_kartController.OnStunning += StunSound;

		_playerHealth.OnTakeDamage += PopBalloonSound;
		_playerHealth.OnRespawn += RespawnSound;
		_playerHealth.OnExplosed += ExplosedSound;

		_playerItem.OnRollItem += RollItemSound;
		_playerItem.OnDropItem += DropItemSound;
	}

	
	#region Fonctions
	
	private void DropItemSound()
	{
		SoundManager.PlaySound(_soundData, "Throw", _audioSource);
	}

	private void RollItemSound()
	{
		SoundManager.PlaySoundAtPoint(_soundData, "RollItem", _audioSource);
	}

	private void ExplosedSound()
	{
		SoundManager.PlaySoundAtPoint(_soundData, "Loose", _audioSource);
	}

	private void RespawnSound()
	{
		SoundManager.PlaySoundAtPoint(_soundData, "Respawn", _audioSource);
	}

	private void PopBalloonSound()
	{
		SoundManager.PlaySoundAtPoint(_soundData, "TakeDamage", _audioSource);
	}

	private void StunSound()
	{
		SoundManager.PlaySoundAtPoint(_soundData, "Loose", _audioSource);
	}

	private void BoostSound()
	{
		SoundManager.PlaySound(_soundData, "Boost", _audioSource);
	}

	private void EndDriftSound()
	{
		_audioSource.Stop();
		_audioSource.loop = false;
	}

	private void StartDriftSound()
	{
		_audioSource.loop = true;
		_audioSource.clip = SoundManager.GetAudioClip(_soundData, "Drift");
		_audioSource.volume = 0.1f;
		_audioSource.Play();
	}
	
	private void ApplyEngineSound(float velocity)
	{
		_engineKartSound.pitch = Mathf.Lerp(0.8f, 1.2f, velocity / 16f);
	}

	#endregion
}
