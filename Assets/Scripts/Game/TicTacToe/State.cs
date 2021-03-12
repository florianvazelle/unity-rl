using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TicTacToe {

    public class State : IState, IEquatable<State> {   

        public readonly int[] board;
        public readonly int currentPlayer;

        public bool IsFinal { get; set; }                   // To check if it's a final state (win)
        public bool HasActions { get; set; }                // To check if we need to add the state to the policy
        public List<int> PossibleActions { get; set; }      // List of all possible action on this state
        public int IsWin { get; set; }
        
        // To init the intial state
        public State(int firstPlayer) {
            this.board = new int[9];
            this.currentPlayer = firstPlayer;
            
            this.IsFinal = false;
            this.HasActions = false;
            this.PossibleActions = new List<int>();

            this.Update();
        }

        public State(int[] board, int player) {
            this.board = new int[9];

            for (int i = 0; i < board.Length; i++ ) {
                this.board[i] = board[i];
            }

            this.currentPlayer = player;

            this.IsFinal = false;
            this.HasActions = false;
            this.PossibleActions = new List<int>();

            this.Update();
        }

        // To update IsFinal, HasActions and PossibleActions
        private void Update() {
            // Compute if is Win for a player
            IsWin = 0;
            
            // Test all possibility of combinaison
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

            for(int j = 1; j < 3; j++)
            {
                for(int i = 0 ; i < 8 ; i++){
                    if(boardList[i,0] == j && boardList[i,1] == j && boardList[i,2] == j)
                    {
                        IsWin = j;
                        break;
                    }
                }

                if (IsWin == j) break;
            }
            
            // Compute if it is a final state
            PossibleActions = new List<int>();
            // regarde la liste des actions qu'il est possible de réaliser
            for(int i = 0 ; i < this.board.Length ; i++) {
                if (this.board[i] == 0)  PossibleActions.Add(i);
            }
            HasActions = PossibleActions.Count > 0;

            // plus aucune possibilitée ou des deux joueurs a gagné
            IsFinal = ((!HasActions) || (IsWin != 0));
        }

        // To get an unique hash for this state
        public override int GetHashCode() {
            int hash = 17;
            hash = hash * 37 + currentPlayer.GetHashCode();
            for (int i = 0; i < this.board.Length; ++i) {
                hash = hash * 37 + this.board[i].GetHashCode();
            }
            return hash;
        }

        // To compare to State
        public bool EqualsParameters(State other) {
            bool isEquals = true;
            for (var i = 0; i < this.board.Length; i++) {
                isEquals = isEquals && (this.board[i] == other.board[i]);
            }

            return isEquals && (this.currentPlayer == other.currentPlayer);
        }

        public bool Equals(State other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return this.EqualsParameters(other);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(State)) return false;
            return Equals((State) obj);
        } 
    }
}