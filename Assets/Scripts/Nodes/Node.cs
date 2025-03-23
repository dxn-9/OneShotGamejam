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
        protected Node(Vector3 position, Vector2 orientation)
        {
            this.position = position;
            this.orientation = orientation;
        }

        public readonly Vector3 position;
        public readonly Vector2 orientation;
        public bool holdsItem;
        public bool nextTickHoldItem; // If in the next tick it should hold item
        public int updateTick = -1;
        public bool markedForDeletion;
        public virtual int Range => 1;
        public virtual Vector3 ItemPosition => (position + Vector3.up).SnapToGrid();


        public Vector2 CalculateOutputWS()
        {
            return Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.up, orientation)) * Output;
        }

        public Vector2 CalculateInputWS()
        {
            return Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.up, orientation)) * Input;
        }

        public virtual void Tick(NodeGrid grid, int tickCount)
        {
            if (updateTick == tickCount)
            {
                holdsItem = nextTickHoldItem;
            }

            if (holdsItem && HasNextNode(grid, out var nextNode))
            {
                if (nextNode.ReceiveItem(CalculateOutputWS()))
                {
                    nextTickHoldItem = false;
                    updateTick = nextNode.updateTick = tickCount + 1;
                }
                else
                {
                    Debug.LogError("Failed to send item to next node. Item is stuck.");
                    Game.I.GameOver(false, "Failed to send item to next node. Item is stuck.");
                }
            }
            else if (holdsItem && !HasNextNode(grid, out nextNode))
            {
                Debug.LogError("Failed to send item to next node. Item is stuck.");
                Game.I.GameOver(false, "Failed to send item to next node. Item is stuck.");
            }
        }

        public virtual bool HasNextNode(NodeGrid grid, out Node node)
        {
            var nextNode = position.SnapToGrid() + CalculateOutputWS().ToGridCoord() * Range;
            if (grid.TryGetValue(nextNode, out var neighbour))
            {
                node = neighbour;
                return true;
            }

            node = null;
            return false;
        }

        public abstract bool ReceiveItem(Vector2 direction);
        public abstract string NodeName { get; }
        public abstract Vector2 Input { get; }
        public abstract Vector2 Output { get; }
        public abstract bool CanBeDeleted { get; }

        //Places the item on the node, t is 0..1 and is the time of the tick between when it started, and when it ends.
        public abstract Vector3 PlaceItemPosition(float t);
    }
}