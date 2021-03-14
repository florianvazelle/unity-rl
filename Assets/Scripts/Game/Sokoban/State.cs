using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sokoban {

    public class State : IState, IEquatable<State> {   

        public readonly Vector2 player;
        public readonly List<Vector2> boxes;
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
            this.boxes = new List<Vector2>(levelInstance.boxes);
            
            this.IsFinal = false;
            this.HasActions = false;
            this.Actions = new List<int>();

            this.Update();
        }

        public State(Vector2 player, List<Vector2> boxes) {
            this.player = player;
            this.boxes = new List<Vector2>(boxes);

            this.IsFinal = false;
            this.HasActions = false;
            this.Actions = new List<int>();

            this.Update();
        }

        // To update IsFinal, HasActions and Actions
        private void Update() {
            var boxes = this.boxes;

            IsFinal = levelInstance.goals.All(goal => boxes.Contains(goal));
            HasActions = !levelInstance.walls.Contains(this.player);

            foreach (var action in Sokoban<QLearning>.ACTIONS) {
                Vector2 move = Sokoban<QLearning>.PlayAction(action);
                Vector2 playerMove = this.player + move;

                bool isValidBoxMove = true;
                for (int i = 0; i < this.boxes.Count; i++) {
                    if (playerMove == this.boxes[i]) {
                        Vector2 movedBox = this.boxes[i] + move;
                        // On ne bouge pas la boite si elle est sur une autre boite ou sur un mur
                        isValidBoxMove = (!(this.boxes.Contains(movedBox) || levelInstance.walls.Contains(movedBox)));
                    }
                }

                // TODO : clamp value

                // C'est un coup valid si le joueur n'est pas allé sur un mur
                if ((!levelInstance.walls.Contains(playerMove)) && isValidBoxMove) Actions.Add(action);
            }
        }

        // To get an unique hash for this state
        public override int GetHashCode() {
            int hash = 17 * 37 + this.player.GetHashCode();
            foreach (var box in this.boxes) hash = hash * 37 + box.GetHashCode();
            return hash;
        }

        // To compare to State
        public bool EqualsParameters(State other) {
            return this.player == other.player && this.boxes.All(box => other.boxes.Contains(box));
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