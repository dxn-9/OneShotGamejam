using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;

namespace Nodes
{
    public class StartNode : Node
    {
        public StartNode(Vector3 position, Vector2 orientation, NodeScriptableObject so) : base(position,
            orientation,
            so)
        {
        }


        public override Vector2 Input => Vector2.down;
        public override Vector2 Output => Vector2.up;
        public override string NodeName => "StartNode";
    }
}