using System;
using System.Collections.Generic;
using System.Linq;
using Nodes;
using ScriptableObjects;
using UnityEditor;
using UnityEngine;


[Serializable]
public struct Hazard
{
    public string type;
    public Vector3 position;
}

// Responsible for creating the initial grid and displaying the game state
[ExecuteInEditMode]
public class Level : MonoBehaviour
{
    [SerializeField] GameObject cubePrefab, startPrefab, endPrefab, fireHazardPrefab;
    [HideInInspector] [SerializeField] public Vector3[] terrain;
    [HideInInspector] [SerializeField] public Hazard[] hazards;

    [SerializeField] public Vector3 startPoint, endPoint;
    [SerializeField] Transform playerObjects;
    [SerializeField] NodeScriptableObject[] availableNodes;

    [HideInInspector] public Dictionary<Vector3, Transform> playerNodes;

    public void SetTerrain(Vector3[] terrain)
    {
        this.terrain = terrain;
        PlaceTiles();
    }

    public void SetHazard(Hazard[] hazards)
    {
        this.hazards = hazards;
        PlaceTiles();
    }

    public void SetEndPoint(Vector3 point)
    {
        endPoint = point;
        PlaceTiles();
    }

    public void SetStartPoint(Vector3 point)
    {
        startPoint = point;
        PlaceTiles();
    }

    void OnEnable()
    {
        if (terrain == null) terrain = new Vector3[] { };
        playerNodes = new Dictionary<Vector3, Transform>();
        PlaceTiles();
    }

    void PlaceTiles()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        foreach (var point in terrain)
        {
            Instantiate(cubePrefab, point, Quaternion.identity, transform);
        }

        foreach (var hazard in hazards)
        {
            if (hazard.type == "fire")
            {
                Instantiate(fireHazardPrefab, hazard.position, Quaternion.identity, transform);
            }
        }

        playerNodes[startPoint] = Instantiate(startPrefab, startPoint, Quaternion.identity, transform).transform;
        playerNodes[endPoint] = Instantiate(endPrefab, endPoint, Quaternion.identity, transform).transform;
    }

    public void MarkForDeletion(Node node)
    {
        var nodeTransform = playerNodes[node.position];
        nodeTransform.GetComponent<NodeVisual>().MarkForDeletion(node.markedForDeletion);
    }

    public void AddPlayerNode(Node node)
    {
        var nodeSO = availableNodes.Single(so => so.nodeName == node.NodeName);
        var orientationVec = node.orientation;
        var rotation = Quaternion.LookRotation(new Vector3(orientationVec.x, 0f, orientationVec.y));
        playerNodes[node.position] = Instantiate(nodeSO.prefab, node.position, rotation, playerObjects).transform;
        Game.I.soundManager.PlayPlacingSound();
    }

    public void ChangePlayerNode(Node node)
    {
        var currentNode = playerNodes[node.position];
        DestroyImmediate(currentNode.gameObject);
        var nodeSO = availableNodes.Single(so => so.nodeName == node.NodeName);
        var orientationVec = node.orientation;
        var rotation = Quaternion.LookRotation(new Vector3(orientationVec.x, 0f, orientationVec.y));
        playerNodes[node.position] = Instantiate(nodeSO.prefab, node.position, rotation, playerObjects).transform;
    }

    public void DeleteNode(Node node)
    {
        Destroy(playerNodes[node.position].gameObject);
    }
}