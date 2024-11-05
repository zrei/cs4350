using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioSettingsSO", menuName = "ScriptableObject/Audio/AudioSettingsSO")]
public class AudioSettingsSO : ScriptableObject
{
    [System.Serializable]
    public struct AudioChannelVolume
    {
        public AudioChannel m_AudioChannel;
        public float m_VolumeLevel;
    }

    [Tooltip("Enter an entry to override the default base volume level")]
    public List<AudioChannelVolume> m_VolumeLevels;

    public float GetVolumeLevel(AudioChannel audioChannel)
    {
        foreach (AudioChannelVolume audioChannelVolume in m_VolumeLevels)
        {
            if (audioChannelVolume.m_AudioChannel == audioChannel)
                return audioChannelVolume.m_VolumeLevel;
        }

        return 1.0f;
    }
}