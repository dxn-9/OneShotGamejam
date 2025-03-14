using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;
using NodeGrid = System.Collections.Generic.Dictionary<UnityEngine.Vector2, Nodes.Node>;

namespace Nodes
{
    public class StartNode : Node
    {
        public StartNode(Vector3 position, Vector2 orientation, NodeScriptableObject so) : base(position,
            orientation,
            so)
        {
        }


        public override Vector2 Input => Vector2.zero;
        public override Vector2 Output => Vector2.zero;


        public override void Tick(NodeGrid grid, int tickCount)
        {
            if (updateTick == tickCount)
            {
                holdsItem = nextTickHoldItem;
            }

            if (holdsItem && HasNextNode(grid, out var nextNode))
            {
                if (nextNode.ReceiveItem(nextNode.CalculateInputWS()))
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
            holdsItem = true;
            return true;
        }

        public override bool HasNextNode(NodeGrid grid, out Node node)
        {
            for (int i = -1; i <= 1; i++)
            {
                if (i == 0) continue;
                var x = new Vector2(position.x + i, position.z);
                var y = new Vector2(position.x, position.z + i);
                Node neighbour;
                if (grid.TryGetValue(x, out neighbour) || grid.TryGetValue(y, out neighbour))
                {
                    node = neighbour;
                    return true;
                }
            }

            node = null;
            return false;
        }

        public override string NodeName => "StartNode";
    }
}