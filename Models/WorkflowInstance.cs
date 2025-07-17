namespace WorkflowEngine.Models;

public class WorkflowInstance
{
    public string Id { get; set; }
    public string WorkflowDefinitionId { get; set; }
    public string CurrentStateId { get; set; }
    public List<InstanceHistoryEntry> History { get; set; }

    public WorkflowInstance(string id, string workflowDefinitionId, string currentStateId, List<InstanceHistoryEntry> history)
    {
        Id = id;
        WorkflowDefinitionId = workflowDefinitionId;
        CurrentStateId = currentStateId;
        History = history;
    }
}
