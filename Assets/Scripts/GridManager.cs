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
    Orientation currentOrientation;
    Mode mode;
    float currentSimulationDuration;


    public event EventHandler<OnOrientationChangeEventArgs> OnOrientationChange;

    public class OnOrientationChangeEventArgs : EventArgs
    {
        public Orientation orientation;
    }


    // Debug params
    Vector3 xzIntersection;
    bool canPlace;


    void Awake()
    {
        currentSimulationDuration = simulationTickDuration;
        mode = Mode.Building;
        currentOrientation = Orientation.Up;
        grid = new Dictionary<Vector2, Node>();
        var startNode =
            new Start(Vector3.zero, Orientation.Up, availableNodes.GetByName<Start>());
        grid[Vector2.zero] = startNode;
        active = startNode;
    }

    void Start()
    {
        OnOrientationChange?.Invoke(this, new OnOrientationChangeEventArgs { orientation = currentOrientation });
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
            var orientation = active.Output.Rotate(active.orientation).ToVector2();
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

        var gridPoint = new Vector3(Mathf.Round(xzIntersection.x), 0f, Mathf.Round(xzIntersection.z));
        canPlace = CanPlaceNode(gridPoint);


        if (Input.GetKeyDown(KeyCode.R))
        {
            currentOrientation = currentOrientation.Next();
            OnOrientationChange?.Invoke(this, new OnOrientationChangeEventArgs { orientation = currentOrientation });
        }


        if (mode == Mode.Building)
        {
            if (canPlace && Input.GetMouseButtonDown(0))
            {
                var scriptableObject = availableNodes.GetByName<ConveyorBelt>();
                var node = new ConveyorBelt(gridPoint, currentOrientation, scriptableObject);
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