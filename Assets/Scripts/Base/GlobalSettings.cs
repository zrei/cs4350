using System.Collections.Generic;
using UnityEngine;

public class GlobalSettings : Singleton<GlobalSettings>
{
    [Header("Debug")]
    [SerializeField] private bool m_DoDebug = false;
    public static bool DoDebug => Instance.m_DoDebug;

    [Header("Demo")]
    [SerializeField] private bool m_IsDemo = true;
    public static bool IsDemo => Instance.m_IsDemo;
    [SerializeField] private int m_FinalDemoLevel = 2;
    public static int FinalDemoLevel => Instance.m_FinalDemoLevel;
 
    [Header("Movement Config")]
    [SerializeField] private bool m_AllowCrossingOverOccupiedSquares = false;
    public static bool AllowCrossingOverOccupiedSquares => Instance.m_AllowCrossingOverOccupiedSquares;

    [Header("Skill Range Display Settings")]
    [SerializeField] private Sprite m_DefaultCasterPosSprite;
    [SerializeField] private Sprite m_DefaultTargetPosSprite;

    public Sprite DefaultCasterPosSprite => m_DefaultCasterPosSprite;
    public Sprite DefaultTargetPosSprite => m_DefaultTargetPosSprite;

    // lower value is higher priority
    public static readonly Dictionary<System.Type, int> SkillTargetPosSpritePriority = new()
    {
        { typeof(LockToSelfTargetRuleSO), 0 },
        { typeof(TargetRowLimitRuleSO), 1 },
        { typeof(TargetColLimitRuleSO), 1 },
        { typeof(TargetSameSideRuleSO), 2 },
        { typeof(TargetOpposingSideRuleSO), 2 },
        // no sprites for these at the moment
        //{ typeof(TargetNotSelfRuleSO), 99 },
        //{ typeof(TargetWithinRangeOfAttackerRuleSO), 99 },
    };

    // lower value is higher priority
    public static readonly Dictionary<System.Type, int> SkillCasterPosSpritePriority = new()
    {
        { typeof(AttackerRowLimitRuleSO), 1 },
        { typeof(AttackerColLimitRuleSO), 1 },
    };

    protected override void HandleAwake()
    {
        base.HandleAwake();

        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    protected override void HandleDestroy()
    {
        base.HandleDestroy();

        GlobalEvents.ClearEvents();
    }
}