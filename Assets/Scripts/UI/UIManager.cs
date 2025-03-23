using System;
using ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] GridManager gridManager;
    [SerializeField] Button simulateButton;
    [SerializeField] UINode[] nodesUI;
    [SerializeField] Transform gameOverUI;

    int currentNodeUI;

    public event EventHandler OnSimulateButton;

    void Awake()
    {
        simulateButton.onClick.AddListener(() => OnSimulateButton?.Invoke(this, EventArgs.Empty));
        gameOverUI.gameObject.SetActive(false);
        SetCurrentNodeUI(0);
        gameOverUI.Find("ButtonGroup/TryAgainButton").GetComponent<Button>().onClick
            .AddListener(OnRetryButtonClicked);
        gameOverUI.Find("ButtonGroup/NextLevelButton").GetComponent<Button>().onClick
            .AddListener(OnNextLevelButtonClicked);
    }

    void Start()
    {
        for (int i = 0; i < nodesUI.Length; i++)
        {
            int index = i;
            nodesUI[i].button.onClick.AddListener(() => SetCurrentNodeUI(index));
        }
    }

    void OnNextLevelButtonClicked()
    {
        HideGameOverUI();
        Game.I.NextLevel();
    }

    void OnRetryButtonClicked()
    {
        HideGameOverUI();
        Game.I.RestartLevel();
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

    public void HideGameOverUI()
    {
        gameOverUI.gameObject.SetActive(false);
    }

    public void ShowGameOverUI(bool win, string message)
    {
        var reason = gameOverUI.Find("Reason");
        var gameOverText = gameOverUI.Find("GameOverText");
        if (win)
        {
            reason.gameObject.SetActive(false);
            gameOverText.GetComponent<TextMeshProUGUI>().text = "Level completed!";
            gameOverText.GetComponent<TextMeshProUGUI>().color = Color.green;
        }
        else
        {
            reason.gameObject.SetActive(true);
            reason.GetComponent<TextMeshProUGUI>().text = message;
            gameOverText.GetComponent<TextMeshProUGUI>().text = "Game over";
            gameOverText.GetComponent<TextMeshProUGUI>().color = Color.red;
        }

        gameOverUI.gameObject.SetActive(true);
    }


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