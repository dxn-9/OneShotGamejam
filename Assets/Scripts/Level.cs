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

// Responsible for creating the initial grid and displaying the game state
[ExecuteInEditMode]
public class Level : MonoBehaviour
{
    [SerializeField] GameObject cubePrefab, startPrefab, endPrefab;
    [HideInInspector] [SerializeField] public Vector3[] points;
    [SerializeField] public Vector3 startPoint, endPoint;
    [SerializeField] Transform playerObjects;
    [SerializeField] NodeScriptableObject[] availableNodes;

    [HideInInspector] public Dictionary<Vector3, Transform> playerNodes;

    public void SetPoints(Vector3[] points)
    {
        this.points = points;
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
        if (points == null) points = new Vector3[] { };
        playerNodes = new Dictionary<Vector3, Transform>();
        PlaceTiles();
    }

    void PlaceTiles()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        foreach (var point in points)
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

    HashSet<Vector3> candidatePoints, serializedPoints;
    SerializedProperty points;
    PlacingMode placingMode = PlacingMode.Delete;


    void CalculateCandidatePoints()
    {
        candidatePoints.Clear();
        serializedPoints = new HashSet<Vector3>();
        for (int i = 0; i < points.arraySize; i++)
        {
            SerializedProperty pointSerialized = points.GetArrayElementAtIndex(i);
            Vector3 point = pointSerialized.vector3Value;
            serializedPoints.Add(point);
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

        if (candidatePoints.Count == 0) candidatePoints.Add(Vector3.zero);
    }

    void OnEnable()
    {
        candidatePoints = new HashSet<Vector3>();
        points = serializedObject.FindProperty("points");
        CalculateCandidatePoints();
    }

    bool DrawCandidatePoint(Vector3 position)
    {
        if (Handles.Button(position, Quaternion.LookRotation(Vector3.up), 0.4f, 0.4f, Handles.CubeHandleCap))
        {
            if (target is Level level)
            {
                Undo.RecordObject(level, "Add Grid Point");
                level.SetPoints(level.points.Append(position).ToArray());
                EditorUtility.SetDirty(level);
                serializedObject.Update();
                serializedObject.ApplyModifiedProperties();
                Debug.Log("Look at custom grid!");
                return true;
            }
        }

        return false;
    }

    protected virtual void OnSceneGUI()
    {
        // Gizmos.DrawCube(0.5f * Vector3.one, Vector3.one);

        var isDirty = false;
        foreach (var point in candidatePoints)
        {
            if (DrawCandidatePoint(point))
            {
                isDirty = true;
            }
        }

        if (isDirty)
        {
            CalculateCandidatePoints();
        }

        Handles.BeginGUI();

        GUIStyle style = new GUIStyle();
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.black);
        texture.Apply();
        style.normal.background = texture;
        using (new GUILayout.HorizontalScope(style))
        {
            if (GUILayout.Toggle(placingMode == PlacingMode.Delete, "Delete"))
            {
                placingMode = PlacingMode.Delete;
            }

            if (GUILayout.Toggle(placingMode == PlacingMode.Start, "Start"))
            {
                placingMode = PlacingMode.Start;
            }

            if (GUILayout.Toggle(placingMode == PlacingMode.End, "End"))
            {
                placingMode = PlacingMode.End;
            }
        }

        Handles.EndGUI();

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
                Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
                if (groundPlane.Raycast(ray, out float distance))
                {
                    Vector3 position = ray.GetPoint(distance);
                    Vector3 gridPosition = new Vector3(Mathf.Round(position.x), 0f, Mathf.Round(position.z));

                    if (target is Level customGrid)
                    {
                        if (customGrid.points.Contains(gridPosition))
                        {
                            e.Use();
                            switch (placingMode)
                            {
                                case PlacingMode.Delete:
                                    Undo.RecordObject(customGrid, "Remove grid point");
                                    customGrid.SetPoints(customGrid.points.Where(p => p != gridPosition).ToArray());
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