using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
[DisallowMultipleComponent]
[ExecuteInEditMode]
public class UISnowMaskController : Graphic
{
    [SerializeField] private Material uiSnowMaskMaterial;

    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        // UI 元素通常不需要复杂形状，这里直接清空即可
        toFill.Clear();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (uiSnowMaskMaterial != null)
        {
            canvasRenderer.SetMaterial(uiSnowMaskMaterial, null);
        }
    }

}