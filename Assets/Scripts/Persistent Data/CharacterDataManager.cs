using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterDataManager : Singleton<CharacterDataManager>
{
    [SerializeField] List<PlayerCharacterSO> m_StartingCharacters;

    // mapping character IDs to their data?
    private readonly Dictionary<int, PlayerCharacterData> m_CharacterData = new();

    protected override void HandleAwake()
    {
        base.HandleAwake();

        GlobalEvents.Morality.MoralitySetEvent += OnMoralitySet;
        GlobalEvents.Flags.SetFlagEvent += OnFlagSet;
    
        HandleDependencies();
    }

    private void HandleDependencies()
    {
        if (!SaveManager.IsReady)
        {
            SaveManager.OnReady += HandleDependencies;
            return;
        }

        if (!PersistentDataManager.IsReady)
        {
            PersistentDataManager.OnReady += HandleDependencies;
            return;
        }

        SaveManager.OnReady -= HandleDependencies;
        PersistentDataManager.OnReady -= HandleDependencies;
        
        if (SaveManager.Instance.TryLoadCharacterSaveData(out List<CharacterSaveData> characterSaveData))
        {
            ParseSaveData(characterSaveData); 
        }
        else
        {
            LoadStartingCharacters();
        }
        
    }

    protected override void HandleDestroy()
    {
        base.HandleDestroy();

        GlobalEvents.Morality.MoralitySetEvent -= OnMoralitySet;
        GlobalEvents.Flags.SetFlagEvent -= OnFlagSet;
    }

    private void ParseSaveData(List<CharacterSaveData> characterSaveData)
    {
        m_CharacterData.Clear();

        foreach (CharacterSaveData data in characterSaveData)
        {
            if (!PersistentDataManager.Instance.TryGetPlayerCharacterSO(data.m_CharacterId, out PlayerCharacterSO characterSO))
            {
                Logger.Log(this.GetType().Name, $"Character data for {data.m_CharacterId} cannot be found", LogLevel.ERROR);
                continue;
            }

            PlayerCharacterData persistentData = new() {m_BaseData = characterSO, m_CurrClassIndex = data.m_ClassIndex, m_CurrExp = data.m_CurrExp,
                m_CurrLevel = data.m_CurrLevel, m_CurrStats = data.m_CurrStats, m_CurrStatsProgress = data.m_CurrStatProgress};
            m_CharacterData.Add(persistentData.Id, persistentData);
        }
    }

    private void LoadStartingCharacters()
    {
        m_CharacterData.Clear();

        ReceiveCharacters(m_StartingCharacters);
    }

    public List<PlayerCharacterData> RetrieveAllCharacterData()
    {
        return m_CharacterData.Values.ToList();
    }

    public List<PlayerCharacterData> RetrieveCharacterData(List<int> IDs)
    {
        return m_CharacterData.Values.Where(x => IDs.Contains(x.Id)).ToList();
    }

    /// <summary>
    /// Update the persistent data with the newly updated data from a finished level
    /// </summary>
    /// <param name="updatedData"></param>
    public void UpdateCharacterData(List<PlayerCharacterData> updatedData)
    {
        foreach (PlayerCharacterData data in updatedData)
        {
            m_CharacterData[data.Id] = data;
        }
    }
    
    /// <summary>
    /// Add a list of characters to the roster
    /// </summary>
    /// <param name="playerCharacterSOs"></param>
    public void ReceiveCharacters(List<PlayerCharacterSO> playerCharacterSOs)
    {
        foreach (var entry in m_CharacterData)
        {
            Debug.Log($"Current: Name: {entry.Value.m_BaseData.m_CharacterName}, Id: {entry.Key}");
        }
        foreach (var entry in playerCharacterSOs)
        {
            Debug.Log($"To Add: Name: {entry.m_CharacterName}, Id: {entry.m_Id}");
        }
        
        playerCharacterSOs.ForEach(x => ReceiveCharacter(x));
    }

    private void ReceiveCharacter(PlayerCharacterSO playerCharacterSO)
    {
        PlayerCharacterData persistentData = new() {m_BaseData = playerCharacterSO, m_CurrClassIndex = playerCharacterSO.StartingClassIndex, m_CurrExp = 0,
                m_CurrLevel = playerCharacterSO.m_StartingLevel, m_CurrStats = playerCharacterSO.m_StartingStats, m_CurrStatsProgress = new StatProgress(),
                m_CurrUnlockedClasses = playerCharacterSO.GetUnlockedClassIndexes(playerCharacterSO.m_StartingLevel)};
        m_CharacterData.Add(persistentData.Id, persistentData);
    }

    #region Helper
    /// <summary>
    /// Helper to parse the save data for unlocked classes
    /// </summary>
    /// <param name="unlockedClass"></param>
    /// <param name="numClasses"></param>
    /// <returns></returns>
    private List<bool> ParseUnlockedClasses(int unlockedClass, int numClasses)
    {
        int baseNum = 2;
        List<bool> unlockedClasses = new();

        for (int i = 0; i < numClasses; ++i)
        {
            unlockedClasses.Add((unlockedClass & (int) Mathf.Pow(baseNum, i)) > 0);
        }
        return unlockedClasses;
    }

    /// <summary>
    /// Helper to serialize the save data for unlocked classes
    /// </summary>
    /// <param name="unlockedClasses"></param>
    /// <param name="numClasses"></param>
    /// <returns></returns>
    private int SerializeUnlockedClasses(List<bool> unlockedClasses, int numClasses)
    {
        int serialized = 0;
        int baseNum = 2;
        for (int i = 0; i < numClasses; ++i)
        {
            if (unlockedClasses[i])
                serialized |= (int) Mathf.Pow(baseNum, i);
        }
        return serialized;
    }
    #endregion

    #region Class Unlock Events
    private void OnMoralitySet(int _)
    {
        CheckAllCharacterClassUnlocks();
    }

    private void OnFlagSet(string flag, bool value, FlagType flagType)
    {
        CheckAllCharacterClassUnlocks();
    }

    private void CheckAllCharacterClassUnlocks()
    {
        foreach (PlayerCharacterData characterData in m_CharacterData.Values)
        {
            characterData.CheckClassUnlocks();
        }
    }
    #endregion
}