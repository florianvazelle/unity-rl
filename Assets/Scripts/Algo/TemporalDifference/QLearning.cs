using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class QLearning : TemporalDifference { 

    public QLearning(List<IState> states, List<int> actions, ConvertMethod transition) : base(states, actions, transition)
    {
        for (int t = 0; t < EPOCHS; t++)
        {
            IState s = states[0];
            int a = GetGreedyAction(s);

            for (int depth = 0; depth < MAX_DEPTH; depth++)
            {
                IState sprime;
                int r = m_transition(s, a, out sprime).value;

                int aprime = ArgMaxAction(sprime);
                
                q[(s, a)] = q[(s, a)] + ALPHA * (r + GAMMA * q[(sprime, aprime)] - q[(s, a)]);
                s = sprime;
                a = aprime;
                
                if (s.IsFinal) break;
            }
        }
    }

    public override int Think(IState state)
    {   
        if (state.IsFinal) return 0;
        return ArgMaxAction(state);
    }
}