using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EnterpriseCore.API.Hubs;

[Authorize]
public class ProjectHub : Hub
{
    private readonly ILogger<ProjectHub> _logger;

    public ProjectHub(ILogger<ProjectHub> logger)
    {
        _logger = logger;
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
        await Groups.AddToGroupAsync(Context.ConnectionId, $"project:{projectId}");
        _logger.LogInformation("Client {ConnectionId} joined project group {ProjectId}", Context.ConnectionId, projectId);
    }

    public async Task LeaveProject(Guid projectId)
    {
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
