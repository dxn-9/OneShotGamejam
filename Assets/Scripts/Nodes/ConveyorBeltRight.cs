using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;

namespace Nodes
{
    public class ConveyorBeltRight : Node
    {
        public ConveyorBeltRight(Vector3 position, Orientation orientation, NodeScriptableObject so) : base(
            position,
            orientation, so)
        {
        }


        public override Orientation Input => Orientation.Down;
        public override Orientation Output => Orientation.Right;
        public override string NodeName => "ConveyorBeltRight";
    }
}