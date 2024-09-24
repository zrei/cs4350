using System.Collections.Generic;
using UnityEngine;

public struct CharacterSaveData
{
    public int m_CharacterId;
    public int m_ClassId;
    public int m_CurrLevel;
    public int m_CurrExp;
    public Stats m_CurrStats; // write serialiser for this :<
}
/*
// need a serialiser hrm
public struct CharacterData
{
    public CharacterSO m_BaseData;
    public ClassSO m_CurrClass;
    public int m_CurrLevel;
    public int m_CurrExp;
    // current base stats accounting for all levelling but not classes
    public Stats m_CurrStats;
}
*/
/// <summary>
/// Saves to JSON only
/// </summary>
public class SaveManager : Singleton<SaveManager>
{

    protected override void HandleAwake()
    {
        base.HandleAwake();
    }

    protected override void HandleDestroy()
    {
        base.HandleDestroy();
    }
}