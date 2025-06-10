using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowPanel : MonoBehaviour
{
    [SerializeField]
    string msg;
    private void OnEnable()
    {
        Manager._ins?._mouseTouchInputManager?.clientController?.Send(DataTypeEnum.LG20001, OrderTypeEnum.Str, msg);
    }
}
