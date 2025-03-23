using System;
using System.Collections.Generic;
using Extensions;
using ScriptableObjects;
using UnityEngine;

namespace Nodes
{
    // Slingshot the item in the air, to avoid hazard
    public class WindConveyorBelt : Node, MultiDir
    {
        public WindConveyorBelt(Vector3 position, Vector2 orientation) : base(position,
            orientation)
        {
        }

        public override Vector2 Input => Vector2.up;
        public override Vector2 Output => Vector2.up;
        public override bool CanBeDeleted => true;
        public override Vector3 ItemPosition => (position + Vector3.up * 2f).SnapToGrid();

        public override Vector3 PlaceItemPosition(float t)
        {
            var posUp = position + Vector3.up * 2f;
            var inputPos = posUp + -CalculateInputWS().ToGridCoord() * 0.5f;
            var outputPos = posUp + CalculateOutputWS().ToGridCoord() * 0.5f;
            return Vector3.Lerp(inputPos, outputPos, t);
        }


        public override bool ReceiveItem(Vector2 direction)
        {
            if (direction == CalculateInputWS())
            {
                nextTickHoldItem = true;
            }

            return nextTickHoldItem;
        }

        public override string NodeName => "WindConveyorBelt";
        public Type GetLeft => typeof(WindConveyorBeltLeft);
        public Type GetRight => typeof(WindConveyorBeltRight);
    }
}