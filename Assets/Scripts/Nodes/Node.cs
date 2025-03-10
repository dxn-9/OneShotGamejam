using System;
using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Nodes
{
    public enum Orientation : int
    {
        Right,
        Down,
        Left,
        Up
    }

    public static class OrientationExtensions
    {
        public static Orientation Next(this Orientation current)
        {
            return (Orientation)(((int)current + 1) % 4);
        }

        // Rotate can be a bit confusing. Think of vectors rotating around in 90 degs. Each next vector is 90 degs rotated
        public static Orientation Rotate(this Orientation current, Orientation by)
        {
            return (Orientation)(((int)current + (int)by + 1) % 4);
        }

        public static Vector2 ToVector2(this Orientation current)
            =>
                current switch
                {
                    Orientation.Left => Vector2.left,
                    Orientation.Down => Vector2.down,
                    Orientation.Right => Vector2.right,
                    Orientation.Up => Vector2.up,
                    _ => throw new Exception("Invalid Orientation: " + current)
                };
    }

    public abstract class Node
    {
        protected Node(Vector3 position, Orientation orientation, NodeScriptableObject so)
        {
            this.position = position;
            this.orientation = orientation;
            var orientationVec = orientation.ToVector2();
            var rotation = Quaternion.LookRotation(new Vector3(orientationVec.x, 0f, orientationVec.y));
            transform = Object.Instantiate(so.prefab, position, rotation).transform;
        }

        public readonly Vector3 position;
        public readonly Orientation orientation;
        public Transform transform;
        public Node nextNode;
        public bool holdsItem;
        public abstract void Tick(LinkedListNode<Node> next);
        public abstract Orientation Input { get; }
        public abstract Orientation Output { get; }
    }
}