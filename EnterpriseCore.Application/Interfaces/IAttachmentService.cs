using EnterpriseCore.Application.Common.Models;
using EnterpriseCore.Application.Features.Attachments.DTOs;

namespace EnterpriseCore.Application.Interfaces;

public interface IAttachmentService
{
    Task<Result<AttachmentDto>> UploadAsync(UploadAttachmentRequest request, CancellationToken cancellationToken = default);
    Task<Result<AttachmentDto>> GetByIdAsync(Guid attachmentId, CancellationToken cancellationToken = default);
    Task<Result<(Stream FileStream, string ContentType, string FileName)>> DownloadAsync(Guid attachmentId, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteAsync(Guid attachmentId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<AttachmentDto>>> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<AttachmentDto>>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
}
