using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class QLearning : TemporalDifference { 

    public override void Init() {
        base.Init();

        for (int t = 0; t < EPOCHS; t++) {

            IState s = States[0];
            int a = GetGreedyAction(s);

            for (int depth = 0; depth < MAX_DEPTH; depth++) {

                IState sprime;
                int r = Transition(s, a, out sprime).value;
                int aprime = ArgMaxAction(sprime);

                if (depth + 1 >= MAX_DEPTH) r = -1000;
                
                if (!Q_sa.ContainsKey((s, a))) AddNewStateToQ(s, a);
                if (!Q_sa.ContainsKey((sprime, aprime))) AddNewStateToQ(sprime, aprime);
                
                // Bellman Optimality Equation
                Q_sa[(s, a)] = r + GAMMA * Q_sa[(sprime, aprime)];
                s = sprime;
                a = aprime;
                
                if (s.IsFinal) break;
            }
        }
    }

    public override int Think(IState state) {   
        if (state.IsFinal) return 0;
        return ArgMaxAction(state);
    }
}