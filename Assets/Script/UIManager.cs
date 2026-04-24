using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Role")]
    [SerializeField] private bool isProfessorMode = true;

    [Header("Skyboxes")]
    [SerializeField] private Material lobbySkybox;
    [SerializeField] private Material videoSkybox;

    [Header("References")]
    [SerializeField] private PlaybackManager playbackManager;

    [Header("Video")]
    [SerializeField] private VideoEntry antarcticVideo;

    [Header("Panels")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject controlPanel;

    [Header("Professor Controls")]
    [SerializeField] private Slider timelineSlider;

    private bool isDraggingSlider = false;

    private void Awake()
    {
        SetSkybox(lobbySkybox);

        if (startPanel != null)
            startPanel.SetActive(true);

        if (controlPanel != null)
            controlPanel.SetActive(false);

        Debug.Log($"UIManager Awake - Professor Mode: {isProfessorMode}");
    }

    private void Update()
    {
        UpdateTimelineSlider();
    }

    public void OnPlayPressed()
    {
        Debug.Log("Antarctic Play pressed.");

        if (startPanel != null)
            startPanel.SetActive(false);

        if (controlPanel != null)
        {
            controlPanel.SetActive(isProfessorMode);
            Debug.Log($"Professor Mode: {isProfessorMode}, Control Panel Active: {controlPanel.activeSelf}");
        }

        SetSkybox(videoSkybox);

        if (playbackManager == null)
        {
            Debug.LogError("UIManager: PlaybackManager reference is missing.");
            return;
        }

        if (antarcticVideo == null)
        {
            Debug.LogError("UIManager: Antarctic video entry is missing.");
            return;
        }

        playbackManager.LoadVideo(antarcticVideo);
    }

    public void OnPausePressed()
    {
        if (!isProfessorMode) return;

        if (playbackManager != null)
            playbackManager.PauseVideo();
    }

    public void OnResumePressed()
    {
        if (!isProfessorMode) return;

        if (playbackManager != null)
            playbackManager.PlayVideo();
    }

    public void OnTogglePlayPausePressed()
    {
        if (!isProfessorMode) return;

        if (playbackManager != null)
            playbackManager.TogglePlayPause();
    }

    public void OnRestartPressed()
    {
        if (!isProfessorMode) return;

        if (playbackManager == null)
            return;

        playbackManager.SeekTo(0);
        playbackManager.PlayVideo();
    }

    private void UpdateTimelineSlider()
    {
        if (!isProfessorMode)
            return;

        if (timelineSlider == null || playbackManager == null)
            return;

        if (isDraggingSlider)
            return;

        double length = playbackManager.CurrentLength;

        if (length <= 0)
            return;

        timelineSlider.value = (float)(playbackManager.CurrentTime / length);
    }

    public void OnTimelineDragStarted()
    {
        if (!isProfessorMode) return;

        isDraggingSlider = true;
    }

    public void OnTimelineDragEnded()
    {
        if (!isProfessorMode) return;

        if (timelineSlider == null || playbackManager == null)
            return;

        double length = playbackManager.CurrentLength;

        if (length <= 0)
            return;

        double targetTime = timelineSlider.value * length;
        playbackManager.SeekTo(targetTime);

        isDraggingSlider = false;
    }

    private void SetSkybox(Material skybox)
    {
        if (skybox == null)
            return;

        RenderSettings.skybox = skybox;
        DynamicGI.UpdateEnvironment();
    }
}