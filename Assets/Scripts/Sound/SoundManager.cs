using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum AudioChannel
{
    UI,
    BGM,
    SFX
}

public enum AudioState
{
    PLAYING,
    PAUSED,
    FADING_IN,
    FADING_OUT
}

public class PlayingAudio
{
    public AudioDataSO m_AudioDataSO;
    public AudioSource m_AudioSourceInstance;
    public AudioState m_AudioState;
    public float m_Volume;

    public AudioChannel AudioChannel => m_AudioDataSO.m_AudioChannel;

    public PlayingAudio(AudioDataSO audioDataSO, AudioSource audioSourceInstance, float volume, AudioState audioState = AudioState.PLAYING)
    {
        m_AudioDataSO = audioDataSO;
        m_AudioSourceInstance = audioSourceInstance;
        m_AudioState = audioState;
        m_Volume = volume;
    }

    public bool HasFinishedPlaying()
    {
        return Application.isFocused && m_AudioState == AudioState.PLAYING && !m_AudioSourceInstance.isPlaying;
    }
}

public class SoundManager : Singleton<SoundManager>
{
    [SerializeField] private AudioSource m_AudioSource;
    [SerializeField] private Transform m_AudioSourceParent;
    [SerializeField] private AudioSettingsSO m_InitialAudioSettings;

    private readonly Dictionary<int, PlayingAudio> m_PlayingAudio = new();

    private int m_TokenNum = 0;

    private const float OVERALL_VOLUME = 1.0f;
    private const float TRANSITION_DURATION = 2f;

    private readonly Dictionary<AudioChannel, float> m_AudioVolumes = new();
    private readonly List<AudioChannel> CHANNELS_TO_UPDATE = new() {AudioChannel.SFX};

    #region Initialisation
    protected override void HandleAwake()
    {
        base.HandleAwake();
        InitVolumes();

        transform.SetParent(null);
        DontDestroyOnLoad(this.gameObject);

        GlobalEvents.MainMenu.OnReturnToMainMenu += OnReturnToMainMenu;
    }

    private void InitVolumes()
    {
        m_AudioVolumes.Clear();

        foreach (AudioChannel audioChannel in Enum.GetValues(typeof(AudioChannel)))
        {
            m_AudioVolumes[audioChannel] = m_InitialAudioSettings.GetVolumeLevel(audioChannel);
        }
    }

    protected override void HandleDestroy()
    {
        base.HandleDestroy();

        GlobalEvents.MainMenu.OnReturnToMainMenu -= OnReturnToMainMenu;
    }
    #endregion

    #region Update
    private void Update()
    {
        IEnumerable<int> keys = m_PlayingAudio.Keys.ToList();
        foreach (int id in keys)
        {
            if (!m_PlayingAudio[id].HasFinishedPlaying())
                continue;
            
            Stop(id);
        }
    }
    #endregion

    #region Fade
    public void FadeOutAudio(int id, float duration, VoidEvent postFade = null)
    {
        if (!m_PlayingAudio.ContainsKey(id))
            return;
        m_PlayingAudio[id].m_AudioState = AudioState.FADING_OUT;
        StartCoroutine(FadeAudio(m_PlayingAudio[id], duration, 0f, postFade));
    }

    public void FadeInAudio(int id, float duration, VoidEvent postFade = null)
    {
        if (!m_PlayingAudio.ContainsKey(id))
            return;

        m_PlayingAudio[id].m_AudioState = AudioState.FADING_IN;
        StartCoroutine(FadeAudio(m_PlayingAudio[id], duration, m_PlayingAudio[id].m_Volume, PostFade));

        void PostFade()
        {
            m_PlayingAudio[id].m_AudioState = AudioState.PLAYING;
            postFade?.Invoke();
        }
    }

    private IEnumerator FadeAudio(PlayingAudio playingAudio, float duration, float finalVolume, VoidEvent postFade = null)
    {
        float t = 0f;
        float initialVolume = playingAudio.m_AudioSourceInstance.volume;
        while (t < duration)
        {
            yield return null;
            t += Time.deltaTime;
            playingAudio.m_AudioSourceInstance.volume = Mathf.Lerp(initialVolume, finalVolume, t / duration);

        }
        playingAudio.m_AudioSourceInstance.volume = finalVolume;
        postFade?.Invoke();
    }
    #endregion

