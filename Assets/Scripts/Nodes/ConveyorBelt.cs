using System.Collections.Generic;
using Extensions;
using ScriptableObjects;
using UnityEngine;
using NodeGrid = System.Collections.Generic.Dictionary<UnityEngine.Vector2, Nodes.Node>;

namespace Nodes
{
    public class ConveyorBelt : Node, MultiDir
    {
        public ConveyorBelt(Vector3 position, Vector2 orientation, NodeScriptableObject so) : base(position,
            orientation, so)
        {
        }


        public override Vector2 Input => Vector2.up;
        public override Vector2 Output => Vector2.up;

        public override void Tick(NodeGrid grid, int tickCount)
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
                }
            }
        }

        public override bool ReceiveItem(Vector2 direction)
        {
            if (direction == CalculateInputWS())
            {
                nextTickHoldItem = true;
            }

            Debug.Log(
                $"Receive Item {NodeName} - {direction} - {Input} {CalculateInputWS()} {position} {nextTickHoldItem} {holdsItem}");

            return nextTickHoldItem;
        }

        public override bool HasNextNode(NodeGrid grid, out Node node)
        {
            var nextNode = position.ToGridCoord() + CalculateOutputWS();
            Debug.Log("has next Node" + nextNode + " " + string.Join(",", grid.Keys));
            if (grid.TryGetValue(nextNode, out var neighbour))
            {
                Debug.Log("yes");
                node = neighbour;
                return true;
            }

            Debug.Log("no");
            node = null;
            return false;
        }

        public override string NodeName => "ConveyorBelt";
    }
}