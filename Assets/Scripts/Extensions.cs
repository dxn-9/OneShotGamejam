using UnityEngine;

namespace Extensions
{
    public static class Vector3Ext
    {
        public static Vector2 ToGridCoord(this Vector3 v) => new Vector2(v.x, v.z);
    }
}