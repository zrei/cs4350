using System.Collections;
using Cinemachine;
using Game.Input;
using UnityEngine;

/// <summary>
/// Manages the controls of the level camera
/// </summary>
public class LevelCameraController : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera m_LevelVCam;
    [SerializeField] Transform m_LevelCameraLookAtTransform;
    [SerializeField] Transform m_LevelLeftLimitTransform;
    [SerializeField] Transform m_LevelRightLimitTransform;
    [SerializeField] Transform m_LevelTopLimitTransform;
    [SerializeField] Transform m_LevelBottomLimitTransform;
    
    private float LookAtPosXLimit => m_LevelRightLimitTransform.position.x;
    private float LookAtNegXLimit => m_LevelLeftLimitTransform.position.x;
    private float LookAtPosZLimit => m_LevelTopLimitTransform.position.z;
    private float LookAtNegZLimit => m_LevelBottomLimitTransform.position.z;
    
    // Camera Movement
    [SerializeField] const float CAMERA_MOVEMENT_SPEED = 5f;
    private Vector2 m_CameraMovementDirection;
    private bool m_IsCameraMoving;

    #region Initialisation

    public void Initialise(Transform playerTokenTransform)
    {
        m_LevelCameraLookAtTransform.parent = playerTokenTransform;
        m_LevelCameraLookAtTransform.localPosition = Vector3.zero;
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
        m_LevelCameraLookAtTransform.localPosition = Vector3.zero;
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
                var currPos = m_LevelCameraLookAtTransform.position;
                var posChange = new Vector3(m_CameraMovementDirection.x, 0, m_CameraMovementDirection.y) * (CAMERA_MOVEMENT_SPEED * Time.deltaTime);
                m_LevelCameraLookAtTransform.position = new Vector3(
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
