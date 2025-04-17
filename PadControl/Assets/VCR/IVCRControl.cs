using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IVCRControl
{
    /// <summary>
    /// 播放上一个
    /// </summary>
    void PlayPrev();
    /// <summary>
    /// 播放下一个
    /// </summary>
    void PlayNext();

    /// <summary>
    /// 停止
    /// </summary>
    void Stop();

    /// <summary>
    /// 播放
    /// </summary>
    void Play();

    /// <summary>
    /// 暂停
    /// </summary>
    void Pause();

    /// <summary>
    /// 播放指定视频
    /// </summary>
    /// <param name="moviename">视频文件名</param>
    void PlayMovieByName(string moviename);

    /// <summary>
    /// 拖动进度条 鼠标抬起
    /// </summary>
    /// <param name="value"></param>
    void MovieSeek();
    /// <summary>
    /// 拖动进度条 鼠标按下
    /// </summary>
    void MovieSliderPointerDown();

    /// <summary>
    /// 设置音量
    /// </summary>
    /// <param name="value"></param>
    void VolumnSeek();

    /// <summary>
    /// 获取播放列表
    /// </summary>
    void GetFileList();
    void GetVolumn();
}

