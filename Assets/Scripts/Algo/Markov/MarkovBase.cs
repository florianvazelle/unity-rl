using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using static Utils.RandomGenerator;

public class MarkovBase : Base {

    public Dictionary<IState, float> V_s; // Estimation du reward pour chaque état

    public override void Init() {
        V_s = new Dictionary<IState, float>();
        foreach (var state in States.ToList()) V_s.Add(state, 0f);
    }

    // Retourne le reward estimé pour un couple (état, action)
    protected float GetRewardForAction(IState state, int action, out IState newState) {
        Cell reward = Transition(state, action, out newState);
        if (!V_s.ContainsKey(newState)) AddNewStateToVS(newState);

        float current = (reward.value + GAMMA * V_s[newState]);
        if (state.Equals(newState)) current -= 1;

        return current;
    }

    // Retourne l'action qui possède le meilleur reward
    // Si il y a plusieurs action avec le même reward, on choisi aléatoirement parmis celle ci
    protected int ArgMaxAction(IState state) {
        IState newState;
        float max = -INFINITY;
        List<int> idxList = new List<int>();
        
        for (int i = 0; i < state.Actions.Count; i++) {
            float current = GetRewardForAction(state, state.Actions[i], out newState);
            
            if (current > max) {
                idxList.Clear();
                max = current;
                idxList.Add(i);
            } else if (current == max) {
                idxList.Add(i);
            }
        }

        return state.Actions[RandomChoice(idxList)];
    }

    protected virtual void AddNewStateToVS(IState state) {
        throw new NotImplementedException();
    }
}