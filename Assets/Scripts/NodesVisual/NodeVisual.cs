using UnityEngine;

public class NodeVisual : MonoBehaviour
{
    [SerializeField] Transform deletionVisual;

    public void MarkForDeletion(bool marked)
    {
        deletionVisual.gameObject.SetActive(marked);
    }
}