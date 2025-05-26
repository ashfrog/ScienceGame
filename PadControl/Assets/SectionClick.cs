using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static VCRControl;

public class SectionClick : MonoBehaviour
{
    public Button btnExplanationMode;    // 讲解模式
    public Button btnTourMode;           // 参观模式
    public Button btnEnergySavingMode;   // 节能模式

    // Start is called before the first frame update
    void Start()
    {
        btnExplanationMode.onClick.AddListener(OnExplanationModeClicked);
        btnTourMode.onClick.AddListener(OnTourModeClicked);
        btnEnergySavingMode.onClick.AddListener(OnEnergySavingModeClicked);
    }

    // 讲解模式按钮点击事件
    void OnExplanationModeClicked()
    {
        Debug.Log("讲解模式按钮被点击");
        // TODO: 添加讲解模式的具体逻辑

        for (int i = (int)DataTypeEnum.Media_XZTY_18; i <= (int)DataTypeEnum.Media_MDFY_29; i++)
        {
            FHClientController.ins.Send((DataTypeEnum)i, OrderTypeEnum.LoopMode, LoopMode.none);
            FHClientController.ins.Send((DataTypeEnum)i, OrderTypeEnum.StopMovie, "");
        }

    }

    // 参观模式按钮点击事件
    void OnTourModeClicked()
    {
        Debug.Log("参观模式按钮被点击");
        // TODO: 添加参观模式的具体逻辑
        for (int i = (int)DataTypeEnum.Media_XZTY_18; i <= (int)DataTypeEnum.Media_MDFY_29; i++)
        {
            FHClientController.ins.Send((DataTypeEnum)i, OrderTypeEnum.LoopMode, LoopMode.all);
            FHClientController.ins.Send((DataTypeEnum)i, OrderTypeEnum.PlayNext, "");
        }
    }

    // 节能模式按钮点击事件
    void OnEnergySavingModeClicked()
    {
        Debug.Log("节能模式按钮被点击");
        // TODO: 添加节能模式的具体逻辑
        for (int i = (int)DataTypeEnum.Media_XZTY_18; i <= (int)DataTypeEnum.Media_MDFY_29; i++)
        {
            FHClientController.ins.Send((DataTypeEnum)i, OrderTypeEnum.LoopMode, LoopMode.all);
            FHClientController.ins.Send((DataTypeEnum)i, OrderTypeEnum.PlayNext, "");
        }

    }
    /// <summary>
    /// 关闭投影灯光
    /// </summary>
    public void TurnOffProjectorLight()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}