using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;

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
        public override string NodeName => "EndNode";
    }
}