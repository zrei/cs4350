using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

public class WorldMapPlayerToken : BaseCharacterToken
{
    private const float MOVE_SPEED = 6.0f;

    #region Initialisation
    private void Awake()
    {
        GlobalEvents.CutsceneEvents.StartCutsceneEvent += OnStartCutscene;
        GlobalEvents.CutsceneEvents.EndCutsceneEvent += OnEndCutscene;
    }

    private void OnDestroy()
    {
        GlobalEvents.CutsceneEvents.StartCutsceneEvent -= OnStartCutscene;
        GlobalEvents.CutsceneEvents.EndCutsceneEvent -= OnEndCutscene;
    }

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

    // Move without initial delay and rotation
    public void MoveAlongSpline(SplineContainer splineContainer, Vector3 pathOffset, Quaternion finalRotation,
        VoidEvent completeMovementEvent, float startOffset = 0f, float endOffset = 0f, float moveSpeed = MOVE_SPEED)
    {
        StartCoroutine(MoveAlongSpline_Coroutine(splineContainer, pathOffset, moveSpeed, finalRotation,
            completeMovementEvent, startOffset, endOffset));
    }
    
    public void MoveAlongSpline(float initialDelay, Quaternion initialRotation, SplineContainer splineContainer, Vector3 pathOffset, Quaternion finalRotation, VoidEvent completeMovementEvent, float startOffset = 0f, float endOffset = 0f, float moveSpeed = MOVE_SPEED)
    {
        StartCoroutine(PreRotation(initialDelay, initialRotation, () => PostRotation(splineContainer, pathOffset, finalRotation, completeMovementEvent, startOffset, endOffset, moveSpeed)));
    }

    private IEnumerator PreRotation(float initialDelay, Quaternion initialRotation, VoidEvent postRotation)
    {
        yield return new WaitForSeconds(initialDelay);
         StartCoroutine(Rotate(initialRotation, postRotation, ROTATION_TIME));
    }

    private void PostRotation(SplineContainer splineContainer, Vector3 pathOffset, Quaternion finalRotation, VoidEvent completeMovementEvent, float startOffset, float endOffset, float moveSpeed)
    {
        StartCoroutine(MoveAlongSpline_Coroutine(splineContainer, pathOffset, moveSpeed, finalRotation, completeMovementEvent, startOffset, endOffset));
    }

    private IEnumerator MoveAlongSpline_Coroutine(SplineContainer splineContainer, Vector3 pathOffset, float speed, Quaternion finalRotation, VoidEvent completeMovementEvent, float startOffset, float endOffset)
    {
        Vector3 previousPosition = this.transform.position;
        float splineLength = splineContainer.CalculateLength();
        
        float progress = 0f + startOffset / splineLength;
        float progressEnd = 1f - endOffset / splineLength;

        m_ArmorVisual.SetMoveAnimator(true);
        m_ArmorVisual.SetDirAnim(ArmorVisual.DirXAnimParam, 0f);
        m_ArmorVisual.SetDirAnim(ArmorVisual.DirYAnimParam, 1f);

        while (progress < progressEnd)
        {
            yield return null;
            progress += speed * Time.deltaTime / splineLength;
            Vector3 currPosition = (Vector3) splineContainer.EvaluatePosition(progress) + pathOffset;
            // Vector3 direction = currPosition - previousPosition;
            Vector3 direction = splineContainer.EvaluateTangent(progress);
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

    #region Cutscene
    private void OnStartCutscene()
    {
        ChangeLayers(LayerConstants.WorldMapLayer);
    }

    private void OnEndCutscene()
    {
        ChangeLayers(LayerConstants.ObjectsLayer);
    }

    private void ChangeLayers(int layer)
    {
        ChangeLayer(transform, layer); 
    }

    private void ChangeLayer(Transform transform, int layer)
    {
        transform.gameObject.layer = layer;
        foreach (Transform child in transform)
        {
            ChangeLayer(child, layer);
        }
    }
    #endregion
}
