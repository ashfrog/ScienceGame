using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 此处为了解决3D图表的线条文本显示不出来的问题
/// </summary>
public class EnableByTime : MonoBehaviour
{
    [SerializeField]
    GameObject[] gameObjects;
    [SerializeField]
    float delayTime = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(EnableObj());
    }

    IEnumerator EnableObj()
    {
        foreach (GameObject go in gameObjects)
        {
            yield return new WaitForSeconds(delayTime);
            go.SetActive(true);
        }
    }
}
