using System.ComponentModel.DataAnnotations;

namespace TaskManager.DTOs;

public static class AppRoles
{
    public const string User = "User";
    public const string Manager = "Manager";
    public const string Admin = "Admin";

    public static readonly HashSet<string> ValidRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        User,
        Manager,
        Admin
    };
}

public static class TaskFieldRules
{
    public static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Pending",
        "InProgress",
        "Done",
        "Blocked"
    };

    public static readonly HashSet<string> ValidPriorities = new(StringComparer.OrdinalIgnoreCase)
    {
        "Low",
        "Medium",
        "High",
        "Critical"
    };
}

public class UserCreateDto
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string Role { get; set; } = AppRoles.User;

    [MinLength(8)]
    public string? Password { get; set; }
}

public class UserUpdateDto : UserCreateDto;

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = AppRoles.User;
}

public class UserTaskSummaryDto
{
    public int TaskItemId { get; set; }
    public string Title { get; set; } = string.Empty;
}

public class UserDetailsDto : UserDto
{
    public IReadOnlyCollection<UserTaskSummaryDto> Tasks { get; set; } = Array.Empty<UserTaskSummaryDto>();
}

public class AuthRegisterDto
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8), StringLength(128)]
    public string Password { get; set; } = string.Empty;
}

public class AuthLoginDto
{
    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8), StringLength(128)]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = AppRoles.User;
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
}

public class ProjectCreateDto
{
    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }
}

public class ProjectUpdateDto : ProjectCreateDto;

public class ProjectDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProjectTaskSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
}

public class ProjectDetailsDto : ProjectDto
{
    public IReadOnlyCollection<ProjectTaskSummaryDto> Tasks { get; set; } = Array.Empty<ProjectTaskSummaryDto>();
}

public class TaskCreateDto
{
    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required, StringLength(20)]
    public string Status { get; set; } = "Pending";

    [Required, StringLength(20)]
    public string Priority { get; set; } = "Medium";

    public DateTime? DueDate { get; set; }

    [Range(1, int.MaxValue)]
    public int ProjectId { get; set; }
}

public class TaskUpdateDto : TaskCreateDto;

public class TaskAssignmentSummaryDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
}

public class TaskDependencySummaryDto
{
    public int DependsOnTaskId { get; set; }
}

public class TaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public int ProjectId { get; set; }
    public IReadOnlyCollection<TaskAssignmentSummaryDto> Assignments { get; set; } = Array.Empty<TaskAssignmentSummaryDto>();
    public IReadOnlyCollection<TaskDependencySummaryDto> Dependencies { get; set; } = Array.Empty<TaskDependencySummaryDto>();
}

public class TaskAssignmentCreateDto
{
    [Range(1, int.MaxValue)]
    public int UserId { get; set; }

    [Range(1, int.MaxValue)]
    public int TaskItemId { get; set; }
}

public class TaskAssignmentDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int TaskItemId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
}

public class TaskDependencyCreateDto
{
    [Range(1, int.MaxValue)]
    public int TaskItemId { get; set; }

    [Range(1, int.MaxValue)]
    public int DependsOnTaskId { get; set; }
}

public class TaskDependencyDto
{
    public int TaskItemId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public int DependsOnTaskId { get; set; }
    public string DependsOnTaskTitle { get; set; } = string.Empty;
}

public class CommentCreateDto
{
    [Range(1, int.MaxValue)]
    public int UserId { get; set; }

    [Required, StringLength(1000)]
    public string Content { get; set; } = string.Empty;
}

public class CommentDto
{
    public int Id { get; set; }
    public int TaskItemId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AttachmentCreateDto
{
    [Required, StringLength(260)]
    public string FileName { get; set; } = string.Empty;

    [Required, Url, StringLength(2000)]
    public string FileUrl { get; set; } = string.Empty;
}

public class AttachmentDto
{
    public int Id { get; set; }
    public int TaskItemId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}
