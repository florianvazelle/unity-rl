using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Sokoban {

    public enum Actions { IDLE = 0, UP = 1, LEFT = 2, DOWN = 3, RIGHT = 4 };

    public class Sokoban<TAlgo> : IGame where TAlgo : Base, new() {

        public readonly static List<int> ACTIONS = new List<int>() { (int)Actions.UP, (int)Actions.LEFT, (int)Actions.DOWN, (int)Actions.RIGHT };

        private Level level;

        private IState gameState;
        private TAlgo ia;

        public void Start() {   
            if (RL.instance.selectedSokobanLevel == RL.SokobanLevel.Easy)
                level = new Level(new List<int>() {
                    1, 1, 1, 1, 1, 1, 
                    1, 4, 0, 0, 0, 1,
                    1, 0, 0, 2, 3, 1,
                    1, 1, 1, 1, 1, 1, 
                }, 4, 6);
            else if (RL.instance.selectedSokobanLevel == RL.SokobanLevel.Medium)
                level = new Level(new List<int>() {
                     1, 1, 1, 1, 1, 1,
                    1, 4, 0, 0, 0, 1,
                    1, 0, 0, 2, 3, 1,
                    1, 0, 0, 2, 3, 1,
                    1, 0, 0, 2, 3, 1,
                    1, 1, 1, 1, 1, 1,
                }, 6, 6);
            else
                level = new Level(new List<int>() {
                    1, 1, 1, 1, 1, 1, 
                    1, 4, 0, 0, 0, 1,
                    1, 0, 0, 0, 0, 1,
                    1, 0, 0, 1, 3, 1,
                    1, 1, 2, 1, 1, 1,
                    1, 0, 0, 0, 0, 1,
                    1, 0, 0, 0, 0, 1,
                    1, 0, 0, 0, 0, 1,
                    1, 1, 1, 1, 1, 1, 
                }, 9, 6);
            
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
            State state = (State) iState;

            Vector2 move = PlayAction(action);

            Vector2 tmp = state.player + move;
            
            // Mais a jour les boites, si une a été bougé
            List<Vector2> tmpBoxes = new List<Vector2>(state.boxes);
            for (int i = 0; i < tmpBoxes.Count; i++) {
                if (tmp == tmpBoxes[i]) {
                    tmpBoxes[i] += move;
                }
            }

            // define the new state
            newIState = new State(tmp, tmpBoxes);

            if (((State) newIState).IsFinal) {
                return new Cell { value = 1000 };
            }

            return new Cell { value = -1 }; 
        }

        protected void Render() {   
            Camera.main.transform.position = new Vector3(level.WIDTH / 2, level.HEIGHT / 2, -10);

            RL.instance.goPlayer.transform.position = ((State) gameState).player;

            for(int y = 0; y < level.HEIGHT; y++) {
                for(int x = 0; x < level.WIDTH; x++) {
                    Vector2 pos = new Vector2(x, y);

                    Color color = new Color(1, 1, 1);
                    if (level.walls.Contains(pos)) color = new Color(0, 0, 0);
                    if (level.goals.Contains(pos)) color = new Color(1, 0, 0);
                    if (((State) gameState).boxes.Contains(pos)) color = new Color(0.59f, 0.29f, 0.00f);

                    Utils.Render.SpawnTile(x, y, RL.instance.tileSprite, color);
                }
            }
        }
    }
}
