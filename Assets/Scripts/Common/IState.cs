using System.Collections.Generic;

public interface IState {
    bool IsFinal { get; set; }          // To check if it's a final state (win)
    bool HasActions { get; set; }       // To check if we need to add the state to the policy
    List<int> Actions { get; set; }     // List of all possible action on this state
}