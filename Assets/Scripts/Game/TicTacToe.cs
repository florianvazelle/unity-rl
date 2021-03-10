using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TicTacToe : MonoBehaviour
{

    public struct State : IState, IEquatable<State>
    {   
        //
        // To compare State
        //

        public bool Equals(State other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            bool isEquals = true;
            for (var i = 0; i < this.board.Length; i++) {
                isEquals = isEquals && (this.board[i] == other.board[i]);
            }

            return isEquals && (this.currentPlayer == other.currentPlayer);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (State)) return false;
            return Equals((State)obj);
        }

        public override int GetHashCode()
        {
            int hash = 17;
            for (int i = 0; i < this.board.Length; ++i) hash = hash * 19 + board[i];
            return hash * 19 + currentPlayer;
        } 

        //
        // Element of the custom GridWorld's State
        //

        public int[] board;
        public int currentPlayer;

        public bool isFinal() {
            // plus aucune possibilitée
            bool isEnd = true;
            for(int i = 0 ; i < this.board.Length ; i++) {
                isEnd = isEnd && this.board[i] != empty;
            }

            if (isEnd) return true;

            // ou des deux joueurs a gagné
            if (isWin(player1)) return true;
            if (isWin(player2)) return true;
            return false;
        }

        public bool isWin(int player)
        {
            int[,] boardList = new int[8,3] {
                {board[0], board[1], board[2]},
                {board[3], board[4], board[5]},
                {board[6], board[7], board[8]},
                {board[0], board[3], board[6]},
                {board[1], board[4], board[7]},
                {board[2], board[5], board[8]},
                {board[0], board[4], board[8]},
                {board[2], board[4], board[6]}
            };

            for(int i = 0 ; i < 8 ; i++){
                if(boardList[i,0] == player && boardList[i,1] == player && boardList[i,2] == player) return true;
            }
            
            return false;
        } 

        public List<int> getActions()
        {
            List<int> availableActions = new List<int>();

            for(int i = 0 ; i < 8 ; i++){
                if (board[i] == empty) availableActions.Add(i);
            }

            return availableActions;
        }
    }

    // Game state
    private State gameState;
    private static int empty = 0, player1 = 1, player2 = 2;

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

    // Unity
    public Button yourButton;
    public Sprite tileSprite;

    // Markov
    private List<IState> states;
    private List<int> actions;

    void Start()
    {   
        //
        // Game State
        gameState = new State {
            board = new int[9],
            currentPlayer = player1
        };

        //
        // Debug
        Button btn = yourButton.GetComponent<Button>();
		btn.onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick() {
        try {
            //int act = markovIA.Think(gameState);
            int act = UnityEngine.Random.Range(0, 9);
            //Debug.Log(ActionToString(act));

            IState iGameState;
            // move player
            Play(gameState, act, out iGameState);
            gameState = (State)iGameState;

            // update rendering
            Render();
        } catch (Exception e) {
            Console.WriteLine("Exception: " + e.Message);
        }
	}

    public static Cell Play(IState iState, int action, out IState newIState)
    {
        State state = (State)iState;
        int player = state.currentPlayer;

        if (state.board[action] == empty) 
        {
            if (action >= 0 && action < 9) state.board[action] = player;
            else Debug.Log("Unknown action.");
        }
        else
        {
            Debug.Log("Impossible action.");
        }

        // Define the output Istate
        newIState = state;
        if (state.isWin(player)) return new Cell { value = 1000 };

        // Update state
        state.currentPlayer = player == player1 ? player2 : player1;
        return new Cell { value = -1 }; 
    }

    void Render()
    {   
        for(int i = 0 ; i < 8 ; i++){
            SpawnTile(i % 3, i / 3, gameState.board[i]);
        }
    }

    //
    // Helpers
    //

    void SpawnTile(int x, int y, int player)
    {
        string name = x + ":" + y;
        GameObject g = GameObject.Find(name);
        if (g == null) {
            g = new GameObject (name);
        }
        g.transform.position = new Vector3(x, y);

        var tile = g.GetComponent<SpriteRenderer>();
        if (tile == null) {
            tile = g.AddComponent<SpriteRenderer>();
        }

        tile.sprite = tileSprite;
        tile.color = player == empty ? new Color(255, 255, 255) : player == player1 ? new Color(255, 0, 0) : player == player2 ? new Color(0, 0, 255) : new Color(0, 0, 0);
    }
}
