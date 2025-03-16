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
    // public enum Orientation : int
    // {
    //     Right,
    //     Down,
    //     Left,
    //     Up,
    //     Any
    // }


    public abstract class Node
    {
        protected Node(Vector3 position, Vector2 orientation, NodeScriptableObject so)
        {
            this.position = position;
            this.orientation = orientation;
            // var orientationVec = orientation;
            // var rotation = Quaternion.LookRotation(new Vector3(orientationVec.x, 0f, orientationVec.y));
            // transform = Object.Instantiate(so.prefab, position, rotation).transform;
        }

        public readonly Vector3 position;
        public readonly Vector2 orientation;
        public bool holdsItem;
        public bool nextTickHoldItem; // If in the next tick it should hold item
        public int updateTick = -1;
        public bool markedForDeletion;

        public Vector2 CalculateOutputWS()
        {
            return Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.up, orientation)) * Output;
        }

        public Vector2 CalculateInputWS()
        {
            return Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.up, orientation)) * Input;
        }

        public abstract void Tick(NodeGrid grid, int tickCount);

        public abstract bool ReceiveItem(Vector2 direction);

        public abstract bool HasNextNode(NodeGrid grid, out Node node);
        public abstract string NodeName { get; }
        public abstract Vector2 Input { get; }
        public abstract Vector2 Output { get; }
        public abstract bool CanBeDeleted { get; }
    }
}