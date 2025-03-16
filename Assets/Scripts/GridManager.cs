using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Nodes;
using ScriptableObjects;
using UnityEngine;


// TODO: Refactor this class. it's doing a lot more than it should.
// This class is responsible for holding the grid data, where the belts are placed and simulating the game.
public class GridManager : MonoBehaviour
{
    [SerializeField, Min(0.5f)] float gridSize;
    [SerializeField] NodeScriptableObject[] availableNodes;
    [SerializeField] float simulationTickDuration = 0.3f;

    public event EventHandler<NodeEventArgs> OnNodePlace;
    public event EventHandler<NodeEventArgs> OnNodeChange;
    public event EventHandler<NodeEventArgs> OnNodeMark;
    public event EventHandler<NodeEventArgs> OnNodeDelete;

    public class NodeEventArgs : EventArgs
    {
        public Node node;
    }

    public NodeGrid grid;
    int tickCount;
    public Node active, markedActive;
    float currentSimulationDuration;
    bool canPlace;
    int maxStuckTicks = 5; // If the active object, has the item for 5 ticks, it's stuck
    int stuckCounter = 0;


    void Awake()
    {
        currentSimulationDuration = simulationTickDuration;
        grid = new NodeGrid();
    }


    void Start()
    {
        grid[Game.I.level.startPoint] =
            new StartNode(Game.I.level.startPoint, Vector2.up, availableNodes.GetByName<StartNode>());
        grid[Game.I.level.endPoint] =
            new EndNode(Game.I.level.endPoint, Vector2.up, availableNodes.GetByName<EndNode>());
        active = grid.GetStart();
        grid.GetStart().ReceiveItem(Vector2.zero);
    }


    // Ticks the active node to transport the item. If it returns true it means we've finished traversing the list.
    void SimulationStep()
    {
        foreach (var node in grid.Values)
        {
            node.Tick(grid, tickCount);
            if (node.holdsItem)
            {
                if (node == active) stuckCounter++;
                else stuckCounter = 0;
                active = node;
                if (stuckCounter >= maxStuckTicks) Game.I.GameOver(false);
            }
        }

        tickCount++;
    }

    void Update()
    {
        if (Game.I.gameMode == Mode.Building)
        {
            // Find the xz intersection
            Vector2 screenMouse = Input.mousePosition;
            var screenRay = Camera.main.ScreenPointToRay(screenMouse);
            var xzIntersection = screenRay.origin - screenRay.direction * (screenRay.origin.y / screenRay.direction.y);

            var gridPoint = CalculateGridPoint(xzIntersection);
            canPlace = CanPlaceNode(gridPoint);


            if (canPlace && Input.GetMouseButtonDown(0))
            {
                Vector2 dir = (gridPoint - active.position).ToDir();

                if (dir != active.orientation && active is MultiDir)
                {
                    var angle = Vector2.SignedAngle(dir, active.orientation);
                    if (angle == 90f)
                    {
                        grid[active.position.SnapToGrid()] =
                            new ConveyorBeltRight(active.position, active.orientation,
                                availableNodes.GetByName<ConveyorBeltRight>());
                        OnNodeChange?.Invoke(this, new NodeEventArgs() { node = grid[active.position.SnapToGrid()] });
                    }
                    else if (angle == -90f)
                    {
                        grid[active.position.SnapToGrid()] =
                            new ConveyorBeltLeft(active.position, active.orientation,
                                availableNodes.GetByName<ConveyorBeltLeft>());
                        OnNodeChange?.Invoke(this, new NodeEventArgs() { node = grid[active.position.SnapToGrid()] });
                    }
                }


                var scriptableObject = availableNodes.GetByName<ConveyorBelt>();
                var node = new ConveyorBelt(gridPoint, dir, scriptableObject);
                grid[gridPoint.SnapToGrid()] = node;
                active = node;
                OnNodePlace?.Invoke(this, new NodeEventArgs() { node = grid[gridPoint.SnapToGrid()] });
            }
            else if (Input.GetMouseButtonDown(1))
            {
                if (Physics.Raycast(screenRay, out var nodeHit, 1000f, LayerMask.GetMask("Node")))
                {
                    var nodePosition = nodeHit.transform.position - new Vector3(0f, 0.5f, 0f);
                    if (grid.TryGetValue(nodePosition, out var clickedNode))
                    {
                        if (clickedNode.CanBeDeleted && !clickedNode.markedForDeletion)
                        {
                            markedActive = clickedNode;
                            MarkDeletion(clickedNode);
                        }
                        else if (clickedNode.CanBeDeleted && clickedNode.markedForDeletion)
                        {
                            var nodes = new List<Node>();
                            foreach (var node in grid.Values)
                            {
                                if (node.markedForDeletion)
                                    nodes.Add(node);
                            }

                            foreach (var node in nodes)
                            {
                                grid.Remove(node.position);
                                OnNodeDelete?.Invoke(this, new NodeEventArgs { node = node });
                            }

                            var fromNode = markedActive.position + -markedActive.CalculateInputWS().ToGridCoord();
                            active = grid[fromNode];
                        }
                    }
                }
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("Unmarking");
                // Unmark all nodes
                foreach (var node in grid.Values)
                {
                    if (node.markedForDeletion)
                    {
                        node.markedForDeletion = false;
                        OnNodeMark?.Invoke(this, new NodeEventArgs { node = node });
                    }
                }
            }
        }
        else if (Game.I.gameMode == Mode.Simulation)
        {
            currentSimulationDuration -= Time.deltaTime;
            if (currentSimulationDuration <= 0.0f)
            {
                SimulationStep();
                currentSimulationDuration = simulationTickDuration + Mathf.Abs(currentSimulationDuration);
            }
        }
    }

    void MarkDeletion(Node node)
    {
        if (node.CanBeDeleted)
        {
            node.markedForDeletion = true;
            OnNodeMark?.Invoke(this, new NodeEventArgs { node = node });
            if (node.HasNextNode(grid, out var nextNode))
            {
                MarkDeletion(nextNode);
            }
        }
    }

    Vector3 CalculateGridPoint(Vector3 xzIntersection)
    {
        var gridPoint = xzIntersection.SnapToGrid();
        if (active == null) return gridPoint + Vector3.zero;
        gridPoint = active.position + (gridPoint - active.position).normalized;
        gridPoint = gridPoint.SnapToGrid();
        return gridPoint;
    }

    bool CanPlaceNode(Vector3 position)
    {
        // If it's already occupied, we cannot place a node.
        if (grid.ContainsKey(position.SnapToGrid())) return false;

        if (!Game.I.level.points.Contains(position)) return false;

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