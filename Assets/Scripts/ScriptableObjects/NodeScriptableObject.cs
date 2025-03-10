using System.Linq;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "NodeScriptableObject", menuName = "ScriptableObjects/NodeScriptableObject")]
    public class NodeScriptableObject : ScriptableObject
    {
        public GameObject prefab;
        public string nodeName;
    }

    public static class NodeScriptableObjectExtension
    {
        public static NodeScriptableObject GetByName<T>(this NodeScriptableObject[] objs)
        {
            return objs.Single(obj => obj.nodeName == typeof(T).Name);
        }
    }
}