using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] GridManager gridManager;
    [SerializeField] Transform orientationDisplay;

    void Awake()
    {
        gridManager.OnOrientationChange += GridManagerOnOnOrientationChange;
    }

    void GridManagerOnOnOrientationChange(object sender, GridManager.OnOrientationChangeEventArgs e)
    {
        orientationDisplay.localRotation = Quaternion.Euler(0f, 0f, (int)e.orientation * -90f);
    }
}