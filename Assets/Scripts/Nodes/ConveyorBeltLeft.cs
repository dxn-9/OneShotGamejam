using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;

namespace Nodes
{
    public class ConveyorBeltLeft : Node
    {
        public ConveyorBeltLeft(Vector3 position, Orientation orientation, NodeScriptableObject so) : base(
            position,
            orientation, so)
        {
        }


        public override Orientation Input => Orientation.Down;
        public override Orientation Output => Orientation.Left;
        public override string NodeName => "ConveyorBeltLeft";
    }
}