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

        PrefabUtility.RecordPrefabInstancePropertyModifications(this.gameObject);
    }

    public void SpawnUnit(CoordPair coordPair, UnitModelData unitModelData, WeaponInstanceSO weaponSO, ClassSO classSO)
    {
        GetTileSetupHelper(coordPair).SpawnUnit(unitModelData, weaponSO, classSO);

        PrefabUtility.RecordPrefabInstancePropertyModifications(this.gameObject);
    }

    public void ResetTileColor(CoordPair coordPair)
    {
        GetTileSetupHelper(coordPair).ResetTileColor();

        PrefabUtility.RecordPrefabInstancePropertyModifications(this.gameObject);
    }

    public void SetTileAsSetupColor(CoordPair coordPair)
    {
        GetTileSetupHelper(coordPair).SetTileAsSetupColor();

        PrefabUtility.RecordPrefabInstancePropertyModifications(this.gameObject);
    }

    public void SetupTileEffects(CoordPair coordPair, params GameObject[] tileEffects)
    {
        GetTileSetupHelper(coordPair).SetupTileEffects(tileEffects);

        PrefabUtility.RecordPrefabInstancePropertyModifications(this.gameObject);
    }

    public void ClearTileEffects(CoordPair coordPair)
    {
        GetTileSetupHelper(coordPair).ClearTileEffects();

        PrefabUtility.RecordPrefabInstancePropertyModifications(this.gameObject);
    }

    public void ToggleTileSelected(CoordPair coordPair, bool isSelected)
    {
        GetTileSetupHelper(coordPair).ToggleSelected(isSelected);

        PrefabUtility.RecordPrefabInstancePropertyModifications(this.gameObject);
    }
    #endif
}