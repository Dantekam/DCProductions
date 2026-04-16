using UnityEngine;

/// <summary>
/// Attach this to the CameraRig GameObject.
/// On Awake it ensures OVRHand + OVRSkeleton components exist on the hand anchors
/// so that VRFingerButton can find finger-tip positions at runtime.
/// If OVRHand components are already present (e.g. via prefab), this is a no-op.
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

        SetupHand(trackingSpace, "LeftHandAnchor", OVRHand.Hand.HandLeft,
            OVRSkeleton.SkeletonType.HandLeft, OVRMesh.MeshType.HandLeft);
        SetupHand(trackingSpace, "RightHandAnchor", OVRHand.Hand.HandRight,
            OVRSkeleton.SkeletonType.HandRight, OVRMesh.MeshType.HandRight);

        Debug.Log("HandTrackingSetup: Hand tracking components verified.");
    }

    private void SetupHand(Transform trackingSpace, string anchorName,
        OVRHand.Hand handType, OVRSkeleton.SkeletonType skelType, OVRMesh.MeshType meshType)
    {
        Transform anchor = trackingSpace.Find(anchorName);
        if (anchor == null)
        {
            Debug.LogWarning($"HandTrackingSetup: Could not find {anchorName}.");
            return;
        }

        // OVRHand
        OVRHand hand = anchor.GetComponent<OVRHand>();
        if (hand == null)
        {
            hand = anchor.gameObject.AddComponent<OVRHand>();
        }
        hand.HandType = handType;

        // OVRSkeleton
        OVRSkeleton skeleton = anchor.GetComponent<OVRSkeleton>();
        if (skeleton == null)
        {
            skeleton = anchor.gameObject.AddComponent<OVRSkeleton>();
        }

        // OVRMesh (optional but enables mesh rendering)
        OVRMesh mesh = anchor.GetComponent<OVRMesh>();
        if (mesh == null)
        {
            mesh = anchor.gameObject.AddComponent<OVRMesh>();
        }

        // OVRMeshRenderer (optional — shows hand mesh)
        OVRMeshRenderer meshRenderer = anchor.GetComponent<OVRMeshRenderer>();
        if (meshRenderer == null)
        {
            anchor.gameObject.AddComponent<OVRMeshRenderer>();
        }
    }
}
