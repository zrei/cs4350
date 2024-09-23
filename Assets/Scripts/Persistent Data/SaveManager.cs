using System.Collections.Generic;
using UnityEngine;

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
//
public class SaveManager : Singleton<SaveManager>
{
    private List<CharacterData> m_CurrData = new();

    protected override void HandleAwake()
    {
        base.HandleAwake();
    }

    protected override void HandleDestroy()
    {
        base.HandleDestroy();
    }
}