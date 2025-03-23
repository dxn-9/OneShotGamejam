using ScriptableObjects;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

public class UINode : MonoBehaviour
{
    [SerializeField] public NodeScriptableObject nodeSO;
    public UnityEngine.UI.Image image;
    public Button button;

    void Awake()
    {
        image = GetComponent<UnityEngine.UI.Image>();
        button = GetComponent<Button>();
    }
}