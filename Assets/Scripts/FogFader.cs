using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FogFader : MonoBehaviour
{
    private static readonly int ColorPropertyHash = Shader.PropertyToID("_BaseColor");

    private Color m_BaseColor;
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
            m_PropBlock.SetColor(ColorPropertyHash, new Color(m_BaseColor.r, m_BaseColor.g, m_BaseColor.b, m_Opacity * m_BaseColor.a));
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
        m_BaseColor = gameObject.GetComponent<Renderer>().material.color;
        SetRenderers(gameObject.GetComponents<Renderer>());
    }
    
    public void SetRenderers(IEnumerable<Renderer> renderers)
    {
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

#if UNITY_EDITOR
[CustomEditor(typeof(FogFader))]
public class FogFaderEditor : Editor {
    private FogFader m_FogFader;

    private void OnEnable() {
        m_FogFader = (FogFader)target;
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        GUILayout.Space(10f);

        if (GUILayout.Button("Fade")) {
            m_FogFader.Fade(0, 1);
            Logger.LogEditor(this.GetType().Name, "Successfully Faded " + target.name, LogLevel.LOG);
        }
    }
}
#endif