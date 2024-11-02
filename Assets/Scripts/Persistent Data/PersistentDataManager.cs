using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PersistentDataManager : Singleton<PersistentDataManager>
{
    [SerializeField] private List<PlayerCharacterSO> m_CharacterSOs;
    [SerializeField] private List<WeaponInstanceSO> m_WeaponInstanceSOs;

    private readonly Dictionary<int, PlayerCharacterSO> m_CharacterSOsMap = new();
    private readonly Dictionary<int, WeaponInstanceSO> m_WeaponInstanceSOsMap = new();

    protected override void HandleAwake()
    {
        base.HandleAwake();

        m_CharacterSOs.ForEach(x => m_CharacterSOsMap.Add(x.m_Id, x));
        m_WeaponInstanceSOs.ForEach(x => m_WeaponInstanceSOsMap.Add(x.m_WeaponId, x));
    }

    public bool TryGetPlayerCharacterSO(int characterId, out PlayerCharacterSO characterSO)
    {
        return TryGetSO<PlayerCharacterSO>(characterId, m_CharacterSOsMap, out characterSO);
    }

    public bool TryGetWeaponInstanceSO(int weaponInstanceId, out WeaponInstanceSO weaponInstanceSO)
    {
        return TryGetSO<WeaponInstanceSO>(weaponInstanceId, m_WeaponInstanceSOsMap, out weaponInstanceSO);
    }
    
    private bool TryGetSO<T>(int id, Dictionary<int, T> map, out T soInstance) where T : ScriptableObject
    {
        if (map.ContainsKey(id))
        {
            soInstance = map[id];
            return true;
        }
        else
        {
            soInstance = null;
            return false;
        }
    }

#if UNITY_EDITOR
    [Header("Weapon Helper")]
    [Tooltip("Whether to limit the search to a separate root folder instead of searching through the entire Assets folder")]
    public bool m_OverrideWeaponRootFolder = true;
    [Tooltip("The root folder to limit the search to - Assets is the top-level folder for the project")]
    public string m_OverriddenWeaponRootFolder = "Assets/Persistent Data";

    [Header("Character Helper")]
    [Tooltip("Whether to limit the search to a separate root folder instead of searching through the entire Assets folder")]
    public bool m_OverrideCharacterRootFolder = true;
    [Tooltip("The root folder to limit the search to - Assets is the top-level folder for the project")]
    public string m_OverriddenCharacterRootFolder = "Assets/Persistent Data";

    public void FillWeapons()
    {
        m_WeaponInstanceSOs = GetAllSOs<WeaponInstanceSO>(m_OverrideWeaponRootFolder, m_OverriddenWeaponRootFolder);
        EditorUtility.SetDirty(this.gameObject);
    }

    public void FillCharacters()
    {
        m_CharacterSOs = GetAllSOs<PlayerCharacterSO>(m_OverrideCharacterRootFolder, m_OverriddenCharacterRootFolder);
        EditorUtility.SetDirty(this.gameObject);
    }

    /// <summary>
    /// Get all SOs of type T in the provided root folder, filtering out null entries
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private List<T> GetAllSOs<T>(bool overrideRootFolder, params string[] overriddenRootFolder) where T : ScriptableObject
    {
        List<T> SOs = new();

        foreach (string instancePath in AssetHelpers.FindAssetPathsByType<T>(overrideRootFolder, overriddenRootFolder))
        {
            T instanceSO = AssetDatabase.LoadAssetAtPath<T>(instancePath);
            
            if (instanceSO == null)
                continue;

            SOs.Add(instanceSO);
        }

        return SOs;
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(PersistentDataManager))]
public class PersistentDataManagerHelper : Editor
{
    private PersistentDataManager m_Target;

    private void OnEnable()
    {
        m_Target = (PersistentDataManager) target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10);

        if (GUILayout.Button("Fill weapons"))
        {
            m_Target.FillWeapons();
        }

        if (GUILayout.Button("Fill characters"))
        {
            m_Target.FillCharacters();
        }
    } 
}
#endif
