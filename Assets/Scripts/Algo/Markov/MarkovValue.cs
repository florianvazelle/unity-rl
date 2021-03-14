using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class MarkovValue : MarkovBase {

    public override void Init() {
        base.Init();
    }

    public override int Think(IState state) {

        if (state.IsFinal) return 0;
        
        IState newState;

        while (true) {
            float delta = 0;
            
            foreach (var s in States.ToList()) {

                if (s.IsFinal) continue; // ne modifie pas V_s si état final
                if (!V_s.ContainsKey(s)) AddNewStateToVS(s);

                float tmp = V_s[s];

                float max = -INFINITY;
                foreach (var action in s.Actions) {  
                    float current = GetRewardForAction(s, action, out newState);
                    if (current > max) max = current;
                }
                
                V_s[s] = max;

                delta = Math.Max(delta, Math.Abs(tmp - V_s[s]));
            }

            if (delta < EPSILON) break;
        }
        
        return ArgMaxAction(state);
    }

    protected override void AddNewStateToVS(IState state) {
        if (!States.Contains(state)) States.Add(state);
        V_s.Add(state, 0f);
    }
}
