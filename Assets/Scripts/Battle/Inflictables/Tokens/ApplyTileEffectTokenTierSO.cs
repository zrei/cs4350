using UnityEngine;

/// <summary>
/// Applies a tile effect. The amount of time it lasts (for non-permanent tile effects) is obtained from the tier
/// </summary>
[CreateAssetMenu(fileName = "ApplyTileEffectTokenTierSO", menuName = "ScriptableObject/Inflictables/Token/ApplyTileEffectTokenTierSO")]
public class ApplyTileEffectTokenTierSO : TargetOtherTilesTokenTierSO
{
    public override TokenType TokenType => TokenType.SPAWN_TILE_EFFECT;
    public override bool RequiresTargetedSquares => false;

    [Header("Tile Effect")]
    public TileEffectSO m_InflictedTileEffect;

    public InflictedTileEffect GetInflictedTileEffect(int tier)
    {
        if (TryRetrieveTier(tier, out TokenSO tokenSO))
        {
            return new InflictedTileEffect {m_TileEffect = m_InflictedTileEffect, m_InitialTime = tier};
        }
        return default;
    }
}
