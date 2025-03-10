using UnityEngine;

[ExecuteInEditMode]
public class TVNoiseEffect : MonoBehaviour
{
    [SerializeField] private Material tvNoiseMaterial;

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (tvNoiseMaterial != null)
        {
            Graphics.Blit(src, dest, tvNoiseMaterial);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }
}