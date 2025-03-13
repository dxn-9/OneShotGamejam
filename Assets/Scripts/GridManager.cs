using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Nodes;
using ScriptableObjects;
using UnityEngine;


enum Mode
{
    Building,
    Simulation,
    EndLevel
}

public class GridManager : MonoBehaviour
{
    [SerializeField, Min(0.5f)] float gridSize;
    [SerializeField] NodeScriptableObject[] availableNodes;
    [SerializeField] UIManager uiManager;
    [SerializeField] float simulationTickDuration = 0.3f;

    Dictionary<Vector2, Node> grid;

    Node active;

    // Vector2 currentOrientation;
    Mode mode;
    float currentSimulationDuration;
    Vector3 endPosition;


    // public event EventHandler<OnOrientationChangeEventArgs> OnOrientationChange;

    // public class OnOrientationChangeEventArgs : EventArgs
    // {
    //     public Vector2 orientation;
    // }


    // Debug params
    Vector3 xzIntersection;
    bool canPlace;


    void Awake()
    {
        endPosition = new Vector3(5f, 0f, 2f);
        currentSimulationDuration = simulationTickDuration;
        mode = Mode.Building;
        // currentOrientation = Vector2.up;
        grid = new Dictionary<Vector2, Node>();
        var startNode =
            new StartNode(Vector3.zero, Vector2.up, availableNodes.GetByName<StartNode>());
        grid[Vector2.zero] = startNode;
        active = startNode;

        PlaceEndNode();
    }

    void PlaceEndNode()
    {
        grid[endPosition.ToGridCoord()] =
            new EndNode(endPosition, Vector2.zero, availableNodes.GetByName<EndNode>());
    }

    void Start()
    {
        // OnOrientationChange?.Invoke(this, new OnOrientationChangeEventArgs { orientation = currentOrientation });
        uiManager.OnSimulateButton += OnStartSimulation;
    }

    void OnStartSimulation(object sender, EventArgs e)
    {
        mode = Mode.Simulation;
        active = grid[Vector2.zero];
        active.holdsItem = true;
    }

    // Ticks the active node to transport the item. If it returns true it means we've finished traversing the list.
    bool SimulationStep()
    {
        if (active.nextNode == null)
        {
            return true;
        }

        active.Tick();
        active = active.nextNode;
        return false;
    }

    void OnDrawGizmos()
    {
        if (canPlace)
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.blue;
        }

        Gizmos.DrawSphere(xzIntersection, 0.5f);

        if (active != null)
        {
            var angle = Vector2.SignedAngle(active.Output, active.orientation);
            var rotation = Quaternion.Euler(0f, 0f, angle);

            var orientation = rotation * active.Output;
            Gizmos.DrawCube(active.position + Vector3.up + new Vector3(orientation.x, 0f, orientation.y) * 0.5f,
                Vector3.one * 0.3f);
        }
    }

    void Update()
    {
        // Find the xz intersection
        Vector2 screenMouse = Input.mousePosition;
        var screenRay = Camera.main.ScreenPointToRay(screenMouse);
        xzIntersection = screenRay.origin - screenRay.direction * (screenRay.origin.y / screenRay.direction.y);

        var gridPoint = CalculateGridPoint(xzIntersection);
        canPlace = CanPlaceNode(gridPoint);


        if (Input.GetKeyDown(KeyCode.R))
        {
            // OnOrientationChange?.Invoke(this, new OnOrientationChangeEventArgs { orientation = currentOrientation });
        }


        if (mode == Mode.Building)
        {
            if (canPlace && Input.GetMouseButtonDown(0))
            {
                Vector2 dir = (gridPoint - active.position).ToGridCoord();

                // TODO: Instead of destroying the game object, it should be reused for the new conveyor belt
                if (dir != active.orientation)
                {
                    var angle = Vector2.SignedAngle(dir, active.orientation);
                    if (angle == 90f)
                    {
                        Destroy(active.transform.gameObject);
                        grid[active.position.ToGridCoord()] =
                            new ConveyorBeltRight(active.position, active.orientation,
                                availableNodes.GetByName<ConveyorBeltRight>());
                    }
                    else if (angle == -90f)
                    {
                        Destroy(active.transform.gameObject);
                        grid[active.position.ToGridCoord()] =
                            new ConveyorBeltLeft(active.position, active.orientation,
                                availableNodes.GetByName<ConveyorBeltLeft>());
                    }
                }


                var scriptableObject = availableNodes.GetByName<ConveyorBelt>();
                var node = new ConveyorBelt(gridPoint, dir, scriptableObject);
                grid[gridPoint.ToGridCoord()] = node;
                active.nextNode = node;
                active = node;
            }
        }
        else if (mode == Mode.Simulation)
        {
            currentSimulationDuration -= Time.deltaTime;
            if (currentSimulationDuration <= 0.0f)
            {
                if (SimulationStep())
                {
                    Debug.Log("Simulation Halted at: " + active.position + " " + active.NodeName);
                    mode = Mode.EndLevel;
                }

                currentSimulationDuration = simulationTickDuration + Mathf.Abs(currentSimulationDuration);
            }
        }
        else
        {
            // Mode = EndLevel
            mode = Mode.Building; // TODO: implement end level.
        }
    }

    Vector3 CalculateGridPoint(Vector3 xzIntersection)
    {
        var gridPoint = new Vector3(Mathf.Round(xzIntersection.x), 0f, Mathf.Round(xzIntersection.z));
        gridPoint = active.position + (gridPoint - active.position).normalized;
        gridPoint = new Vector3(Mathf.Round(gridPoint.x), 0f, (gridPoint.z));
        return gridPoint;
    }

    bool CanPlaceNode(Vector3 position)
    {
        // If it's already occupied, we cannot place a node.
        if (grid.ContainsKey(position.ToGridCoord())) return false;
        // Could be a floating point error. But the values are floored before.. TODO: Check if there can be any fp error
        if (Mathf.Abs(active.position.x - position.x) == 1.0f)
        {
            if (active.position.z == position.z) return true;
        }

        if (Mathf.Abs(active.position.z - position.z) == 1.0f)
        {
            if (active.position.x == position.x) return true;
        }

        return false;
    }
}