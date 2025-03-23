using System.Collections.Generic;
using Extensions;
using ScriptableObjects;
using UnityEngine;

namespace Nodes
{
    public class CatapultNode : Node
    {
        public CatapultNode(Vector3 position, Vector2 orientation) : base(position,
            orientation)
        {
        }

        public override int Range => 2;
        public override Vector2 Input => Vector2.up;
        public override Vector2 Output => Vector2.up;
        public override bool CanBeDeleted => true;

        public override Vector3 PlaceItemPosition(float t)
        {
            var posUp = position + Vector3.up;
            if (t <= .15f)
            {
                var inputPos = posUp + -CalculateInputWS().ToGridCoord() * 0.5f;
                return Vector3.Lerp(inputPos, position + Vector3.up, t / .15f);
            }
            else
            {
                var outputPos = (posUp + CalculateOutputWS().ToGridCoord() * Range) -
                                (CalculateOutputWS().ToGridCoord() * 0.5f);
                // Since t >= .15
                float tt = (t - 0.15f) / (1.0f - 0.15f);
                float y = Mathf.Sin(tt * Mathf.PI);

                return new Vector3(Mathf.Lerp(posUp.x, outputPos.x, tt), y + Mathf.Lerp(posUp.y, outputPos.y, tt),
                    Mathf.Lerp(posUp.z, outputPos.z, tt));
            }
        }


        public override bool ReceiveItem(Vector2 direction)
        {
            if (direction == CalculateInputWS())
            {
                nextTickHoldItem = true;
            }

            return nextTickHoldItem;
        }

        public override string NodeName => "CatapultNode";
    }
}