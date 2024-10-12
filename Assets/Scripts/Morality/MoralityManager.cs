using UnityEngine;

public class MoralityManager : Singleton<MoralityManager>
{
    [SerializeField] MoralitySettings m_MoralitySetting;

    private int m_CurrMorality;

    protected override void HandleAwake()
    {
        base.HandleAwake();
    }

    protected override void HandleDestroy()
    {
        base.HandleDestroy();

        HandleDependencies();
    }

    private void HandleDependencies()
    {
        if (!SaveManager.IsReady)
        {
            SaveManager.OnReady += HandleDependencies;
            return;
        }

        SaveManager.OnReady -= HandleDependencies;

        m_CurrMorality = SaveManager.Instance.LoadMorality();
    }

    public void ChangeMorality(int changeAmount)
    {
        m_CurrMorality = Mathf.Clamp(m_CurrMorality + changeAmount, 0, m_MoralitySetting.m_MaxMorality);
    }
}
