using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// For easy access and constant flags
/// Can be used for game state
/// </summary>
public enum Flag
{
    WIN_LEVEL_FLAG,
    LOSE_LEVEL_FLAG,
    QUIT_LEVEL_FLAG
}

public enum FlagType
{
    PERSISTENT,
    SESSION
}

public class FlagManager : Singleton<FlagManager>
{
    private struct FlagWrapper 
    {
        public bool m_Value;
        public FlagType m_FlagType;

        public FlagWrapper(FlagType flagType, bool value = false)
        {
            m_FlagType = flagType;
            m_Value = value;
        }
    }

    private readonly Dictionary<string, FlagWrapper> m_Flags = new();

    [Tooltip("Persistent flags that start off as true")]
    [SerializeField] private List<Flag> m_StartingPersistentFlags;

    #region Initialisation
    protected override void HandleAwake()
    {
        base.HandleAwake();

        HandleDependencies();

        GlobalEvents.Level.LevelResultsEvent += OnLevelResult;
        GlobalEvents.Scene.EarlyQuitEvent += OnEarlyQuit;
        GlobalEvents.WorldMap.OnEndPreCutsceneEvent += OnEndPreCutscene;
    }

    protected override void HandleDestroy()
    {
        base.HandleDestroy();

        GlobalEvents.Level.LevelResultsEvent -= OnLevelResult;
        GlobalEvents.Scene.EarlyQuitEvent -= OnEarlyQuit;
        GlobalEvents.WorldMap.OnEndPreCutsceneEvent -= OnEndPreCutscene;
    }

    private void HandleDependencies()
    {
        if (!SaveManager.IsReady)
        {
            SaveManager.OnReady += HandleDependencies;
            return;
        }

        SaveManager.OnReady -= HandleDependencies;

        LoadPersistentFlags();
    }
    #endregion

    #region Level Result
    private void OnLevelResult(LevelSO _, LevelResultType levelResultType)
    {
        HandleLevelResult(levelResultType == LevelResultType.SUCCESS);
    }

    private void OnEndPreCutscene()
    {
        HandleLevelResult(true);
    }

    private void OnEarlyQuit()
    {
        HandleLevelResult(false);
    }

    private void HandleLevelResult(bool save)
    {
        if (save)
        {
            SavePersistentFlags();
        }
        else
        {
            ClearPersistentFlags();
            TryLoadSavedPersistentFlags();
        }
    }
    #endregion

    #region Save
    private void LoadPersistentFlags()
    {
        ClearPersistentFlags();

        if (!TryLoadSavedPersistentFlags())
        {
            LoadStartingFlags();
            SavePersistentFlags();
        }
    }

    private bool TryLoadSavedPersistentFlags()
    {
        if (!SaveManager.Instance.TryLoadPersistentFlags(out List<string> persistentFlags))
        {
            return false;
        }
        
        foreach (string flag in persistentFlags)
        {
            m_Flags[flag] = new() {m_Value = true, m_FlagType = FlagType.PERSISTENT};
        }
        return true;
    }

    private void LoadStartingFlags()
    {
        foreach (Flag flag in m_StartingPersistentFlags)
        {
            m_Flags[flag.ToString()] = new() {m_Value = true, m_FlagType = FlagType.PERSISTENT};
        }
    }

    private void SavePersistentFlags()
    {
        List<string> flagsToSave = new();
        foreach (KeyValuePair<string, FlagWrapper> flag in m_Flags)
        {
            if (flag.Value.m_FlagType == FlagType.PERSISTENT && flag.Value.m_Value)
            {
                flagsToSave.Add(flag.Key);
            }
        }
        SaveManager.Instance.SavePersistentFlags(flagsToSave);
    } 
    #endregion

    public void SetFlagValue(string flag, bool value, FlagType flagType)
    {
        m_Flags[flag] = new(flagType: flagType, value: value);
        GlobalEvents.Flags.SetFlagEvent?.Invoke(flag, value, flagType);
    }

    public void SetFlagValue(Flag flag, bool value, FlagType flagType)
    {
        SetFlagValue(flag.ToString(), value, flagType);
    }

    public bool GetFlagValue(string flag)
    {
        if (!m_Flags.TryGetValue(flag, out FlagWrapper flagValue))
        {
            return false;
        }
        return flagValue.m_Value;
    }

    public bool GetFlagValue(Flag flag)
    {
        return GetFlagValue(flag.ToString());
    }

    #region Helper
    private void ClearPersistentFlags()
    {
        List<string> flags = m_Flags.Keys.ToList();
        foreach (string flag in flags)
        {
            if (m_Flags[flag].m_FlagType == FlagType.PERSISTENT)
            {
                m_Flags[flag] = new FlagWrapper(FlagType.PERSISTENT, false);
            }
        }
    }
    #endregion

#if UNITY_EDITOR
    public void SetStartingFlags(List<Flag> persistentFlags)
    {
        m_StartingPersistentFlags = persistentFlags;
    }
#endif
}
