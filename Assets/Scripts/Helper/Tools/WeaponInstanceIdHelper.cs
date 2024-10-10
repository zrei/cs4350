using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponInstanceIdHelper", menuName = "ScriptableObject/IdHelpers/WeaponInstanceIdHelper")]
public class WeaponInstanceIdHelper : IdHelper
{
    public override void CheckForDuplicateIds()
    {
        Dictionary<int, List<string>> idMap = new();
        foreach (string weaponInstancePath in FindAssetPathsByType<WeaponInstanceSO>())
        {
            WeaponInstanceSO weaponInstanceSO = AssetDatabase.LoadAssetAtPath<WeaponInstanceSO>(weaponInstancePath);
            
            if (weaponInstanceSO == null)
                continue;

            if (!idMap.ContainsKey(weaponInstanceSO.m_WeaponId))
                idMap[weaponInstanceSO.m_WeaponId] = new();
            idMap[weaponInstanceSO.m_WeaponId].Add(weaponInstancePath);
        }

        foreach (KeyValuePair<int, List<string>> keyValuePair in idMap)
        {
            if (keyValuePair.Value.Count <= 1)
                continue;
            
            StringBuilder stringBuilder = new("\nWeaponInstanceSOs at paths:\n\n");
            foreach (string s in keyValuePair.Value)
            {
                stringBuilder.Append(s + "\n");
            }
            stringBuilder.Append($"\nhave the same id {keyValuePair.Key}\n");
            Logger.Log(this.GetType().Name, stringBuilder.ToString(), LogLevel.WARNING);
        }
    }

    public override void RenumberIds()
    {
        int id = 0;

        foreach (string weaponInstancePath in FindAssetPathsByType<WeaponInstanceSO>())
        {
            WeaponInstanceSO weaponInstanceSO = AssetDatabase.LoadAssetAtPath<WeaponInstanceSO>(weaponInstancePath);
            
            if (weaponInstanceSO == null)
                continue;

            weaponInstanceSO.m_WeaponId = id;
            Logger.Log(this.GetType().Name, $"Set weapon instance SO {weaponInstanceSO.name} at location {weaponInstancePath} id to {id}", LogLevel.LOG);
            EditorUtility.SetDirty(weaponInstanceSO);
            ++id;
        }
    }
}
