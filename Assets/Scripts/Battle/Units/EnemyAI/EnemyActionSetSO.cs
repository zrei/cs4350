using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyActionSetSO", menuName = "ScriptableObject/Battle/EnemyAI/EnemyActionSetSO")]
public class EnemyActionSetSO : ScriptableObject
{
    public List<EnemyAction> m_EnemyActions;

    public EnemyActionSO GetChosenAction(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        List<(EnemyActionSO, float)> finalWeights = new();
        foreach (EnemyAction action in m_EnemyActions)
        {
            if (action.CanActionBePerformed(enemyUnit, mapLogic))
            {
                float finalWeight = action.GetFinalWeight(enemyUnit, mapLogic);
                if (finalWeight > 0)
                    finalWeights.Add((action.m_EnemyAction, finalWeight));
            }
        }

        return RandomHelper.GetRandomT(finalWeights);
    }

    public void PerformAction(EnemyUnit enemyUnit, MapLogic mapLogic, VoidEvent completeActionEvent)
    {
        EnemyActionSO action = GetChosenAction(enemyUnit, mapLogic);
        Logger.Log(this.GetType().Name, $"{name} chose action: {action.name}", LogLevel.LOG);
        action.PerformAction(enemyUnit, mapLogic, completeActionEvent);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        float sum = 0;
        m_EnemyActions.ForEach(x => sum += x.m_InitialWeight);

        if (sum != 1f)
            Logger.LogEditor(this.GetType().Name, $"Weights for enemy action set {name} do not add up to 1", LogLevel.WARNING);
    }
#endif
}

public static class RandomHelper
{
    public static T GetRandomT<T>(List<(T, float weight)> values)
    {
        float sumOfWeights = 0f;
        values.ForEach(x => sumOfWeights += x.weight);
        float randomVal = Random.Range(0f, sumOfWeights);
        Logger.Log("RandomHelper", $"Random value: {randomVal}, totalSum: {sumOfWeights}", LogLevel.LOG);

        float lowerBound = 0f;
        float upperBound = 0f;
        foreach ((T val, float weight) in values)
        {
            upperBound += weight;
            Logger.Log("RandomHelper", $"LowerBound: {lowerBound}, UpperBound: {upperBound}", LogLevel.LOG);
            if (randomVal >= lowerBound && randomVal < upperBound)
            {
                return val;
            }
            lowerBound += weight;
        }

        return values.Last().Item1;
    }
}
