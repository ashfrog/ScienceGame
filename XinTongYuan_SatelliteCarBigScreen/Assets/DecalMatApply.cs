using RenderHeads.Media.AVProVideo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

/// <summary>
/// 动态创建mat 防止显示不出来的bug
/// </summary>
public class DecalMatApply : MonoBehaviour
{
    [SerializeField]
    ApplyToMaterial applyToMaterial;
    // Start is called before the first frame update
    void Start()
    {
        DecalProjector decal = GetComponent<DecalProjector>();
        Material mat = new Material(decal.material); // 创建实例避免影响原始材质
        mat.SetInt("_AffectBaseColor", 0);
        decal.material = mat;
        applyToMaterial._material = decal.material;
        applyToMaterial.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
