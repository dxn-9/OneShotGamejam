using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Nodes;
using ScriptableObjects;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.Serialization;


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
    [SerializeField] GameObject cubePrefab, startPrefab, endPrefab;
    [HideInInspector] [SerializeField] public Vector3[] terrain;
    [HideInInspector] [SerializeField] public Hazard[] hazards;

    [SerializeField] public Vector3 startPoint, endPoint;
    [SerializeField] Transform playerObjects;
    [SerializeField] NodeScriptableObject[] availableNodes;

    [HideInInspector] public Dictionary<Vector3, Transform> playerNodes;

    public void SetPoints(Vector3[] points)
    {
        this.terrain = points;
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

[CustomEditor(typeof(Level))]
class LevelTool : Editor
{
    enum PlacingMode
    {
        Delete,
        Start,
        End
    }

    enum PlacingType
    {
        Node,
        Hazard
    }

    HashSet<Vector3> candidatePoints, serializedPoints;
    SerializedProperty terrain;
    int currentLevel = 0;
    PlacingMode placingMode = PlacingMode.Delete;
    PlacingType placingType = PlacingType.Node;


    void CalculateCandidatePoints()
    {
        candidatePoints.Clear();
        serializedPoints = new HashSet<Vector3>();
        for (int i = 0; i < terrain.arraySize; i++)
        {
            SerializedProperty pointSerialized = terrain.GetArrayElementAtIndex(i);
            Vector3 point = pointSerialized.vector3Value;
            if (point.y == currentLevel)
            {
                serializedPoints.Add(point);
            }
        }

        foreach (var point in serializedPoints)
        {
            for (int i = -1; i <= 1; i++)
            {
                if (i == 0) continue;
                Vector3 xPoint = point + new Vector3(i, 0, 0);
                Vector3 zPoint = point + new Vector3(0, 0, i);
                if (!serializedPoints.Contains(xPoint)) candidatePoints.Add(xPoint);
                if (!serializedPoints.Contains(zPoint)) candidatePoints.Add(zPoint);
            }
        }

        if (candidatePoints.Count == 0) candidatePoints.Add(Vector3.zero + new Vector3(0, currentLevel, 0));
    }

    void OnEnable()
    {
        candidatePoints = new HashSet<Vector3>();
        terrain = serializedObject.FindProperty("terrain");
        CalculateCandidatePoints();
    }

    bool DrawCandidatePoint(Vector3 position)
    {
        if (Handles.Button(position, Quaternion.LookRotation(Vector3.up), 0.4f, 0.4f, Handles.CubeHandleCap))
        {
            if (target is Level level)
            {
                Undo.RecordObject(level, "Add Grid Point");
                level.SetPoints(level.terrain.Append(position).ToArray());
                EditorUtility.SetDirty(level);
                serializedObject.Update();
                serializedObject.ApplyModifiedProperties();
                return true;
            }
        }

        return false;
    }

    protected virtual void OnSceneGUI()
    {
        var isDirty = false;
        foreach (var point in candidatePoints)
        {
            if (DrawCandidatePoint(point))
            {
                isDirty = true;
            }
        }

        Handles.BeginGUI();

        GUIStyle style = new GUIStyle();
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.black);
        texture.Apply();
        style.normal.background = texture;

        GUIStyle labelStyle = new GUIStyle();
        labelStyle.stretchWidth = false;
        using (new GUILayout.HorizontalScope(style))
        {
            placingMode = (PlacingMode)GUILayout.Toolbar((int)placingMode, new string[] { "Delete", "Start", "End" });
        }

        using (new GUILayout.HorizontalScope(style))
        {
            placingType = (PlacingType)GUILayout.Toolbar((int)placingType, new string[] { "Node", "Hazard" });
            using (new GUILayout.HorizontalScope(labelStyle))
            {
                if (GUILayout.Button("-"))
                {
                    currentLevel--;
                    isDirty = true;
                }

                GUILayout.Label("Level: " + currentLevel);
                if (GUILayout.Button("+"))
                {
                    currentLevel++;
                    isDirty = true;
                }
            }
        }

        Handles.EndGUI();

        if (isDirty)
        {
            CalculateCandidatePoints();
        }


        var e = Event.current;
        // Reset
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.R)
        {
            if (target is Level customGrid)
            {
                e.Use();
                Undo.RecordObject(customGrid, "Reset Grid");
                customGrid.SetPoints(new Vector3[] { });
                EditorUtility.SetDirty(customGrid);
                serializedObject.Update();
                serializedObject.ApplyModifiedProperties();
                CalculateCandidatePoints();
            }
        }

        if (e.type == EventType.MouseDown && e.button == 1)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                Plane groundPlane = new Plane(Vector3.up, new Vector3(0, currentLevel, 0));
                if (groundPlane.Raycast(ray, out float distance))
                {
                    Vector3 position = ray.GetPoint(distance);
                    Vector3 gridPosition = new Vector3(Mathf.Round(position.x), currentLevel, Mathf.Round(position.z));

                    if (target is Level customGrid)
                    {
                        if (customGrid.terrain.Contains(gridPosition))
                        {
                            e.Use();
                            switch (placingMode)
                            {
                                case PlacingMode.Delete:
                                    Undo.RecordObject(customGrid, "Remove grid point");
                                    customGrid.SetPoints(customGrid.terrain.Where(p => p != gridPosition).ToArray());
                                    EditorUtility.SetDirty(customGrid);
                                    serializedObject.Update();
                                    serializedObject.ApplyModifiedProperties();
                                    CalculateCandidatePoints();
                                    break;

                                case PlacingMode.End:
                                    Undo.RecordObject(customGrid, "Place end point");
                                    customGrid.SetEndPoint(gridPosition);
                                    EditorUtility.SetDirty(customGrid);
                                    serializedObject.Update();
                                    serializedObject.ApplyModifiedProperties();
                                    break;
                                case PlacingMode.Start:
                                    Undo.RecordObject(customGrid, "Place start point");
                                    customGrid.SetStartPoint(gridPosition);
                                    EditorUtility.SetDirty(customGrid);
                                    serializedObject.Update();
                                    serializedObject.ApplyModifiedProperties();
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }
}