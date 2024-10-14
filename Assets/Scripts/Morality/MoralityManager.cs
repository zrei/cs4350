using UnityEngine;

public class MoralityManager : Singleton<MoralityManager>
{
    [SerializeField] MoralitySettingsSO m_MoralitySetting;

    private int m_CurrMorality;
    public float CurrMoralityPercentage => (float) m_CurrMorality / m_MoralitySetting.m_MaxMorality;
    public int CurrMorality => m_CurrMorality;

    protected override void HandleAwake()
    {
        base.HandleAwake();

        GlobalEvents.Morality.MoralityChangeEvent += ChangeMorality;

        HandleDependencies();
    }

    protected override void HandleDestroy()
    {
        base.HandleDestroy();

        GlobalEvents.Morality.MoralityChangeEvent -= ChangeMorality;
    }

    private void HandleDependencies()
    {        
        if (!SaveManager.IsReady)
        {
            SaveManager.OnReady += HandleDependencies;
            return;
        }

        SaveManager.OnReady -= HandleDependencies;

        if (SaveManager.Instance.TryLoadMorality(out int currMorality))
        {
            m_CurrMorality = currMorality;
        }
        else
        {
            m_CurrMorality = Mathf.FloorToInt(m_MoralitySetting.m_StartingMoralityPercentage * m_MoralitySetting.m_MaxMorality);
        }
    }

    private void ChangeMorality(int changeAmount)
    {
        m_CurrMorality = Mathf.Clamp(m_CurrMorality + changeAmount, -m_MoralitySetting.m_MaxMorality, m_MoralitySetting.m_MaxMorality);
    }
}
