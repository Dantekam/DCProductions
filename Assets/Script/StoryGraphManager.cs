using System.Collections.Generic;
using UnityEngine;

public class StoryGraphManager : MonoBehaviour
{
    [Header("Story Graph")]
    [SerializeField] private List<StoryNode> nodes = new();

    private Dictionary<string, StoryNode> nodeLookup;

    private void Awake()
    {
        BuildLookup();
    }

    private void BuildLookup()
    {
        nodeLookup = new Dictionary<string, StoryNode>();

        foreach (StoryNode node in nodes)
        {
            if (node == null)
            {
                Debug.LogWarning("StoryGraphManager: Found a null node in the graph list.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(node.nodeId))
            {
                Debug.LogWarning("StoryGraphManager: Found a node with an empty nodeId.");
                continue;
            }

            if (nodeLookup.ContainsKey(node.nodeId))
            {
                Debug.LogWarning($"StoryGraphManager: Duplicate nodeId found: {node.nodeId}");
                continue;
            }

            nodeLookup.Add(node.nodeId, node);
        }

        Debug.Log($"StoryGraphManager: Loaded {nodeLookup.Count} story nodes.");
    }

    public StoryNode GetNode(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
        {
            Debug.LogWarning("StoryGraphManager: Tried to get a node with a null or empty nodeId.");
            return null;
        }

        if (nodeLookup.TryGetValue(nodeId, out StoryNode node))
        {
            return node;
        }

        Debug.LogWarning($"StoryGraphManager: No node found with id '{nodeId}'.");
        return null;
    }

    public StoryNode GetFirstNode()
    {
        if (nodes.Count == 0)
        {
            Debug.LogWarning("StoryGraphManager: No story nodes assigned.");
            return null;
        }

        return nodes[0];
    }

    public List<StoryNode> GetAllNodes()
    {
        return nodes;
    }
}