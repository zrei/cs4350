using System.Collections.Generic;
using UnityEngine;

public interface IConcreteAction
{
    /// <summary>
    /// Check if the current node is completed
    /// </summary>
    /// <returns></returns>
    public abstract bool IsCompleted(EnemyUnit enemyUnit, MapLogic mapLogic);

    /// <summary>
    /// Goes directly to running, will not do any further checks.
    /// Will progress internal state
    /// </summary>
    /// <param name="enemyUnit"></param>
    /// <param name="mapLogic"></param>
    /// <param name="completeActionEvent"></param>
    public abstract void Run(EnemyUnit enemyUnit, MapLogic mapLogic, BoolEvent completeActionEvent);
    
    /// <summary>
    /// Checks whether the parent should stop performing this child
    /// </summary>
    /// <param name="enemyUnit"></param>
    /// <param name="mapLogic"></param>
    /// <returns></returns>
    public abstract bool ShouldBreakOut(EnemyUnit enemyUnit, MapLogic mapLogic);

    /// <summary>
    /// Reset the state
    /// </summary>
    public abstract void Reset();

    /// <summary>
    /// Gets nested active skills to help with 
    /// </summary>
    /// <returns></returns>
    public abstract HashSet<ActiveSkillSO> GetNestedActiveSkills();

    /// <summary>
    /// Gets the action to be performed for the next turn, without updating the internal state.
    /// Caches values for when the action should actually be performed
    /// </summary>
    /// <param name="enemyUnit"></param>
    /// <param name="mapLogic"></param>
    /// <returns></returns>
    public abstract EnemyActionWrapper GetActionToBePerformed(EnemyUnit enemyUnit, MapLogic mapLogic);
}

// has to be set to an object so it's serialisable
public abstract class EnemyAction : MonoBehaviour
{
    public abstract IConcreteAction GenerateConcreteAction();
}
