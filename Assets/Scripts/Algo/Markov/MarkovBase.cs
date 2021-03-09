using System;
using System.Collections;
using System.Collections.Generic;

public class MarkovBase {

    public delegate Cell ConvertMethod(IState state, int action, out IState newState);

    protected const float GAMMA = 0.75f; // Facteur de dévaluation
    protected const float EPSILON = 0.0001f;
    protected const float INFINITY = 1000000;
    
    protected readonly List<IState> m_states;
    protected readonly List<int> m_actions;
    protected readonly ConvertMethod m_transition;

    public Dictionary<IState, float> v_s; // Estimation du reward pour chaque état

    protected static Utils.Logger logger = new Utils.Logger("Markov");


    /**
     * Constructeur, on passe à l'algo :
     *
     * S : ensemble des états du MDP
     * A : ensemble des actions
     * T : fonction de transition
     * R : fonction de récompenses
     */
    public MarkovBase(List<IState> states, List<int> actions, ConvertMethod transition)
    {
        m_states = states;
        m_actions = actions;
        m_transition = transition;
    }

    public virtual int Think(IState state)
    {   
        // Not developed yet.
        throw new NotImplementedException();
    }
}