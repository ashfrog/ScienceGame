using UnityEngine;
using RenderHeads.Media.AVProVideo;

// This script assumes you have AVProVideo V1 imported:
// https://assetstore.unity.com/packages/tools/video/avpro-video-56355
// Attach this script to the same GameObject as the AVPro MediaPlayer component (or anywhere suitable).
// Assign the TVNoise material to 'tvNoiseMaterial' and the MediaPlayer to 'mediaPlayer'.
// OnRenderImage will read from the video texture and apply the TV noise effect via shader.
[RequireComponent(typeof(MediaPlayer))]
[ExecuteInEditMode]
public class TVNoiseWithAVPro : MonoBehaviour
{
    [Tooltip("The TV noise post-processing material.")]
    [SerializeField] private Material tvNoiseMaterial;

    [Tooltip("Reference to the AVPro Video MediaPlayer.")]
    [SerializeField] private MediaPlayer mediaPlayer;

    // Temporary reference to the source texture from AVPro
    private RenderTexture videoTexture;

    private void OnValidate()
    {
        // Automatically grab the MediaPlayer if not assigned
        if (mediaPlayer == null)
        {
            mediaPlayer = GetComponent<MediaPlayer>();
        }
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        // If we have a valid material and a video texture from AVPro
        if (tvNoiseMaterial != null && mediaPlayer != null)
        {
            // Ensure the texture is ready
            Texture avproTexture = mediaPlayer.TextureProducer?.GetTexture();
            if (avproTexture != null)
            {
                // If the AVPro texture gets updated (size, format), rebuild videoTexture
                if (videoTexture == null ||
                    videoTexture.width != avproTexture.width ||
                    videoTexture.height != avproTexture.height)
                {
                    RecreateVideoTexture(avproTexture);
                }

                // Copy the AVPro texture to our videoTexture
                Graphics.Blit(avproTexture, videoTexture);

                // Assign the videoTexture to the noise material as _MainTex
                tvNoiseMaterial.SetTexture("_MainTex", videoTexture);

                // Finally, apply noise effect on screen
                Graphics.Blit(src, dest, tvNoiseMaterial);
                return;
            }
        }
        // If something is missing, just pass the original src to dest
        Graphics.Blit(src, dest);
    }

    private void RecreateVideoTexture(Texture avproTexture)
    {
        if (videoTexture != null)
        {
            videoTexture.Release();
            videoTexture = null;
        }

        videoTexture = new RenderTexture(avproTexture.width, avproTexture.height, 0,
            RenderTextureFormat.ARGB32);
        videoTexture.Create();
    }
}