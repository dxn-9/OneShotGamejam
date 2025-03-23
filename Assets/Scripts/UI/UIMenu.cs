using System;
using UnityEngine;
using UnityEngine.UI;

public class UIMenu : MonoBehaviour
{
    [SerializeField] Button startGameButton,
        howToPlayButton,
        closePanelButton,
        selectLevelButton,
        closeSelectLevelButton,
        quitGameButton;

    [SerializeField] Transform howToPlayPanel, selectLevelPanel, buttonContainer;
    [SerializeField] Vector3 cameraPosition = new Vector3(-8f, 11.3f, -14f);


    void Awake()
    {
        startGameButton.onClick.AddListener(StartGame);
        howToPlayButton.onClick.AddListener(ShowHowToPlayPanel);
        closePanelButton.onClick.AddListener(HideHowToPlayPanel);
        selectLevelButton.onClick.AddListener(ShowSelectLevelPanel);
        closeSelectLevelButton.onClick.AddListener(HideSelectLevelPanel);
        quitGameButton.onClick.AddListener(Application.Quit);


        for (int i = 0; i < buttonContainer.childCount; i++)
        {
            Button button = buttonContainer.GetChild(i).GetComponent<Button>();
            int index = i;
            button.onClick.AddListener(() => Game.I.LoadLevel("Level" + (index + 1)));
        }
    }

    void Start()
    {
        Camera.main.transform.position = cameraPosition;
    }

    void HideSelectLevelPanel()
    {
        selectLevelPanel.gameObject.SetActive(false);
    }

    void ShowSelectLevelPanel()
    {
        selectLevelPanel.gameObject.SetActive(true);
    }

    void ShowHowToPlayPanel()
    {
        howToPlayPanel.gameObject.SetActive(true);
    }

    void HideHowToPlayPanel()
    {
        howToPlayPanel.gameObject.SetActive(false);
    }

    void StartGame()
    {
        Game.I.LoadLevel("Level1");
    }

    void OnDestroy()
    {
        startGameButton?.onClick.RemoveAllListeners();
    }
}