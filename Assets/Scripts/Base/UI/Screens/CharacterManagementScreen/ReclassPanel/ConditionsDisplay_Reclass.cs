using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class ConditionsDisplay_Reclass : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI m_ConditionText;

        public void SetDisplay(PathClass pathClass)
        {
            if (!pathClass.m_UnlockCondition.HasConditions())
            {
                m_ConditionText.text = "NONE";
            }
            else
            {
                m_ConditionText.text = pathClass.m_UnlockCondition.GetDescription();
            }
        }
    }
}
