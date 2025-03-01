using UnityEngine;

public class TelegraphSound : MonoBehaviour
{
    public AudioSource telegraphSound; // 拖拽你的AudioSource组件到这个字段

    void Update()
    {
        if (Input.GetKeyDown(KeyCodeInput.keyCode)) // 按下鼠标左键
        {
            PlayTelegraphSound();
        }
        else if (Input.GetKeyUp(KeyCodeInput.keyCode)) // 松开鼠标左键
        {
            StopTelegraphSound();
        }
    }

    void PlayTelegraphSound()
    {
        if (!telegraphSound.isPlaying)
        {
            telegraphSound.Play();
        }
    }

    void StopTelegraphSound()
    {
        if (telegraphSound.isPlaying)
        {
            telegraphSound.Stop();
        }
    }
}