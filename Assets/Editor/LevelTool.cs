using System.Collections.Generic;
using System.Linq;
using Extensions;
using UnityEditor;
using UnityEngine;

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

    HashSet<Vector3> candidatePoints, serializedPoints, candidateHazards;
    HashSet<Hazard> serializedHazards;
    SerializedProperty terrain, hazards;

    class HazardPositionComparer : EqualityComparer<Hazard>
    {
        public override bool Equals(Hazard x, Hazard y)
        {
            return x.position == y.position;
        }

        public override int GetHashCode(Hazard obj)
            => obj.position.GetHashCode();
    }

    int currentLevel
    {
        get => EditorPrefs.GetInt("LevelTool_CurrentLevel", 0);
        set => EditorPrefs.SetInt("LevelTool_CurrentLevel", value);
    }

    PlacingMode placingMode = PlacingMode.Delete;
    PlacingType placingType = PlacingType.Node;


    void CalculatePoints()
    {
        candidatePoints.Clear();
        candidateHazards.Clear();
        serializedPoints = new HashSet<Vector3>();
        serializedHazards = new HashSet<Hazard>();
        for (int i = 0; i < terrain.arraySize; i++)
        {
            SerializedProperty terrainPoint = terrain.GetArrayElementAtIndex(i);
            Vector3 point = terrainPoint.vector3Value;

            if (point.y == currentLevel)
            {
                serializedPoints.Add(point);
            }
        }

        for (int i = 0; i < hazards.arraySize; i++)
        {
            SerializedProperty hazardProp = hazards.GetArrayElementAtIndex(i);
            SerializedProperty typeProp = hazardProp.FindPropertyRelative("type");
            SerializedProperty posProp = hazardProp.FindPropertyRelative("position");

            Hazard hazard = new Hazard { type = typeProp.stringValue, position = posProp.vector3Value };
            if (hazard.position.y == currentLevel)
            {
                serializedHazards.Add(hazard);
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

        foreach (var hazard in serializedHazards)
        {
            for (int i = -1; i <= 1; i++)
            {
                if (i == 0) continue;
                Hazard xPoint = new Hazard { position = hazard.position + new Vector3(i, 0, 0), type = "" };
                Hazard zPoint = new Hazard { position = hazard.position + new Vector3(0, 0, i), type = "" };
                var comparer = new HazardPositionComparer();
                if (!serializedHazards.Contains(xPoint, comparer))
                    candidateHazards.Add(xPoint.position.SnapToGrid());
                if (!serializedHazards.Contains(zPoint, comparer))
                    candidateHazards.Add(zPoint.position.SnapToGrid());
            }
        }

        if (candidatePoints.Count == 0) candidatePoints.Add(Vector3.zero + new Vector3(0, currentLevel, 0));
        if (candidateHazards.Count == 0) candidateHazards.Add(Vector3.zero + new Vector3(0, currentLevel, 0));
    }


    void OnEnable()
    {
        candidatePoints = new HashSet<Vector3>();
        candidateHazards = new HashSet<Vector3>();
        terrain = serializedObject.FindProperty("terrain");
        hazards = serializedObject.FindProperty("hazards");
        CalculatePoints();
    }

    bool DrawNodes()
    {
        foreach (var position in candidatePoints)
        {
            Handles.color = Color.white;
            if (Handles.Button(position, Quaternion.LookRotation(Vector3.up), 0.4f, 0.4f, Handles.CubeHandleCap))
            {
                if (target is Level level)
                {
                    Undo.RecordObject(level, "Add Grid Point");
                    level.SetTerrain(level.terrain.Append(position).ToArray());
                    EditorUtility.SetDirty(level);
                    serializedObject.Update();
                    serializedObject.ApplyModifiedProperties();
                    return true;
                }
            }
        }

        return false;
    }

    bool DrawHazards()
    {
        foreach (var position in candidateHazards)
        {
            Handles.color = Color.red;
            if (Handles.Button(position, Quaternion.LookRotation(Vector3.up), 0.4f, 0.4f, Handles.CubeHandleCap))
            {
                if (target is Level level)
                {
                    Undo.RecordObject(level, "Add Hazard");
                    level.SetHazard(level.hazards.Append(new Hazard { position = position, type = "fire" }).ToArray());
                    EditorUtility.SetDirty(level);
                    serializedObject.Update();
                    serializedObject.ApplyModifiedProperties();
                    return true;
                }
            }
        }

        return false;
    }

    bool DrawPoints()
    {
        if (placingType == PlacingType.Node)
        {
            return DrawNodes();
        }

        if (placingType == PlacingType.Hazard)
        {
            return DrawHazards();
        }

        return false;
    }

    protected virtual void OnSceneGUI()
    {
        bool isDirty = DrawPoints();

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
                    currentLevel -= 1;
                    isDirty = true;
                }

                GUILayout.Label("Level: " + currentLevel);
                if (GUILayout.Button("+"))
                {
                    currentLevel += 1;
                    isDirty = true;
                }
            }
        }

        Handles.EndGUI();

        if (isDirty)
        {
            CalculatePoints();
        }


        var e = Event.current;
        // Reset
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.R)
        {
            if (target is Level customGrid)
            {
                e.Use();
                Undo.RecordObject(customGrid, "Reset Grid");
                customGrid.SetTerrain(new Vector3[] { });
                EditorUtility.SetDirty(customGrid);
                serializedObject.Update();
                serializedObject.ApplyModifiedProperties();
                CalculatePoints();
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
                    Vector3 gridPosition = new Vector3(position.x, currentLevel, position.z).SnapToGrid();

                    var customGrid = (Level)target;
                    if (placingType == PlacingType.Node && customGrid.terrain.Contains(gridPosition))
                    {
                        e.Use();
                        switch (placingMode)
                        {
                            case PlacingMode.Delete:
                                Undo.RecordObject(customGrid, "Remove grid point");
                                customGrid.SetTerrain(customGrid.terrain.Where(p => p != gridPosition).ToArray());
                                EditorUtility.SetDirty(customGrid);
                                serializedObject.Update();
                                serializedObject.ApplyModifiedProperties();
                                CalculatePoints();
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
                    else if (placingType == PlacingType.Hazard && customGrid.hazards.ContainsAt(gridPosition))
                    {
                        e.Use();
                        switch (placingMode)
                        {
                            case PlacingMode.Delete:
                                Undo.RecordObject(customGrid, "Remove hazard ");
                                customGrid.SetHazard(
                                    customGrid.hazards.Where(p => p.position != gridPosition).ToArray());
                                EditorUtility.SetDirty(customGrid);
                                serializedObject.Update();
                                serializedObject.ApplyModifiedProperties();
                                CalculatePoints();
                                break;
                            default:
                                Debug.Log("Placing mode " + placingMode + " not supported for hazards");
                                break;
                        }
                    }
                }
            }
        }
    }
}