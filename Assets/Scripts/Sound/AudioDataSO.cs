using UnityEngine;

[CreateAssetMenu(fileName = "AudioDataSO", menuName = "ScriptableObject/Audio/AudioDataSO")]
public class AudioDataSO : ScriptableObject
{
    public AudioChannel m_AudioChannel;
    public AudioClip m_AudioClip;
    [Tooltip("Expressed as a ratio of the audio channel volume")]
    public float m_Volume = 1.0f;
    public bool m_Loop = false;
}
