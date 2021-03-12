using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacToe {

    public enum Actions { 
        TOP_LEFT = 0,
        TOP_MIDDLE = 1,
        TOP_RIGHT = 2,
        MIDDLE_LEFT = 3,
        CENTER = 4,
        MIDDLE_RIGTH = 5,
        BOTTOM_LEFT = 6,
        BOTTOM_MIDDLE = 7,
        BOTTOM_RIGHT = 8,
    };

    public class TicTacToe<TAlgo> : IGame where TAlgo : Base, new() {

        private readonly static List<int> ACTIONS = new List<int>() { 
            (int)Actions.TOP_LEFT, (int)Actions.TOP_MIDDLE, (int)Actions.TOP_RIGHT, 
            (int)Actions.MIDDLE_LEFT, (int)Actions.CENTER, (int)Actions.MIDDLE_RIGTH, 
            (int)Actions.BOTTOM_LEFT, (int)Actions.BOTTOM_MIDDLE, (int)Actions.BOTTOM_RIGHT
        };

        public State gameState;
        private readonly static int empty = 0, player1 = 1, player2 = 2;

        public TAlgo markovIA;

        public void Start() {  
            gameState = new State(player1);  // initial game

            markovIA = new TAlgo();
            markovIA.States = new List<IState>() { gameState };
            markovIA.Actions = ACTIONS;
            markovIA.Transition = Play;

            markovIA.Init();

            Render();
        }

        public void Update() {
            int action = -1;

            if (Input.GetKeyDown("1")) action = 0;
            if (Input.GetKeyDown("2")) action = 1;
            if (Input.GetKeyDown("3")) action = 2;
            if (Input.GetKeyDown("4")) action = 3;
            if (Input.GetKeyDown("5")) action = 4;
            if (Input.GetKeyDown("6")) action = 5;
            if (Input.GetKeyDown("7")) action = 6;
            if (Input.GetKeyDown("8")) action = 7;
            if (Input.GetKeyDown("9")) action = 8;

            if (action != -1) {
                IState currentIState = gameState;

                if (!((State)currentIState).IsFinal) {
                    // move player
                    Play(currentIState, action, out currentIState);
                    gameState = (State) currentIState;

                    Render();
                }
            }
        }

        public void TaskOnClick() {
            IState currentIState = gameState;

            int act = markovIA.Think(currentIState);

            // update game state
            Play(currentIState, act, out currentIState);
            gameState = (State) currentIState;

            // update rendering
            Render();
        }

        public static Cell Play(IState iState, int action, out IState newIState) {
            State state = (State)iState;
            int player = state.currentPlayer;

            int[] board = new int[9];
            for (int i = 0; i < state.board.Length; i++) {
                board[i] = state.board[i];
            }

            if (board[action] == empty) {
                if (action >= 0 && action < 9) board[action] = player;
                else Debug.Log("Unknown action.");
            } else {
                Debug.Log("Impossible action.");
            }

            // Update state
            // Define the output Istate
            newIState = new State(board, player == player1 ? player2 : player1);

            if (((State)newIState).IsWin == player1) return new Cell { value = 1000 };
            if (((State)newIState).IsWin == player2) return new Cell { value = -1000 };
            return new Cell { value = -1 }; 
        }

        void Render() {
            Camera.main.transform.position = new Vector3(3 / 2, 3 / 2, -10);

            for(int i = 0 ; i < 9 ; i++){
                int value = gameState.board[i];
                Color color = (value == 0) ? new Color(1, 1, 1) : (value == 1) ? new Color(1, 0, 0) : new Color(0, 0, 1);
                Utils.Render.SpawnTile(i % 3, i / 3, RL.instance.tileSprite, color);
            }
        }
    }
}