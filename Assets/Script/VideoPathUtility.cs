using System.IO;
using UnityEngine;

public static class VideoPathUtility
{
    public static string GetStreamingVideoPath(string fileName)
    {
        return Path.Combine(Application.streamingAssetsPath, "Videos", fileName);
    }
}