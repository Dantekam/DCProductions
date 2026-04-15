using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StoryNode
{
    [Header("Node Info")]
    public string nodeId;
    public string displayName;

    [Header("Video")]
    public VideoEntry video;

    [Header("Trigger")]
    public ChoiceTriggerMode triggerMode = ChoiceTriggerMode.OnVideoEnd;
    public double triggerTimeSeconds = 0;

    [Header("Auto Continue")]
    public string autoContinueNextNodeId;

    [Header("Choices")]
    public List<StoryChoice> choices = new();
}