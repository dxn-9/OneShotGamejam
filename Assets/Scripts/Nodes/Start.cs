using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;

namespace Nodes
{
    public class Start : Node
    {
        public Start(Vector3 position, Orientation orientation, NodeScriptableObject so) : base(position, orientation,
            so)
        {
        }

        public override void Tick(LinkedListNode<Node> next)
        {
        }

        public override Orientation Input => Orientation.Down;
        public override Orientation Output => Orientation.Up;
    }
}