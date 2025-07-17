namespace WorkflowEngine.Models;

public record Action(
    string Id,
    string Name,
    bool Enabled,
    List<string> FromStates,
    string ToState
);
