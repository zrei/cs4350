using UnityEngine;
using System.Collections.Generic;

public class VFXAudioPlayer : MonoBehaviour
{
    [SerializeField] List<VFXAudio> m_AudioToPlay;

    private void Awake()
    {
        foreach (VFXAudio vFXAudio in m_AudioToPlay)
        {
            SoundManager.Instance.Play(vFXAudio.m_AudioDataSO, vFXAudio.m_AudioDelay);
        }
    }
}
