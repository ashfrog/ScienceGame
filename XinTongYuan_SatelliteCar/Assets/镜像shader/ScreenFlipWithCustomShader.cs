using UnityEngine;

public class ScreenFlipWithCustomShader : MonoBehaviour
{
    [Tooltip("勾选此项水平翻转屏幕")]
    public bool flipHorizontal = true;
    [Tooltip("勾选此项垂直翻转屏幕")]
    public bool flipVertical = false;

    private Material flipMaterial;
    public Shader flipShader;

    void Awake()
    {


        // 如果Shader不存在，则使用内置的Unlit/Texture Shader作为替代
        if (flipShader == null)
        {
            Debug.LogWarning("ScreenFlip shader not found. Please create a shader file with the code provided in the comments.");
        }

        flipMaterial = new Material(flipShader);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (flipMaterial != null)
        {
            flipMaterial.SetFloat("_FlipHorizontal", flipHorizontal ? 1 : 0);
            flipMaterial.SetFloat("_FlipVertical", flipVertical ? 1 : 0);
            Graphics.Blit(source, destination, flipMaterial);
        }
        else
        {
            // 如果材质为空，直接复制源纹理到目标
            Graphics.Blit(source, destination);
        }
    }

    void OnDestroy()
    {
        if (flipMaterial != null)
            Destroy(flipMaterial);
    }
}