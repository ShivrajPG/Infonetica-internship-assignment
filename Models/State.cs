namespace WorkflowEngine.Models;

public record State(
    string Id,
    string Name,
    bool IsInitial = false,
    bool IsFinal = false,
    bool Enabled = true
);
