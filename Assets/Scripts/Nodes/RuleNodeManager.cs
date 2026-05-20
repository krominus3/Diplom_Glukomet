using UnityEngine;
using System.Collections.Generic;

public class RuleNodeManager : MonoBehaviour
{
    public static RuleNodeManager Instance { get; private set; }
    
    [Header("Node Network Settings")]
    public List<RuleNode> allNodes = new List<RuleNode>();
    public bool autoConnectNodes = true;
    public float nodeConnectionRadius = 10f;
    
    [Header("Transfer Settings")]
    public float globalTransferDelay = 0.3f;
    public bool enableChainReaction = true;
    public int maxChainLength = 10;
    
    //void Awake()
    //{
    //    if (Instance == null)
    //    {
    //        Instance = this;
    //        //DontDestroyOnLoad(gameObject);
    //    }
    //    else
    //    {
    //        Destroy(gameObject);
    //    }
    //}
    
    void Start()
    {
        FindAllNodes();
        
        if (autoConnectNodes)
        {
            AutoConnectAllNodes();
        }
    }
    
    void FindAllNodes()
    {
        allNodes.Clear();
        allNodes.AddRange(FindObjectsOfType<RuleNode>());
        Debug.Log($"Найдено {allNodes.Count} нод в сцене");
    }
    
    void AutoConnectAllNodes()
    {
        foreach (var node in allNodes)
        {
            node.connectionRadius = nodeConnectionRadius;
            node.FindNearbyNodes();
        }
    }
    
    // Передать правило через всю сеть
    public void BroadcastRule(RuleBase rule, GameObject sourceObject, RuleNode startNode)
    {
        if (!enableChainReaction) return;
        
        HashSet<RuleNode> visitedNodes = new HashSet<RuleNode>();
        Queue<RuleNode> nodesToProcess = new Queue<RuleNode>();
        
        nodesToProcess.Enqueue(startNode);
        visitedNodes.Add(startNode);
        
        int chainLength = 0;
        
        while (nodesToProcess.Count > 0 && chainLength < maxChainLength)
        {
            RuleNode currentNode = nodesToProcess.Dequeue();
            
            foreach (var connectedNode in currentNode.connectedNodes)
            {
                if (!visitedNodes.Contains(connectedNode))
                {
                    visitedNodes.Add(connectedNode);
                    nodesToProcess.Enqueue(connectedNode);
                    
                    // Передаем правило
                    connectedNode.ReceiveRule(rule, sourceObject, currentNode);
                }
            }
            
            chainLength++;
        }
        
        Debug.Log($"Правило {rule.ruleName} распространено по {visitedNodes.Count} нодам");
    }
    
    // Добавить новую ноду в сеть
    public void RegisterNode(RuleNode node)
    {
        if (!allNodes.Contains(node))
        {
            allNodes.Add(node);
            
            if (autoConnectNodes)
            {
                node.connectionRadius = nodeConnectionRadius;
                node.FindNearbyNodes();
            }
        }
    }
    
    // Удалить ноду из сети
    public void UnregisterNode(RuleNode node)
    {
        if (allNodes.Contains(node))
        {
            allNodes.Remove(node);
            
            // Удаляем связи с этой нодой у других
            foreach (var otherNode in allNodes)
            {
                if (otherNode.connectedNodes.Contains(node))
                {
                    otherNode.connectedNodes.Remove(node);
                }
            }
        }
    }
}