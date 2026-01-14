using EnterpriseCore.Application.Common.Models;
using EnterpriseCore.Application.Features.Attachments.DTOs;
using EnterpriseCore.Application.Interfaces;
using EnterpriseCore.Domain.Entities;
using EnterpriseCore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

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

    public AttachmentService(
        IRepository<Attachment> attachmentRepository,
        IRepository<TaskItem> taskRepository,
        IRepository<Project> projectRepository,
        IFileStorageService fileStorageService,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        DbContext dbContext)
    {
        _attachmentRepository = attachmentRepository;
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _fileStorageService = fileStorageService;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
    }

    public async Task<Result<AttachmentDto>> UploadAsync(UploadAttachmentRequest request, CancellationToken cancellationToken = default)
    {
        // Validate that either TaskId or ProjectId is provided (but not both)
        if (request.TaskId == null && request.ProjectId == null)
        {
            return Result.Failure<AttachmentDto>("Either TaskId or ProjectId must be provided", "VALIDATION_ERROR");
        }

        if (request.TaskId != null && request.ProjectId != null)
        {
            return Result.Failure<AttachmentDto>("Cannot attach to both Task and Project", "VALIDATION_ERROR");
        }

        // Verify the task or project exists
        if (request.TaskId.HasValue)
        {
            var taskExists = await _taskRepository.ExistsAsync(request.TaskId.Value, cancellationToken);
            if (!taskExists)
            {
                return Result.Failure<AttachmentDto>("Task not found", "NOT_FOUND");
            }
        }

        if (request.ProjectId.HasValue)
        {
            var projectExists = await _projectRepository.ExistsAsync(request.ProjectId.Value, cancellationToken);
            if (!projectExists)
            {
                return Result.Failure<AttachmentDto>("Project not found", "NOT_FOUND");
            }
        }

        // Save file to storage
        var storagePath = await _fileStorageService.SaveFileAsync(
            request.FileStream,
            request.FileName,
            request.ContentType,
            cancellationToken);

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
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToDto(attachment));
    }

    public async Task<Result<AttachmentDto>> GetByIdAsync(Guid attachmentId, CancellationToken cancellationToken = default)
    {
        var attachment = await _attachmentRepository.Query()
            .Include(a => a.UploadedBy)
            .FirstOrDefaultAsync(a => a.Id == attachmentId, cancellationToken);

        if (attachment == null)
        {
            return Result.Failure<AttachmentDto>("Attachment not found", "NOT_FOUND");
        }

        return Result.Success(MapToDto(attachment));
    }

    public async Task<Result<(Stream FileStream, string ContentType, string FileName)>> DownloadAsync(
        Guid attachmentId, CancellationToken cancellationToken = default)
    {
        var attachment = await _attachmentRepository.GetByIdAsync(attachmentId, cancellationToken);

        if (attachment == null)
        {
            return Result.Failure<(Stream, string, string)>("Attachment not found", "NOT_FOUND");
        }

        var fileStream = await _fileStorageService.GetFileAsync(attachment.StoragePath, cancellationToken);

        if (fileStream == null)
        {
            return Result.Failure<(Stream, string, string)>("File not found in storage", "NOT_FOUND");
        }

        return Result.Success((fileStream, attachment.ContentType, attachment.OriginalFileName));
    }

    public async Task<Result<bool>> DeleteAsync(Guid attachmentId, CancellationToken cancellationToken = default)
    {
        var attachment = await _attachmentRepository.GetByIdAsync(attachmentId, cancellationToken);

        if (attachment == null)
        {
            return Result.Failure<bool>("Attachment not found", "NOT_FOUND");
        }

        // Check if user has permission to delete (must be the uploader or have admin rights)
        if (attachment.UploadedById != _currentUserService.UserId)
        {
            return Result.Failure<bool>("You can only delete your own attachments", "FORBIDDEN");
        }

        // Delete file from storage
        await _fileStorageService.DeleteFileAsync(attachment.StoragePath, cancellationToken);

        // Soft delete the attachment record
        await _attachmentRepository.DeleteAsync(attachment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }

    public async Task<Result<IReadOnlyList<AttachmentDto>>> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var taskExists = await _taskRepository.ExistsAsync(taskId, cancellationToken);
        if (!taskExists)
        {
            return Result.Failure<IReadOnlyList<AttachmentDto>>("Task not found", "NOT_FOUND");
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
            return Result.Failure<IReadOnlyList<AttachmentDto>>("Project not found", "NOT_FOUND");
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
