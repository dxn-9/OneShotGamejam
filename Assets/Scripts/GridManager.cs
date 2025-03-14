using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Nodes;
using ScriptableObjects;
using UnityEngine;
using NodeGrid = System.Collections.Generic.Dictionary<UnityEngine.Vector2, Nodes.Node>;


public enum Mode
{
    Building,
    Simulation,
    EndLevel
}


public class GridPointComparer : IEqualityComparer<Vector2>
{
    private const float Epsilon = 0.001f;

    public bool Equals(Vector2 a, Vector2 b)
    {
        return Vector2.Distance(a, b) < Epsilon;
    }

    public int GetHashCode(Vector2 obj)
    {
        return Mathf.RoundToInt(obj.x * 100) * 1000 + Mathf.RoundToInt(obj.y * 100);
    }
}

public class GridManager : MonoBehaviour
{
    [SerializeField, Min(0.5f)] float gridSize;
    [SerializeField] NodeScriptableObject[] availableNodes;
    [SerializeField] UIManager uiManager;
    [SerializeField] float simulationTickDuration = 0.3f;
    [SerializeField] Transform itemIndicator;
    [SerializeField] Transform endPosition;

    NodeGrid grid;
    int tickCount;

    Node active;

    public Mode mode;
    float currentSimulationDuration;

    // Debug params
    Vector3 xzIntersection;
    bool canPlace;


    void Awake()
    {
        Debug.Log("Awake called");
        currentSimulationDuration = simulationTickDuration;
        mode = Mode.Building;
        grid = new Dictionary<Vector2, Node>(new GridPointComparer());
        var startNode =
            new StartNode(Vector3.zero, Vector2.up, availableNodes.GetByName<StartNode>());
        grid[Vector2.zero] = startNode;
        active = startNode;
        startNode.ReceiveItem(Vector2.zero);

        PlaceEndNode();
    }

    void PlaceEndNode()
    {
        grid[endPosition.position.ToGridCoord()] =
            new EndNode(endPosition.position, Vector2.zero, availableNodes.GetByName<EndNode>());
    }

    void Start()
    {
        Debug.Log("Start");
        uiManager.OnSimulateButton += OnStartSimulation;
    }


    void OnStartSimulation(object sender, EventArgs e)
    {
        mode = Mode.Simulation;
        active = grid[Vector2.zero];
        active.ReceiveItem(Vector2.zero);
        Debug.Log("Simulation Start");
    }

    // Ticks the active node to transport the item. If it returns true it means we've finished traversing the list.
    void SimulationStep()
    {
        foreach (var node in grid.Values)
        {
            node.Tick(grid, tickCount);
            if (node.holdsItem)
            {
                itemIndicator.position = node.position + Vector3.up;
            }
        }

        tickCount++;
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
                if (dir != active.orientation && active is MultiDir)
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
                active = node;
            }
        }
        else if (mode == Mode.Simulation)
        {
            currentSimulationDuration -= Time.deltaTime;
            if (currentSimulationDuration <= 0.0f)
            {
                SimulationStep();

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
        if (active == null) return gridPoint + Vector3.zero;
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