using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

public class TestSpawner : MonoBehaviour
{
    //[SerializeField] private GameObject m_LargeNode;
    [SerializeField] private Level.CharacterToken m_CharacterToken;
    [SerializeField] private GameObject m_NodeObj;
    [SerializeField] private SplineContainer m_Spline;
    [SerializeField] private PlayerCharacterSO m_Character;
    [SerializeField] private WeaponInstanceSO m_EquippedWeapon;
    [SerializeField] private PlayerClassSO m_PlayerClass;
    
    // want to change the system to be by distance instead... hm
    private Level.CharacterToken m_CharacterTokenInstance;

    // in world space
    private const float OFFSET = 0.1f;
    private const float NODE_INTERVALS = 0.1f;
    private const float SCALE = 3f;

    private const float DELAY = 0.5f;
    private const float MOVE_DELAY = 0.6f;

    private void Start()
    {
        //InstantiateAll();
        
        float totalDistance = m_Spline.CalculateLength();
        float instantiateTime = (totalDistance - 2 * OFFSET) / NODE_INTERVALS * DELAY;
        StartCoroutine(TimedSpawn(OFFSET, totalDistance - OFFSET));
        StartCoroutine(MoveUnit(MOVE_DELAY, instantiateTime, m_Spline.EvaluatePosition(1f)));
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

    private IEnumerator MoveUnit(float initialDelay, float totalTime, Vector3 finalPosition)
    {
        m_CharacterTokenInstance = Instantiate(m_CharacterToken);
        m_CharacterTokenInstance.transform.position = m_Spline.EvaluatePosition(0f);
        m_CharacterTokenInstance.Initialise(new PlayerCharacterBattleData() {m_BaseData = m_Character, m_ClassSO = m_PlayerClass, m_CurrEquippedWeapon = m_EquippedWeapon});

        float t = 0;
        while (t < initialDelay)
        {
            yield return null;
            t += Time.deltaTime;
        }

        m_CharacterTokenInstance.MoveToPosition(finalPosition, () => {Debug.Log("Complete");}, totalTime);
    }

    private IEnumerator TimedSpawn(float pathStartLength, float pathEndLength)
    {
        while (pathStartLength < pathEndLength)
        {
            yield return new WaitForSeconds(DELAY);
            InstantiatePathNode(GetPathNodePosition(pathStartLength));
            pathStartLength += NODE_INTERVALS;
        }
    }

    private Vector3 GetPathNodePosition(float timeProportion)
    {
        return m_Spline.EvaluatePosition(timeProportion);
    }

    private void InstantiatePathNode(Vector3 position)
    {
        GameObject node = Instantiate(m_NodeObj, position, Quaternion.identity);
        node.transform.localScale = new Vector3(SCALE, SCALE, SCALE);
    }
}