    #region Channels
    public void ToggleAudioChannel(AudioChannel channel, bool play)
    {
        foreach (PlayingAudio playingAudio in m_PlayingAudio.Values)
        {
            if (playingAudio.AudioChannel != channel)
                continue;

            if (play && playingAudio.m_AudioState != AudioState.PLAYING)
            {
                playingAudio.m_AudioSourceInstance.UnPause();
            }
            else if (!play && playingAudio.m_AudioState != AudioState.PAUSED)
            {
                playingAudio.m_AudioSourceInstance.Pause();
            }
        }
    }
    #endregion

    #region Controls
    public void Pause(int id)
    {
        if (!m_PlayingAudio.ContainsKey(id))
            return;

        m_PlayingAudio[id].m_AudioSourceInstance.Pause();
        m_PlayingAudio[id].m_AudioState = AudioState.PAUSED;
    }

    public void FadeOutAndStop(int id, float duration = TRANSITION_DURATION, VoidEvent postFade = null)
    {
        FadeOutAudio(id, duration, PostFade);

        void PostFade()
        {
            Stop(id);
            postFade?.Invoke();
        }
    }

    public void Stop(int id)
    {
        if (!m_PlayingAudio.ContainsKey(id))
            return;
        
        m_PlayingAudio[id].m_AudioSourceInstance.Stop();
        Destroy(m_PlayingAudio[id].m_AudioSourceInstance.gameObject);
        m_PlayingAudio.Remove(id);
    }

    public int Play(AudioDataSO audioDataSO, float volumeModifier = 1f, float delay = 0f, Transform parent = null)
    {
        (AudioSource audioSource, float volume) = GetAudioSourceAndVolume(audioDataSO, volumeModifier, parent);
        audioSource.volume = volume;
        audioSource.PlayDelayed(delay);
        int cachedTokenNum = m_TokenNum;
        ++m_TokenNum;
        m_PlayingAudio[cachedTokenNum] = new PlayingAudio(audioDataSO, audioSource, volume);

        return cachedTokenNum;
    }

    public int PlayWithFadeIn(AudioDataSO audioDataSO, float volumeModifier = 1f, float fadeInDuration = TRANSITION_DURATION, Transform parent = null)
    {
        (AudioSource audioSource, float volume) = GetAudioSourceAndVolume(audioDataSO, volumeModifier, parent);
        audioSource.volume = 0f;
        audioSource.Play();
        int cachedTokenNum = m_TokenNum;
        ++m_TokenNum;
        m_PlayingAudio[cachedTokenNum] = new PlayingAudio(audioDataSO, audioSource, volume, AudioState.FADING_IN);
        FadeInAudio(cachedTokenNum, fadeInDuration);
        return cachedTokenNum;
    }

    private (AudioSource, float) GetAudioSourceAndVolume(AudioDataSO audioDataSO, float volumeModifier = 1f, Transform parent = null)
    {
        AudioSource audioSource = Instantiate(m_AudioSource, parent ?? m_AudioSourceParent);
        audioSource.playOnAwake = false;
        audioSource.loop = audioDataSO.m_Loop;
        audioSource.clip = audioDataSO.m_AudioClip;
        float volume = m_AudioVolumes[audioDataSO.m_AudioChannel] * audioDataSO.m_Volume * OVERALL_VOLUME * volumeModifier;
        return (audioSource, volume);
    }
    #endregion

    #region Event Callbacks
    private void OnReturnToMainMenu()
    {
        // fade out current bgm
        foreach (KeyValuePair<int, PlayingAudio> keyValuePair in m_PlayingAudio)
        {
            if (keyValuePair.Value.AudioChannel == AudioChannel.BGM)
            {
                FadeOutAndStop(keyValuePair.Key);
                return;
            }
        }
    }
    #endregion
}
