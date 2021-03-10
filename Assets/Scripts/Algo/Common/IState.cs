using System.Collections.Generic;

public interface IState {
    bool IsFinal { get; set; }
    bool HasActions { get; set; }
    List<int> PossibleActions { get; set; }
}