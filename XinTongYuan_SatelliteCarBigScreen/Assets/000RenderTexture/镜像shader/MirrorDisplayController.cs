using UnityEngine;

public class MirrorDisplayController : MonoBehaviour
{
    public Camera mainCamera; // 主相机
    public Camera mirrorCamera; // 镜像相机
    public RenderTexture renderTexture; // 渲染目标
    public bool isHorizontalMirror = false; // 是否水平镜像
    public bool isVerticalMirror = false; // 是否垂直镜像

    private Material mirrorMaterial;

    void Start()
    {
        // 检查资源是否正确设置
        if (mainCamera == null || mirrorCamera == null || renderTexture == null)
        {
            Debug.LogError("请确保主相机、镜像相机和渲染目标已正确设置！");
            return;
        }

        // 将主相机的内容渲染到 RenderTexture
        mainCamera.targetTexture = renderTexture;

        // 镜像相机使用材质显示 RenderTexture 内容
        mirrorCamera.targetTexture = null; // 镜像相机直接渲染到屏幕
        mirrorMaterial = new Material(Shader.Find("Custom/MirrorEffect"));
        mirrorMaterial.SetTexture("_MainTex", renderTexture);
        mirrorMaterial.SetInt("_HorizontalMirror", isHorizontalMirror ? 1 : 0);
        mirrorMaterial.SetInt("_VerticalMirror", isVerticalMirror ? 1 : 0);

        // 应用材质到镜像相机的渲染目标
        mirrorCamera.GetComponent<Renderer>().material = mirrorMaterial;
    }

    // 动态更新镜像参数（可选）
    void Update()
    {
        if (mirrorMaterial != null)
        {
            mirrorMaterial.SetInt("_HorizontalMirror", isHorizontalMirror ? 1 : 0);
            mirrorMaterial.SetInt("_VerticalMirror", isVerticalMirror ? 1 : 0);
        }
    }
}