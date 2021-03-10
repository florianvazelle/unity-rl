using System;
using System.Collections;
using System.Collections.Generic;

public class MarkovBase : Base {

    public Dictionary<IState, float> v_s; // Estimation du reward pour chaque état

    protected static Utils.Logger logger = new Utils.Logger("Markov");

    public MarkovBase(List<IState> states, List<int> actions, ConvertMethod transition) : base(states, actions, transition)
    {
        
    }
}