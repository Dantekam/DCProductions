using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Builds all UI at runtime as world-space canvases that float in front of the player.
/// Provides a Start Menu and a Choice Panel for the branching story.
/// Every button gets a VRFingerButton for hand-poke interaction.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Canvas Positioning")]
    [SerializeField] private Transform cameraRig;
    [SerializeField] private float canvasDistance = 2.5f;
    [SerializeField] private float canvasHeight = 1.4f;
    [SerializeField] private float canvasScale = 0.002f;

    // ── Events ──────────────────────────────────────────────
    public event Action OnStartPressed;
    public event Action<int> OnChoiceSelected;

    // ── Runtime references ──────────────────────────────────
    private Canvas startMenuCanvas;
    private Canvas choiceCanvas;
    private Canvas nodeInfoCanvas;

    private GameObject startMenuPanel;
    private GameObject choicePanel;
    private GameObject nodeInfoPanel;

    private TextMeshProUGUI nodeNameLabel;
    private readonly List<GameObject> choiceButtons = new();

    // ── Colours ─────────────────────────────────────────────
    private readonly Color panelBg        = new Color(0.08f, 0.08f, 0.12f, 0.92f);
    private readonly Color btnNormal      = new Color(0.15f, 0.15f, 0.20f, 0.95f);
    private readonly Color btnHover       = new Color(0.25f, 0.25f, 0.32f, 0.95f);
    private readonly Color btnPress       = new Color(0.06f, 0.50f, 0.90f, 1f);
    private readonly Color startBtnNormal = new Color(0.06f, 0.50f, 0.90f, 1f);
    private readonly Color startBtnHover  = new Color(0.10f, 0.60f, 1.00f, 1f);
    private readonly Color startBtnPress  = new Color(0.04f, 0.35f, 0.70f, 1f);

    // ────────────────────────────────────────────────────────
    #region Lifecycle

    private void Awake()
    {
        if (cameraRig == null)
        {
            var rig = FindObjectOfType<OVRCameraRig>();
            if (rig != null) cameraRig = rig.transform;
        }

        BuildStartMenu();
        BuildChoicePanel();
        BuildNodeInfoPanel();

        ShowStartMenu(true);
        ShowChoicePanel(false);
        ShowNodeInfo(false);
    }

    #endregion

    // ────────────────────────────────────────────────────────
    #region Public API

    public void ShowStartMenu(bool show)
    {
        if (startMenuCanvas != null)
        {
            PositionCanvasInFront(startMenuCanvas.transform);
            startMenuCanvas.gameObject.SetActive(show);
        }
    }

    public void ShowChoicePanel(bool show)
    {
        if (choiceCanvas != null)
        {
            PositionCanvasInFront(choiceCanvas.transform);
            choiceCanvas.gameObject.SetActive(show);
        }
    }

    public void ShowNodeInfo(bool show)
    {
        if (nodeInfoCanvas != null)
        {
            PositionCanvasInFront(nodeInfoCanvas.transform);
            nodeInfoCanvas.gameObject.SetActive(show);
        }
    }

    public void SetNodeName(string text)
    {
        if (nodeNameLabel != null) nodeNameLabel.text = text;
    }

    /// <summary>
    /// Populates the choice panel with buttons for the given StoryNode's choices.
    /// </summary>
    public void PopulateChoices(StoryNode node)
    {
        ClearChoiceButtons();

        if (node == null || node.choices == null || node.choices.Count == 0)
            return;

        for (int i = 0; i < node.choices.Count; i++)
        {
            int index = i; // capture for closure
            StoryChoice choice = node.choices[i];
            GameObject btn = CreateButton(
                choicePanel.transform,
                choice.choiceText,
                new Vector2(500, 70),
                () => OnChoiceSelected?.Invoke(index)
            );
            choiceButtons.Add(btn);
        }

        PositionCanvasInFront(choiceCanvas.transform);
        ShowChoicePanel(true);
    }

    #endregion

    // ────────────────────────────────────────────────────────
    #region Build Start Menu

    private void BuildStartMenu()
    {
        startMenuCanvas = CreateWorldCanvas("StartMenuCanvas");
        startMenuPanel = CreatePanel(startMenuCanvas.transform, "StartMenuPanel", new Vector2(620, 500));

        // Title
        CreateLabel(startMenuPanel.transform, "TitleLabel",
            "CHOOSE YOUR\nADVENTURE",
            new Vector2(560, 180), 52, TextAlignmentOptions.Center,
            new Color(1f, 1f, 1f, 1f));

        // Subtitle
        CreateLabel(startMenuPanel.transform, "SubtitleLabel",
            "An interactive story with embedded video.\nUse your hands to select choices.",
            new Vector2(500, 80), 22, TextAlignmentOptions.Center,
            new Color(0.65f, 0.65f, 0.72f, 1f));

        // Start Button
        GameObject startBtn = CreateButton(
            startMenuPanel.transform, "BEGIN", new Vector2(320, 80),
            () => OnStartPressed?.Invoke()
        );
        var vrBtn = startBtn.GetComponent<VRFingerButton>();
        if (vrBtn != null) vrBtn.SetColors(startBtnNormal, startBtnHover, startBtnPress);

        // Set text white on the start button
        var btnText = startBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (btnText != null) { btnText.color = Color.white; btnText.fontSize = 30; }

        // Layout
        var layout = startMenuPanel.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 20;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.padding = new RectOffset(30, 30, 40, 40);
    }

    #endregion

    // ────────────────────────────────────────────────────────
    #region Build Choice Panel

    private void BuildChoicePanel()
    {
        choiceCanvas = CreateWorldCanvas("ChoiceCanvas");
        choicePanel = CreatePanel(choiceCanvas.transform, "ChoicePanel", new Vector2(580, 500));

        // Title
        CreateLabel(choicePanel.transform, "ChoiceTitle",
            "MAKE YOUR CHOICE",
            new Vector2(520, 60), 34, TextAlignmentOptions.Center,
            new Color(1f, 1f, 1f, 1f));

        // Buttons are added dynamically via PopulateChoices()

        var layout = choicePanel.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 14;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.padding = new RectOffset(30, 30, 20, 30);

        // Add a ContentSizeFitter so the panel grows to fit choices
        var fitter = choicePanel.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    #endregion

    // ────────────────────────────────────────────────────────
    #region Build Node Info Panel (small overlay showing current node name)

    private void BuildNodeInfoPanel()
    {
        nodeInfoCanvas = CreateWorldCanvas("NodeInfoCanvas");
        nodeInfoPanel = CreatePanel(nodeInfoCanvas.transform, "NodeInfoPanel", new Vector2(400, 60));

        nodeNameLabel = CreateLabel(nodeInfoPanel.transform, "NodeNameLabel",
            "",
            new Vector2(380, 50), 24, TextAlignmentOptions.Center,
            new Color(0.85f, 0.85f, 0.9f, 1f));
    }

    #endregion

    // ────────────────────────────────────────────────────────
    #region Factory helpers

    private Canvas CreateWorldCanvas(string canvasName)
    {
        GameObject go = new GameObject(canvasName);
        go.transform.SetParent(transform);

        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 10;

        go.AddComponent<CanvasScaler>();
        go.AddComponent<GraphicRaycaster>();

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(800, 600);
        rt.localScale = Vector3.one * canvasScale;

        return canvas;
    }

    private GameObject CreatePanel(Transform parent, string panelName, Vector2 size)
    {
        GameObject panel = new GameObject(panelName);
        panel.transform.SetParent(parent, false);

        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchoredPosition = Vector2.zero;

        Image bg = panel.AddComponent<Image>();
        bg.color = panelBg;

        // Round corners via sprite type — use default UI sprite
        bg.type = Image.Type.Sliced;

        return panel;
    }

    private TextMeshProUGUI CreateLabel(Transform parent, string labelName,
        string text, Vector2 size, float fontSize,
        TextAlignmentOptions alignment, Color color)
    {
        GameObject go = new GameObject(labelName);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = size;

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = color;
        tmp.enableWordWrapping = true;
        tmp.overflowMode = TextOverflowModes.Ellipsis;

        return tmp;
    }

    private GameObject CreateButton(Transform parent, string label, Vector2 size, Action onClick)
    {
        GameObject btnGo = new GameObject("Btn_" + label);
        btnGo.transform.SetParent(parent, false);

        RectTransform rt = btnGo.AddComponent<RectTransform>();
        rt.sizeDelta = size;

        Image img = btnGo.AddComponent<Image>();
        img.color = btnNormal;
        img.type = Image.Type.Sliced;

        Button btn = btnGo.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() => onClick?.Invoke());

        // Disable the default colour tint so VRFingerButton handles it
        ColorBlock cb = btn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = Color.white;
        cb.pressedColor = Color.white;
        cb.selectedColor = Color.white;
        btn.colors = cb;

        // VR finger poke support
        VRFingerButton vrBtn = btnGo.AddComponent<VRFingerButton>();
        vrBtn.SetColors(btnNormal, btnHover, btnPress);

        // Label
        GameObject textGo = new GameObject("Label");
        textGo.transform.SetParent(btnGo.transform, false);
        RectTransform textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 26;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.9f, 0.9f, 0.95f, 1f);
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Ellipsis;

        // Add a LayoutElement so the button respects sizing in layout groups
        LayoutElement le = btnGo.AddComponent<LayoutElement>();
        le.preferredWidth = size.x;
        le.preferredHeight = size.y;

        return btnGo;
    }

    private void ClearChoiceButtons()
    {
        foreach (var btn in choiceButtons)
        {
            if (btn != null) Destroy(btn);
        }
        choiceButtons.Clear();
    }

    private void PositionCanvasInFront(Transform canvasTransform)
    {
        Transform cam = null;
        if (cameraRig != null)
        {
            // Try to find CenterEyeAnchor
            cam = cameraRig.Find("TrackingSpace/CenterEyeAnchor");
            if (cam == null) cam = Camera.main?.transform;
        }
        else
        {
            cam = Camera.main?.transform;
        }

        if (cam == null) return;

        Vector3 forward = cam.forward;
        forward.y = 0;
        forward.Normalize();
        if (forward == Vector3.zero) forward = Vector3.forward;

        Vector3 pos = cam.position + forward * canvasDistance;
        pos.y = canvasHeight;

        canvasTransform.position = pos;
        canvasTransform.rotation = Quaternion.LookRotation(forward);
    }

    #endregion
}
