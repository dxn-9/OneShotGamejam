using System;
using System.Collections.Generic;
using Extensions;
using JetBrains.Annotations;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace Nodes
{
    public enum Orientation : int
    {
        Right,
        Down,
        Left,
        Up,
        Any
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
        [CanBeNull] public Node nextNode;
        public bool holdsItem;

        public void Tick()
        {
            if (holdsItem && nextNode != null)
            {
                Debug.DrawLine(position, position + Vector3.up, Color.red, 0.1f);
                holdsItem = false;
                nextNode.holdsItem = true;
            }
        }

        public abstract string NodeName { get; }
        public abstract Orientation Input { get; }
        public abstract Orientation Output { get; }
    }
}