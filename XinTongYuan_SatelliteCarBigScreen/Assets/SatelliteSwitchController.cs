using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SatelliteSwitchController : MonoBehaviour
{
    public Transform earth; // 地球模型
    public List<Transform> satellites; // 卫星模型列表
    public Camera mainCamera; // 主摄像机
    public float animationDuration = 2f; // 动画持续时间
    public TabSwitcher tabSwitcher;

    void Start()
    {

    }

    private void OnEnable()
    {
        // 初始化摄像机位置，让其显示地球和卫星全貌

        SwitchToSatellite(0);
    }

    public void SwitchToSatellite(int satelliteIndex)
    {
        tabSwitcher.SwitchTab(satelliteIndex);
        mainCamera.transform.position = new Vector3(0, 0, -50);
        mainCamera.transform.LookAt(satellites[satelliteIndex].position);
        earth.gameObject.SetActive(false);


        // 计算摄像机目标位置（卫星方向上的一个点）
        Vector3 targetPosition = new Vector3(0, 0, -1);

        // 动画：摄像机从当前位置移动到目标位置
        mainCamera.transform.DOMove(targetPosition, animationDuration).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            // 动画完成后，让摄像机始终看向地球
            mainCamera.transform.LookAt(earth.position);
        });
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchToSatellite(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchToSatellite(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchToSatellite(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchToSatellite(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SwitchToSatellite(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) SwitchToSatellite(5);
        if (Input.GetKeyDown(KeyCode.Alpha7)) SwitchToSatellite(6);
        if (Input.GetKeyDown(KeyCode.Alpha8)) SwitchToSatellite(7);
    }

}