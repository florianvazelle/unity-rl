using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

/**
 * Infos :
 * - L'algorithme SARSA est on-policy : la politique apprise est la même que celle utilisée pour faire les choix des actions à exécuter. 
 * - L'algorithme de type TD : on utilise l'état suivant s' pour mettre à jour la valeur en l'état courant s.
 */
public class SARSA : Base {

    private const float ALPHA = 0.75f; // Learning rate 

    private Dictionary<(IState, int), float> q;

    public SARSA(List<IState> states, List<int> actions, ConvertMethod transition) : base(states, actions, transition)
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

    public override int Think(IState state)
    {   
        if (state.IsFinal) return 0;

        for (int t = 0; t < 200; t++)
        {
            IState s = state;
            int a = GetGreedyAction(s);

            while (true) {
                IState sprime;
                int r = m_transition(s, a, out sprime).value;
                int aprime = GetGreedyAction(s);

                if (!q.ContainsKey((sprime, aprime)))
                {
                    if (sprime.HasActions) AddNewStateToQ(sprime, aprime);
                    else continue;
                }

                q[(s, a)] = q[(s, a)] + ALPHA * (r + GAMMA * q[(sprime, aprime)] - q[(s, a)]);
                s = sprime;
                a = aprime;
                
                if (s.IsFinal) break;
            }
        }
        
        // argmax
        float max = 0;
        int action = 0;
        foreach (var act in state.PossibleActions)
        {
            if (q[(state, act)] > max)
            {
                max = q[(state, act)];
                action = act;
            }
        }
        return action;
    }

    /**
     * Epsilon Greedy
     */ 
    private int GetGreedyAction(IState state)
    {
        int action = 0;
        float p = Utils.RandomGenerator.RandomFloat(0, 1);
        if (p < 0.1)
        {
            // random action
            action = state.PossibleActions[Utils.RandomGenerator.RandomNumber(0, state.PossibleActions.Count)];
        } 
        else 
        {   
            // argmax
            float max = 0;
            foreach (var act in state.PossibleActions)
            {
                if (!q.ContainsKey((state, act)))
                {
                    if (state.HasActions) AddNewStateToQ(state, act);
                    else continue;
                }

                if (q[(state, act)] > max)
                {
                    max = q[(state, act)];
                    action = act;
                }
            }
        }

        return action;
    }

    //
    // Helpers
    //

    private void AddNewStateToQ(IState state, int action)
    {
        if (!m_states.Contains(state)) m_states.Add(state);
        float value = Utils.RandomGenerator.RandomFloat(0, 1);
        q.Add((state, action), value);
        if (state.IsFinal) q[(state, action)] = 0;
    }
}