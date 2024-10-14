using UnityEngine;

public static class Flags
{

}

public class FlagManager : Singleton<FlagManager>
{
    private Dictionary<string, bool> m_SessionFlags;
    private Dictionary<string, bool> m_PersistentFlags;

    protected override void HandleAwake()
    {
        base.HandleAwake();
    }

    protected override void HandleDestroy()
    {
        base.HandleDestroy();
    }

    private void HandleDependencies()
    {
        if (!SaveManager.IsReady)
        {
            SaveManager.OnReady += HandleDependencies;
            return;
        }

        SaveManager.OnReady -= HandleDependencies;


    }

    public void SetFlagValue(string flag, bool value, bool isPersisent)
    {
        
    }
}
