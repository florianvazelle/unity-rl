using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class MarkovPolicy : MarkovBase {
    
    public Dictionary<IState, int> policy;

    public override void Init() {
        base.Init();

        policy = new Dictionary<IState, int>();

        foreach (var state in States.ToList()) {
            AddNewStateToPolicy(state);
        }

        // train policy
        while (true) {
            PolicyEvaluation();
            if (PolicyImprovement()) break;
        }
    }

    public override int Think(IState state) {
        if (state.IsFinal) return 0;
        return (int)policy[state];
    }

    // Met à jour V_s pour la stratégie courante.
    private void PolicyEvaluation() {
        IState newState;

        while (true) {
            float delta = 0;
            
            foreach (var state in States.ToList()) {

                if (state.IsFinal) continue; // ne modifie pas V_s si état final
                
                if (!V_s.ContainsKey(state)) AddNewStateToVS(state);

                if (!policy.ContainsKey(state)) {
                    if (state.HasActions) AddNewStateToPolicy(state);
                    else continue; // skip if it's an no action state
                }

                float tmp = V_s[state];

                V_s[state] = GetRewardForAction(state, (int)policy[state], out newState);

                delta = Math.Max(delta, Math.Abs(tmp - V_s[state]));
            }

            if (delta < EPSILON) break;
        }
    }

    // Met à jour la stratégie, retourne vrai si la policy converge
    private bool PolicyImprovement() {
        bool isStable = true;
        IState newState;

        foreach (var state in States.ToList()) {
            
            if (state.IsFinal) continue; // ne modifie pas V_s si état final

            if (!policy.ContainsKey(state)) {
                if (state.HasActions) AddNewStateToPolicy(state);
                else continue; // skip if it's an no action state
            }
            
            int tmp = (int)policy[state];

            policy[state] = ArgMaxAction(state);

            isStable = isStable && (tmp == (int)policy[state]);
        }

        return isStable;
    }

    private void AddNewStateToPolicy(IState state) {
        if (!States.Contains(state)) States.Add(state);
        
        // Intialise la stratégie avec des actions aléatoires pour chaque état possible.
        if (!state.IsFinal) {
            int action = state.PossibleActions[Utils.RandomGenerator.RandomNumber(0, state.PossibleActions.Count)];
            policy.Add(state, action);
        }
    }

    protected override void AddNewStateToVS(IState state) {
        if (!States.Contains(state)) States.Add(state);

        // Intitialize V_s with a random number
        float value = Utils.RandomGenerator.RandomFloat(0, 1);
        V_s.Add(state, value);
        if (state.IsFinal) V_s[state] = 0; // Until if is a final state
    }
}
