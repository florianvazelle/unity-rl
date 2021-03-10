using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class MarkovValue : MarkovBase
{
    public MarkovValue(List<IState> states, List<int> actions, ConvertMethod T) : base(states, actions, T)
    {
        // intitialize v_s with a random number
        v_s = new Dictionary<IState, float>();
        foreach (var state in m_states) v_s.Add(state, 0f);
    }

    public override int Think(IState state)
    {
        if (state.isFinal()) return 0;
        
        IState newState;
        float max = -INFINITY;

        while (true)
        {
            float delta = 0;
            
            foreach (var s in m_states)
            {
                if (s.isFinal()) continue; // ne modifie pas v_s si état final
                if (!v_s.ContainsKey(s)) continue;

                float tmp = v_s[s];

                max = -INFINITY;
                foreach (var action in m_actions)
                {  
                    Cell reward = m_transition(s, action, out newState);
                    if (!v_s.ContainsKey(newState)) continue;
                    float current = (reward.value + GAMMA * v_s[newState]);
                    if (s.Equals(newState)) current -= 1;
                    if (current > max) max = current;
                }
                
                v_s[s] = max;

                delta = Math.Max(delta, Math.Abs(tmp - v_s[s]));
            }

            if (delta < EPSILON) break;
        }

        // argmax
        max = -INFINITY;
        int act = 0;
        foreach (var action in m_actions)
        {  
            Cell reward = m_transition(state, action, out newState);
            if (!v_s.ContainsKey(newState)) continue;
            float current = (reward.value + GAMMA * v_s[newState]);
            if (state.Equals(newState)) current -= 1;
            if (current > max) 
            {
                max = current;
                act = action;
            }
        }
        
        return act;
    }
}
