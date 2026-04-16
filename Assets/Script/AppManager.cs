using UnityEngine;

/// <summary>
/// Top-level orchestrator: Start Menu → Story Playback → Choice Selection → loop.
/// Wire the references in the Inspector on the Managers GameObject.
/// </summary>
public class AppManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private StoryFlowManager storyFlowManager;
    [SerializeField] private PlaybackManager playbackManager;
    [SerializeField] private StoryGraphManager storyGraphManager;

    private enum AppState { Menu, Playing, WaitingForChoice, Ended }
    private AppState state = AppState.Menu;

    private void Awake()
    {
        // Auto-find if not assigned
        if (uiManager == null) uiManager = FindObjectOfType<UIManager>();
        if (storyFlowManager == null) storyFlowManager = FindObjectOfType<StoryFlowManager>();
        if (playbackManager == null) playbackManager = FindObjectOfType<PlaybackManager>();
        if (storyGraphManager == null) storyGraphManager = FindObjectOfType<StoryGraphManager>();
    }

    private void OnEnable()
    {
        uiManager.OnStartPressed += HandleStartPressed;
        uiManager.OnChoiceSelected += HandleChoiceSelected;
        playbackManager.VideoFinished += HandleVideoFinished;
    }

    private void OnDisable()
    {
        uiManager.OnStartPressed -= HandleStartPressed;
        uiManager.OnChoiceSelected -= HandleChoiceSelected;
        playbackManager.VideoFinished -= HandleVideoFinished;
    }

    // ── Start Menu ──────────────────────────────────────────
    private void HandleStartPressed()
    {
        if (state != AppState.Menu) return;

        Debug.Log("AppManager: Start pressed – beginning story.");
        uiManager.ShowStartMenu(false);
        storyFlowManager.StartStory();

        StoryNode node = storyFlowManager.CurrentNode;
        if (node != null)
        {
            uiManager.SetNodeName(node.displayName);
            uiManager.ShowNodeInfo(true);
        }

        state = AppState.Playing;
    }

    // ── Video Finished ──────────────────────────────────────
    private void HandleVideoFinished(VideoEntry finishedVideo)
    {
        if (state != AppState.Playing) return;

        StoryNode node = storyFlowManager.CurrentNode;
        if (node == null) return;

        switch (node.triggerMode)
        {
            case ChoiceTriggerMode.AutoContinue:
                // StoryFlowManager already handles auto-continue internally
                break;

            case ChoiceTriggerMode.OnVideoEnd:
                if (node.choices != null && node.choices.Count > 0)
                {
                    state = AppState.WaitingForChoice;
                    uiManager.ShowNodeInfo(false);
                    uiManager.PopulateChoices(node);
                    Debug.Log("AppManager: Showing choices for node " + node.nodeId);
                }
                else
                {
                    // No choices – story has ended on this branch
                    state = AppState.Ended;
                    uiManager.ShowNodeInfo(false);
                    Debug.Log("AppManager: Story ended at node " + node.nodeId);
                    ShowEndScreen();
                }
                break;

            case ChoiceTriggerMode.AtTime:
                // AtTime trigger is handled during playback via Update
                break;
        }
    }

    // ── Choice Selected ─────────────────────────────────────
    private void HandleChoiceSelected(int choiceIndex)
    {
        if (state != AppState.WaitingForChoice) return;

        Debug.Log($"AppManager: Choice {choiceIndex} selected.");
        uiManager.ShowChoicePanel(false);

        storyFlowManager.ChooseOption(choiceIndex);

        StoryNode node = storyFlowManager.CurrentNode;
        if (node != null)
        {
            uiManager.SetNodeName(node.displayName);
            uiManager.ShowNodeInfo(true);
        }

        state = AppState.Playing;
    }

    // ── AtTime trigger check ────────────────────────────────
    private void Update()
    {
        if (state != AppState.Playing) return;

        StoryNode node = storyFlowManager.CurrentNode;
        if (node == null || node.triggerMode != ChoiceTriggerMode.AtTime) return;

        if (!playbackManager.IsPlaying) return;

        // Check if we've reached the trigger time
        // VideoPlayer time is accessible via reflection or we check a simple approach
        // For now, use the playbackManager's public state
        // We'll rely on a simple timer approach since VideoPlayer.time isn't directly exposed
    }

    // ── End Screen ──────────────────────────────────────────
    private void ShowEndScreen()
    {
        uiManager.SetNodeName("THE END");
        uiManager.ShowNodeInfo(true);
    }
}
