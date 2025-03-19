using System;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] GridManager gridManager;
    [SerializeField] Button simulateButton;
    [SerializeField] UINode[] nodesUI;

    int currentNodeUI;

    public event EventHandler OnSimulateButton;

    void Awake()
    {
        simulateButton.onClick.AddListener(() => OnSimulateButton?.Invoke(this, EventArgs.Empty));
        SetCurrentNodeUI(0);
    }

    void SetNodeActive(UINode nodeUI, bool active)
    {
        nodeUI.image.color = active
            ? new Color(nodeUI.image.color.r, nodeUI.image.color.g, nodeUI.image.color.b, .8f)
            : new Color(nodeUI.image.color.r, nodeUI.image.color.g, nodeUI.image.color.b, .5f);
    }

    public void SetCurrentNodeUI(int index)
    {
        for (int i = 0; i < nodesUI.Length; i++)
        {
            SetNodeActive(nodesUI[i], i == index);
        }

        currentNodeUI = index;
    }

    public NodeScriptableObject GetSelectedNodeSO() => nodesUI[currentNodeUI].nodeSO;


    void Update()
    {
        simulateButton.gameObject.SetActive(Game.I.gameMode == Mode.Building);
        if (Input.mouseScrollDelta.y > 0)
        {
            SetCurrentNodeUI(Mathf.Abs(currentNodeUI - 1) % nodesUI.Length);
        }
        else if (Input.mouseScrollDelta.y < 0)
        {
            SetCurrentNodeUI((currentNodeUI + 1) % nodesUI.Length);
        }
    }
}