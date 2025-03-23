using System.Collections.Generic;
using Extensions;
using ScriptableObjects;
using UnityEngine;

namespace Nodes
{
    public class WindConveyorBeltLeft : WindConveyorBelt
    {
        public WindConveyorBeltLeft(Vector3 position, Vector2 orientation) : base(position,
            orientation)
        {
        }

        public override Vector2 Input => Vector2.up;
        public override Vector2 Output => Vector2.left;
        public override string NodeName => "WindConveyorBeltLeft";
    }
}