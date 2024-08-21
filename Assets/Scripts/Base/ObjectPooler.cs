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
    public GameObject RetrievePooledObject(Transform parent)
    {
        Transform pooledTransform = InstantiatePooledObject();
        pooledTransform.parent = parent;
        pooledTransform.localScale = Vector3.one;
        pooledTransform.position = Vector3.zero;
        pooledTransform.rotation = Quaternion.identity;

        GameObject pooledObject = pooledTransform.gameObject;
        pooledObject.SetActive(false);
        return pooledObject;
    }

    /// <summary>
    /// Retrieve a pooled game object through a component on the pooled object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="parent"></param>
    /// <returns></returns>
    public T RetrievePooledObject<T>(Transform parent)
    {
        return RetrievePooledObject(parent).GetComponent<T>();
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