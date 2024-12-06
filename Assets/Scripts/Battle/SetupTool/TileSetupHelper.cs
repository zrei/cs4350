using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(TileVisual))]
public class TileSetupHelper : MonoBehaviour
{
    [SerializeField] private TileVisual m_TileVisual;
    [SerializeField] private ArmorVisual m_ArmorVisual;
    [HideInInspector]
    [SerializeField] private bool m_IsSelected = false;

    #if UNITY_EDITOR
    private void Reset()
    {
        m_TileVisual = GetComponent<TileVisual>();
        GameObject unitParent = Instantiate(new GameObject(), this.transform);
        m_ArmorVisual = unitParent.AddComponent<ArmorVisual>();
        unitParent.transform.localScale = Vector3.one;
        unitParent.transform.localPosition = Vector3.zero;
        unitParent.transform.localRotation = Quaternion.identity;
        PrefabUtility.RecordPrefabInstancePropertyModifications(this.gameObject);
    }

    public void ClearUnit()
    {
        foreach (Transform child in m_ArmorVisual.transform)
        {
            DestroyImmediate(child.gameObject);
        }
        PrefabUtility.RecordPrefabInstancePropertyModifications(this.gameObject);
    }

    public void SpawnUnit(UnitModelData unitModelData, WeaponInstanceSO weaponSO, ClassSO classSO)
    {
        ClearUnit();
        m_ArmorVisual.InstantiateModel(unitModelData, weaponSO, classSO);
        PrefabUtility.RecordPrefabInstancePropertyModifications(this.gameObject);
    }

    public void ResetTileColor()
    {
        m_TileVisual.SetTileState(TileState.NONE);
        PrefabUtility.RecordPrefabInstancePropertyModifications(this.gameObject);
    }

    public void SetTileAsSetupColor()
    {
        m_TileVisual.SetTileState(TileState.SWAPPABLE);
        PrefabUtility.RecordPrefabInstancePropertyModifications(this.gameObject);
    }

    public void SetupTileEffects(params GameObject[] tileEffects)
    {
        m_TileVisual.SpawnTileEffects(tileEffects);
        PrefabUtility.RecordPrefabInstancePropertyModifications(this.gameObject);
    }

    public void ClearTileEffects()
    {
        m_TileVisual.ClearEffects();
        PrefabUtility.RecordPrefabInstancePropertyModifications(this.gameObject);
    }

    public void ToggleSelected(bool isSelected)
    {
        m_IsSelected = isSelected;
    }

    public void OnDrawGizmos()
    {
        if (m_IsSelected)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(this.transform.position + new Vector3(0, 0.5f, 0), 0.5f);
        }
            
    }
    #endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(TileSetupHelper))]
public class TileSetupHelperEditor : Editor
{

}
#endif