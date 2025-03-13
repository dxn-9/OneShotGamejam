using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;

namespace Nodes
{
    public class ConveyorBeltRight : Node
    {
        public ConveyorBeltRight(Vector3 position, Vector2 orientation, NodeScriptableObject so) : base(
            position,
            orientation, so)
        {
        }


        public override Vector2 Input => Vector2.down;
        public override Vector2 Output => Vector2.right;
        public override string NodeName => "ConveyorBeltRight";
    }
}