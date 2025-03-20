using System;
using System.Collections.Generic;
using Extensions;
using ScriptableObjects;
using UnityEngine;

namespace Nodes
{
    public class ConveyorBelt : Node, MultiDir
    {
        public ConveyorBelt(Vector3 position, Vector2 orientation) : base(position,
            orientation)
        {
        }


        public override Vector2 Input => Vector2.up;
        public override Vector2 Output => Vector2.up;
        public override bool CanBeDeleted => true;

        public override Vector3 PlaceItemPosition(float t)
        {
            var posUp = position + Vector3.up;
            var inputPos = posUp + -CalculateInputWS().ToGridCoord() * 0.5f;
            var outputPos = posUp + CalculateOutputWS().ToGridCoord() * 0.5f;
            return Vector3.Lerp(inputPos, outputPos, t);
        }

        public override void Tick(NodeGrid grid, int tickCount)
        {
            if (updateTick == tickCount)
            {
                holdsItem = nextTickHoldItem;
                Debug.Log("updated " + holdsItem);
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
                }
            }
        }

        public override bool ReceiveItem(Vector2 direction)
        {
            if (direction == CalculateInputWS())
            {
                nextTickHoldItem = true;
            }

            return nextTickHoldItem;
        }

        public override bool HasNextNode(NodeGrid grid, out Node node)
        {
            var nextNode = position.SnapToGrid() + CalculateOutputWS().ToGridCoord();
            if (grid.TryGetValue(nextNode, out var neighbour))
            {
                node = neighbour;
                return true;
            }

            node = null;
            return false;
        }

        public override string NodeName => "ConveyorBelt";
        public Type GetLeft => typeof(ConveyorBeltLeft);
        public Type GetRight => typeof(ConveyorBeltRight);
    }
}