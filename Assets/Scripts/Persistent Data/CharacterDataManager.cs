using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class StartingPlayerCharacter
{
    public PlayerCharacterSO m_PlayerCharacter;
    public bool m_OverrideStartingLevel = false;
    public int m_OverriddenStartingLevel = 1;
}

public class CharacterDataManager : Singleton<CharacterDataManager>
{
    [Tooltip("Characters to start with")]
    [SerializeField] List<StartingPlayerCharacter> m_StartingCharacters;

    private readonly Dictionary<int, PlayerCharacterData> m_CharacterData = new();

    #region Initialisation
    protected override void HandleAwake()
    {
        base.HandleAwake();

        GlobalEvents.Morality.MoralitySetEvent += OnMoralitySet;
        GlobalEvents.Flags.SetFlagEvent += OnFlagSet;

        GlobalEvents.Level.LevelResultsEvent += OnLevelEnd;
        GlobalEvents.UI.SavePartyChangesEvent += SaveCharacterData;

        GlobalEvents.Scene.EarlyQuitEvent += OnEarlyQuit;
    
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

        if (!LevellingManager.IsReady)
        {
            LevellingManager.OnReady += HandleDependencies;
            return;
        }

        SaveManager.OnReady -= HandleDependencies;
        PersistentDataManager.OnReady -= HandleDependencies;
        LevellingManager.OnReady -= HandleDependencies;
        
        if (!TryLoadSaveData())
        {
            LoadStartingCharacters();
            SaveCharacterData();
        }
    }

    protected override void HandleDestroy()
    {
        base.HandleDestroy();

        GlobalEvents.Morality.MoralitySetEvent -= OnMoralitySet;
        GlobalEvents.Flags.SetFlagEvent -= OnFlagSet;

        GlobalEvents.Level.LevelResultsEvent -= OnLevelEnd;
        GlobalEvents.UI.SavePartyChangesEvent -= SaveCharacterData;

        GlobalEvents.Scene.EarlyQuitEvent -= OnEarlyQuit;
    }
    #endregion

    #region Level Result
    private void OnLevelEnd(LevelSO _, LevelResultType result)
    {
        HandleLevelResult(result == LevelResultType.SUCCESS);
    }

    private void OnEarlyQuit()
    {
        HandleLevelResult(false);
    }

    private void HandleLevelResult(bool save)
    {
        if (save)
        {
            SaveCharacterData();
        }
        else
        {
            TryLoadSaveData();
        }
    }
    #endregion

    #region Saving
    private bool TryLoadSaveData()
    {
        if (!SaveManager.Instance.TryLoadCharacterSaveData(out List<CharacterSaveData> characterSaveData))
        {
            return false;
        }
        ParseSaveData(characterSaveData); 
        return true;
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

            PlayerCharacterData persistentData = new(
                baseData: characterSO, 
                currClassIndex: 
                data.m_ClassIndex, 
                currExp: data.m_CurrExp,
                currLevel: data.m_CurrLevel, 
                currStats: data.m_CurrStats, 
                statProgress: data.m_CurrStatProgress, 
                currUnlockedClasses: ParseUnlockedClasses(data.m_UnlockedClasses, characterSO.NumClasses),
                currEquippedWeaponId: data.m_CurrEquippedWeaponId
            );
            m_CharacterData.Add(persistentData.Id, persistentData);
        }
    }

    private void LoadStartingCharacters()
    {
        m_CharacterData.Clear();

        ReceiveStartingCharacters(m_StartingCharacters);
    }

    private void SaveCharacterData()
    {
        SaveManager.Instance.SaveCharacterData(m_CharacterData.Values.Select(x => GetCharacterSaveData(x)));
    }

    private CharacterSaveData GetCharacterSaveData(PlayerCharacterData playerCharacterData)
    {
        return new CharacterSaveData(
            characterId: playerCharacterData.Id, 
            classIndex: playerCharacterData.m_CurrClassIndex, 
            currLevel: playerCharacterData.m_CurrLevel, 
            currExp: playerCharacterData.m_CurrExp, 
            currStats: playerCharacterData.m_CurrStats, 
            currStatProgress: playerCharacterData.m_CurrStatsProgress, 
            unlockedClasses: SerializeUnlockedClasses(playerCharacterData.m_CurrUnlockedClasses, playerCharacterData.NumClasses), 
            currEquippedWeaponId: playerCharacterData.m_CurrEquippedWeaponId
        );
    }
    #endregion

    #region Retrievers
    public List<PlayerCharacterData> RetrieveAllCharacterData(IEnumerable<int> additionalExcludedCharacterIds, bool excludeLord = false)
    {
        return m_CharacterData.Values.Where(x => !excludeLord || !x.IsLord).Where(x => !additionalExcludedCharacterIds.Contains(x.Id)).ToList();
    }

    public bool TryRetrieveCharacterData(int id, out PlayerCharacterData characterData)
    {
        return m_CharacterData.TryGetValue(id, out characterData);
    }

    public PlayerCharacterData RetrieveCharacterData(int id)
    {
        return m_CharacterData.Values.Where(x => x.Id == id).First();
    }

    public List<PlayerCharacterData> RetrieveCharacterData(IEnumerable<int> IDs, bool excludeLord = false)
    {
        return IDs.Select(x => RetrieveCharacterData(x)).Where(x => !excludeLord || !x.IsLord).ToList();        
    }

    /// <summary>
    /// Retrieves the character data belonging to the lord
    /// This makes the assumption that there is only one lord in the entire roster
    /// </summary>
    /// <returns></returns>
    public bool TryRetrieveLordCharacterData(out PlayerCharacterData lordData)
    {
        foreach (PlayerCharacterData data in m_CharacterData.Values)
        {
            if (data.IsLord)
            {
                lordData = data;
                return true;
            }
        }
        lordData = default;
        return false;
    }
    #endregion

    #region Edit
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
        
        playerCharacterSOs.ForEach(x => ReceiveCharacter(x, x.m_StartingLevel));
    }

    private void ReceiveStartingCharacters(List<StartingPlayerCharacter> startingPlayerCharacters)
    {
        startingPlayerCharacters.ForEach(x => ReceiveCharacter(x.m_PlayerCharacter, x.m_OverrideStartingLevel ? x.m_OverriddenStartingLevel : x.m_PlayerCharacter.m_StartingLevel));
    }

    private void ReceiveCharacter(PlayerCharacterSO playerCharacterSO, int startingLevel)
    {
        PlayerCharacterData persistentData = new(
            baseData: playerCharacterSO, 
            currClassIndex: playerCharacterSO.StartingClassIndex, 
            currExp: 0,
            currLevel: 1, 
            currStats: playerCharacterSO.m_StartingStats, 
            statProgress: new StatProgress(),
            currUnlockedClasses: playerCharacterSO.GetUnlockedClassIndexes(playerCharacterSO.m_StartingLevel)
        );
        LevellingManager.Instance.LevelCharacterToLevel(persistentData, startingLevel);
        m_CharacterData.Add(persistentData.Id, persistentData);
    }
    #endregion

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

#if UNITY_EDITOR
    public void SetStartingCharacters(List<StartingPlayerCharacter> startingPlayerCharacters)
    {
        m_StartingCharacters = startingPlayerCharacters;
    }
#endif
}