using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class TemporalDifference : Base {

    protected new const float GAMMA = 0.9f; 
    protected const float ALPHA = 0.3f; // Learning rate

    protected const float MAX_DEPTH = 100; 
    protected const float EPOCHS = 10000; 

    protected Dictionary<(IState, int), float> Q_sa;

    public override void Init() {
        Q_sa = new Dictionary<(IState, int), float>();

        foreach (var state in States.ToList()) {
            foreach (var action in state.PossibleActions) {
                AddNewStateToQ(state, action);
            }
        }
    }

    // Epsilon Greedy
    protected int GetGreedyAction(IState state) {
        int action = -1;
        float p = Utils.RandomGenerator.RandomFloat(0, 1);
        if (p < 0.1) {
            // random action
            action = state.PossibleActions[Utils.RandomGenerator.RandomNumber(0, state.PossibleActions.Count)];
        } else {   
            action = ArgMaxAction(state);
        }
        
        return action;
    }

    protected void AddNewStateToQ(IState state, int action) {
        if (!States.Contains(state)) States.Add(state);
        float value = Utils.RandomGenerator.RandomFloat(0, 1);
        Q_sa.Add((state, action), value);
        if (state.IsFinal) Q_sa[(state, action)] = 0;
    }

    protected int ArgMaxAction(IState state) {
        float max = -INFINITY;
        int bestAction = -1;
        
        foreach (var act in state.PossibleActions) {
            if (!Q_sa.ContainsKey((state, act))) AddNewStateToQ(state, act);
            if (Q_sa[(state, act)] > max) {
                max = Q_sa[(state, act)];
                bestAction = act;
            }
        }

        return bestAction;
    }
}