using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to a world-space UI Button to allow VR finger-poke interaction.
/// Detects index-finger-tip collision with the button's BoxCollider and
/// invokes the Button.onClick event when the finger pushes through.
/// Works alongside standard controller raycasting.
/// </summary>
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(BoxCollider))]
public class VRFingerButton : MonoBehaviour
{
    [Header("Visual Feedback")]
    [SerializeField] private Color normalColor = new Color(0.18f, 0.18f, 0.22f, 0.95f);
    [SerializeField] private Color hoverColor = new Color(0.28f, 0.28f, 0.35f, 0.95f);
    [SerializeField] private Color pressColor = new Color(0.08f, 0.55f, 0.95f, 1f);

    [Header("Press Settings")]
    [SerializeField] private float pressDepth = 0.015f;
    [SerializeField] private float cooldown = 0.4f;

    private Button button;
    private Image image;
    private BoxCollider boxCollider;
    private float lastPressTime = -1f;
    private bool fingerInside = false;
    private float fingerEntryZ;

    // Store all discovered hand+skeleton pairs (no need to distinguish left/right)
    private readonly List<HandPair> handPairs = new();
    private float lastSearchTime = -1f;
    private const float SearchInterval = 2f;

    private struct HandPair
    {
        public OVRHand hand;
        public OVRSkeleton skeleton;
    }

    private void Awake()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();
        boxCollider = GetComponent<BoxCollider>();
        boxCollider.isTrigger = true;

        // Auto-size the collider to match the RectTransform
        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null)
        {
            Vector2 size = rt.sizeDelta;
            boxCollider.size = new Vector3(size.x, size.y, pressDepth * 4f);
            boxCollider.center = new Vector3(0, 0, -pressDepth * 2f);
        }
    }

    private void Start()
    {
        FindHands();
        if (image != null)
            image.color = normalColor;
    }

    private void Update()
    {
        // Re-search periodically if we haven't found any hands yet
        if (handPairs.Count == 0 && Time.time - lastSearchTime > SearchInterval)
            FindHands();

        bool anyFingerNear = false;
        Vector3 fingerPos = Vector3.zero;

        for (int i = 0; i < handPairs.Count; i++)
        {
            if (TryGetFingerTip(handPairs[i].hand, handPairs[i].skeleton, out Vector3 tip))
            {
                if (boxCollider.bounds.Contains(tip))
                {
                    anyFingerNear = true;
                    fingerPos = tip;
                    break;
                }
            }
        }

        HandleFingerState(anyFingerNear, fingerPos);
    }

    private void HandleFingerState(bool isInside, Vector3 fingerWorldPos)
    {
        if (isInside && !fingerInside)
        {
            // Finger just entered
            fingerInside = true;
            fingerEntryZ = transform.InverseTransformPoint(fingerWorldPos).z;
            if (image != null) image.color = hoverColor;
        }
        else if (!isInside && fingerInside)
        {
            // Finger left
            fingerInside = false;
            if (image != null) image.color = normalColor;
        }

        if (fingerInside)
        {
            float localZ = transform.InverseTransformPoint(fingerWorldPos).z;
            float pushed = fingerEntryZ - localZ;

            if (pushed >= pressDepth && Time.time - lastPressTime > cooldown)
            {
                lastPressTime = Time.time;
                if (image != null) image.color = pressColor;
                button.onClick.Invoke();
                Invoke(nameof(ResetColor), 0.15f);
            }
        }
    }

    private void ResetColor()
    {
        if (image != null)
            image.color = fingerInside ? hoverColor : normalColor;
    }

    private bool TryGetFingerTip(OVRHand hand, OVRSkeleton skeleton, out Vector3 tipPos)
    {
        tipPos = Vector3.zero;

        if (hand == null || skeleton == null)
            return false;

        if (!hand.IsTracked || hand.HandConfidence == OVRHand.TrackingConfidence.Low)
            return false;

        var bones = skeleton.Bones;
        if (bones == null || bones.Count == 0)
            return false;

        // Index fingertip is typically the last index bone
        foreach (var bone in bones)
        {
            if (bone == null) continue;
            if (bone.Id == OVRSkeleton.BoneId.Hand_IndexTip)
            {
                Transform boneTransform = bone.Transform;
                if (boneTransform == null) continue;
                tipPos = boneTransform.position;
                return true;
            }
        }

        return false;
    }

    private void FindHands()
    {
        lastSearchTime = Time.time;
        handPairs.Clear();

        OVRHand[] hands = FindObjectsOfType<OVRHand>();
        foreach (var h in hands)
        {
            OVRSkeleton skel = h.GetComponent<OVRSkeleton>();
            if (skel != null)
            {
                handPairs.Add(new HandPair { hand = h, skeleton = skel });
            }
        }
    }

    /// <summary>
    /// Set the visual colors from code (used by UIManager when building buttons).
    /// </summary>
    public void SetColors(Color normal, Color hover, Color press)
    {
        normalColor = normal;
        hoverColor = hover;
        pressColor = press;
        if (image != null) image.color = normalColor;
    }
}
