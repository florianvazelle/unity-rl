using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class MarkovPolicy : MarkovBase
{
    public Dictionary<IState, int> policy;

    public MarkovPolicy(List<IState> states, List<int> actions, ConvertMethod T) : base(states, actions, T)
    {
        // intitialize v_s with a random number
        v_s = new Dictionary<IState, float>();
        foreach (var state in m_states)
        {
            float value = Utils.RandomGenerator.RandomFloat(0, 1);
            v_s.Add(state, value);
            if (state.isFinal()) v_s[state] = 0;
        }

        RandomPolicy(out policy);

        // train policy
        while (true)
        {
            PolicyEvaluation();
            if (PolicyImprovement()) break;
        }
    }

    public override int Think(IState state)
    {
        if (state.isFinal()) return 0;
        return (int)policy[state];
    }

    /**
     * Intialise la stratégie avec des actions aléatoires pour chaque état possible.
     */
    private void RandomPolicy(out Dictionary<IState, int> policy)
    {
        policy = new Dictionary<IState, int>();

        foreach (var state in m_states)
        {
            if (state.isFinal()) continue;
            int action = m_actions[Utils.RandomGenerator.RandomNumber(0, m_actions.Count)];
            policy.Add(state, action);
        }
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
            
            foreach (var state in m_states)
            {
                if (state.isFinal()) continue; // ne modifie pas v_s si état final

                float tmp = v_s[state];

                Cell reward = m_transition(state, (int)policy[state], out newState);
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

        foreach (var state in m_states)
        {
            if (state.isFinal()) continue; // ne modifie pas v_s si état final
            
            int tmp = (int)policy[state];

            // argmax
            float max = -INFINITY;
            int act = 0;
            foreach (var action in m_actions)
            {  
                Cell reward = m_transition(state, action, out newState);
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
}
