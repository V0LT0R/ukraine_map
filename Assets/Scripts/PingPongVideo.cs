using UnityEngine.Video;
using UnityEngine;

public class CleanLoop : MonoBehaviour
{
    public VideoPlayer vp;

    void Start()
    {
        vp.isLooping = false;
        vp.loopPointReached += RestartSmooth;
        vp.Play();
    }

    void RestartSmooth(VideoPlayer source)
    {
        vp.frame = 0;
        vp.Play();
    }
}
