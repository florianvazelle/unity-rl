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

        // Property implementation:
        public bool IsFinal { get; set; }
        public bool HasActions { get; set; }
        public List<int> PossibleActions { get; set; }
        public int IsWin { get; set; }

        public State(int[] pBoard, int player)
        {
            board = new int[9];
            for ( int i = 0; i < pBoard.Length; i++ ) {
                board[i] = pBoard[i];
            }
            currentPlayer = player;

            // Compute if is Win for a player
            IsWin = 0;
            for(int j = 1; j < 3; j++)
            {
                // test all possibility of combinaison
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
                    if(boardList[i,0] == j && boardList[i,1] == j && boardList[i,2] == j)
                    {
                        IsWin = i;
                        break;
                    }
                }
            }
            
            // Compute if it is a final state
            // plus aucune possibilitée
            bool isEnd = true;
            for(int i = 0 ; i < this.board.Length ; i++) {
                isEnd = isEnd && this.board[i] != empty;
            }

            if (isEnd) {
                IsFinal = true;
            } else {
                // ou des deux joueurs a gagné
                if (IsWin != 0) {
                    IsFinal = true;
                } else {
                    IsFinal = false;
                }
            }

            // regarde la liste des actions qu'il est possible de réaliser
            List<int> availableActions = new List<int>();

            for(int i = 0 ; i < 8 ; i++){
                if (board[i] == empty) availableActions.Add(i);
            }

            HasActions = availableActions.Count > 0;
            PossibleActions = availableActions;
        }
    }

    // Game state
    private State gameState;
    private readonly static int empty = 0, player1 = 1, player2 = 2;

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
    private readonly static List<int> actions = new List<int>() { 
        (int)Actions.TOP_LEFT, (int)Actions.TOP_MIDDLE, (int)Actions.TOP_RIGHT, 
        (int)Actions.MIDDLE_LEFT, (int)Actions.CENTER, (int)Actions.MIDDLE_RIGTH, 
        (int)Actions.BOTTOM_LEFT, (int)Actions.BOTTOM_MIDDLE, (int)Actions.BOTTOM_RIGHT
    };
    private MarkovPolicy markovIA;

    void Start()
    {   
        //
        // Game State
        gameState = new State(new int[9], player1);

        states = new List<IState>() {
            gameState
        };

        markovIA = new MarkovPolicy(states, actions, Play);

        //
        // Debug
        Button btn = yourButton.GetComponent<Button>();
		btn.onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick() {
        try {
            int act = markovIA.Think(gameState);

            IState iGameState = gameState;
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

        int[] board = new int[9];
        for ( int i = 0; i < state.board.Length; i++ ) {
            board[i] = state.board[i];
        }

        if (board[action] == empty) 
        {
            if (action >= 0 && action < 9) board[action] = player;
            else Debug.Log("Unknown action.");
        }
        else
        {
            Debug.Log("Impossible action.");
        }

        // Update state
        // Define the output Istate
        newIState = new State(board, player == player1 ? player2 : player1);

        if (state.IsWin == player) return new Cell { value = 1000 };
        return new Cell { value = -1 }; 
    }

    void Render()
    {   
        for(int i = 0 ; i < 9 ; i++){
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
