using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MCTS : MonoBehaviour
{
    // Pseudo code for Monte Carlo

        // Starting Point
            // 4 possible fonctions (Selection, Expansion, Simulation, Update)

        // While a node hasn't been explored
            //-> Expansion(Investigation) play random moves for the player until the end
            // Write the result on the node


        // -> Selection (based on the stats of each nodes and how ignored the childs are (see the selection equation)) 
            // run the selection equation for each nodes
            // Select one

        // -> Expansion for each childs unexplored

        // -> Simulation (random moves for each players)
    
        // -> Update tree

        // Selection from the top of the tree

        // If we explored enouth and we want to make a choice now, we choose the most explored move

        // Run a brand new simulation from the begining but start node is the chosen node.
}
