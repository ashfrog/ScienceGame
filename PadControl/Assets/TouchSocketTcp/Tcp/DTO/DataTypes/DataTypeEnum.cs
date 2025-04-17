using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// IPNO枚举
/// </summary>
public enum DataTypeEnum
{
    /// <summary>
    /// mac地址 魔法包开机用
    /// </summary>
    UDP = 0,
    /// <summary>
    /// 中控主机标识
    /// </summary>
    S_MainHost = 1,

    /// <summary>
    /// 开关ipNo 192.168.3.10-3.17
    /// </summary>
    SW_XZTY_10 = 10, //新增投影
    SW_XTDP_11 = 11, //序厅大屏
    SW_HDTY_12 = 12, //互动投影
    SW_QXTY_13 = 13, //全息投影
    SW_ZM_14 = 14, //逐梦
    SW_QC_15 = 15, //强策
    SW_HM_16 = 16, //汇盟
    SW_ZY_17 = 17, //智御
    /// <summary>
    /// 电脑ipNo 192.168.3.18-3.25
    /// </summary>
    Media_XZTY_18 = 18, //新增投影
    Media_XTDP_19 = 19, //序厅大屏
    Media_HDTY_20 = 20, //互动投影
    Media_QXTY_21 = 21, //全息投影
    Media_ZM_22 = 22, //逐梦
    Media_QC_23 = 23, //强策
    Media_HM_24 = 24, //汇盟
    Media_ZY_25 = 25, //智御
    TY_PROJECTOR_26 = 26,//  投影机
    TY_PROJECOTR_27 = 27,//投影机

    /// <summary>
    /// 控制端 平板播控组
    /// </summary>
    S_Pad = 50,
    S_Pad1 = 51,
    S_Pad2 = 52,
    S_Pad3 = 53,
    S_Pad4 = 54,
    S_Pad5 = 55,
    S_Pad6 = 56,

    /// <summary>
    /// 展厅区域灯光插座ipNo IP：192.168.3.9 端口：20001
    /// </summary>
    SW_DG_9_20001 = 20001,
    /// <summary>
    /// 办公区域灯光插座ipNo IP：192.168.3.9 端口：20002
    /// </summary>
    SW_DG_9_20002 = 20002,
    /// <summary>
    /// LED大屏控制ipNo IP：192.168.3.9 端口：20003
    /// </summary>
    SW_DG_9_20003 = 20003,
    /// <summary>
    /// 互动投影开关ipNo IP：192.168.3.9 端口：20004
    /// </summary>
    SW_DG_9_20004 = 20004,
    /// <summary>
    /// 展厅区域灯LED大屏开关ipNo IP：192.168.3.9 端口：20006
    /// </summary>
    SW_DG_9_20006 = 20006,
    /// <summary>
    /// 大屏功放电源时序器ipNo  IP：192.168.3.9 端口：20007
    /// </summary>
    SW_GF_9_20007 = 20007,
    /// <summary>
    /// 展厅区域电动门开关ipNo IP：192.168.3.9 端口：20008
    /// </summary>
    SW_DG_9_20008 = 20008,

}

