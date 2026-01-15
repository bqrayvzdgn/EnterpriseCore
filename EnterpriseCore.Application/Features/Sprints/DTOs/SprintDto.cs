using EnterpriseCore.Domain.Enums;

namespace EnterpriseCore.Application.Features.Sprints.DTOs;

public class SprintDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Goal { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public SprintStatus Status { get; set; }
    public Guid ProjectId { get; set; }
    public int TaskCount { get; set; }
    public int CompletedTaskCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateSprintRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Goal { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class UpdateSprintRequest
{
    public string? Name { get; set; }
    public string? Goal { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
