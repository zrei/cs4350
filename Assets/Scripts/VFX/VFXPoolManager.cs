using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class VFXPoolManager : Singleton<VFXPoolManager>
{
    private Dictionary<VFXSO, ObjectPool<VFXSystem>> pools = new();
    private Dictionary<VFXSO, Transform> poolParents = new();

    protected override void HandleAwake()
    {
        base.HandleAwake();

        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    public VFXSystem Get(VFXSO vfxSO)
    {
        if (!pools.ContainsKey(vfxSO))
        {
            InitializePool(vfxSO);
        }
        return pools[vfxSO].Get();
    }

    private void InitializePool(VFXSO vfxSO)
    {
        var parent = new GameObject($"Pool_{vfxSO.name}").transform;
        parent.SetParent(transform);
        poolParents[vfxSO] = parent;

        pools[vfxSO] = new ObjectPool<VFXSystem>(
            createFunc: () => {
                var vfx = Instantiate(vfxSO.m_VFXPrefab, parent, false);
                vfx.gameObject.SetActive(false);
                vfx.onParticleSystemStop += x => pools[vfxSO].Release(x);
                return vfx;
            },
            actionOnGet: vfx => { vfx.gameObject.SetActive(true); },
            actionOnRelease: vfx => {
                vfx.gameObject.SetActive(false);
                vfx.transform.SetParent(parent, false);
                vfx.transform.localPosition = Vector3.zero;
                vfx.transform.localEulerAngles = Vector3.zero;
            },
            actionOnDestroy: vfx => { },
            collectionCheck: false,
            defaultCapacity: 10,
            maxSize: 10000
        );
    }
}
