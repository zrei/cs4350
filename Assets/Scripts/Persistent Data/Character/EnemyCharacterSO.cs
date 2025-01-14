﻿using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyCharacterSO", menuName = "ScriptableObject/Battle/Enemy/EnemyCharacterSO")]
public class EnemyCharacterSO : CharacterSO
{
    [Header("Enemy-only details")]
    public EnemyClassSO m_EnemyClass;
    [Tooltip("Stats")]
    public Stats m_Stats;
    public WeaponInstanceSO m_EquippedWeapon;

    public UnitModelData GetUnitModelData()
    {
        return m_Race.GetUnitModelData(m_Gender, m_SkinColor, m_EyeColor, m_EnemyClass.m_OutfitType);
    }

    public List<InflictedToken> GetInflictedTokens(int maxLevel)
    {
        List<InflictedToken> inflictedTokens = new();
        inflictedTokens.AddRange(m_EnemyClass.GetInflictedTokens(maxLevel));
        inflictedTokens.AddRange(m_EquippedWeapon.GetInflictedTokens(maxLevel));
        return inflictedTokens;
    }

    public BehaviourTree EnemyActionSetSO => m_EnemyClass.m_EnemyBehaviourTree;
}
