using System.Collections;
using Cinemachine;
using Game.Input;
using UnityEngine;

/// <summary>
/// Manages the controls of the level camera
/// </summary>
public class PlaneCameraController : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera m_VCam;
    [SerializeField] Transform m_CameraLookAtTransform;
    [SerializeField] Transform m_LeftLimitTransform;
    [SerializeField] Transform m_RightLimitTransform;
    [SerializeField] Transform m_TopLimitTransform;
    [SerializeField] Transform m_BottomLimitTransform;
    
    private float LookAtPosXLimit => m_RightLimitTransform.position.x;
    private float LookAtNegXLimit => m_LeftLimitTransform.position.x;
    private float LookAtPosZLimit => m_TopLimitTransform.position.z;
    private float LookAtNegZLimit => m_BottomLimitTransform.position.z;
    
    // Camera Movement
    [SerializeField] const float CAMERA_MOVEMENT_SPEED = 5f;
    private Vector2 m_CameraMovementDirection;
    private bool m_IsCameraMoving;

    #region Initialisation

    public void Initialise(Transform followTransform)
    {
        m_CameraLookAtTransform.parent = followTransform;
        m_CameraLookAtTransform.localPosition = Vector3.zero;
    }

    #endregion

    #region Camera Movement
    
    public void EnableCameraMovement()
    {
        InputManager.Instance.NavigateInput.OnChangeEvent += OnCameraMove;
    }

    public void DisableCameraMovement()
    {
        InputManager.Instance.NavigateInput.OnChangeEvent -= OnCameraMove;
        
        // Stop current movement
        m_CameraMovementDirection = Vector2.zero;
    }
    
    public void RecenterCamera()
    {
        m_CameraLookAtTransform.localPosition = Vector3.zero;
    }
    
    private void OnCameraMove(IInput input)
    {
        m_CameraMovementDirection = input.GetValue<Vector2>();
        
        if (!m_IsCameraMoving)
            StartCoroutine(CameraMovementCoroutine());
        
        IEnumerator CameraMovementCoroutine()
        {
            m_IsCameraMoving = true;
            
            while (m_CameraMovementDirection.magnitude > 0.1f)
            {
                var currPos = m_CameraLookAtTransform.position;
                var posChange = new Vector3(m_CameraMovementDirection.x, 0, m_CameraMovementDirection.y) * (CAMERA_MOVEMENT_SPEED * Time.deltaTime);
                m_CameraLookAtTransform.position = new Vector3(
                    Mathf.Clamp(currPos.x + posChange.x, LookAtNegXLimit, LookAtPosXLimit),
                    currPos.y,
                    Mathf.Clamp(currPos.z + posChange.z, LookAtNegZLimit, LookAtPosZLimit));
                yield return null;
            }
            
            m_IsCameraMoving = false;
        }
    }

    #endregion

}
