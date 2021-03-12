using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GridWorld {

    public enum Actions { IDLE = 0, UP = 1, LEFT = 2, DOWN = 3, RIGHT = 4 };

    public class GridWorld<TAlgo> : IGame where TAlgo : Base, new() {

        public readonly static List<int> ACTIONS = new List<int>() { (int)Actions.UP, (int)Actions.LEFT, (int)Actions.DOWN, (int)Actions.RIGHT };
        
        public State gameState;
        public static Level level;

        public TAlgo markovIA;

        public void Start() {  
            level = new Level(new List<int>() {
                0, 3, 0, 1, 0, 0, 0, 0, 
                0, 1, 0, 0, 0, 0, 0, 0, 
                0, 0, 1, 0, 0, 0, 0, 0, 
                0, 0, 0, 1, 0, 0, 0, 0, 
                0, 0, 0, 2, 1, 0, 0, 0, 
                0, 0, 0, 0, 0, 0, 0, 0, 
                0, 0, 0, 0, 0, 0, 0, 0, 
                0, 0, 0, 0, 0, 0, 0, 0, 
            }, 8, 8);

            gameState = new State(level);  // initial game

            markovIA = new TAlgo();
            markovIA.States = new List<IState>() { gameState };
            markovIA.Actions = ACTIONS;
            markovIA.Transition = Play;

            markovIA.Init();

            Render();
        }

        public void Update() {}

        public void TaskOnClick() {
            IState currentIState = gameState;

            int act = markovIA.Think(currentIState);

            // update game state
            Play(currentIState, act, out currentIState);
            gameState = (State) currentIState;

            // update rendering
            Render();
        }

        public static Vector2 PlayAction(int action) {
            Vector2 move = new Vector2(0, 0);

            switch(action) {
                case (int)Actions.IDLE:
                    break;
                case (int)Actions.UP:
                    move = new Vector2(0, 1);
                    break;
                case (int)Actions.LEFT:
                    move = new Vector2(-1, 0);
                    break;
                case (int)Actions.DOWN:
                    move = new Vector2(0, -1);
                    break;
                case (int)Actions.RIGHT:
                    move = new Vector2(1, 0);
                    break;
                default:
                    Debug.Log("Unkonwn action.");
                    break; 
            }

            return move;
        }

        public static Cell Play(IState iState, int action, out IState newIState) {
            // execute action
            State state = (State)iState;

            Vector2 playerMove = state.player + PlayAction(action);

            // clamp position in the area
            playerMove.x = (int)Mathf.Clamp(playerMove.x, 0, level.WIDTH - 1);
            playerMove.y = (int)Mathf.Clamp(playerMove.y, 0, level.HEIGHT - 1);

            // define the new state
            newIState = new State(playerMove);

            int reward = level.walls.Contains(playerMove) ? reward = -1000 : -1;

            return new Cell { value = reward }; 
        }

        void Render() {   
            Camera.main.transform.position = new Vector3(level.WIDTH / 2, level.HEIGHT / 2, -10);

            RL.instance.goPlayer.transform.position = gameState.player;

            for(int y = 0; y < level.HEIGHT; y++) {
                for(int x = 0; x < level.WIDTH; x++) {
                    Vector2 pos = new Vector2(x, y);

                    Color color = new Color(1, 1, 1);
                    if (level.walls.Contains(pos)) color = new Color(0, 0, 0);
                    if (level.goal == pos) color = new Color(1, 0, 0);

                    Utils.Render.SpawnTile(x, y, RL.instance.tileSprite, color);
                }
            }
        }

        // private void SpawnArrow(int x, int y, int action) {

        //     Sprite sprite = tileSprite;
        //     switch(action) {
        //         case (int)Actions.UP:
        //             sprite = arrowUp;
        //             break;
        //         case (int)Actions.LEFT:
        //             sprite = arrowLeft;
        //             break;
        //         case (int)Actions.DOWN:
        //             sprite = arrowDown;
        //             break;
        //         case (int)Actions.RIGHT:
        //             sprite = arrowRight;
        //             break;
        //     }

        //     string name = x + ":" + y + "arrow";
        //     GameObject g = GameObject.Find(name);
        //     if (g == null) {
        //         g = new GameObject(name);
        //     }

        //     g.transform.position = new Vector3(x, y);
        //     g.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

        //     var tile = g.GetComponent<SpriteRenderer>();
        //     if (tile == null) {
        //         tile = g.AddComponent<SpriteRenderer>();
        //     }

        //     tile.sprite = sprite;
        // }
    }
}