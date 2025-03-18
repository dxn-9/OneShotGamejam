using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Mode
{
    Building,
    Simulation,
    EndLevel
}

public class Game : MonoBehaviour
{
    Logger logger;
    public static Game I { get; private set; }
    public Mode gameMode;

    [SerializeField] GridManager gridManager;
    [SerializeField] UIManager uiManager;
    [SerializeField] CameraController camera;
    [SerializeField] public Level level;
    [SerializeField] public Transform itemIndicator;

    void Awake()
    {
        if (I == null)
        {
            I = this;
        }


        SceneManager.LoadScene("UIScene", LoadSceneMode.Additive);

        gameMode = Mode.Building;
        gridManager.OnNodePlace += (sender, args) => level.AddPlayerNode(args.node);
        gridManager.OnNodeChange += (sender, args) => level.ChangePlayerNode(args.node);
        gridManager.OnNodeMark += (sender, args) => level.MarkForDeletion(args.node);
        gridManager.OnNodeDelete += (sender, args) => level.DeleteNode(args.node);
    }

    void Start()
    {
        uiManager.OnSimulateButton += OnStartSimulation;
    }

    void OnStartSimulation(object sender, EventArgs e)
    {
        foreach (var node in gridManager.grid.Values)
        {
            node.holdsItem = false;
        }

        gridManager.active = gridManager.grid.GetStart();
        gridManager.active.ReceiveItem(Vector2.zero);
        gameMode = Mode.Simulation;
    }

    public void GameOver(bool win)
    {
        Debug.Log("GameOver" + win);
        if (win)
        {
            // TODO: Implement win logic
            gameMode = Mode.Building;
        }
        else
        {
            gameMode = Mode.Building;
        }
    }

    void Update()
    {
        if (gridManager.active != null)
        {
            camera.target = level.playerNodes[gridManager.active.position];
            if (gameMode == Mode.Simulation)
            {
                itemIndicator.position = camera.target.position + Vector3.up;
            }
        }
    }
}