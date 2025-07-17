namespace WorkflowEngine.Models;

public record WorkflowDefinition(
    string Id,
    List<State> States,
    List<Action> Actions
);
