using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
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
    [SerializeField] private TMP_Text choiceButtonAText;
    [SerializeField] private TMP_Text choiceButtonBText;

    private void Awake()
    {
        ShowStartPanel();
    }

    private void OnEnable()
    {
        if (playbackManager != null)
        {
            playbackManager.VideoFinished += HandleVideoFinished;
        }
    }

    private void OnDisable()
    {
        if (playbackManager != null)
        {
            playbackManager.VideoFinished -= HandleVideoFinished;
        }
    }

    public void OnPlayPressed()
    {
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

        if (node.choices.Count > 0)
        {
            choiceButtonA.gameObject.SetActive(true);
            choiceButtonAText.text = node.choices[0].choiceText;

            choiceButtonA.onClick.RemoveAllListeners();
            choiceButtonA.onClick.AddListener(() =>
            {
                choicePanel.SetActive(false);
                storyFlowManager.ChooseOption(0);
            });
        }
        else
        {
            choiceButtonA.gameObject.SetActive(false);
        }

        if (node.choices.Count > 1)
        {
            choiceButtonB.gameObject.SetActive(true);
            choiceButtonBText.text = node.choices[1].choiceText;

            choiceButtonB.onClick.RemoveAllListeners();
            choiceButtonB.onClick.AddListener(() =>
            {
                choicePanel.SetActive(false);
                storyFlowManager.ChooseOption(1);
            });
        }
        else
        {
            choiceButtonB.gameObject.SetActive(false);
        }
    }
}