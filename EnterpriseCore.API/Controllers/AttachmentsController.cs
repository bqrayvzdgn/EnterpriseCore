using EnterpriseCore.Application.Features.Attachments.DTOs;
using EnterpriseCore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCore.API.Controllers;

[Authorize]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class AttachmentsController : BaseController
{
    private readonly IAttachmentService _attachmentService;

    public AttachmentsController(IAttachmentService attachmentService)
    {
        _attachmentService = attachmentService;
    }

    /// <summary>
    /// Upload a new attachment
    /// </summary>
    [HttpPost]
    [RequestSizeLimit(50 * 1024 * 1024)] // 50MB limit
    [ProducesResponseType(typeof(AttachmentDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Upload(
        IFormFile file,
        [FromQuery] Guid? taskId,
        [FromQuery] Guid? projectId,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file provided" });
        }

        var request = new UploadAttachmentRequest
        {
            FileStream = file.OpenReadStream(),
            FileName = file.FileName,
            ContentType = file.ContentType,
            FileSize = file.Length,
            TaskId = taskId,
            ProjectId = projectId
        };

        var result = await _attachmentService.UploadAsync(request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get attachment by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AttachmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _attachmentService.GetByIdAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Download attachment file
    /// </summary>
    [HttpGet("{id:guid}/download")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var result = await _attachmentService.DownloadAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

        var (fileStream, contentType, fileName) = result.Value;
        return File(fileStream, contentType, fileName);
    }

    /// <summary>
    /// Delete an attachment
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _attachmentService.DeleteAsync(id, cancellationToken);
        return HandleResult(result);
    }
}

[Authorize]
[Route("api/tasks/{taskId:guid}/attachments")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class TaskAttachmentsController : BaseController
{
    private readonly IAttachmentService _attachmentService;

    public TaskAttachmentsController(IAttachmentService attachmentService)
    {
        _attachmentService = attachmentService;
    }

    /// <summary>
    /// Get all attachments for a task
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AttachmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByTaskId(Guid taskId, CancellationToken cancellationToken)
    {
        var result = await _attachmentService.GetByTaskIdAsync(taskId, cancellationToken);
        return HandleResult(result);
    }
}

[Authorize]
[Route("api/projects/{projectId:guid}/attachments")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class ProjectAttachmentsController : BaseController
{
    private readonly IAttachmentService _attachmentService;

    public ProjectAttachmentsController(IAttachmentService attachmentService)
    {
        _attachmentService = attachmentService;
    }

    /// <summary>
    /// Get all attachments for a project
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AttachmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByProjectId(Guid projectId, CancellationToken cancellationToken)
    {
        var result = await _attachmentService.GetByProjectIdAsync(projectId, cancellationToken);
        return HandleResult(result);
    }
}
