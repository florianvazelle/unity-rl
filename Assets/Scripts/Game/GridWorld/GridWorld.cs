using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GridWorld {

    public enum Actions { IDLE = 0, UP = 1, LEFT = 2, DOWN = 3, RIGHT = 4 };

    public class GridWorld<TAlgo> : IGame where TAlgo : Base, new() {

        public readonly static List<int> ACTIONS = new List<int>() { (int)Actions.UP, (int)Actions.LEFT, (int)Actions.DOWN, (int)Actions.RIGHT };
        
        private static Level level;

        private IState gameState;
        private TAlgo ia;

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

            ia = new TAlgo();
            ia.States = new List<IState>() { gameState };
            ia.Transition = Play;

            ia.Init();

            Render();
        }

        public void Update() {}

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

        public void TaskOnClick() {
            int act = ia.Think(gameState);
            Play(gameState, act, out gameState); // update game state
            Render(); // update rendering
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

        protected void Render() {   
            Camera.main.transform.position = new Vector3(level.WIDTH / 2, level.HEIGHT / 2, -10);

            RL.instance.goPlayer.transform.position = ((State) gameState).player;

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
    }
}