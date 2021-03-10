using System;
using System.Collections;
using System.Collections.Generic;

public class Base {

    public delegate Cell ConvertMethod(IState state, int action, out IState newState);

    protected const float GAMMA = 0.75f; // Facteur de dévaluation
    protected const float EPSILON = 0.0001f;
    protected const float INFINITY = 1000000;
    
    protected readonly List<IState> m_states;
    protected readonly List<int> m_actions;
    protected readonly ConvertMethod m_transition;

    /**
     * Constructeur, on passe à l'algo :
     *
     * S : ensemble des états du MDP
     * A : ensemble des actions
     * T : fonction de transition
     * R : fonction de récompenses
     */
    public Base(List<IState> states, List<int> actions, ConvertMethod transition)
    {
        m_states = new List<IState>(states);
        m_actions = new List<int>(actions);
        m_transition = transition;
    }

    public virtual int Think(IState state)
    {   
        // Not developed yet.
        throw new NotImplementedException();
    }
}