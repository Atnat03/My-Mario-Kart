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
	
	public static void PlaySound(AudioClip clip, AudioSource source, float volume = 0.5f, float pitch = 1f)
	{
		source.pitch = pitch;
		source.volume = volume;
		AudioSource.PlayClipAtPoint(clip, source.transform.position, source.volume);
	}
}