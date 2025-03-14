using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;
using NodeGrid = System.Collections.Generic.Dictionary<UnityEngine.Vector2, Nodes.Node>;

namespace Nodes
{
    public class ConveyorBeltRight : ConveyorBelt
    {
        public ConveyorBeltRight(Vector3 position, Vector2 orientation, NodeScriptableObject so) : base(
            position,
            orientation, so)
        {
        }


        public override Vector2 Input => Vector2.up;
        public override Vector2 Output => Vector2.right;
        public override string NodeName => "ConveyorBeltRight";
    }
}