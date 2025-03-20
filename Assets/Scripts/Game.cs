using System;
using Nodes;
using ScriptableObjects;
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
    [SerializeField] float tickDuration = 0.3f;
    [SerializeField] public int maxStuckTicks = 5;
    [SerializeField] Transform buildingControls;
    int tickCount;
    float currentTickDuration;

    public Node ActiveNode => gridManager.active;


    public NodeScriptableObject SelectedNodeSO => uiManager.GetSelectedNodeSO();

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

    // Assume that the check for it being placeable is already done.
    public void PlaceBlock(Vector3 dir)
    {
        var nodePosition = gridManager.active.position + dir;
        gridManager.PlaceNode(nodePosition);
    }

    public bool CanPlaceBlock(Vector3 worldPos)
        => gridManager.CanPlaceNode(worldPos);


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
                camera.target = itemIndicator;
            }
        }

        if (gameMode == Mode.Building)
        {
            buildingControls.gameObject.SetActive(true);
            buildingControls.position = gridManager.active.position + Vector3.up;
        }

        if (gameMode == Mode.Simulation)
        {
            buildingControls.gameObject.SetActive(false);
            currentTickDuration -= Time.deltaTime;
            itemIndicator.position =
                gridManager.active.PlaceItemPosition(1.0f - Mathf.Max(currentTickDuration / tickDuration, 0.0f));
            if (currentTickDuration <= 0.0f)
            {
                gridManager.SimulationStep(tickCount);
                currentTickDuration = tickDuration + Mathf.Abs(currentTickDuration);
                tickCount++;
            }
        }
    }
}