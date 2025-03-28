﻿using System;
using Nodes;
using UnityEngine;
using UnityEngine.Assertions;

namespace Extensions
{
    public static class Vector3Ext
    {
        public static Vector3 SnapToGrid(this Vector3 v) =>
            new Vector3(Mathf.Round(v.x), Mathf.Round(v.y), Mathf.Round(v.z));

        public static Vector2 ToDir(this Vector3 v) => new Vector2(Mathf.Round(v.x), Mathf.Round(v.z));
    }

    public static class Vector2Ext
    {
        public static Vector3 ToGridCoord(this Vector2 v) =>
            new Vector3(Mathf.Round(v.x), 0, Mathf.Round(v.y));
    }

    public static class HazardExt
    {
        public static bool ContainsAt(this Hazard[] hazards, Vector3 pos)
        {
            for (int i = 0; i < hazards.Length; i++)
            {
                var h = hazards[i];
                if (h.position == pos) return true;
            }

            return false;
        }
    }


    // public static class OrientationExtensions
    // {
    //     public static Orientation Next(this Orientation current)
    //     {
    //         Assert.IsFalse(current == Orientation.Any);
    //
    //         return (Orientation)(((int)current + 1) % 4);
    //     }
    //
    //     // Rotate can be a bit confusing. Think of vectors rotating around in 90 degs. Each next vector is 90 degs rotated
    //     public static Orientation Rotate(this Orientation current, Orientation by)
    //     {
    //         Assert.IsFalse(current == Orientation.Any);
    //         Assert.IsFalse(by == Orientation.Any);
    //
    //         return (Orientation)(((int)current + (int)by + 1) % 4);
    //     }
    //
    //     public static Vector2 ToVector2(this Orientation current)
    //         =>
    //             current switch
    //             {
    //                 Orientation.Left => Vector2.left,
    //                 Orientation.Down => Vector2.down,
    //                 Orientation.Right => Vector2.right,
    //                 Orientation.Up => Vector2.up,
    //                 Orientation.Any => Vector2.zero,
    //                 _ => throw new Exception("Invalid Orientation: " + current)
    //             };
    //
    //     public static Orientation FromVector2(Vector2 vec)
    //     {
    //         if (vec == Vector2.left) return Orientation.Left;
    //         if (vec == Vector2.down) return Orientation.Down;
    //         if (vec == Vector2.right) return Orientation.Right;
    //         if (vec == Vector2.up) return Orientation.Up;
    //         throw new Exception("Invalid Conversion: " + vec);
    //     }
    // }
}