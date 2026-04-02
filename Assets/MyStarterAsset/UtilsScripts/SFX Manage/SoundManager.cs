using ScriptableObjectsDefinitions;
using UnityEngine;

public class SoundManager
{
	public static AudioClip GetAudioClip(SoundsDataSO data, string soundName)
	{
		AudioClip clip = null;

		foreach (SoundData soundData in data.sounds)
		{
			if(soundData.soundName == soundName)
			{
				clip = soundData.audioClip;
			}
		}
		
		return clip;
	}
	
	public static void PlaySound(AudioSource source, AudioClip clip, float volume = 0.5f, float pitch = 1f)
	{
		source.pitch = pitch;
		source.volume = volume;
		AudioSource.PlayClipAtPoint(clip, source.transform.position, source.volume);
	}

	public static void PlaySound(SoundsDataSO data, string name, AudioSource source)
	{
		AudioClip clip = null;
		float volume = 0.5f;

		foreach (SoundData soundData in data.sounds)
		{
			if(soundData.soundName == name)
			{
				clip = soundData.audioClip;
				volume = soundData.volume;
			}
		}
		
		source.pitch = 1f;
		source.volume = volume;
		source.PlayOneShot(clip);
	}
	
	public static void PlaySoundAtPoint(SoundsDataSO data, string name, AudioSource source)
	{
		AudioClip clip = null;
		float volume = 0.5f;

		foreach (SoundData soundData in data.sounds)
		{
			if(soundData.soundName == name)
			{
				clip = soundData.audioClip;
				volume = soundData.volume;
			}
		}
		
		source.pitch = 1f;
		source.volume = volume;
		AudioSource.PlayClipAtPoint(clip, source.transform.position, source.volume);
	}
}