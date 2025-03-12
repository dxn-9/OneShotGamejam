using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;

namespace Nodes
{
    public class EndNode : Node
    {
        public EndNode(Vector3 position, Orientation orientation, NodeScriptableObject so) : base(position, orientation,
            so)
        {
        }


        public override Orientation Input => Orientation.Down;
        public override Orientation Output => Orientation.Up;
        public override string NodeName => "EndNode";
    }
}