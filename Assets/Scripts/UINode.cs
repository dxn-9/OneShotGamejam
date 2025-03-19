using ScriptableObjects;
using UnityEngine;
using UnityEngine.UIElements;

public class UINode : MonoBehaviour
{
    [SerializeField] public NodeScriptableObject nodeSO;
    public UnityEngine.UI.Image image;

    void Awake()
    {
        image = GetComponent<UnityEngine.UI.Image>();
    }
}