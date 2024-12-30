using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ConvertDealtDamageTypeStatusEffectSO", menuName = "ScriptableObject/Inflictables/StatusEffect/ConvertDealtDamageTypeStatusEffectSO")]
public class ConvertDealtDamageTypeStatusEffectSO : StatusEffectSO
{
    public override StatusEffectType StatusEffectType => StatusEffectType.CONVERT_DEALT_DAMAGE_TYPE;
}
