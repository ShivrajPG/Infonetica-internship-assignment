using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using WorkflowEngine.Models;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var workflowsDb = new ConcurrentDictionary<string, WorkflowDefinition>();
var instancesDb = new ConcurrentDictionary<string, WorkflowInstance>();

app.MapPost("/workflows", (WorkflowDefinition workflow) =>
{
    if (!IsValidWorkflow(workflow, out var error))
        return Results.BadRequest(error);

    workflowsDb[workflow.Id] = workflow;

    Console.WriteLine($"✅ New workflow created: {workflow.Id} at {DateTime.UtcNow}");
    return Results.Created($"/workflows/{workflow.Id}", workflow);
});

app.MapGet("/workflows/{id}", (string id) =>
    workflowsDb.TryGetValue(id, out var workflow) ? Results.Ok(workflow) : Results.NotFound());

app.MapGet("/workflows", () => Results.Ok(workflowsDb.Values));

app.MapPost("/workflows/{workflowId}/instances", (string workflowId) =>
{
    if (!workflowsDb.TryGetValue(workflowId, out var workflow))
        return Results.NotFound("Workflow definition not found.");

    var initialState = workflow.States.FirstOrDefault(s => s.IsInitial && s.Enabled);
    if (initialState is null)
        return Results.BadRequest("No enabled initial state found.");

    string instanceId;
    do
    {
        instanceId = Guid.NewGuid().ToString();
    } while (instancesDb.ContainsKey(instanceId));

    var wfInstance = new WorkflowInstance(
        instanceId,
        workflowId,
        initialState.Id,
        new List<InstanceHistoryEntry>()
    );

    instancesDb[wfInstance.Id] = wfInstance;

    Console.WriteLine($"📦 Instance started: {wfInstance.Id} at state '{initialState.Id}'");
    return Results.Created($"/instances/{wfInstance.Id}", wfInstance);
});


app.MapPost("/instances/{id}/actions/{actionId}", (string id, string actionId) =>
{
    if (!instancesDb.TryGetValue(id, out var wfInstance) ||
        !workflowsDb.TryGetValue(wfInstance.WorkflowDefinitionId, out var workflow))
        return Results.NotFound("Workflow instance or definition not found.");

    var currentState = workflow.States.FirstOrDefault(s => s.Id == wfInstance.CurrentStateId);
    if (currentState is null)
        return Results.BadRequest("Current state does not exist.");

    if (currentState.IsFinal)
        return Results.BadRequest("Cannot perform actions on a final state.");

    var transition = workflow.Actions.FirstOrDefault(a => a.Id == actionId);
    if (transition is null)
        return Results.BadRequest("Action not found in workflow.");

    if (!transition.Enabled)
        return Results.BadRequest("Action is disabled.");

    if (!transition.FromStates.Contains(wfInstance.CurrentStateId))
        return Results.BadRequest("Action not valid from current state.");

    if (!workflow.States.Any(s => s.Id == transition.ToState))
        return Results.BadRequest("Target state does not exist.");

    
    wfInstance.CurrentStateId = transition.ToState;
    wfInstance.History.Add(new InstanceHistoryEntry(actionId, DateTime.UtcNow));

    Console.WriteLine($"🔁 Action '{actionId}' executed on instance {id}");
    return Results.Ok(wfInstance);
});

app.MapGet("/instances/{id}", (string id) =>
    instancesDb.TryGetValue(id, out var wfInstance) ? Results.Ok(wfInstance) : Results.NotFound());


app.MapGet("/instances", () => Results.Ok(instancesDb.Values));

bool IsValidWorkflow(WorkflowDefinition workflow, out string error)
{
    error = "";

    if (workflowsDb.ContainsKey(workflow.Id))
    {
        error = "Duplicate workflow ID.";
        return false;
    }

    if (workflow.States.Count(s => s.IsInitial) != 1)
    {
        error = "Exactly one initial state is required.";
        return false;
    }

    if (workflow.States.Select(s => s.Id).Distinct().Count() != workflow.States.Count)
    {
        error = "Duplicate state IDs found.";
        return false;
    }

    if (workflow.Actions.Select(a => a.Id).Distinct().Count() != workflow.Actions.Count)
    {
        error = "Duplicate action IDs found.";
        return false;
    }

    var stateIds = workflow.States.Select(s => s.Id).ToHashSet();

    foreach (var transition in workflow.Actions)
    {
        if (!transition.Enabled)
            continue;

        if (!stateIds.Contains(transition.ToState))
        {
            error = $"Action '{transition.Id}' targets unknown state '{transition.ToState}'.";
            return false;
        }

        if (transition.FromStates.Any(from => !stateIds.Contains(from)))
        {
            error = $"Action '{transition.Id}' references unknown from-state.";
            return false;
        }
    }

    return true;
}

app.Run();