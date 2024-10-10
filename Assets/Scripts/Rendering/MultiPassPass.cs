using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MultiPassPass : ScriptableRenderPass
{
    private List<ShaderTagId> m_Tags;

    public MultiPassPass(List<string> tags)
    {
        m_Tags = new();
        foreach (var tag in tags)
        {
            m_Tags.Add(new(tag));
        }

        renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var filteringSettings = FilteringSettings.defaultValue;

        foreach (var pass in m_Tags)
        {
            var drawingSettings = CreateDrawingSettings(pass, ref renderingData, SortingCriteria.CommonOpaque);
            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
        }

        context.Submit();
    }
}
