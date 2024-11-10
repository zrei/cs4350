using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public class TextDisplayPooler : MonoBehaviour
{
    private const int MaxTextDisplays = 5;
    
    [SerializeField] LayoutGroup m_ParentLayout;
    
    [SerializeField] MultiTextDisplay m_TextDisplayPrefab;
    
    private ObjectPool<MultiTextDisplay> displayPool;
    private HashSet<MultiTextDisplay> activeDisplays = new();

    private void Awake()
    {
        displayPool = new(
            createFunc: () => { var display = Instantiate(m_TextDisplayPrefab, m_ParentLayout.transform); display.gameObject.SetActive(false); return display; },
            actionOnGet: display => { display.gameObject.SetActive(true); display.transform.SetAsLastSibling(); activeDisplays.Add(display); },
            actionOnRelease: display => { display.gameObject.SetActive(false); activeDisplays.Remove(display); },
            actionOnDestroy: display => Destroy(display.gameObject),
            collectionCheck: true,
            defaultCapacity: 3,
            maxSize: MaxTextDisplays
        );
    }

    public MultiTextDisplay Get()
    {
        return displayPool.Get();
    }
    
    public void Release(MultiTextDisplay display)
    {
        displayPool.Release(display);
    }
    
    public void Clear()
    {
        List<MultiTextDisplay> displays = activeDisplays.ToList();
        foreach (var display in displays)
        {
            displayPool.Release(display);
        }
    }
}