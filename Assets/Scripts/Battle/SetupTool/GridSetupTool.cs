using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(GridLogic))]
public class GridSetupHelper : MonoBehaviour
{
    #if UNITY_EDITOR
    private void Reset()
    {
        foreach (TileVisual tileVisual in GetComponentsInChildren<TileVisual>())
        {
            tileVisual.gameObject.AddComponent<TileSetupHelper>();
        }
        PrefabUtility.RecordPrefabInstancePropertyModifications(this.gameObject);
    }

    private TileSetupHelper GetTileSetupHelper(CoordPair coordPair)
    {
        return transform.GetChild(coordPair.m_Row).GetChild(coordPair.m_Col).GetComponent<TileSetupHelper>();
    }

    public void ClearUnit(CoordPair coordPair)
    {
        GetTileSetupHelper(coordPair).ClearUnit();
    }

    public void SpawnUnit(CoordPair coordPair, UnitModelData unitModelData, WeaponInstanceSO weaponSO, ClassSO classSO)
    {
        GetTileSetupHelper(coordPair).SpawnUnit(unitModelData, weaponSO, classSO);
    }

    public void ResetTileColor(CoordPair coordPair)
    {
        GetTileSetupHelper(coordPair).ResetTileColor();
    }

    public void SetTileAsSetupColor(CoordPair coordPair)
    {
        GetTileSetupHelper(coordPair).SetTileAsSetupColor();
    }

    public void SetupTileEffects(CoordPair coordPair, params GameObject[] tileEffects)
    {
        GetTileSetupHelper(coordPair).SetupTileEffects(tileEffects);
    }

    public void ClearTileEffects(CoordPair coordPair)
    {
        GetTileSetupHelper(coordPair).ClearTileEffects();
    }
    #endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(GridSetupHelper))]
public class GridSetupHelperEditor : Editor
{

}
#endif