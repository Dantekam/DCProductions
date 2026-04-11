using System.Collections.Generic;
using UnityEngine;

public class VideoCatalogManager : MonoBehaviour
{
    [Header("Available Videos")]
    public List<VideoEntry> videos = new();

    public VideoEntry GetVideoByIndex(int index)
    {
        if (index < 0 || index >= videos.Count)
        {
            Debug.LogWarning($"Video index {index} is out of range.");
            return null;
        }

        return videos[index];
    }

    public VideoEntry GetFirstVideo()
    {
        if (videos.Count == 0)
        {
            Debug.LogWarning("No videos assigned in VideoCatalogManager.");
            return null;
        }

        return videos[0];
    }
}