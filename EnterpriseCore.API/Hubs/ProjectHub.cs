using EnterpriseCore.Domain.Entities;
using EnterpriseCore.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EnterpriseCore.API.Hubs;

[Authorize]
public class ProjectHub : Hub
{
    private readonly ILogger<ProjectHub> _logger;
    private readonly IRepository<Project> _projectRepository;

    public ProjectHub(ILogger<ProjectHub> logger, IRepository<Project> projectRepository)
    {
        _logger = logger;
        _projectRepository = projectRepository;
    }

    public override async Task OnConnectedAsync()
    {
        var tenantId = Context.User?.FindFirst("tenant_id")?.Value;
        if (!string.IsNullOrEmpty(tenantId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"tenant:{tenantId}");
            _logger.LogInformation("Client {ConnectionId} joined tenant group {TenantId}", Context.ConnectionId, tenantId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var tenantId = Context.User?.FindFirst("tenant_id")?.Value;
        if (!string.IsNullOrEmpty(tenantId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"tenant:{tenantId}");
            _logger.LogInformation("Client {ConnectionId} left tenant group {TenantId}", Context.ConnectionId, tenantId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinProject(Guid projectId)
    {
        // Verify user has access to this project (global tenant filter ensures only tenant's projects are visible)
        var projectExists = await _projectRepository.ExistsAsync(projectId);
        if (!projectExists)
        {
            _logger.LogWarning("Client {ConnectionId} attempted to join non-existent or unauthorized project {ProjectId}",
                Context.ConnectionId, projectId);
            throw new HubException("Project not found or access denied.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"project:{projectId}");
        _logger.LogInformation("Client {ConnectionId} joined project group {ProjectId}", Context.ConnectionId, projectId);
    }

    public async Task LeaveProject(Guid projectId)
    {
        // No need to verify access for leaving - just remove from group
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"project:{projectId}");
        _logger.LogInformation("Client {ConnectionId} left project group {ProjectId}", Context.ConnectionId, projectId);
    }
}

public interface IProjectHubClient
{
    Task TaskCreated(Guid taskId, Guid projectId);
    Task TaskUpdated(Guid taskId, object changes);
    Task TaskStatusChanged(Guid taskId, string oldStatus, string newStatus);
    Task TaskAssigned(Guid taskId, Guid? assigneeId);
    Task CommentAdded(Guid taskId, Guid commentId);
    Task MilestoneCompleted(Guid milestoneId);
}
