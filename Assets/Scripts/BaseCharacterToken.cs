using System.Collections;
using UnityEngine;

public abstract class BaseCharacterToken : MonoBehaviour
{
    [SerializeField] protected ArmorVisual m_ArmorVisual;
        
    #region Static Data
    public Vector3 GridYOffset { get; private set; }

    protected const float ROTATION_TIME = 0.2f;
    #endregion
    protected void Initialise(UnitModelData unitModelData, WeaponInstanceSO weaponSO, ClassSO classSO)
    {
        GridYOffset = new Vector3(0f, unitModelData.m_GridYOffset);
        m_ArmorVisual.InstantiateModel(unitModelData, weaponSO, classSO);
    }

    protected IEnumerator Rotate(Quaternion targetRot, VoidEvent onCompleteRotation, float rotateTime)
    {
        float time = 0f;
        Quaternion currRot = transform.rotation;
            
        while (time < rotateTime)
        {
            time += Time.deltaTime;
            float l = time / rotateTime;
            Quaternion newRot = Quaternion.Lerp(currRot, targetRot, l);

            transform.rotation = newRot;
            yield return null;
        }
        transform.rotation = targetRot;
            
        onCompleteRotation?.Invoke();
    }
}
