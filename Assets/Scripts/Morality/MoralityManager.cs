using UnityEngine;

public class MoralityManager : Singleton<MoralityManager>
{
    [SerializeField] MoralitySettings m_MoralitySetting;

    private int m_CurrMorality;
    public float CurrMoralityPercentage => (float) m_CurrMorality / m_MoralitySetting.m_MaxMorality;
    public int CurrMorality => m_CurrMorality;

    protected override void HandleAwake()
    {
        base.HandleAwake();

        HandleDependencies();
    }

    protected override void HandleDestroy()
    {
        base.HandleDestroy();
    }

    private void HandleDependencies()
    {
        // TODO: Change this back later
        m_CurrMorality = m_MoralitySetting.m_StartingMorality;
        /*
        if (!SaveManager.IsReady)
        {
            SaveManager.OnReady += HandleDependencies;
            return;
        }

        SaveManager.OnReady -= HandleDependencies;

        m_CurrMorality = SaveManager.Instance.LoadMorality();
        */
    }

    public void ChangeMorality(int changeAmount)
    {
        m_CurrMorality = Mathf.Clamp(m_CurrMorality + changeAmount, 0, m_MoralitySetting.m_MaxMorality);
    }
}
