using System;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] GridManager gridManager;
    [SerializeField] Button simulateButton;

    public event EventHandler OnSimulateButton;

    void Awake()
    {
        simulateButton.onClick.AddListener(() => OnSimulateButton?.Invoke(this, EventArgs.Empty));
    }

    void Update()
    {
        simulateButton.gameObject.SetActive(Game.I.gameMode == Mode.Building);
    }
}