using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

/**
 * Infos :
 * - L'algorithme SARSA est on-policy : la politique apprise est la même que celle utilisée pour faire les choix des actions à exécuter. 
 */
public class SARSA : TemporalDifference {

    protected const float GAMMA = 0.65f;

    public SARSA(List<IState> states, List<int> actions, ConvertMethod transition) : base(states, actions, transition)
    {
        for (int t = 0; t < EPOCHS; t++)
        {
            IState s = states[0];
            int a = GetGreedyAction(s);

            for (int depth = 0; depth < MAX_DEPTH; depth++)
            {
                IState sprime;
                int r = m_transition(s, a, out sprime).value;
                int aprime = GetGreedyAction(s);

                if (!q.ContainsKey((s, a))) AddNewStateToQ(s, a);
                if (!q.ContainsKey((sprime, aprime))) AddNewStateToQ(sprime, aprime);

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