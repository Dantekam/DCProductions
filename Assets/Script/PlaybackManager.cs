using UnityEngine;
using UnityEngine.Video;

public class PlaybackManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private VideoCatalogManager catalogManager;

    [Header("Startup")]
    [SerializeField] private bool autoLoadFirstVideo = true;
    [SerializeField] private bool autoPlayWhenPrepared = true;

    private VideoEntry currentVideo;

    private void Awake()
    {
        if (videoPlayer == null)
        {
            Debug.LogError("PlaybackManager: VideoPlayer reference is missing.");
            enabled = false;
            return;
        }

        if (catalogManager == null)
        {
            Debug.LogError("PlaybackManager: VideoCatalogManager reference is missing.");
            enabled = false;
            return;
        }

        videoPlayer.playOnAwake = false;
        videoPlayer.waitForFirstFrame = true;

        videoPlayer.prepareCompleted += OnPrepareCompleted;
        videoPlayer.errorReceived += OnVideoError;
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    private void Start()
    {
        if (autoLoadFirstVideo)
        {
            VideoEntry firstVideo = catalogManager.GetFirstVideo();
            if (firstVideo != null)
            {
                LoadVideo(firstVideo);
            }
        }
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnPrepareCompleted;
            videoPlayer.errorReceived -= OnVideoError;
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }

    public void LoadVideo(VideoEntry entry)
    {
        if (entry == null)
        {
            Debug.LogWarning("PlaybackManager: Tried to load a null VideoEntry.");
            return;
        }

        currentVideo = entry;
        videoPlayer.Stop();

        switch (entry.sourceMode)
        {
            case VideoSourceMode.LocalClip:
                if (entry.localClip == null)
                {
                    Debug.LogError($"PlaybackManager: Local clip missing for '{entry.title}'.");
                    return;
                }

                videoPlayer.source = VideoSource.VideoClip;
                videoPlayer.clip = entry.localClip;
                videoPlayer.url = string.Empty;
                break;

            case VideoSourceMode.Url:
                if (string.IsNullOrWhiteSpace(entry.url))
                {
                    Debug.LogError($"PlaybackManager: URL missing for '{entry.title}'.");
                    return;
                }

                videoPlayer.source = VideoSource.Url;
                videoPlayer.clip = null;
                videoPlayer.url = entry.url;
                break;
        }

        Debug.Log($"Preparing video: {entry.title}");
        videoPlayer.Prepare();
    }

    public void LoadVideoByIndex(int index)
    {
        VideoEntry entry = catalogManager.GetVideoByIndex(index);
        if (entry != null)
        {
            LoadVideo(entry);
        }
    }

    public void PlayVideo()
    {
        if (!videoPlayer.isPrepared)
        {
            Debug.LogWarning("PlaybackManager: Video is not prepared yet.");
            return;
        }

        videoPlayer.Play();
        Debug.Log("Video playback started.");
    }

    public void PauseVideo()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
            Debug.Log("Video paused.");
        }
    }

    public void StopVideo()
    {
        videoPlayer.Stop();
        Debug.Log("Video stopped.");
    }

    public void ResumeVideo()
    {
        if (videoPlayer.isPrepared && !videoPlayer.isPlaying)
        {
            videoPlayer.Play();
            Debug.Log("Video resumed.");
        }
    }

    public void SeekTo(double timeSeconds)
    {
        if (!videoPlayer.isPrepared)
        {
            Debug.LogWarning("PlaybackManager: Cannot seek before prepare completes.");
            return;
        }

        videoPlayer.time = timeSeconds;
        Debug.Log($"Seeked to {timeSeconds:F2} seconds.");
    }

    private void OnPrepareCompleted(VideoPlayer source)
    {
        Debug.Log($"Prepare complete: {currentVideo?.title}");

        if (autoPlayWhenPrepared)
        {
            PlayVideo();
        }
    }

    private void OnVideoError(VideoPlayer source, string message)
    {
        Debug.LogError($"VideoPlayer error: {message}");
    }

    private void OnVideoFinished(VideoPlayer source)
    {
        Debug.Log("Video playback finished.");
    }
}