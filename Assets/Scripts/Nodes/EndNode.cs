using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;
using NodeGrid = System.Collections.Generic.Dictionary<UnityEngine.Vector2, Nodes.Node>;

namespace Nodes
{
    public class EndNode : Node
    {
        public EndNode(Vector3 position, Vector2 orientation, NodeScriptableObject so) : base(position, orientation,
            so)
        {
        }


        public override Vector2 Input => Vector2.zero;
        public override Vector2 Output => Vector2.zero;

        public override void Tick(NodeGrid grid, int tickCount)
        {
            if (holdsItem)
            {
                Debug.Log("Game over");
                Game.Instance.OnGameOver();
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