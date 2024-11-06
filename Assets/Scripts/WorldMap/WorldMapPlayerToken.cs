using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

public class WorldMapPlayerToken : BaseCharacterToken
{
    private const float MOVE_SPEED = 6.0f;

    #region Initialisation
    public void Initialise(PlayerCharacterSO lordCharacter, ClassSO lordClass, WeaponInstanceSO lordEquippedWeapon)
    {
        Initialise(lordCharacter.GetUnitModelData(lordClass.m_OutfitType), lordEquippedWeapon, lordClass);
    }

    public void UpdateAppearance(PlayerCharacterSO lordCharacter, ClassSO lordClass, WeaponInstanceSO lordEquippedWeapon)
    {
        ChangeAppearance(lordCharacter.GetUnitModelData(lordClass.m_OutfitType), lordEquippedWeapon, lordClass);
    }
    #endregion

    #region Follow Path
    public void MoveAlongSpline(float initialDelay, Quaternion initialRotation, SplineContainer splineContainer, Quaternion finalRotation, VoidEvent completeMovementEvent)
    {
        StartCoroutine(PreRotation(initialDelay, initialRotation, () => PostRotation(splineContainer, finalRotation, completeMovementEvent)));
    }

    private IEnumerator PreRotation(float initialDelay, Quaternion initialRotation, VoidEvent postRotation)
    {
        yield return new WaitForSeconds(initialDelay);
         StartCoroutine(Rotate(initialRotation, postRotation, ROTATION_TIME));
    }

    private void PostRotation(SplineContainer splineContainer, Quaternion finalRotation, VoidEvent completeMovementEvent)
    {
        StartCoroutine(MoveAlongSpline_Coroutine(splineContainer, MOVE_SPEED, finalRotation, completeMovementEvent));
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
    #endregion

    #region Graphics
    public void FadeMesh(float targetOpacity, float duration)
    {
        m_ArmorVisual.FadeMesh(targetOpacity, duration);
    }
    #endregion
}
