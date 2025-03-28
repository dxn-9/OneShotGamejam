using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Nodes;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.EventSystems;


// TODO: Refactor this class. it's doing a lot more than it should.
// This class is responsible for holding the grid data, where the belts are placed and simulating the game.
public class GridManager : MonoBehaviour
{
    [SerializeField, Min(0.5f)] float gridSize;
    [SerializeField] NodeScriptableObject[] availableNodes;

    public event EventHandler<NodeEventArgs> OnNodePlace;
    public event EventHandler<NodeEventArgs> OnNodeChange;
    public event EventHandler<NodeEventArgs> OnNodeMark;
    public event EventHandler<NodeEventArgs> OnNodeDelete;

    public class NodeEventArgs : EventArgs
    {
        public Node node;
    }

    public NodeGrid grid;
    public Node active, markedActive;
    bool canPlace;


    void Start()
    {
        grid = new NodeGrid
        {
            [Game.I.level.startPoint] = new StartNode(Game.I.level.startPoint, Vector2.up),
            [Game.I.level.endPoint] = new EndNode(Game.I.level.endPoint, Vector2.up)
        };
        active = grid.GetStart();
        grid.GetStart().ReceiveItem(Vector2.zero);
        Game.I.itemIndicator.position = active.ItemPosition;
    }


    // Ticks the active node to transport the item. If it returns true it means we've finished traversing the list.
    public void SimulationStep(int tickCount)
    {
        foreach (var node in grid.Values)
        {
            node.Tick(grid, tickCount);
            if (node.holdsItem)
            {
                if (Game.I.level.hazards.ContainsAt(node.ItemPosition))
                {
                    Debug.LogError("Hazard hit");
                    Game.I.GameOver(false, "Hazard hit");
                    return;
                }

                active = node;
            }
        }
    }


    public void PlaceNode(Vector3 gridPoint)
    {
        gridPoint = gridPoint.SnapToGrid();
        if (Utils.typeMap.TryGetValue(Game.I.SelectedNodeSO.nodeName, out var nodeClass))
        {
            Debug.Log(Game.I.SelectedNodeSO.nodeName);
            Vector2 dir = (gridPoint - active.position).ToDir();
            if (dir != active.orientation && active is MultiDir multiDirNode)
            {
                var angle = Vector2.SignedAngle(dir, active.orientation);
                if (angle == 90f)
                {
                    var rightType = multiDirNode.GetRight;
                    if (typeof(Node).IsAssignableFrom(rightType))
                    {
                        Debug.Log("Left type" + rightType);
                        grid[active.position.SnapToGrid()] =
                            (Node)Activator.CreateInstance(rightType, active.position, active.orientation);
                    }

                    OnNodeChange?.Invoke(this, new NodeEventArgs() { node = grid[active.position.SnapToGrid()] });
                }
                else if (angle == -90f)
                {
                    var leftType = multiDirNode.GetLeft;
                    if (typeof(Node).IsAssignableFrom(leftType))
                    {
                        Debug.Log("Right type" + leftType);
                        grid[active.position.SnapToGrid()] =
                            (Node)Activator.CreateInstance(leftType, active.position, active.orientation);
                    }

                    OnNodeChange?.Invoke(this, new NodeEventArgs() { node = grid[active.position.SnapToGrid()] });
                }
            }


            var node = (Node)Activator.CreateInstance(nodeClass, gridPoint, dir);
            grid[gridPoint] = node;
            active = node;
            OnNodePlace?.Invoke(this, new NodeEventArgs() { node = grid[gridPoint] });
        }
        else
        {
            Debug.Log("Cannot instantiate: " + Game.I.SelectedNodeSO);
        }
    }


    void Update()
    {
        if (Game.I.gameMode == Mode.Building)
        {
            // Skip over ui elements
            if (EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("Is Over game object");
                return;
            }

            // Handle deletion of nodes
            if (Input.GetMouseButtonDown(1))
            {
                Debug.Log("Right click");
                Vector2 screenMouse = Input.mousePosition;
                var screenRay = Camera.main.ScreenPointToRay(screenMouse);
                if (Physics.Raycast(screenRay, out var nodeHit, 1000f, LayerMask.GetMask("Node")))
                {
                    var nodePosition = nodeHit.transform.position - new Vector3(0f, 0.5f, 0f);
                    if (grid.TryGetValue(nodePosition.SnapToGrid(), out var clickedNode))
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

                            Vector3 fromNode = Vector3.zero;
                            // Try to get the closest node to the deleted one, we need to scan the area since we dont know the range of the previous one.
                            // Ideally, i should be at max the max range of a node
                            for (int i = 1; i <= 2; i++)
                            {
                                if (grid.ContainsKey(
                                        markedActive.position + -markedActive.CalculateInputWS().ToGridCoord() * i
                                    ))
                                {
                                    fromNode =
                                        markedActive.position + -markedActive.CalculateInputWS().ToGridCoord() * i;
                                    break;
                                }
                            }

                            active = grid[fromNode];
                        }
                    }
                }
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
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
        gridPoint = active.position + (gridPoint - active.position).normalized * active.Range;
        gridPoint = gridPoint.SnapToGrid();
        return gridPoint;
    }

    public bool CanPlaceNode(Vector3 position)
    {
        // If it's already occupied, we cannot place a node.
        if (grid.ContainsKey(position.SnapToGrid())) return false;

        if (!Game.I.level.terrain.Contains(position)) return false;
        if (Game.I.level.terrain.Contains(position + Vector3.up)) return false;


        if (active is MultiDir)
        {
            if (Mathf.RoundToInt(Mathf.Abs(active.position.x - position.x)) == active.Range)
            {
                if (active.position.z == position.z) return true;
            }

            if (Mathf.RoundToInt(Mathf.Abs(active.position.z - position.z)) == active.Range)
            {
                if (active.position.x == position.x) return true;
            }
        }
        else
        {
            var activeDir = active.CalculateOutputWS() * active.Range;
            if (position == (active.position + activeDir.ToGridCoord()).SnapToGrid()) return true;
        }

        return false;
    }
}