using UnityEngine;

public class EarthRotation : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 10f; // 调整地球自转速度
    [SerializeField] private Vector3 rotationAxis = Vector3.up; // 地球自转轴，默认为Y轴

    void Update()
    {
        // 绕自转轴旋转地球，乘以Time.deltaTime使旋转与帧率无关
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
    }
}