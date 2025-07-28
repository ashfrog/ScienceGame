using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 指令类型
/// </summary>
public enum OrderTypeEnum
{
    AutoSwitchLine = 1,
    PowerOnMacAddress = 7,

    /// <summary>
    /// 发送str到配置的Udp客户端
    /// </summary>
    UdpClientStr = 8,


    LoadUrl = 9,//加载网页
    GoBack = 10,//网页返回
    GoForward = 11,//网页向后
    Reload = 12,//刷新网页
    EnableBrowser = 14,//设置显示网页
    GetEnableBrowser = 15,//获取显示网页开关
    GetPlayInfo = 18,//获取播放信息

    /// <summary>
    /// 由主控转发字符串
    /// </summary>
    Str = 20,

    /// <summary>
    /// 绘制卫星轨道
    /// </summary>
    DrawOrbit = 21,

    /// <summary>
    /// 旋转增量
    /// </summary>
    Rotate = 30,

    /// <summary>
    /// 卫星点
    /// </summary>
    WeiXingDot = 31,

    /// <summary>
    /// 卫星视图
    /// </summary>
    WeiXingView = 32,

    /// <summary>
    /// 获取音量
    /// </summary>
    GetVolumn = 101,
    /// <summary>
    /// 设置音量
    /// </summary>
    SetVolumn = 102,

    VolumnUp = 105,
    VolumnDown = 106,

    ShowGroup = 110,

    /// <summary>
    /// 获取当前播放进度
    /// </summary>
    GetMovSeek = 501,
    /// <summary>
    /// 设置播放进度
    /// </summary>
    SetMovSeek = 502,

    /// <summary>
    /// 获取视频列表
    /// </summary>
    GetFileList = 301,

    /// <summary>
    /// 获取网页列表
    /// </summary>
    GetUrls = 401,

    /// <summary>
    /// 指定播放某个视频
    /// </summary>
    SetPlayMovie = 402,

    /// <summary>
    /// 指定轮播视频目录
    /// </summary>
    SetPlayMovieFolder = 400,
    /// <summary>
    /// 停止播放的视频
    /// </summary>
    StopMovie = 403,

    /// <summary>
    /// 播放下一个视频
    /// </summary>
    PlayNext = 404,

    /// <summary>
    /// 播放上一个视频
    /// </summary>
    PlayPrev = 405,

    /// <summary>
    /// 播放
    /// </summary>
    PlayMovie = 406,

    /// <summary>
    /// 暂停
    /// </summary>
    PauseMovie = 407,

    /// <summary>
    /// 获取当前播放视频名称
    /// </summary>
    GetCurMovieName = 408,

    TabControl = 500,

    /// <summary>
    /// 国家筛选器变更
    /// </summary>
    CountryFilterChange = 501,

    /// <summary>
    /// 播放地屏视频
    /// </summary>
    PlayGroundMovie = 510,

    /// <summary>
    /// 获取视频总时长
    /// </summary>
    GetMovAllSecond = 601,

    /// <summary>
    /// 关机
    /// </summary>
    Shutdown = 1000,

    /// <summary>
    /// 打开电源
    /// </summary>
    PowerOn = 1001,

    /// <summary>
    /// 关闭电源
    /// </summary>
    PowerOff = 1002,

    /// <summary>
    /// 开门视频
    /// </summary>
    OpenTheDoor = 2000,

    /// <summary>
    /// 关门视频
    /// </summary>
    CloseTheDoor = 2001,

    ///遮罩
    ZheZhao = 3000,

    Browser = 4000,

    /// <summary>
    ///  关闭所有主机
    /// </summary>
    ShutDown = 10000,
}

