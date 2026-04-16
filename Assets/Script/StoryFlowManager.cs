using UnityEngine;

public class StoryFlowManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StoryGraphManager storyGraphManager;
    [SerializeField] private PlaybackManager playbackManager;

    [Header("Starting Node")]
    [SerializeField] private string startingNodeId = "event_zero";

    public StoryNode CurrentNode { get; private set; }

    private void Awake()
    {
        if (storyGraphManager == null)
        {
            Debug.LogError("StoryFlowManager: StoryGraphManager reference is missing.");
            enabled = false;
            return;
        }

        if (playbackManager == null)
        {
            Debug.LogError("StoryFlowManager: PlaybackManager reference is missing.");
            enabled = false;
            return;
        }
    }

    private void OnEnable()
    {
        playbackManager.VideoFinished += HandleVideoFinished;
    }

    private void OnDisable()
    {
        playbackManager.VideoFinished -= HandleVideoFinished;
    }

    private void Start()
    {
        // StartStory();
    }

    public void StartStory()
    {
        StoryNode startNode = storyGraphManager.GetNode(startingNodeId);

        if (startNode == null)
        {
            Debug.LogError($"StoryFlowManager: Could not find starting node '{startingNodeId}'.");
            return;
        }

        LoadNode(startNode);
    }

    public void LoadNodeById(string nodeId)
    {
        StoryNode node = storyGraphManager.GetNode(nodeId);

        if (node == null)
        {
            Debug.LogError($"StoryFlowManager: Could not find node '{nodeId}'.");
            return;
        }

        LoadNode(node);
    }

    public void LoadNode(StoryNode node)
    {
        if (node == null)
        {
            Debug.LogWarning("StoryFlowManager: Tried to load a null node.");
            return;
        }

        if (node.video == null)
        {
            Debug.LogError($"StoryFlowManager: Node '{node.nodeId}' has no video assigned.");
            return;
        }

        CurrentNode = node;
        Debug.Log($"StoryFlowManager: Loading node '{node.nodeId}'.");

        playbackManager.LoadVideo(node.video);
    }

    public void ChooseOption(int choiceIndex)
    {
        if (CurrentNode == null)
        {
            Debug.LogWarning("StoryFlowManager: No current node set.");
            return;
        }

        if (choiceIndex < 0 || choiceIndex >= CurrentNode.choices.Count)
        {
            Debug.LogWarning($"StoryFlowManager: Choice index {choiceIndex} is out of range for node '{CurrentNode.nodeId}'.");
            return;
        }

        StoryChoice selectedChoice = CurrentNode.choices[choiceIndex];

        if (string.IsNullOrWhiteSpace(selectedChoice.nextNodeId))
        {
            Debug.LogWarning($"StoryFlowManager: Choice '{selectedChoice.choiceId}' has no nextNodeId.");
            return;
        }

        LoadNodeById(selectedChoice.nextNodeId);
    }

    private void HandleVideoFinished(VideoEntry finishedVideo)
    {
        if (CurrentNode == null)
        {
            Debug.LogWarning("StoryFlowManager: Video finished but CurrentNode is null.");
            return;
        }

        Debug.Log($"StoryFlowManager: Video finished for node '{CurrentNode.nodeId}'.");

        switch (CurrentNode.triggerMode)
        {
            case ChoiceTriggerMode.AutoContinue:
                if (!string.IsNullOrWhiteSpace(CurrentNode.autoContinueNextNodeId))
                {
                    LoadNodeById(CurrentNode.autoContinueNextNodeId);
                }
                else
                {
                    Debug.LogWarning($"StoryFlowManager: Node '{CurrentNode.nodeId}' is AutoContinue but has no next node.");
                }
                break;

            case ChoiceTriggerMode.OnVideoEnd:
                Debug.Log($"StoryFlowManager: Node '{CurrentNode.nodeId}' is waiting for choice selection.");
                break;

            case ChoiceTriggerMode.AtTime:
                Debug.Log($"StoryFlowManager: Node '{CurrentNode.nodeId}' was set to AtTime; choice UI logic will handle this later.");
                break;
        }
    }
}