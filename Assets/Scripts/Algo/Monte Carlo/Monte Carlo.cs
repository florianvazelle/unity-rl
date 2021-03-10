using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonteCarlo : MarkovBase
{

    // Pi(s) => dictionnaire de Istate (clef = Istate, value = int "action associée")
    public Dictionary<IState, int> Pi_s;

    // -> initialisé à random
    private void RandomPolicy()
    {
        Pi_s = new Dictionary<IState, int>();

        foreach (var state in m_states)
        {
            if (state.isFinal()) continue;
            int action = m_actions[Utils.RandomGenerator.RandomNumber(0, m_actions.Count)];
            Pi_s.Add(state, action);
        }
    }
    // States

    // Q(s,a) => Dictionary<(IState, int), int> (on stocke la moyenne des returns)
    public Dictionary<IState, int> Q_s;
    // -> initialisé à random

    // Returns(s, a) 
    // -> initialiser à vide
    public List<IState> Return_s;

    public MonteCarlo(List<IState> states, List<int> actions, ConvertMethod T) : base(states, actions, T)
    {
        // Pour chaque état, initialiser la politique avec une action random
        // (Comme bcp de possibilité, un seul élément dans le dictionnaire)

        // intitialize v_s with a random number
        v_s = new Dictionary<IState, float>();
        foreach (var state in m_states) v_s.Add(state, 0f);

        // Loop forever (for each episode or game)
        // start from a random state

        // creation d'une liste de (State Action Reward)

        // generate random game until the end
        // apply transition method on S0 & A0
        // add An, Sn, Rn à la liste 

        // G = 0
        // Loop for each step (on remonte les infos qu'on a obtenue (reward)) (on commence par l'avant derniére étape)
        // G = gamma * G + R(t+1) (reward de l'itération suivante) (on va de la fin au debut) (la derniére en premmier)

        // Si on est pas deja passé par cet état
        // on ajoute G à Returns(s, a)

        // on ajoute la moyenne des Returns(s, a) dans Q(s,a)
        // pour toute les actions possibles de l'état on ajoute l'action qui a la meilleure moyenne
    }


    public override int Think(IState state)
    {

        if (state.isFinal()) return 0;
        return (int)Pi_s[state];

    }




}
