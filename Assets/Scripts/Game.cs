using System;
using Nodes;
using ScriptableObjects;
using Unity.Cinemachine;
using UnityEditor;
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
    [SerializeField] GameObject buildingControlsPrefab;
    [SerializeField] public SoundManager soundManager;
    [SerializeField] TrainController trainController;

    Transform buildingControls;
    bool isInitialized;

    string currentLevel;
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

        isInitialized = false;

        currentLevel = "Level1";

        SceneManager.sceneLoaded += OnSceneLoaded;

        LoadMenu();
        // SceneManager.LoadScene("UIScene", LoadSceneMode.Additive);
        // SceneManager.LoadScene("Level1", LoadSceneMode.Additive);

        GameObject[] objs = GameObject.FindGameObjectsWithTag("DontDestroy");
        foreach (var obj in objs) DontDestroyOnLoad(obj);
    }

    void LoadMenu()
    {
        isInitialized = false;
        SceneManager.LoadScene("MenuScene");
    }

    void OnNodePlace(object sender, GridManager.NodeEventArgs e)
    {
        level.AddPlayerNode(e.node);
    }

    void OnNodeChange(object sender, GridManager.NodeEventArgs e)
    {
        level.ChangePlayerNode(e.node);
    }

    void OnNodeMark(object sender, GridManager.NodeEventArgs e)
    {
        level.MarkForDeletion(e.node);
    }

    void OnNodeDelete(object sender, GridManager.NodeEventArgs e)
    {
        level.DeleteNode(e.node);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MenuScene")
        {
            uiManager.gameObject.SetActive(false);
            itemIndicator.gameObject.SetActive(false);
            return;
        }

        if (scene.name == currentLevel)
        {
            uiManager.gameObject.SetActive(true);
            itemIndicator.gameObject.SetActive(true);
            gameMode = Mode.Building;
            gridManager = GameObject.FindGameObjectWithTag("GridManager").GetComponent<GridManager>();
            level = GameObject.FindGameObjectWithTag("Level").GetComponent<Level>();
            buildingControls = Instantiate(buildingControlsPrefab).transform;
            gridManager.OnNodePlace += OnNodePlace;
            gridManager.OnNodeChange += OnNodeChange;
            gridManager.OnNodeMark += OnNodeMark;
            gridManager.OnNodeDelete += OnNodeDelete;
            isInitialized = true;
        }

        Debug.Log("Scene loaded " + scene.name);
        // SceneManager.sceneLoaded -= OnSceneLoaded;
        //
    }

    public void LoadLevel(string levelName)
    {
        currentLevel = levelName;
        SceneManager.LoadScene(levelName);
    }

    // Assume that the check for it being placeable is already done.
    public void PlaceBlock(Vector3 dir)
    {
        var nodePosition = gridManager.active.position + dir;
        gridManager.PlaceNode(nodePosition);
    }

    public bool CanPlaceNode(Vector3 worldPos)
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

    void UnloadScene()
    {
        gridManager.OnNodePlace -= OnNodePlace;
        gridManager.OnNodeChange -= OnNodeChange;
        gridManager.OnNodeMark -= OnNodeMark;
        gridManager.OnNodeDelete -= OnNodeDelete;
    }

    public void NextLevel()
    {
        isInitialized = false;
        UnloadScene();
        LoadLevel(Utils.GetNextLevel(currentLevel));
    }

    public void RestartLevel()
    {
        isInitialized = false;
        UnloadScene();
        LoadLevel(currentLevel);
    }

    public void GameOver(bool win, string reason = "")
    {
        uiManager.ShowGameOverUI(win, reason);
        gameMode = Mode.EndLevel;
        if (win)
        {
            soundManager.PlayWinSound();
        }
        else
        {
            soundManager.PlayLooseSound();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LoadMenu();
            return;
        }

        if (!isInitialized) return;
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
            trainController.IsRunning(false);
        }

        if (gameMode == Mode.Simulation)
        {
            trainController.IsRunning(true);
            buildingControls.gameObject.SetActive(false);
            currentTickDuration -= Time.deltaTime;
            var nextPos =
                gridManager.active.PlaceItemPosition(1.0f - Mathf.Max(currentTickDuration / tickDuration, 0.0f));
            var dtPos = nextPos - itemIndicator.position;
            itemIndicator.position = nextPos;
            itemIndicator.rotation = Quaternion.Lerp(itemIndicator.rotation, Quaternion.LookRotation(dtPos),
                Time.deltaTime * 3.5f);
            if (currentTickDuration <= 0.0f)
            {
                gridManager.SimulationStep(tickCount);
                currentTickDuration = tickDuration + Mathf.Abs(currentTickDuration);
                tickCount++;
            }
        }
    }
}