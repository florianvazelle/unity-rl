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

        public State gameState;
        public Level level;

        public TAlgo markovIA;

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

        void Render() {   
            Camera.main.transform.position = new Vector3(level.WIDTH / 2, level.HEIGHT / 2, -10);

            RL.instance.goPlayer.transform.position = gameState.player;

            for(int y = 0; y < level.HEIGHT; y++) {
                for(int x = 0; x < level.WIDTH; x++) {
                    Vector2 pos = new Vector2(x, y);

                    Color color = new Color(1, 1, 1);
                    if (level.walls.Contains(pos)) color = new Color(0, 0, 0);
                    if (level.goals.Contains(pos)) color = new Color(1, 0, 0);
                    if (gameState.boxes.Contains(pos)) color = new Color(0.59f, 0.29f, 0.00f);

                    Utils.Render.SpawnTile(x, y, RL.instance.tileSprite, color);
                }
            }
        }
    }
}
