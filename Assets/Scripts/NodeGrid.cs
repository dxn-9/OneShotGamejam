using System.Collections.Generic;
using Nodes;
using UnityEngine;

public class GridPointComparer : IEqualityComparer<Vector3>
{
    private const float Epsilon = 0.001f;

    public bool Equals(Vector3 a, Vector3 b)
    {
        return Vector3.Distance(a, b) < Epsilon;
    }

    public int GetHashCode(Vector3 obj)
    {
        return Mathf.RoundToInt(obj.x * 100) * 1000 * Mathf.RoundToInt(obj.z) * 1000 + Mathf.RoundToInt(obj.y * 100);
    }
}

public class NodeGrid : Dictionary<Vector3, Node>
{
    public NodeGrid() : base(new GridPointComparer())
    {
    }

    public Node GetStart()
    {
        return this[Game.I.level.startPoint];
    }

    public Node GetEnd()
    {
        return this[Game.I.level.endPoint];
    }
}