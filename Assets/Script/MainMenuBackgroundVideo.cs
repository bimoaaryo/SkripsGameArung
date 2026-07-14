using UnityEngine;
using UnityEngine.Video;


public class MainMenuBackgroundVideo : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    void Start()
    {
        if (videoPlayer == null)
        {
            Debug.LogWarning("[MainMenuBackgroundVideo] VideoPlayer belum di-assign!");
            return;
        }

        videoPlayer.isLooping = true;

        if (!videoPlayer.isPlaying)
            videoPlayer.Play();
    }
}