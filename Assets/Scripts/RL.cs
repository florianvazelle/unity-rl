using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RapidGUI;

public class RL : MonoBehaviour {

    private Rect windowRect = new Rect(0, 0, 500, 100);     // Rect window for ImGUI
    private long elapsedMs = -1;                            // To calculate how many time the method execute 

    private enum Game { GridWorld, TicTacToe, Sokoban };
    private enum Algo { MarkovPolicy, MarkovValue, MonteCarlo, SARSA, QLearning };
    public enum SokobanLevel { Easy, Medium, Hard };

    private Game selectedGame, oldGame;
    private Algo selectedAlgo, oldAlgo;
    public SokobanLevel selectedSokobanLevel, oldSokobanLevel;

    public static RL instance = null;

    private IGame game;
    public bool mcES, mcOnPolicy, mcFirstVisit;

    public GameObject goPlayer;
    public Button playButton;
    public Sprite tileSprite;
    private Button btnComponent;

    public static RL GetInstance() {
        return instance;
    }

    void Awake() {
        instance = this;
    }

    void Start() {
        oldGame = Game.TicTacToe;
        oldAlgo = Algo.MarkovValue;
        oldSokobanLevel = SokobanLevel.Easy;

        selectedGame = Game.GridWorld;
        selectedAlgo = Algo.MarkovValue;
        selectedSokobanLevel = SokobanLevel.Easy;

        mcES = true;
        mcOnPolicy = false;
        mcFirstVisit = false;

        btnComponent = playButton.GetComponent<Button>();
    }

    void Update() {
        // Detect if a game or algo are change
        if (selectedGame != oldGame || selectedAlgo != oldAlgo || selectedSokobanLevel != oldSokobanLevel) {
            try {
                btnComponent.onClick.RemoveListener(game.TaskOnClick);
            } catch {
                Debug.Log("No listener attach to the play button.");
            }

            // update old values
            oldGame = selectedGame;
            oldAlgo = selectedAlgo;
            oldSokobanLevel = selectedSokobanLevel;

            // Select the RL algorithm 
            Base algoType;
            if (selectedAlgo == Algo.MarkovPolicy) algoType = new MarkovPolicy();
            else if (selectedAlgo == Algo.MarkovValue) algoType = new MarkovValue();
            else if (selectedAlgo == Algo.MonteCarlo) algoType = new MonteCarlo();
            else if (selectedAlgo == Algo.SARSA) algoType = new SARSA();
            else algoType = new QLearning();

            // Select the game
            Type gameType;
            if (selectedGame == Game.GridWorld) gameType = typeof(GridWorld.GridWorld<>);
            else if (selectedGame == Game.TicTacToe) gameType = typeof(TicTacToe.TicTacToe<>);
            else gameType = typeof(Sokoban.Sokoban<>);

            // Create the game instance for the RL algorithm
            var type = gameType.MakeGenericType(algoType.GetType());
            game = (IGame) Activator.CreateInstance(type);  

            // Clear the screen
            GameObject[] gos = GameObject.FindGameObjectsWithTag("Tile");
            foreach(GameObject go in gos) {
                var tile = go.GetComponent<SpriteRenderer>();
                tile.color = new Color(0, 0, 0, 0);
            }
            // Hide the player
            goPlayer.transform.position = new Vector3(50, 50, 0);

            // Start the game
            var watch = System.Diagnostics.Stopwatch.StartNew();
            game.Start();
            watch.Stop();
            elapsedMs = watch.ElapsedMilliseconds;

            // Debug to move the game step by step
		    btnComponent.onClick.AddListener(game.TaskOnClick);
        }

        game.Update();
    }

    private void OnGUI() {
        if (transform.parent == null) {
            GUILayout.Label($"<b>Reinforcement Learning</b>");
            DoGUI();
        }
    }

    public void DoGUI() {
        selectedGame = RGUI.Field(selectedGame, "Game");
        selectedAlgo = RGUI.Field(selectedAlgo, "Algo");

        if (selectedAlgo == Algo.MonteCarlo) {
            GUILayout.Label("Disable ES for more options.");
            mcES = RGUI.Field(mcES, "ES");
            if (!mcES) {
                mcOnPolicy = RGUI.Field(mcOnPolicy, "On/Off Policy");
                mcFirstVisit = RGUI.Field(mcFirstVisit, "First/Every Visit");
            }
        }

        if (selectedGame == Game.Sokoban) {
            selectedSokobanLevel = RGUI.Field(selectedSokobanLevel, "Level");
        }

        if (elapsedMs != -1) {
            GUILayout.Label("In " + elapsedMs + " milliseconds");
        }
    }
}