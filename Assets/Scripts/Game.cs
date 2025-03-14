using System;
using UnityEngine;

public class Game : MonoBehaviour
{
    Logger logger;
    public static Game Instance { get; private set; }
    [SerializeField] GridManager gridManager;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void OnGameOver()
    {
        gridManager.mode = Mode.EndLevel;
    }
}