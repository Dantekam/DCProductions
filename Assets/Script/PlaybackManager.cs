using System;
using UnityEngine;
using UnityEngine.Video;

public class PlaybackManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Startup")]
    [SerializeField] private bool autoPlayWhenPrepared = true;

    public VideoEntry CurrentVideo => currentVideo;
    public bool IsPrepared => videoPlayer != null && videoPlayer.isPrepared;
    public bool IsPlaying => videoPlayer != null && videoPlayer.isPlaying;

    public event Action<VideoEntry> VideoPrepareStarted;
    public event Action<VideoEntry> VideoPrepared;
    public event Action<VideoEntry> VideoStarted;
    public event Action<VideoEntry> VideoPaused;
    public event Action<VideoEntry> VideoFinished;
    public event Action<VideoEntry, string> VideoError;

    public double CurrentTime => videoPlayer != null ? videoPlayer.time : 0;
    public double CurrentLength => videoPlayer != null ? videoPlayer.length : 0;

    private VideoEntry currentVideo;

    private void Awake()
    {
        if (videoPlayer == null)
        {
            Debug.LogError("PlaybackManager: VideoPlayer reference is missing.");
            enabled = false;
            return;
        }

        videoPlayer.playOnAwake = false;
        videoPlayer.waitForFirstFrame = true;

        videoPlayer.prepareCompleted += OnPrepareCompleted;
        videoPlayer.errorReceived += OnVideoError;
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    private void OnDestroy()
    {
        if (videoPlayer == null)
            return;

        videoPlayer.prepareCompleted -= OnPrepareCompleted;
        videoPlayer.errorReceived -= OnVideoError;
        videoPlayer.loopPointReached -= OnVideoFinished;
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
                    string message = $"PlaybackManager: Local clip missing for '{entry.title}'.";
                    Debug.LogError(message);
                    VideoError?.Invoke(entry, message);
                    return;
                }

                videoPlayer.source = VideoSource.VideoClip;
                videoPlayer.clip = entry.localClip;
                videoPlayer.url = string.Empty;
                break;

            case VideoSourceMode.Url:
                if (string.IsNullOrWhiteSpace(entry.url))
                {
                    string message = $"PlaybackManager: URL missing for '{entry.title}'.";
                    Debug.LogError(message);
                    VideoError?.Invoke(entry, message);
                    return;
                }

                videoPlayer.source = VideoSource.Url;
                videoPlayer.clip = null;
                videoPlayer.url = entry.url;
                break;
        }

        Debug.Log($"Preparing video: {entry.title}");
        VideoPrepareStarted?.Invoke(entry);
        videoPlayer.Prepare();
    }

    public void PlayVideo()
    {
        if (!videoPlayer.isPrepared)
        {
            Debug.LogWarning("PlaybackManager: Video is not prepared yet.");
            return;
        }

        videoPlayer.Play();
        Debug.Log($"Video playback started: {currentVideo?.title}");
        VideoStarted?.Invoke(currentVideo);
    }

    public void PauseVideo()
    {
        if (!videoPlayer.isPlaying)
            return;

        videoPlayer.Pause();
        Debug.Log($"Video paused: {currentVideo?.title}");
        VideoPaused?.Invoke(currentVideo);
    }

    public void TogglePlayPause()
    {
        if (!videoPlayer.isPrepared)
        {
            Debug.LogWarning("PlaybackManager: Cannot toggle play/pause before prepare completes.");
            return;
        }

        if (videoPlayer.isPlaying)
            PauseVideo();
        else
            PlayVideo();
    }

    public void SeekTo(double timeSeconds)
    {
        if (!videoPlayer.isPrepared)
        {
            Debug.LogWarning("PlaybackManager: Cannot seek before prepare completes.");
            return;
        }

        videoPlayer.time = timeSeconds;
        Debug.Log($"Seeked to {timeSeconds:F2} seconds in {currentVideo?.title}");
    }

    private void OnPrepareCompleted(VideoPlayer source)
    {
        Debug.Log($"Prepare complete: {currentVideo?.title}");
        VideoPrepared?.Invoke(currentVideo);

        if (autoPlayWhenPrepared)
        {
            PlayVideo();
        }
    }

    private void OnVideoError(VideoPlayer source, string message)
    {
        Debug.LogError($"VideoPlayer error on '{currentVideo?.title}': {message}");
        VideoError?.Invoke(currentVideo, message);
    }

    private void OnVideoFinished(VideoPlayer source)
    {
        Debug.Log($"Video playback finished: {currentVideo?.title}");
        VideoFinished?.Invoke(currentVideo);
    }
}