using System;
using System.Collections.Generic;
using System.Linq;
using Nodes;
using ScriptableObjects;
using UnityEngine;


public class GridManager : MonoBehaviour
{
    [SerializeField, Min(0.5f)] float gridSize;
    [SerializeField] NodeScriptableObject[] availableNodes;
    LinkedList<Node> nodes;
    LinkedListNode<Node> active;
    Orientation currentOrientation;


    // Debug params
    Vector3 xzIntersection;
    bool canPlace;


    void Awake()
    {
        currentOrientation = Orientation.Up;
        nodes = new LinkedList<Node>();
        Debug.Log(nameof(Start));
        Debug.Log(typeof(Start).Name);
        var startNode =
            new LinkedListNode<Node>(new Start(Vector3.zero, Orientation.Up, availableNodes.GetByName<Start>()));
        nodes.AddFirst(startNode);
        active = startNode;
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
            var node = active.Value;
            var orientation = node.Output.Rotate(node.orientation).ToVector2();
            Gizmos.DrawCube(node.position + Vector3.up + new Vector3(orientation.x, 0f, orientation.y) * 0.5f,
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
        }

        if (canPlace && Input.GetMouseButtonDown(0))
        {
            var scriptableObject = availableNodes.GetByName<ConveyorBelt>();
            var node = nodes.AddLast(new ConveyorBelt(gridPoint, currentOrientation, scriptableObject));
            active = node;
        }
    }

    bool CanPlaceNode(Vector3 position)
    {
        // Could be a floating point error. But the values are floored before.. TODO: Check if there can be any fp error
        if (Mathf.Abs(active.Value.position.x - position.x) == 1.0f)
        {
            if (active.Value.position.z == position.z) return true;
        }

        if (Mathf.Abs(active.Value.position.z - position.z) == 1.0f)
        {
            if (active.Value.position.x == position.x) return true;
        }

        return false;
    }
}