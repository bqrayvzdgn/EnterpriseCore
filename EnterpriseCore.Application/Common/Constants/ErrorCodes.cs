namespace EnterpriseCore.Application.Common.Constants;

public static class ErrorCodes
{
    // General
    public const string NotFound = "NOT_FOUND";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string ValidationError = "VALIDATION_ERROR";
    public const string InternalError = "INTERNAL_ERROR";
    public const string ConcurrencyError = "CONCURRENCY_ERROR";
    public const string DatabaseError = "DATABASE_ERROR";

    // Auth
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    public const string InvalidToken = "INVALID_TOKEN";
    public const string TokenExpired = "TOKEN_EXPIRED";
    public const string AccountInactive = "ACCOUNT_INACTIVE";
    public const string EmailExists = "EMAIL_EXISTS";

    // User
    public const string UserNotFound = "USER_NOT_FOUND";
    public const string CannotDeleteSelf = "CANNOT_DELETE_SELF";
    public const string RoleNotFound = "ROLE_NOT_FOUND";

    // Project
    public const string ProjectNotFound = "PROJECT_NOT_FOUND";
    public const string AlreadyMember = "ALREADY_MEMBER";
    public const string NotMember = "NOT_MEMBER";
    public const string CannotRemoveOwner = "CANNOT_REMOVE_OWNER";

    // Task
    public const string TaskNotFound = "TASK_NOT_FOUND";
    public const string InvalidAssignee = "INVALID_ASSIGNEE";
    public const string InvalidStatus = "INVALID_STATUS";

    // Sprint
    public const string SprintNotFound = "SPRINT_NOT_FOUND";
    public const string SprintAlreadyStarted = "SPRINT_ALREADY_STARTED";
    public const string SprintNotActive = "SPRINT_NOT_ACTIVE";
    public const string TaskAlreadyInSprint = "TASK_ALREADY_IN_SPRINT";

    // Attachment
    public const string AttachmentNotFound = "ATTACHMENT_NOT_FOUND";
    public const string StorageError = "STORAGE_ERROR";
    public const string InvalidFile = "INVALID_FILE";
}
