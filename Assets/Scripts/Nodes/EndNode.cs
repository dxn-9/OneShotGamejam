using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;

namespace Nodes
{
    public class EndNode : Node
    {
        public EndNode(Vector3 position, Vector2 orientation) : base(position, orientation
        )
        {
        }


        public override Vector2 Input => Vector2.zero;
        public override Vector2 Output => Vector2.zero;
        public override bool CanBeDeleted => false;

        public override void Tick(NodeGrid grid, int tickCount)
        {
            if (holdsItem)
            {
                Game.I.GameOver(true);
            }
        }

        public override bool ReceiveItem(Vector2 direction)
        {
            holdsItem = true;
            return true;
        }

        public override bool HasNextNode(NodeGrid grid, out Node node)
        {
            node = null;
            return false;
        }

        public override string NodeName => "EndNode";
    }
}