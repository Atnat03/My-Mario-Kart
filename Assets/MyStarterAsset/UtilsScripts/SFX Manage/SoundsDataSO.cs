using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjectsDefinitions
{
    [CreateAssetMenu(fileName = "Sound", menuName = "Sound/new Sound Data")]
    public class SoundsDataSO : ScriptableObject
    {
        public List<SoundData> sounds = new List<SoundData>();
    }

    [Serializable]
    public class SoundData
    {
        public string soundName;
        public AudioClip audioClip;
        [Range(0, 1)]public float volume = 0.5f;
    }
}