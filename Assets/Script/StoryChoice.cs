using System;
using UnityEngine;

[Serializable]
public class StoryChoice
{
    [Header("Choice Info")]
    public string choiceId;
    public string choiceText;

    [Header("Navigation")]
    public string nextNodeId;
}