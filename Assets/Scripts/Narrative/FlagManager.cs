using System.Collections.Generic;

/// <summary>
/// For easy access and constant flags
/// Can be used for game state
/// </summary>
public static class Flags
{

}

public enum FlagType
{
    PERSISTENT,
    SESSION
}

public class FlagManager : Singleton<FlagManager>
{
    private struct Flag 
    {
        public bool m_Value;
        public FlagType m_FlagType;
    }

    private readonly Dictionary<string, Flag> m_Flags = new();

    protected override void HandleAwake()
    {
        base.HandleAwake();

        GlobalEvents.Flags.SetFlagEvent += SetFlagValue;

        HandleDependencies();
    }

    protected override void HandleDestroy()
    {
        base.HandleDestroy();

        GlobalEvents.Flags.SetFlagEvent -= SetFlagValue;
    }

    private void HandleDependencies()
    {
        if (!SaveManager.IsReady)
        {
            SaveManager.OnReady += HandleDependencies;
            return;
        }

        SaveManager.OnReady -= HandleDependencies;

        LoadFlags();
    }

    private void LoadFlags()
    {
        m_Flags.Clear();

        if (SaveManager.Instance.TryLoadPersistentFlags(out List<string> persistentFlags))
        {
            foreach (string flag in persistentFlags)
            {
                m_Flags[flag] = new() {m_Value = true, m_FlagType = FlagType.PERSISTENT};
            }
        }
    }

    public void SavePersistentFlags()
    {
        List<string> flagsToSave = new();
        foreach (KeyValuePair<string, Flag> flag in m_Flags)
        {
            if (flag.Value.m_FlagType == FlagType.PERSISTENT && flag.Value.m_Value)
            {
                flagsToSave.Add(flag.Key);
            }
        }
        SaveManager.Instance.SavePersistentFlags(flagsToSave);
    } 

    private void SetFlagValue(string flag, bool value, FlagType flagType)
    {
        m_Flags[flag] = new() {m_Value = value, m_FlagType = flagType};
    }

    public bool GetFlagValue(string flag)
    {
        if (!m_Flags.TryGetValue(flag, out Flag flagValue))
        {
            return false;
        }
        return flagValue.m_Value;
    }
}
