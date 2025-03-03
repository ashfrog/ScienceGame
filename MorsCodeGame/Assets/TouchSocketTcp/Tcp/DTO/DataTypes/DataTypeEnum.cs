using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 区分主机中控 S_开头为播控服务 ZK_开头为中控  一般ip最后一个值和端口号拼接起来就是枚举值
/// </summary>
public enum DataTypeEnum
{
    /// <summary>
    /// 播控程序
    /// </summary>
    Media0 = 0,
    Media1 = 1,
    Media2 = 2,
    Media3 = 3,
    Media4 = 4,
    Media5 = 5,

    Media20 = 20,
    Media21 = 21,
    Media22 = 22,

    Media30 = 30,
    Media31 = 31,
    Media32 = 32,



    /// <summary>
    /// 旧主控主机
    /// </summary>
    S_MainHostOld = 10,

    /// <summary>
    /// udp指令相关
    /// </summary>
    UDP = 11,

    ///// <summary>
    ///// 新主控主机
    ///// </summary>
    //S_MainHost = 20,

    /// <summary>
    /// 控制端 平板
    /// </summary>
    S_Pad = 50,
    S_Pad1 = 51,
    S_Pad2 = 52,
    S_Pad3 = 53,
    S_Pad4 = 54,
    S_Pad5 = 55,
    S_Pad6 = 56,

    S_Pad20 = 520,
    S_Pad21 = 521,
    S_Pad22 = 522,

    S_Pad30 = 530,
    S_Pad31 = 531,
    S_Pad32 = 532,

    /// <summary>
    /// 序厅主机AV播控服务
    /// </summary>
    S_XuTing = 100,

    /// <summary>
    /// 航线 3投影融合AV播控服务
    /// </summary>
    S_HangXian = 200,

    /// <summary>
    /// 开放之门 AV播控服务
    /// </summary>
    S_KaiFangZhiMen = 300,

    /// <summary>
    /// 沙盘背后 AV播控服务
    /// </summary>
    S_ShaPan = 400,

    /// <summary>
    /// 沙盘右边 AV播控服务
    /// </summary>
    S_ShaPanR = 500,

    /// <summary>
    /// 开放之门声音
    /// </summary>
    QP_KFZM_Audio = 8080,

    /// <summary>
    /// 开放之门切换矩阵卡 场景和Group
    /// </summary>
    QP_KFZM = 28000,

    /// <summary>
    /// 序厅切换矩阵卡 场景和Group
    /// </summary>
    QP_XTCard = 28001,

    /// <summary>
    /// 中控_沙盘 253 1024
    /// </summary>
    ZK_ShaPan = 31024,

    /// <summary>
    /// 投影仪左
    /// </summary>
    ZK_TouYing_Left = 31025,


    /// <summary>
    /// 投影仪中
    /// </summary>
    ZK_TouYing_Center = 31026,

    /// <summary>
    /// 投影仪右
    /// </summary>
    ZK_TouYing_Right = 31027,

    /// <summary>
    /// 开放之门右边屏幕
    /// </summary>
    ZK_LED_KFZM_Right = 31028,

    /// <summary>
    /// 序厅屏幕
    /// </summary>
    ZK_LED_XT = 31029,

    /// <summary>
    /// 开放之门顶部屏幕
    /// </summary>
    ZK_LED_KFZM_Top = 31030,

    /// <summary>
    /// 开放之门中间屏幕
    /// </summary>
    ZK_LED_KFZM_Center = 31031,


    /// <summary>
    /// 开放之门左边屏幕
    /// </summary>
    ZK_LED_KFZM_Left = 41024,

    /// <summary>
    /// 投影仪灯光联动 播放 暂停 停止 的灯光 灯光总开关 254:1025
    /// </summary>
    ZK_Light = 41025,

    /// <summary>
    /// 开放之门平移门
    /// </summary>
    ZK_KFZM_Door = 41026,

    /// <summary>
    /// 序厅大屏(未使用)
    /// </summary>
    ZK_LED_XuTing1028 = 41028,

    /// <summary>
    /// 沙盘背后大屏
    /// </summary>
    ZK_LED_SPBack = 41029,

    /// <summary>
    /// 开放之门音响
    /// </summary>
    ZK_KFZM_Sound = 41030,

    /// <summary>
    /// 开放之门主机
    /// </summary>
    ZK_KFZM_PC = 41031,

    /// 遮罩主机
    ZheZhao = 3000,

    /// <summary>
    /// 临港 九龙园区
    /// </summary>
    LG20001 = 20001,
    LG20002 = 20002,
    LG20003 = 20003,
    LG20004 = 20004,
    LG20005 = 20005,
    LG20006 = 20006,

    /// <summary>
    /// 临港 投影
    /// </summary>
    TY1011 = 1011,
    TY1012 = 1012,
    TY1013 = 1013,
    TY1014 = 1014,
    TY1015 = 1015,
    TY1016 = 1016,
    TY1017 = 1017,
    TY1018 = 1018,
    TY1019 = 1019,

    /// <summary>
    /// 关闭所有主机
    /// </summary>
    ShutDown = 10000,

    /// <summary>
    /// 中控返回数据
    /// </summary>
    S_ZK_Receive = 50000,

    /// <summary>
    /// 切屏_开放之门
    /// </summary>
    QP_KFZM_12 = 20001,
    QP_KFZM_34 = 20002,
    QP_KFZM_56 = 20003,
    QP_KFZM_78 = 20004,

    /// <summary>
    /// 切屏_序厅
    /// </summary>
    QP_XT = 20005,

    /// <summary>
    /// 切屏_投影
    /// </summary>
    QP_TY12 = 20006,
    QP_TY3 = 20007,

    /// <summary>
    ///  电源时序器
    /// </summary>
    ZK_Power = 20008
}

