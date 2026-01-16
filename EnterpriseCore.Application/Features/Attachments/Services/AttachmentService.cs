using EnterpriseCore.Application.Common.Constants;
using EnterpriseCore.Application.Common.Models;
using EnterpriseCore.Application.Features.Attachments.DTOs;
using EnterpriseCore.Application.Interfaces;
using EnterpriseCore.Domain.Entities;
using EnterpriseCore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnterpriseCore.Application.Features.Attachments.Services;

public class AttachmentService : IAttachmentService
{
    private readonly IRepository<Attachment> _attachmentRepository;
    private readonly IRepository<TaskItem> _taskRepository;
    private readonly IRepository<Project> _projectRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DbContext _dbContext;
    private readonly ILogger<AttachmentService> _logger;

    public AttachmentService(
        IRepository<Attachment> attachmentRepository,
        IRepository<TaskItem> taskRepository,
        IRepository<Project> projectRepository,
        IFileStorageService fileStorageService,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        DbContext dbContext,
        ILogger<AttachmentService> logger)
    {
        _attachmentRepository = attachmentRepository;
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _fileStorageService = fileStorageService;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<AttachmentDto>> UploadAsync(UploadAttachmentRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Uploading attachment. FileName: {FileName}, FileSize: {FileSize}, TaskId: {TaskId}, ProjectId: {ProjectId}, UserId: {UserId}",
            request.FileName, request.FileSize, request.TaskId, request.ProjectId, _currentUserService.UserId);

        // Validate that either TaskId or ProjectId is provided (but not both)
        if (request.TaskId == null && request.ProjectId == null)
        {
            _logger.LogWarning("Upload failed: Neither TaskId nor ProjectId provided");
            return Result.Failure<AttachmentDto>("Either TaskId or ProjectId must be provided", ErrorCodes.ValidationError);
        }

        if (request.TaskId != null && request.ProjectId != null)
        {
            _logger.LogWarning("Upload failed: Both TaskId and ProjectId provided");
            return Result.Failure<AttachmentDto>("Cannot attach to both Task and Project", ErrorCodes.ValidationError);
        }

        // Verify the task or project exists
        if (request.TaskId.HasValue)
        {
            var taskExists = await _taskRepository.ExistsAsync(request.TaskId.Value, cancellationToken);
            if (!taskExists)
            {
                _logger.LogWarning("Upload failed: Task not found. TaskId: {TaskId}", request.TaskId);
                return Result.Failure<AttachmentDto>("Task not found", ErrorCodes.NotFound);
            }
        }

        if (request.ProjectId.HasValue)
        {
            var projectExists = await _projectRepository.ExistsAsync(request.ProjectId.Value, cancellationToken);
            if (!projectExists)
            {
                _logger.LogWarning("Upload failed: Project not found. ProjectId: {ProjectId}", request.ProjectId);
                return Result.Failure<AttachmentDto>("Project not found", ErrorCodes.NotFound);
            }
        }

        // Save file to storage
        string storagePath;
        try
        {
            storagePath = await _fileStorageService.SaveFileAsync(
                request.FileStream,
                request.FileName,
                request.ContentType,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "File storage error during upload. FileName: {FileName}", request.FileName);
            return Result.Failure<AttachmentDto>("Failed to save file to storage.", ErrorCodes.DatabaseError);
        }

        var attachment = new Attachment
        {
            FileName = Path.GetFileName(storagePath),
            OriginalFileName = request.FileName,
            ContentType = request.ContentType,
            FileSize = request.FileSize,
            StoragePath = storagePath,
            TaskId = request.TaskId,
            ProjectId = request.ProjectId,
            UploadedById = _currentUserService.UserId!.Value
        };

        await _attachmentRepository.AddAsync(attachment, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error during attachment upload. FileName: {FileName}", request.FileName);
            return Result.Failure<AttachmentDto>("Failed to save attachment.", ErrorCodes.DatabaseError);
        }

        _logger.LogInformation("Attachment uploaded successfully. AttachmentId: {AttachmentId}, FileName: {FileName}",
            attachment.Id, attachment.OriginalFileName);

        return Result.Success(MapToDto(attachment));
    }

    public async Task<Result<AttachmentDto>> GetByIdAsync(Guid attachmentId, CancellationToken cancellationToken = default)
    {
        var attachment = await _attachmentRepository.Query()
            .Include(a => a.UploadedBy)
            .FirstOrDefaultAsync(a => a.Id == attachmentId, cancellationToken);

        if (attachment == null)
        {
            _logger.LogWarning("Attachment not found. AttachmentId: {AttachmentId}", attachmentId);
            return Result.Failure<AttachmentDto>("Attachment not found", ErrorCodes.NotFound);
        }

        return Result.Success(MapToDto(attachment));
    }

