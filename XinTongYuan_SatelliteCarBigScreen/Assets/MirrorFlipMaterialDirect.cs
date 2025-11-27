using UnityEngine;

public class MirrorFlipMaterialDirect : MonoBehaviour
{
    public Material targetMaterial;
    private const string FlipX = "_FlipX";

    private void Awake()
    {
#if !UNITY_EDITOR
        if (targetMaterial != null && targetMaterial.HasProperty(FlipX))
        {
            targetMaterial.SetFloat(FlipX, 1f);
        }
#endif
    }
}