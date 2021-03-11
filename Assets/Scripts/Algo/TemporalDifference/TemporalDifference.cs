using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class TemporalDifference : Base {

    protected const float GAMMA = 0.9f; 
    protected const float ALPHA = 0.3f; // Learning rate

    protected const float MAX_DEPTH = 100; 
    protected const float EPOCHS = 10000; 

    protected Dictionary<(IState, int), float> q;

    public TemporalDifference(List<IState> states, List<int> actions, ConvertMethod transition) : base(states, actions, transition)
    {
        q = new Dictionary<(IState, int), float>();
        foreach (var state in m_states.ToList())
        {
            foreach (var action in state.PossibleActions) 
            {
                AddNewStateToQ(state, action);
            }
        }
    }

    /**
     * Epsilon Greedy
     */ 
    protected int GetGreedyAction(IState state)
    {
        int action = -1;
        float p = Utils.RandomGenerator.RandomFloat(0, 1);
        if (p < 0.1)
        {
            // random action
            action = state.PossibleActions[Utils.RandomGenerator.RandomNumber(0, state.PossibleActions.Count)];
        } 
        else 
        {   
            action = ArgMaxAction(state);
        }
        
        return action;
    }

    //
    // Helpers
    //

    protected void AddNewStateToQ(IState state, int action)
    {
        if (!m_states.Contains(state)) m_states.Add(state);
        float value = Utils.RandomGenerator.RandomFloat(0, 1);
        q.Add((state, action), value);
        if (state.IsFinal) q[(state, action)] = 0;
    }

    protected int ArgMaxAction(IState state)
    {
        float max = -INFINITY;
        int bestAction = -1;
        foreach (var act in state.PossibleActions)
        {
            if (!q.ContainsKey((state, act))) AddNewStateToQ(state, act);
            if (q[(state, act)] > max)
            {
                max = q[(state, act)];
                bestAction = act;
            }
        }
        return bestAction;
    }
}