using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class MonteCarlo : TemporalDifference {

    protected new const float EPOCHS = 2000; 

    private Dictionary<IState, int> policy;
    private Dictionary<(IState, int), List<float>> Return_s;

    private bool es, firstVisit, onPolicy;

    public override void Init() {
        base.Init();

        es = RL.instance.mcES;
        if (es) {
            firstVisit = false;
            onPolicy = true;
        } else {
            onPolicy = RL.instance.mcOnPolicy;
            firstVisit = RL.instance.mcFirstVisit;
        }

        Return_s = new Dictionary<(IState, int), List<float>>();

        // initialize policy with random action (like in MarkovPolicy)
        policy = new Dictionary<IState, int>();
        foreach (var state in States) AddNewStateToPolicy(state);
    }


    public override int Think(IState state) {

        if (state.IsFinal) return 0;

        IState newState;
        // Loop forever (for each episode or game)
        for (int t = 0; t < EPOCHS; t++) {
            // start from a random state
            IState s0 = state;
            int a0 = s0.PossibleActions[0];

            // creation d'une liste de (State Action Reward)
            List<(IState, int, int)> episode = new List<(IState, int, int)>();

            for (int depth = 0; depth < MAX_DEPTH; depth++) {
                // generate random game until the end
                // apply transition method on S0 & A0
                // add An, Sn, Rn à la liste 
                int r = Transition(s0, a0, out newState).value;
                if (depth + 1 >= MAX_DEPTH) r = -1000;
                episode.Add((s0, a0, r));

                if (!policy.ContainsKey(newState)) {
                    if (newState.HasActions) AddNewStateToPolicy(newState);
                    else break;
                }

                if (newState.IsFinal) break;

                s0 = newState;
                a0 = policy[newState];
            }

            float G = 0;

            // Loop for each step (on remonte les infos qu'on a obtenue (reward)) (on commence par l'avant derniére étape)
            for (int i = episode.Count - 1; i >= 0; i--) {
                // G = gamma * G + R(t+1) (reward de l'itération suivante) (on va de la fin au debut) (la derniére en premmier)
                G = GAMMA * G + episode[i].Item3;

                // Si on est pas deja passé par cet état
                bool isAppear = false;

                if (firstVisit) {
                    for (int j = 0; j < i; j++) {
                        if (episode[j].Item1.Equals(episode[i].Item1) && episode[j].Item2 == episode[i].Item2) isAppear = true;
                    }
                }

                if ((!isAppear && firstVisit) || !firstVisit) {
                    var key = (episode[i].Item1, episode[i].Item2);

                    // on ajoute G à Returns(s, a)
                    if (!Return_s.ContainsKey(key)) Return_s.Add(key, new List<float>());
                    Return_s[key].Add(G);

                    // on ajoute la moyenne des Returns(s, a) dans Q(s,a)
                    if (!Q_sa.ContainsKey(key)) AddNewStateToQ(episode[i].Item1, episode[i].Item2);
                    Q_sa[key] = Return_s[key].Average();

                    // pour toute les actions possibles de l'état on ajoute l'action qui a la meilleure moyenne

                    // argmax
                    if (onPolicy) {
                        if (!policy.ContainsKey(episode[i].Item1)) {
                            if (episode[i].Item1.HasActions) AddNewStateToPolicy(episode[i].Item1);
                            else continue;
                        }
                        policy[episode[i].Item1] = ArgMaxAction(episode[i].Item1);
                    }

                }

            }
            // argmax
            if (!onPolicy) {
                foreach (var s in States) {
                    if (!policy.ContainsKey(s)) {
                        if (s.HasActions) AddNewStateToPolicy(s);
                        else continue;
                    }
                    policy[s] = ArgMaxAction(s);
                }
            }
        }

        return (int)policy[state];
    }

    private void AddNewStateToPolicy(IState state) {
        if (!States.Contains(state)) States.Add(state);
        
        // Intialise la stratégie avec des actions aléatoires pour chaque état possible.
        if (!state.IsFinal) {
            int action = state.PossibleActions[Utils.RandomGenerator.RandomNumber(0, state.PossibleActions.Count)];
            policy.Add(state, action);
        }
    }
}
