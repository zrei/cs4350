using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LayerConstants
{
    public static readonly int WorldMapLayer = LayerMask.NameToLayer("WorldMap");
    public static readonly int DefaultLayer = LayerMask.NameToLayer("Default");
    public static readonly int ObjectsLayer = LayerMask.NameToLayer("Objects");
}

public class MeshFader : MonoBehaviour
{
    private static readonly int OpacityPropertyHash = Shader.PropertyToID("_Opacity");

    private List<Renderer> m_Renderers = new();

    private MaterialPropertyBlock m_PropBlock;

    public float Opacity
    {
        get => m_Opacity;
        set
        {
            if (m_Opacity == value) return;
            value = Mathf.Clamp01(value);

            m_Opacity = value;
            m_PropBlock.SetFloat(OpacityPropertyHash, value);
            m_Renderers.ForEach(x =>
            {
                x.SetPropertyBlock(m_PropBlock);
                if (x.enabled ^ m_Opacity > 0)
                {
                    x.enabled = m_Opacity > 0;
                }
            });

            IsTranslucent = m_Opacity < 1;
        }
    }
    [SerializeProperty("Opacity")]
    [SerializeField]
    private float m_Opacity = 1;

    private bool IsTranslucent
    {
        get => m_IsTranslucent;
        set
        {
            if (m_IsTranslucent == value) return;

            m_IsTranslucent = value;
            m_Renderers.ForEach(x =>
            {
                x.shadowCastingMode = value ? UnityEngine.Rendering.ShadowCastingMode.Off : UnityEngine.Rendering.ShadowCastingMode.On;
                x.gameObject.layer = value ? LayerConstants.DefaultLayer : LayerConstants.ObjectsLayer;
            });
        }
    }
    private bool m_IsTranslucent;

    private Coroutine m_FadeCoroutine;

    private void Awake()
    {
        m_PropBlock = new MaterialPropertyBlock();
        m_PropBlock.SetFloat(OpacityPropertyHash, 1);
    }

    public void SetRenderers(IEnumerable<Renderer> renderers)
    {
        m_Renderers.Clear();
        foreach (var renderer in renderers) AddRenderer(renderer);
    }

    public void AddRenderer(Renderer renderer)
    {
        m_Renderers.Add(renderer);
        renderer.SetPropertyBlock(m_PropBlock);
    }

    public void Fade(float targetOpacity, float duration)
    {
        if (m_FadeCoroutine != null)
        {
            StopCoroutine(m_FadeCoroutine);
            m_FadeCoroutine = null;
        }
        m_FadeCoroutine = StartCoroutine(FadeOverTime(targetOpacity, duration));
    }

    private IEnumerator FadeOverTime(float targetOpacity, float duration)
    {
        float t = 0;
        float startOp = Opacity;
        while (t < duration)
        {
            Opacity = Mathf.Lerp(startOp, targetOpacity, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        Opacity = targetOpacity;
    }
}
