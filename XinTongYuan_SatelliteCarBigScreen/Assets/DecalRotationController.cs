using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class DecalRotationController : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 1.0f; // 每次按键旋转的角度
    [SerializeField] private float yzSpeed = 0.5f; // 每次按键旋转的角度

    private bool debugProjector;

    [SerializeField]
    DecalProjector decalProjector;



    private void Start()
    {
        Settings.ini.Game.debugProjector = Settings.ini.Game.debugProjector;
        debugProjector = Settings.ini.Game.debugProjector;
        if (decalProjector == null)
        {
            decalProjector = GetComponent<DecalProjector>();
        }
    }

    private void Update()
    {
        if (debugProjector)
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
            else if (Input.GetKeyDown(KeyCode.A))
            {
                decalProjector.size = new Vector3(decalProjector.size.x, decalProjector.size.y + yzSpeed, decalProjector.size.z);
                Settings.ini.Game.ProjectorY = decalProjector.size.y;
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                decalProjector.size = new Vector3(decalProjector.size.x, decalProjector.size.y - yzSpeed, decalProjector.size.z);
                Settings.ini.Game.ProjectorY = decalProjector.size.y;
            }
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                decalProjector.size = new Vector3(decalProjector.size.x, decalProjector.size.y, decalProjector.size.z + yzSpeed);
                Settings.ini.Game.ProjectorZ = decalProjector.size.z;
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                decalProjector.size = new Vector3(decalProjector.size.x, decalProjector.size.y, decalProjector.size.z - yzSpeed);
                Settings.ini.Game.ProjectorZ = decalProjector.size.z;
            }
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

            Settings.ini.Game.ProjectorRX = currentRotation.x;
        }
    }
}