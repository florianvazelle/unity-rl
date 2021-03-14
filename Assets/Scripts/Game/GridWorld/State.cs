using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GridWorld {

    public class State : IState, IEquatable<State> {   

        public readonly Vector2 player;
        public static Level? level = null;

        public bool IsFinal { get; set; }
        public bool HasActions { get; set; }
        public List<int> Actions { get; set; }
        
        public static Level levelInstance {
            get {
                if (level == null) {
                    throw new ArgumentNullException("For the first state you need to call the State(Level) constructor.");
                }
                return level.Value;
            }
            set { level = new Level?(value); }
        }

        // To init the intial state
        public State(Level level) {
            levelInstance = level;
            this.player = levelInstance.player;
            
            this.IsFinal = false;
            this.HasActions = false;
            this.Actions = new List<int>();

            this.Update();
        }

        public State(Vector2 player) {
            this.player = player;

            this.IsFinal = false;
            this.HasActions = false;
            this.Actions = new List<int>();

            this.Update();
        }

        // To update IsFinal, HasActions and Actions
        private void Update() {
            IsFinal = (this.player == levelInstance.goal);
            HasActions = !levelInstance.walls.Contains(this.player);

            foreach (var action in GridWorld<QLearning>.ACTIONS) {
                Vector2 move = GridWorld<QLearning>.PlayAction(action);
                Vector2 playerMove = this.player + move;

                // C'est un coup valid si le joueur n'est pas allé sur un mur
                if (!levelInstance.walls.Contains(playerMove)) Actions.Add(action);
            }
        }

        // To get an unique hash for this state
        public override int GetHashCode() {
            return (this.player != null ? this.player.GetHashCode() : 0);
        }

        // To compare to State
        public bool EqualsParameters(State other) {
            return this.player == other.player;
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