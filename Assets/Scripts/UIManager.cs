using System;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] GridManager gridManager;
    [SerializeField] Transform orientationDisplay;
    [SerializeField] Button simulateButton;

    public event EventHandler OnSimulateButton;

    void Awake()
    {
        gridManager.OnOrientationChange += GridManagerOnOnOrientationChange;
        simulateButton.onClick.AddListener(() => OnSimulateButton?.Invoke(this, EventArgs.Empty));
    }

    void GridManagerOnOnOrientationChange(object sender, GridManager.OnOrientationChangeEventArgs e)
    {
        orientationDisplay.localRotation = Quaternion.Euler(0f, 0f, (int)e.orientation * -90f);
    }
}