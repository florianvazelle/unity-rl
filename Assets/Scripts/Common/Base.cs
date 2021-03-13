using System;
using System.Collections;
using System.Collections.Generic;

public class Base {

    public delegate Cell ConvertMethod(IState state, int action, out IState newState);

    protected const float GAMMA = 0.75f;            // Facteur de dévaluation
    protected const float EPSILON = 0.0001f;
    protected const float INFINITY = 1000000;
    
    public List<IState> States { get; set; }         // S : ensemble des états du MDP
    // public List<int> Actions { get; set; }        // A : ensemble des actions
    public ConvertMethod Transition { get; set; }    // T : fonction de transition

    public virtual void Init() {   
        throw new NotImplementedException();
    }

    public virtual int Think(IState state) {   
        throw new NotImplementedException();
    }
}