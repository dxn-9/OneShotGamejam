using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;
using NodeGrid = System.Collections.Generic.Dictionary<UnityEngine.Vector2, Nodes.Node>;

namespace Nodes
{
    public class ConveyorBeltLeft : ConveyorBelt
    {
        public ConveyorBeltLeft(Vector3 position, Vector2 orientation) : base(
            position,
            orientation)
        {
        }


        public override Vector2 Input => Vector2.up;
        public override Vector2 Output => Vector2.left;
        public override string NodeName => "ConveyorBeltLeft";
    }
}