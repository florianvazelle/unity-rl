using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using static Utils.RandomGenerator;

public class TemporalDifference : Base {

    protected new const float GAMMA = 0.9f; 
    protected const float ALPHA = 0.01f; // Learning rate

    protected const float MAX_DEPTH = 100; 
    protected const float EPOCHS = 10000; 

    protected Dictionary<(IState, int), float> Q_sa;

    public override void Init() {
        Q_sa = new Dictionary<(IState, int), float>();

        foreach (var state in States.ToList()) {
            foreach (var action in state.Actions) {
                AddNewStateToQ(state, action);
            }
        }
    }

    // Epsilon Greedy
    protected int GetGreedyAction(IState state) {
        float p = RandomFloat(0, 1);
        return (p < 0.1f) ? RandomChoice(state.Actions) : ArgMaxAction(state);
    }

    protected void AddNewStateToQ(IState state, int action) {
        if (!States.Contains(state)) States.Add(state);

        float value = RandomFloat(0, 1);
        Q_sa.Add((state, action), value);
        if (state.IsFinal) Q_sa[(state, action)] = 0;
    }

    protected int ArgMaxAction(IState state) {
        float max = -INFINITY;
        List<int> idxList = new List<int>();
        
        for (int i = 0; i < state.Actions.Count; i++) {
            int action = state.Actions[i];

            if (!Q_sa.ContainsKey((state, action))) AddNewStateToQ(state, action);
            float current = Q_sa[(state, action)];

            if (current > max) {
                idxList.Clear();
                max = current;
                idxList.Add(i);
            } else if (current == max) {
                idxList.Add(i);
            }
        }

        return state.Actions[RandomChoice(idxList)];
    }
}