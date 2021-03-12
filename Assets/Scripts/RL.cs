using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RapidGUI;

public class RL : MonoBehaviour {

    private Rect windowRect = new Rect(0, 0, 500, 100);     // Rect window for ImGUI
    private long elapsedMs = -1;                            // To calculate how many time the method execute 

    public enum Game { GridWorld, TicTacToe, Sokoban };
    public enum Algo { MarkovPolicy, MarkovValue, MonteCarlo, SARSA, QLearning };
    public enum SokobanLevel { Easy, Medium, Hard };

    public Game selectedGame, oldGame;
    public Algo selectedAlgo, oldAlgo;
    public SokobanLevel selectedSokobanLevel, oldSokobanLevel;

    public static RL instance = null;

    public IGame game;
    public bool mcES, mcOnPolicy, mcFirstVisit;

    public GameObject goPlayer;
    public Button yourButton;
    public Sprite tileSprite, arrowUp, arrowLeft, arrowDown, arrowRight;
    public Button btn;

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
        selectedAlgo = Algo.MarkovPolicy;
        selectedSokobanLevel = SokobanLevel.Easy;

        mcES = true;
        mcOnPolicy = false;
        mcFirstVisit = false;

        btn = yourButton.GetComponent<Button>();
    }

    void Update() {
        if (selectedGame != oldGame || selectedAlgo != oldAlgo || selectedSokobanLevel != oldSokobanLevel) {
            try {
                btn.onClick.RemoveListener(game.TaskOnClick);
            } catch {
            }

            oldGame = selectedGame;
            oldAlgo = selectedAlgo;
            oldSokobanLevel = selectedSokobanLevel;

            Base algoType = new MarkovPolicy();
            if (selectedAlgo == Algo.MarkovValue) algoType = new MarkovValue();
            else if (selectedAlgo == Algo.MonteCarlo) algoType = new MonteCarlo();
            else if (selectedAlgo == Algo.SARSA) algoType = new SARSA();
            else if (selectedAlgo == Algo.QLearning) algoType = new QLearning();

            var type = typeof(GridWorld.GridWorld<>).MakeGenericType(algoType.GetType());
            game = (IGame) Activator.CreateInstance(type);  

            if (selectedGame == Game.TicTacToe) {
                goPlayer.transform.position = new Vector3(50, 50, 0);
                type = typeof(TicTacToe.TicTacToe<>).MakeGenericType(algoType.GetType());
                game = (IGame) Activator.CreateInstance(type);
            }

            else if (selectedGame == Game.Sokoban) {
                type = typeof(Sokoban.Sokoban<>).MakeGenericType(algoType.GetType());
                game = (IGame) Activator.CreateInstance(type);
            }

            // Clear the screen
            GameObject[] gos = GameObject.FindGameObjectsWithTag("Tile");
            foreach(GameObject go in gos) {
                var tile = go.GetComponent<SpriteRenderer>();
                tile.color = new Color(0, 0, 0, 0);
            }

            // Start the game
            var watch = System.Diagnostics.Stopwatch.StartNew();
            game.Start();
            watch.Stop();
            elapsedMs = watch.ElapsedMilliseconds;

            // Debug to move the game step by step
		    btn.onClick.AddListener(game.TaskOnClick);
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