    public async Task<Result<(Stream FileStream, string ContentType, string FileName)>> DownloadAsync(
        Guid attachmentId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Downloading attachment. AttachmentId: {AttachmentId}, UserId: {UserId}",
            attachmentId, _currentUserService.UserId);

        var attachment = await _attachmentRepository.GetByIdAsync(attachmentId, cancellationToken);

        if (attachment == null)
        {
            _logger.LogWarning("Download failed: Attachment not found. AttachmentId: {AttachmentId}", attachmentId);
            return Result.Failure<(Stream, string, string)>("Attachment not found", ErrorCodes.NotFound);
        }

        var fileStream = await _fileStorageService.GetFileAsync(attachment.StoragePath, cancellationToken);

        if (fileStream == null)
        {
            _logger.LogWarning("Download failed: File not found in storage. AttachmentId: {AttachmentId}, StoragePath: {StoragePath}",
                attachmentId, attachment.StoragePath);
            return Result.Failure<(Stream, string, string)>("File not found in storage", ErrorCodes.NotFound);
        }

        _logger.LogInformation("Attachment downloaded successfully. AttachmentId: {AttachmentId}", attachmentId);
        return Result.Success((fileStream, attachment.ContentType, attachment.OriginalFileName));
    }

    public async Task<Result<bool>> DeleteAsync(Guid attachmentId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting attachment. AttachmentId: {AttachmentId}, UserId: {UserId}",
            attachmentId, _currentUserService.UserId);

        var attachment = await _attachmentRepository.GetByIdAsync(attachmentId, cancellationToken);

        if (attachment == null)
        {
            _logger.LogWarning("Attachment deletion failed: Attachment not found. AttachmentId: {AttachmentId}", attachmentId);
            return Result.Failure<bool>("Attachment not found", ErrorCodes.NotFound);
        }

        // Check if user has permission to delete (must be the uploader or have admin rights)
        if (attachment.UploadedById != _currentUserService.UserId)
        {
            _logger.LogWarning("Attachment deletion failed: Access denied. AttachmentId: {AttachmentId}, UploaderId: {UploaderId}, UserId: {UserId}",
                attachmentId, attachment.UploadedById, _currentUserService.UserId);
            return Result.Failure<bool>("You can only delete your own attachments", ErrorCodes.Forbidden);
        }

        // Delete file from storage
        try
        {
            await _fileStorageService.DeleteFileAsync(attachment.StoragePath, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "File storage error during attachment deletion. AttachmentId: {AttachmentId}", attachmentId);
            return Result.Failure<bool>("Failed to delete file from storage.", ErrorCodes.DatabaseError);
        }

        // Soft delete the attachment record
        await _attachmentRepository.DeleteAsync(attachment, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error during attachment deletion. AttachmentId: {AttachmentId}", attachmentId);
            return Result.Failure<bool>("Failed to delete attachment.", ErrorCodes.DatabaseError);
        }

        _logger.LogInformation("Attachment deleted successfully. AttachmentId: {AttachmentId}", attachmentId);
        return Result.Success(true);
    }

    public async Task<Result<IReadOnlyList<AttachmentDto>>> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var taskExists = await _taskRepository.ExistsAsync(taskId, cancellationToken);
        if (!taskExists)
        {
            _logger.LogWarning("Get attachments by task failed: Task not found. TaskId: {TaskId}", taskId);
            return Result.Failure<IReadOnlyList<AttachmentDto>>("Task not found", ErrorCodes.NotFound);
        }

        var attachments = await _attachmentRepository.Query()
            .Include(a => a.UploadedBy)
            .Where(a => a.TaskId == taskId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<AttachmentDto>>(attachments.Select(MapToDto).ToList());
    }

    public async Task<Result<IReadOnlyList<AttachmentDto>>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var projectExists = await _projectRepository.ExistsAsync(projectId, cancellationToken);
        if (!projectExists)
        {
            _logger.LogWarning("Get attachments by project failed: Project not found. ProjectId: {ProjectId}", projectId);
            return Result.Failure<IReadOnlyList<AttachmentDto>>("Project not found", ErrorCodes.NotFound);
        }

        var attachments = await _attachmentRepository.Query()
            .Include(a => a.UploadedBy)
            .Where(a => a.ProjectId == projectId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<AttachmentDto>>(attachments.Select(MapToDto).ToList());
    }

    private static AttachmentDto MapToDto(Attachment attachment)
    {
        return new AttachmentDto
        {
            Id = attachment.Id,
            FileName = attachment.FileName,
            OriginalFileName = attachment.OriginalFileName,
            ContentType = attachment.ContentType,
            FileSize = attachment.FileSize,
            TaskId = attachment.TaskId,
            ProjectId = attachment.ProjectId,
            UploadedById = attachment.UploadedById,
            UploadedByName = attachment.UploadedBy != null
                ? $"{attachment.UploadedBy.FirstName} {attachment.UploadedBy.LastName}"
                : null,
            CreatedAt = attachment.CreatedAt
        };
    }
}
