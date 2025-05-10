using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraMirrorController : MonoBehaviour
{
    [Header("镜像设置")]
    [SerializeField] private bool _flipHorizontal = false;
    [SerializeField] private bool _flipVertical = false;

    // 相机引用
    private Camera _camera;
    // 原始投影矩阵
    private Matrix4x4 _originalProjectionMatrix;

    void Awake()
    {
        _camera = GetComponent<Camera>();
        // 保存原始投影矩阵
        _originalProjectionMatrix = _camera.projectionMatrix;
    }

    void OnEnable()
    {
        // 确保每次启用组件时应用当前的镜像设置
        UpdateProjectionMatrix();
    }

    void OnValidate()
    {
        // 当在Inspector中更改值时更新
        if (_camera != null)
        {
            UpdateProjectionMatrix();
        }
    }

    /// <summary>
    /// 更新相机投影矩阵以应用镜像效果
    /// </summary>
    public void UpdateProjectionMatrix()
    {
        // 从原始矩阵开始
        Matrix4x4 matrix = _originalProjectionMatrix;

        // 应用水平镜像（左右翻转）
        if (_flipHorizontal)
        {
            matrix *= Matrix4x4.Scale(new Vector3(-1, 1, 1));
        }

        // 应用垂直镜像（上下翻转）
        if (_flipVertical)
        {
            matrix *= Matrix4x4.Scale(new Vector3(1, -1, 1));
        }

        // 应用修改后的投影矩阵
        _camera.projectionMatrix = matrix;
    }

    /// <summary>
    /// 设置水平镜像（左右翻转）
    /// </summary>
    public void SetHorizontalFlip(bool flip)
    {
        _flipHorizontal = flip;
        UpdateProjectionMatrix();
    }

    /// <summary>
    /// 设置垂直镜像（上下翻转）
    /// </summary>
    public void SetVerticalFlip(bool flip)
    {
        _flipVertical = flip;
        UpdateProjectionMatrix();
    }

    /// <summary>
    /// 获取当前水平镜像状态
    /// </summary>
    public bool GetHorizontalFlip()
    {
        return _flipHorizontal;
    }

    /// <summary>
    /// 获取当前垂直镜像状态
    /// </summary>
    public bool GetVerticalFlip()
    {
        return _flipVertical;
    }

    /// <summary>
    /// 切换水平镜像状态
    /// </summary>
    public void ToggleHorizontalFlip()
    {
        _flipHorizontal = !_flipHorizontal;
        UpdateProjectionMatrix();
    }

    /// <summary>
    /// 切换垂直镜像状态
    /// </summary>
    public void ToggleVerticalFlip()
    {
        _flipVertical = !_flipVertical;
        UpdateProjectionMatrix();
    }
}