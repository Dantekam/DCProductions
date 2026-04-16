using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    [Header("Skyboxes")]
    [SerializeField] private Material lobbySkybox;
    [SerializeField] private Material videoSkybox;

    [Header("References")]
    [SerializeField] private StoryFlowManager storyFlowManager;
    [SerializeField] private PlaybackManager playbackManager;

    [Header("Panels")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject choicePanel;

    [Header("Choice UI")]
    [SerializeField] private TMP_Text choicePromptText;

    [SerializeField] private Button choiceButtonA;
    [SerializeField] private Button choiceButtonB;
    [SerializeField] private Button choiceButtonC;

    [SerializeField] private TMP_Text choiceButtonAText;
    [SerializeField] private TMP_Text choiceButtonBText;
    [SerializeField] private TMP_Text choiceButtonCText;

    private bool choiceShownForCurrentNode = false;
    private string lastNodeId = "";

    private void Awake()
    {
        ShowStartPanel();

        if (lobbySkybox != null)
        {
            RenderSettings.skybox = lobbySkybox;
            DynamicGI.UpdateEnvironment();
        }
    }

    private void Update()
    {
        StoryNode currentNode = storyFlowManager.CurrentNode;
        if (currentNode == null || playbackManager == null)
            return;

        if (currentNode.nodeId != lastNodeId)
        {
            lastNodeId = currentNode.nodeId;
            choiceShownForCurrentNode = false;
        }

        if (choiceShownForCurrentNode)
            return;

        if (currentNode.triggerMode == ChoiceTriggerMode.AtTime)
        {
            if (playbackManager.IsPrepared && playbackManager.IsPlaying &&
                playbackManager.CurrentTime >= currentNode.triggerTimeSeconds)
            {
                if (currentNode.choices != null && currentNode.choices.Count > 0)
                {
                    playbackManager.PauseVideo();
                    ShowChoicePanel(currentNode);
                    choiceShownForCurrentNode = true;
                }
            }
        }
    }

    private void OnEnable()
    {
        if (playbackManager != null)
        {
            playbackManager.VideoFinished += HandleVideoFinished;
            playbackManager.VideoStarted += HandleVideoStarted;
        }
    }

    private void OnDisable()
    {
        if (playbackManager != null)
        {
            playbackManager.VideoFinished -= HandleVideoFinished;
            playbackManager.VideoStarted -= HandleVideoStarted;
        }
    }

    private void HandleVideoStarted(VideoEntry startedVideo)
    {
        StoryNode currentNode = storyFlowManager.CurrentNode;
        if (currentNode != null)
        {
            lastNodeId = currentNode.nodeId;
            choiceShownForCurrentNode = false;
        }
    }

    public void OnPlayPressed()
    {
        Debug.Log("Play button pressed.");

        if (videoSkybox != null)
        {
            RenderSettings.skybox = videoSkybox;
            DynamicGI.UpdateEnvironment();
        }

        HideAllPanels();
        storyFlowManager.StartStory();
    }

    private void HandleVideoFinished(VideoEntry finishedVideo)
    {
        StoryNode currentNode = storyFlowManager.CurrentNode;
        if (currentNode == null)
            return;

        if (currentNode.triggerMode == ChoiceTriggerMode.AutoContinue)
            return;

        if (currentNode.triggerMode == ChoiceTriggerMode.OnVideoEnd)
        {
            if (currentNode.choices == null || currentNode.choices.Count == 0)
            {
                Debug.Log("UIManager: No choices to show for this node.");
                return;
            }

            ShowChoicePanel(currentNode);
            choiceShownForCurrentNode = true;
        }
    }

    public void ShowStartPanel()
    {
        if (startPanel != null) startPanel.SetActive(true);
        if (choicePanel != null) choicePanel.SetActive(false);
    }

    public void HideAllPanels()
    {
        if (startPanel != null) startPanel.SetActive(false);
        if (choicePanel != null) choicePanel.SetActive(false);
    }

    public void ShowChoicePanel(StoryNode node)
    {
        if (choicePanel == null) return;

        choicePanel.SetActive(true);

        if (choicePromptText != null)
            choicePromptText.text = "Choose what happens next";

        SetupChoiceButton(choiceButtonA, choiceButtonAText, node, 0);
        SetupChoiceButton(choiceButtonB, choiceButtonBText, node, 1);
        SetupChoiceButton(choiceButtonC, choiceButtonCText, node, 2);
    }

    private void SetupChoiceButton(Button button, TMP_Text buttonText, StoryNode node, int index)
    {
        if (button == null || buttonText == null)
            return;

        if (node.choices != null && index < node.choices.Count)
        {
            button.gameObject.SetActive(true);

            // Show the choice text from the node setup
            buttonText.text = node.choices[index].choiceText;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                choicePanel.SetActive(false);
                storyFlowManager.ChooseOption(index);
            });
        }
        else
        {
            button.gameObject.SetActive(false);
        }
    }
}