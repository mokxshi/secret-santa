﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : Singleton<GameManager>
{
    public enum GameState
    {
        PREGAME,
        RUNNING,
        PAUSE
    }
    
    public GameObject[] SystemPrefab;
    public Events.EventGameState OnGameStateChanged;

    List<GameObject> _instancedSystemPrefabs;
    List<AsyncOperation> _loadOperations = new List<AsyncOperation>();
    GameState _currentGameState = GameState.PREGAME;

    private string _currentLevelName = string.Empty;
    public bool hasKizune;
    public bool hasHelmet;
    public bool enteredLR;
    public bool enteredK;
    private GameObject kizune;

    public GameState CurrentGameState
    {
        get { return _currentGameState; }
        private set { _currentGameState = value; }
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);

        _instancedSystemPrefabs = new List<GameObject>();

        InstantiateSystemPrefabs();

        UIManager.Instance.OnMainMenuFadeComplete.AddListener(HandleMainMenuFadeComplete);
        UIManager.Instance.HandleDeath.AddListener(HandleDeathScreen);
        
    }

    void OnLoadOperationComplete(AsyncOperation ao)
    {
        if (GameObject.Find("Kizune") != null)
        {
            kizune = GameObject.Find("Kizune");
            kizune.SetActive(false);
        }
        
        if (_currentLevelName == "livingroom" && !enteredLR)
        {
            UIManager.Instance.KizuneTalk(5);
            enteredLR = true;
        }

        if (_currentLevelName == "kitchen" && !enteredK)
        {
            UIManager.Instance.KizuneTalk(3);
            enteredLR = true;
        }

        if (GameObject.Find("KizunePlüsch") != null && hasKizune)
        {
            GameObject.Find("KizunePlüsch").SetActive(false);
        }

        if (GameObject.Find("mask_moster_prefab") != null && !hasHelmet)
        {
            GameObject.Find("mask_moster_prefab").SetActive(false);
        }
        else if (GameObject.Find("mask_moster_prefab") != null)
        {
            GameObject.Find("mask_prefab").SetActive(false);
            GameObject.Find("Spot Light").SetActive(false);
        }
    }

    void OnUnloadOperationComplete(AsyncOperation ao)
    {
        if (_loadOperations.Contains(ao))
        {
            _loadOperations.Remove(ao);

            if (_loadOperations.Count == 0)
            {
                UpdateState(GameState.PREGAME);
            }
        }
    }

    void HandleMainMenuFadeComplete(bool fadeOut)
    {
        if (!fadeOut)
        {    
            UnloadLevel(_currentLevelName);
            UIManager.Instance.SetDummyCameraActive(true);
        }
    }

    void HandleDeathScreen(bool x)
    {
        UnloadLevel(_currentLevelName);
        LoadLevel("livingroom");
        UIManager.Instance.StopDeath();
    }

    void UpdateState (GameState state)
    {
        GameState previouseGameState = _currentGameState;
        _currentGameState = state;

        switch(_currentGameState)
        {
            case GameState.PREGAME:
                Time.timeScale = 1.0f;
                Cursor.visible = true;
                break;

            case GameState.RUNNING:
                Time.timeScale = 1.0f;
                Cursor.visible = false;
                break;

            case GameState.PAUSE:
                Time.timeScale = 0.0f;
                Cursor.visible = true;
                break;

            default:
                break;
        }

        OnGameStateChanged.Invoke(_currentGameState, previouseGameState);
    }

    void InstantiateSystemPrefabs()
    {
        GameObject prefabInstance;
        for (int i = 0; i < SystemPrefab.Length; i++)
        {
            prefabInstance = Instantiate(SystemPrefab[i]);
            _instancedSystemPrefabs.Add(prefabInstance);
        }
    }

    public void LoadLevel(string levelName)
    {
        AsyncOperation ao = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
        if (ao == null)
        {
            Debug.LogError("[GameManager] Unable to load " + levelName);
            return;
        }

        ao.completed += OnLoadOperationComplete;
        _loadOperations.Add(ao);
        _currentLevelName = levelName;
    }

    public void UnloadLevel (string levelName)
    {
        AsyncOperation ao = SceneManager.UnloadSceneAsync(levelName);
        if (ao == null)
        {
            Debug.LogError("[GameManager] Unable to unload " + levelName);
            return;
        }

        ao.completed += OnUnloadOperationComplete;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        for (int i = 0; i < _instancedSystemPrefabs.Count; ++i)
        {
            Destroy(_instancedSystemPrefabs[i]);
        }
        _instancedSystemPrefabs.Clear();
    }

    public void StartGame()
    {
        UpdateState(GameState.RUNNING);
        LoadLevel("child_room");      
    }

    public void TogglePause()
    {
        UpdateState(_currentGameState == GameState.RUNNING ? GameState.PAUSE : GameState.RUNNING);
    }

    public void RestartGame()
    {      
        hasKizune = false;
        hasHelmet = false;
        enteredLR = false;
        enteredK = false;
        UpdateState(GameState.PREGAME);
        UnloadLevel(_currentLevelName);
    }

    public void QuitGame()
    {
        // implement features for quitting

        Application.Quit();
    }

    public void EnterDoor(string door)
    {
        if (!hasKizune)
        {
            UIManager.Instance.SetNeed(true);
            return;
        }
        switch (door)
        {
            case "childRoomDoor":
                UnloadLevel(_currentLevelName);
                LoadLevel("child_room");
                break;

            case "livingRoomDoor":
                UnloadLevel(_currentLevelName);
                LoadLevel("livingroom");
                break;

            case "kitchenDoor":
                UnloadLevel(_currentLevelName);
                LoadLevel("kitchen");
                break;

            default:
                break;
        }
    }

    public void ToggleKizune()
    {
        if(kizune != null)
        {
            kizune.SetActive(!kizune.activeInHierarchy);
        }
    }

    void FirstTimeLRoom()
    {
        UIManager.Instance.KizuneTalk(5);
    }
}
