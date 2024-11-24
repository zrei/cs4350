using UnityEngine;

public class MoralityManager : Singleton<MoralityManager>
{
    [SerializeField] MoralitySettingsSO m_MoralitySetting;
    [Tooltip("Percentage of morality to start from")]
    [Range(-1f, 1f)]
    [SerializeField] private float m_StartingMoralityPercentage = 0f;

    private int m_CurrMorality;
    public float CurrMoralityPercentage => (float) m_CurrMorality / m_MoralitySetting.m_MaxMorality;
    public int CurrMorality => m_CurrMorality;

    #region Initialisation
    protected override void HandleAwake()
    {
        base.HandleAwake();

        GlobalEvents.Morality.MoralityChangeEvent += ChangeMorality;
        GlobalEvents.Scene.OnBeginSceneChange += OnSceneChange;

        SaveManager.OnSaveEvent += SaveMorality;
        
        if (!TryLoadMoralitySave())
        {
            SetMorality(Mathf.FloorToInt(m_StartingMoralityPercentage * m_MoralitySetting.m_MaxMorality));
        }
    }

    protected override void HandleDestroy()
    {
        base.HandleDestroy();

        GlobalEvents.Morality.MoralityChangeEvent -= ChangeMorality;
        GlobalEvents.Scene.OnBeginSceneChange -= OnSceneChange;

        SaveManager.OnSaveEvent -= SaveMorality;
    }

    protected override void AddDependencies()
    {
        AddDependency<SaveManager>();
    }
    #endregion

    #region Level Result
    private void OnSceneChange(SceneEnum fromScene, SceneEnum toScene)
    {
        if (toScene != SceneEnum.WORLD_MAP)
            return;

        if (FlagManager.Instance.GetFlagValue(Flag.LOSE_LEVEL_FLAG) || FlagManager.Instance.GetFlagValue(Flag.QUIT_LEVEL_FLAG))
        {
            TryLoadMoralitySave();
        }
    }
    #endregion

    #region Save
    private bool TryLoadMoralitySave()
    {
        if (!SaveManager.Instance.TryLoadMorality(out int currMorality))
        {
            return false;
        }

        SetMorality(currMorality);
        return true;
    }

    private void SaveMorality(ISave save)
    {
        save.SaveMorality(m_CurrMorality);
    }
    #endregion

    private void SetMorality(int morality)
    {
        m_CurrMorality = morality;
        GlobalEvents.Morality.MoralitySetEvent?.Invoke(m_CurrMorality);
    }

    private void ChangeMorality(int changeAmount)
    {
        m_CurrMorality = Mathf.Clamp(m_CurrMorality + changeAmount, -m_MoralitySetting.m_MaxMorality, m_MoralitySetting.m_MaxMorality);
    }
    
    public int GetMoralityValue(float percentage) => Mathf.FloorToInt(percentage * m_MoralitySetting.m_MaxMorality);

#if UNITY_EDITOR
    public void SetStartingMorality(float startingMoralityPercentage)
    {
        m_StartingMoralityPercentage = startingMoralityPercentage;
    }
#endif
}
