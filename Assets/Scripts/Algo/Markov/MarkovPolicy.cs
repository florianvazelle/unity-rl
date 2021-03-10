using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class MarkovPolicy : MarkovBase
{
    public Dictionary<IState, int> policy;

    public MarkovPolicy(List<IState> states, List<int> actions, ConvertMethod T) : base(states, actions, T)
    {
        v_s = new Dictionary<IState, float>();
        policy = new Dictionary<IState, int>();
        foreach (var state in m_states.ToList())
        {
            // intitialize v_s with a random number
            AddNewStateToVS(state);

            // Intialise la stratégie avec des actions aléatoires pour chaque état possible.
            AddNewStateToPolicy(state);
        }

        // train policy
        while (true)
        {
            PolicyEvaluation();
            if (PolicyImprovement()) break;
        }
    }

    public override int Think(IState state)
    {
        if (state.IsFinal) return 0;
        return (int)policy[state];
    }

    /**
     * Met à jour v_s pour la stratégie courante.
     */
    private void PolicyEvaluation()
    {
        IState newState;

        while (true)
        {
            float delta = 0;
            
            foreach (var state in m_states.ToList())
            {
                if (state.IsFinal) continue; // ne modifie pas v_s si état final
                if (!v_s.ContainsKey(state)) AddNewStateToVS(state);
                if (!policy.ContainsKey(state))
                {
                    if (state.HasActions) AddNewStateToPolicy(state);
                    else continue;
                }

                float tmp = v_s[state];

                Cell reward = m_transition(state, (int)policy[state], out newState);
                if (!v_s.ContainsKey(newState)) AddNewStateToVS(newState);
                float current = (reward.value + GAMMA * v_s[newState]);
                if (state.Equals(newState)) current -= 1;
                
                v_s[state] = current;

                delta = Math.Max(delta, Math.Abs(tmp - v_s[state]));
            }

            if (delta < EPSILON) break;
        }
    }

    /**
     * Met à jour la stratégie, retourne vrai si la policy converge
     */
    private bool PolicyImprovement()
    {
        bool isStable = true;
        IState newState;

        foreach (var state in m_states.ToList())
        {
            if (state.IsFinal) continue; // ne modifie pas v_s si état final
            if (!policy.ContainsKey(state))
            {
                if (state.HasActions) AddNewStateToPolicy(state);
                else continue;
            }
            
            int tmp = (int)policy[state];

            // argmax
            float max = -INFINITY;
            int act = -1;
            foreach (var action in state.PossibleActions)
            {  
                Cell reward = m_transition(state, action, out newState);
                if (!v_s.ContainsKey(newState)) AddNewStateToVS(newState);
                float current = (reward.value + GAMMA * v_s[newState]);
                if (state.Equals(newState)) current -= 1;
                if (current > max) 
                {
                    max = current;
                    act = action;
                }
            }
            
            policy[state] = act;

            isStable = isStable && (tmp == (int)policy[state]);
        }

        return isStable;
    }

    //
    // Helpers
    //

    private void AddNewStateToPolicy(IState state)
    {
        if (!m_states.Contains(state)) m_states.Add(state);
        if (!state.IsFinal)
        {
            int action = state.PossibleActions[Utils.RandomGenerator.RandomNumber(0, state.PossibleActions.Count)];
            policy.Add(state, action);
        }
    }

    private void AddNewStateToVS(IState state)
    {
        if (!m_states.Contains(state)) m_states.Add(state);
        float value = Utils.RandomGenerator.RandomFloat(0, 1);
        v_s.Add(state, value);
        if (state.IsFinal) v_s[state] = 0;
    }
}
