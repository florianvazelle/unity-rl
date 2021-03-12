using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class MarkovBase : Base {

    public Dictionary<IState, float> V_s; // Estimation du reward pour chaque état

    public override void Init() {
        V_s = new Dictionary<IState, float>();
        foreach (var state in States.ToList()) V_s.Add(state, 0f);
    }

    protected float GetRewardForAction(IState state, int action, out IState newState) {
        Cell reward = Transition(state, action, out newState);
        if (!V_s.ContainsKey(newState)) AddNewStateToVS(newState);

        float current = (reward.value + GAMMA * V_s[newState]);
        if (state.Equals(newState)) current -= 1;

        return current;
    }

    protected int ArgMaxAction(IState state) {
        IState newState;
        float max = -INFINITY;
        int bestAction = -1;
        
        foreach (var action in state.PossibleActions) {
            float current = GetRewardForAction(state, action, out newState);
            
            if (current > max) {
                max = current;
                bestAction = action;
            }
        }

        return bestAction;
    }

    protected virtual void AddNewStateToVS(IState state) {
        throw new NotImplementedException();
    }
}