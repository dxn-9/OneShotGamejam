using System;
using System.Collections.Generic;
using Extensions;
using ScriptableObjects;
using UnityEngine;

namespace Nodes
{
    public class StartNode : Node, MultiDir
    {
        public StartNode(Vector3 position, Vector2 orientation) : base(position,
            orientation)
        {
        }


        public override Vector2 Input => Vector2.zero;
        public override Vector2 Output => Vector2.zero;
        public override bool CanBeDeleted => false;
        Vector3 connectedNodePosition = Vector3.zero;


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
                    Game.I.GameOver(false, "Failed to send item to next node. Item is stuck.");
                }
            }
            else if (holdsItem && !HasNextNode(grid, out nextNode))
            {
                Debug.LogError("Could not find a entry point. Simulation cannot start.");
                Game.I.GameOver(false, "Could not find a entry point. Simulation cannot start.");
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
                var x = new Vector3(position.x + i, position.y, position.z).SnapToGrid();
                var y = new Vector3(position.x, position.y, position.z + i).SnapToGrid();
                Node neighbour;
                if (grid.TryGetValue(x, out neighbour))
                {
                    if (neighbour.CalculateInputWS() == (neighbour.position - position).ToDir())
                    {
                        node = neighbour;
                        connectedNodePosition = node.position;
                        return true;
                    }
                }

                if (grid.TryGetValue(y, out neighbour))
                {
                    if (neighbour.CalculateInputWS() == (neighbour.position - position).ToDir())
                    {
                        node = neighbour;
                        connectedNodePosition = node.position;
                        return true;
                    }
                }
            }

            node = null;
            return false;
        }

        public override Vector3 PlaceItemPosition(float t)
        {
            if (connectedNodePosition != Vector3.zero)
            {
                Vector3 d = connectedNodePosition - position;
                return Vector3.Lerp(position + Vector3.up, position + d * 0.5f + Vector3.up, t);
            }
            else
            {
                return position + Vector3.up;
            }
        }

        public override string NodeName => "StartNode";
        public Type GetLeft => typeof(StartNode);
        public Type GetRight => typeof(StartNode);
    }
}