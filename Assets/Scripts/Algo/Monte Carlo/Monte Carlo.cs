using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonteCarlo : Base
{
    // Pi(s) => dictionnaire de Istate (clef = Istate, value = int "action associée")
    public Dictionary<IState, int> Policy;

    // -> initialisé à random
    private void RandomPolicy()
    {
        Policy = new Dictionary<IState, int>();

        foreach (var state in m_states)
        {
            int action = state.PossibleActions[Utils.RandomGenerator.RandomNumber(0, state.PossibleActions.Count)];
            Policy.Add(state, action);
        }
    }
    // States

    // Q(s,a) => Dictionary<(IState, int), int> (on stocke la moyenne des returns)
    Dictionary<(IState, int), float> Q_s;
    // -> initialisé à random

    // Returns(s, a) 
    // -> initialiser à vide
    Dictionary<(IState, int), List<float>> Return_s;

    public MonteCarlo(List<IState> states, List<int> actions, ConvertMethod T) : base(states, actions, T)
    {
        // Pour chaque état, initialiser la politique avec une action random
        // (Comme bcp de possibilité, un seul élément dans le dictionnaire)

        Return_s = new Dictionary<(IState, int), List<float>>();
        Q_s = new Dictionary<(IState, int), float>();

        RandomPolicy();
    }

    private void AddNewStateToPolicy(IState state)
    {
        if (!m_states.Contains(state)) m_states.Add(state);
        if (!state.IsFinal)
        {
            int action = state.PossibleActions[Utils.RandomGenerator.RandomNumber(0, state.PossibleActions.Count)];
            Policy.Add(state, action);
        }
    }


    public override int Think(IState state)
    {
        if (state.IsFinal) return 0;

        IState newState;
        // Loop forever (for each episode or game)
        for (int t = 0; t < 200; t++)
        {
            // start from a random state
            IState s0 = state;
            int a0 = s0.PossibleActions[0];

            // creation d'une liste de (State Action Reward)
            List<(IState, int, int)> episode = new List<(IState, int, int)>();
            while (true)
            {
                // generate random game until the end
                // apply transition method on S0 & A0
                // add An, Sn, Rn à la liste 
                Cell reward = m_transition(s0, a0, out newState);
                episode.Add((s0, a0, reward.value));

                if (!Policy.ContainsKey(newState))
                {
                    if (newState.HasActions) AddNewStateToPolicy(newState);
                    else break;
                }

                if (newState.IsFinal) break;

                s0 = newState;
                a0 = Policy[newState];
            }

            float G = 0;

            // Loop for each step (on remonte les infos qu'on a obtenue (reward)) (on commence par l'avant derniére étape)
            for (int i = episode.Count - 1; i >= 0; i--)
            {
                // G = gamma * G + R(t+1) (reward de l'itération suivante) (on va de la fin au debut) (la derniére en premmier)
                G = GAMMA * G + episode[i].Item3;

                // Si on est pas deja passé par cet état
                bool isAppear = false;
                for (int j = 0; j < i; j++)
                {
                    if (episode[j].Item1.Equals(episode[i].Item1) && episode[j].Item2 == episode[i].Item2) isAppear = true;
                }

                if (!isAppear)
                {
                    var key = (episode[i].Item1, episode[i].Item2);

                    // on ajoute G à Returns(s, a)
                    if (!Return_s.ContainsKey(key)) Return_s.Add(key, new List<float>());
                    Return_s[key].Add(G);

                    // on ajoute la moyenne des Returns(s, a) dans Q(s,a)
                    if (!Q_s.ContainsKey(key)) Q_s.Add(key, 0);
                    Q_s[key] = Return_s[key].Average();

                    // pour toute les actions possibles de l'état on ajoute l'action qui a la meilleure moyenne

                    // argmax
                    float max = -INFINITY;
                    int act = -1;
                    foreach (var action in episode[i].Item1.PossibleActions)
                    {
                        var tmpKey = (episode[i].Item1, action);
                        if (!Q_s.ContainsKey(tmpKey)) Q_s.Add(tmpKey, 0);

                        if (Q_s[tmpKey] > max)
                        {
                            max = Q_s[tmpKey];
                            act = action;
                        }
                    }

                    if (!Policy.ContainsKey(episode[i].Item1))
                    {
                        if (episode[i].Item1.HasActions) AddNewStateToPolicy(episode[i].Item1);
                        else break;
                    }
                    Policy[episode[i].Item1] = act;
                }

            }
            
                // argmax
                float max = -INFINITY;
                int act = -1;
            foreach (var action in state.PossibleActions)
            {
                Cell reward = m_transition(state, action, out newState);
                if (!Q_s.ContainsKey(newState))
                {
                    if (newState.HasActions) AddNewStateToVS(newState);
                    else continue;
                }

                float current = (reward.value + GAMMA * Q_s[newState]);
                if (state.Equals(newState)) current -= 1;
                if (current > max)
                {
                    max = current;
                    act = action;
                }
            }
            
        }
        //return act;
        return (int)Policy[state];
    }

    public int ThinkEV(IState state)
    {
        if (state.IsFinal) return 0;

        IState newState;
        // Loop forever (for each episode or game)
        for (int t = 0; t < 200; t++)
        {
            // start from a random state
            IState s0 = state;
            int a0 = s0.PossibleActions[0];

            // creation d'une liste de (State Action Reward)
            List<(IState, int, int)> episode = new List<(IState, int, int)>();
            while (true)
            {
                // generate random game until the end
                // apply transition method on S0 & A0
                // add An, Sn, Rn à la liste 
                Cell reward = m_transition(s0, a0, out newState);
                episode.Add((s0, a0, reward.value));

                if (!Policy.ContainsKey(newState))
                {
                    if (newState.HasActions) AddNewStateToPolicy(newState);
                    else break;
                }

                if (newState.IsFinal) break;

                s0 = newState;
                a0 = Policy[newState];
            }

            float G = 0;

            // Loop for each step (on remonte les infos qu'on a obtenue (reward)) (on commence par l'avant derniére étape)
            for (int i = episode.Count - 1; i >= 0; i--)
            {
                // G = gamma * G + R(t+1) (reward de l'itération suivante) (on va de la fin au debut) (la derniére en premmier)
                G = GAMMA * G + episode[i].Item3;

                    var key = (episode[i].Item1, episode[i].Item2);

                    // on ajoute G à Returns(s, a)
                    if (!Return_s.ContainsKey(key)) Return_s.Add(key, new List<float>());
                    Return_s[key].Add(G);

                    // on ajoute la moyenne des Returns(s, a) dans Q(s,a)
                    if (!Q_s.ContainsKey(key)) Q_s.Add(key, 0);
                    Q_s[key] = Return_s[key].Average();

                    // pour toute les actions possibles de l'état on ajoute l'action qui a la meilleure moyenne

                    // argmax
                    float max = -INFINITY;
                    int act = -1;
                    foreach (var action in episode[i].Item1.PossibleActions)
                    {
                        var tmpKey = (episode[i].Item1, action);
                        if (!Q_s.ContainsKey(tmpKey)) Q_s.Add(tmpKey, 0);

                        if (Q_s[tmpKey] > max)
                        {
                            max = Q_s[tmpKey];
                            act = action;
                        }
                    }

                    if (!Policy.ContainsKey(episode[i].Item1))
                    {
                        if (episode[i].Item1.HasActions) AddNewStateToPolicy(episode[i].Item1);
                        else break;
                    }
                    Policy[episode[i].Item1] = act;

            }
        }

        return (int)Policy[state];
    }
}
