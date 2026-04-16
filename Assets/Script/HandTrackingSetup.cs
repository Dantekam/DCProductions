using UnityEngine;

/// <summary>
/// Attach this to the CameraRig GameObject.
/// On Awake it ensures OVRHand + OVRSkeleton components exist on the hand anchors
/// so that VRFingerButton can find finger-tip positions at runtime.
/// If OVRHand components are already present (e.g. via a prefab or manual setup),
/// this script does nothing.
///
/// If you prefer full control, add OVRHand + OVRSkeleton to LeftHandAnchor
/// and RightHandAnchor manually in the scene and you can remove this script.
/// </summary>
public class HandTrackingSetup : MonoBehaviour
{
    private void Awake()
    {
        OVRCameraRig rig = GetComponent<OVRCameraRig>();
        if (rig == null)
        {
            rig = GetComponentInParent<OVRCameraRig>();
        }

        if (rig == null)
        {
            Debug.LogWarning("HandTrackingSetup: No OVRCameraRig found.");
            return;
        }

        Transform trackingSpace = rig.transform.Find("TrackingSpace");
        if (trackingSpace == null)
        {
            Debug.LogWarning("HandTrackingSetup: No TrackingSpace found under CameraRig.");
            return;
        }

        EnsureHandComponents(trackingSpace, "LeftHandAnchor");
        EnsureHandComponents(trackingSpace, "RightHandAnchor");

        Debug.Log("HandTrackingSetup: Hand tracking components verified.");
    }

    private void EnsureHandComponents(Transform trackingSpace, string anchorName)
    {
        Transform anchor = trackingSpace.Find(anchorName);
        if (anchor == null)
        {
            Debug.LogWarning($"HandTrackingSetup: Could not find {anchorName}.");
            return;
        }

        // OVRHand — required for hand tracking state
        if (anchor.GetComponent<OVRHand>() == null)
        {
            anchor.gameObject.AddComponent<OVRHand>();
            Debug.Log($"HandTrackingSetup: Added OVRHand to {anchorName}.");
        }

        // OVRSkeleton — required for bone / finger-tip positions
        if (anchor.GetComponent<OVRSkeleton>() == null)
        {
            anchor.gameObject.AddComponent<OVRSkeleton>();
            Debug.Log($"HandTrackingSetup: Added OVRSkeleton to {anchorName}.");
        }
    }
}
