using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    [SerializeField] Transform m_PooledObject;

    /// <summary>
    /// Return a game object to the pool
    /// </summary>
    /// <param name="pooledObject"></param>
    public void ReturnToPool(Transform pooledObject)
    {
        pooledObject.parent = transform;
        pooledObject.gameObject.SetActive(false);
    }

    /// <summary>
    /// Retrieve a pooled game object. This function also parents the
    /// game object, resetting its local scale and position
    /// the local scale and position
    /// </summary>
    /// <param name="parent"></param>
    /// <returns></returns>
    public GameObject RetrievePooledObject(Transform parent, bool setActive = true)
    {
        Transform pooledTransform = InstantiatePooledObject();
        pooledTransform.SetParent(parent, false);
        pooledTransform.localScale = Vector3.one;
        pooledTransform.position = Vector3.zero;
        pooledTransform.rotation = Quaternion.identity;

        GameObject pooledObject = pooledTransform.gameObject;
        pooledObject.SetActive(setActive);
        return pooledObject;
    }

    /// <summary>
    /// Try to retrieve a component from a pooled game object.
    /// If the retrieval fails, the game object is returned to the pool
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="parent"></param>
    /// <returns></returns>
    public bool TryRetrievePooledObject<T>(Transform parent, out T component, bool setActive = true)
    {
        GameObject obj = RetrievePooledObject(parent, setActive);
        component = obj.GetComponentInChildren<T>();

        if (component == null)
        {
            ReturnToPool(obj.transform);
            Logger.Log(this.GetType().Name, gameObject.name, $"Failure to find component of type {typeof(T)}", gameObject, LogLevel.ERROR);
            return false;
        }
        else
        {
            return true;
        }
    }

    private Transform InstantiatePooledObject()
    {
        if (transform.childCount > 0)
        {
            return transform.GetChild(0);
        }
        else
        {
            return Instantiate(m_PooledObject);
        }
    }
}