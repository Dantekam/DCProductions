using System;
using UnityEngine;
using UnityEngine.Video;

[Serializable]
public class VideoEntry
{
    public string title;
    [TextArea]
    public string description;

    public VideoSourceMode sourceMode = VideoSourceMode.LocalClip;

    public VideoClip localClip;
    public string url;

    public Texture2D thumbnail;
}