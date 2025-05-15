using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class DecalRotationController : MonoBehaviour
{
    [SerializeField] private GameObject decalProjector;
    [SerializeField] private float rotationSpeed = 1.0f; // 每次按键旋转的角度

    private void Start()
    {
        
    }

    private void Update()
    {
        // 检测按键输入
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // 减小 X 轴旋转
            RotateDecalX(-rotationSpeed);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            // 增加 X 轴旋转
            RotateDecalX(rotationSpeed);
        }
    }

    private void RotateDecalX(float angle)
    {
        if (decalProjector != null)
        {
            // 获取当前旋转
            Vector3 currentRotation = transform.localRotation.eulerAngles;
            
            // 修改 X 轴旋转
            currentRotation.x += angle;
            
            // 应用新的旋转
            transform.localRotation = Quaternion.Euler(currentRotation);
            
            // 输出日志以便调试
            Debug.Log($"Decal X Rotation: {currentRotation.x}");
        }
    }
}