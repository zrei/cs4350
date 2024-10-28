using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

public class WorldMapPlayerToken : BaseCharacterToken
{
    public void Initialise(PlayerCharacterSO lordCharacter, ClassSO lordClass, WeaponInstanceSO lordEquippedWeapon)
    {
        Initialise(lordCharacter.GetUnitModelData(lordClass.m_OutfitType), lordEquippedWeapon, lordClass);
    }

    public void MoveAlongSpline(SplineContainer splineContainer, float speed, Quaternion finalRotation, VoidEvent completeMovementEvent)
    {
        StartCoroutine(MoveAlongSpline_Coroutine(splineContainer, speed, finalRotation, completeMovementEvent));
    }

    private IEnumerator MoveAlongSpline_Coroutine(SplineContainer splineContainer, float speed, Quaternion finalRotation, VoidEvent completeMovementEvent)
    {
        float progress = 0f;

        Vector3 previousPosition = this.transform.position;
        float splineLength = splineContainer.CalculateLength();

        m_ArmorVisual.SetMoveAnimator(true);
        m_ArmorVisual.SetDirAnim(ArmorVisual.DirXAnimParam, 0f);
        m_ArmorVisual.SetDirAnim(ArmorVisual.DirYAnimParam, 1f);

        while (progress < 1f)
        {
            yield return null;
            progress += speed * Time.deltaTime / splineLength;
            Vector3 currPosition = splineContainer.EvaluatePosition(progress);
            Vector3 direction = currPosition - previousPosition;
            this.transform.position = currPosition;
            this.transform.rotation = Quaternion.LookRotation(-direction, transform.up);
        }

        StartCoroutine(Rotate(finalRotation, OnComplete, ROTATION_TIME));

        void OnComplete()
        {
            m_ArmorVisual.SetMoveAnimator(false);
            completeMovementEvent?.Invoke();
        }
    }
}
