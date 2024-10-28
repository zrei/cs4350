using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

public class TestSpawner : MonoBehaviour
{
    //[SerializeField] private GameObject m_LargeNode;
    [SerializeField] private WorldMapPlayerToken m_CharacterToken;
    [SerializeField] private GameObject m_NodeObj;
    [SerializeField] private SplineContainer m_Spline;
    [SerializeField] private PlayerCharacterSO m_Character;
    [SerializeField] private WeaponInstanceSO m_EquippedWeapon;
    [SerializeField] private PlayerClassSO m_PlayerClass;
    
    // want to change the system to be by distance instead... hm
    private WorldMapPlayerToken m_CharacterTokenInstance;

    // in world space
    private const float OFFSET = 3.5f;
    private const float NODE_INTERVALS = 2f;
    private const float SCALE = 3f;

    private const float DELAY = 0.5f;
    private const float MOVE_DELAY = 0.6f;

    private const float CHAR_MOVE_SPEED = 2f;

    private void Start()
    {
        //InstantiateAll();
        
        float totalDistance = m_Spline.CalculateLength();
        StartCoroutine(TimedSpawn(OFFSET, totalDistance - OFFSET, totalDistance));
        StartCoroutine(MoveUnit(MOVE_DELAY));
    }

    private void InstantiateAll()
    {
        float progress = OFFSET;
        while (progress < 1 - OFFSET)
        {
            InstantiatePathNode(GetPathNodePosition(progress));
            progress += NODE_INTERVALS;
        }
    }

    private IEnumerator MoveUnit(float initialDelay)
    {
        m_CharacterTokenInstance = Instantiate(m_CharacterToken);
        m_CharacterTokenInstance.Initialise(m_Character, m_PlayerClass, m_EquippedWeapon);
        m_CharacterTokenInstance.transform.position = m_Spline.EvaluatePosition(0f);

        float t = 0;
        while (t < initialDelay)
        {
            yield return null;
            t += Time.deltaTime;
        }

        m_CharacterTokenInstance.MoveAlongSpline(m_Spline, CHAR_MOVE_SPEED, Quaternion.LookRotation(m_NodeObj.transform.forward, m_CharacterTokenInstance.transform.up), OnCompleteMovement);

        void OnCompleteMovement()
        {
            Debug.Log("Complete");
        }
    }

    private IEnumerator TimedSpawn(float pathStartLength, float pathEndLength, float totalPathLength)
    {
        while (pathStartLength < pathEndLength)
        {
            yield return new WaitForSeconds(DELAY);
            InstantiatePathNode(GetPathNodePosition(pathStartLength / totalPathLength));
            pathStartLength += NODE_INTERVALS;
        }
    }

    private Vector3 GetPathNodePosition(float proportionOfDistance)
    {
        return m_Spline.EvaluatePosition(proportionOfDistance);
    }

    private void InstantiatePathNode(Vector3 position)
    {
        GameObject node = Instantiate(m_NodeObj, position, Quaternion.identity);
        node.transform.localScale = new Vector3(SCALE, SCALE, SCALE);
    }
}