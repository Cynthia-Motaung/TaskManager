using TaskManager.DTOs;
using TaskManager.Models;

namespace TaskManager.Mappings;

public static class DtoMappings
{
    public static UserDto ToUserDto(this User user) =>
        new()
        {
            Id = user.Id,
            Name = user.Name ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Role = user.Role
        };

    public static UserDetailsDto ToUserDetailsDto(this User user) =>
        new()
        {
            Id = user.Id,
            Name = user.Name ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Role = user.Role,
            Tasks = user.TaskAssignments
                .Where(a => a.TaskItem is not null)
                .Select(a => new UserTaskSummaryDto
                {
                    TaskItemId = a.TaskItemId,
                    Title = a.TaskItem!.Title ?? string.Empty
                })
                .ToArray()
        };

    public static ProjectDto ToProjectDto(this Project project) =>
        new()
        {
            Id = project.Id,
            Name = project.Name ?? string.Empty,
            Description = project.Description,
            CreatedAt = project.CreatedAt
        };

    public static ProjectDetailsDto ToProjectDetailsDto(this Project project) =>
        new()
        {
            Id = project.Id,
            Name = project.Name ?? string.Empty,
            Description = project.Description,
            CreatedAt = project.CreatedAt,
            Tasks = project.Tasks
                .Select(t => new ProjectTaskSummaryDto
                {
                    Id = t.Id,
                    Title = t.Title ?? string.Empty,
                    Status = t.Status,
                    Priority = t.Priority
                })
                .ToArray()
        };

    public static TaskDto ToTaskDto(this TaskItem task) =>
        new()
        {
            Id = task.Id,
            Title = task.Title ?? string.Empty,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            DueDate = task.DueDate,
            ProjectId = task.ProjectId,
            Assignments = task.TaskAssignments
                .Select(a => new TaskAssignmentSummaryDto
                {
                    UserId = a.UserId,
                    UserName = a.User?.Name ?? string.Empty
                })
                .ToArray(),
            Dependencies = task.Dependencies
                .Select(d => new TaskDependencySummaryDto
                {
                    DependsOnTaskId = d.DependsOnTaskId
                })
                .ToArray()
        };

    public static TaskAssignmentDto ToTaskAssignmentDto(this TaskAssignment assignment) =>
        new()
        {
            UserId = assignment.UserId,
            UserName = assignment.User?.Name ?? string.Empty,
            TaskItemId = assignment.TaskItemId,
            TaskTitle = assignment.TaskItem?.Title ?? string.Empty
        };

    public static TaskDependencyDto ToTaskDependencyDto(this TaskDependency dependency) =>
        new()
        {
            TaskItemId = dependency.TaskItemId,
            TaskTitle = dependency.TaskItem?.Title ?? string.Empty,
            DependsOnTaskId = dependency.DependsOnTaskId,
            DependsOnTaskTitle = dependency.DependsOnTask?.Title ?? string.Empty
        };

    public static CommentDto ToCommentDto(this Comment comment) =>
        new()
        {
            Id = comment.Id,
            TaskItemId = comment.TaskItemId,
            UserId = comment.UserId,
            UserName = comment.User?.Name ?? string.Empty,
            Content = comment.Content ?? string.Empty,
            CreatedAt = comment.CreatedAt
        };

    public static AttachmentDto ToAttachmentDto(this Attachment attachment) =>
        new()
        {
            Id = attachment.Id,
            TaskItemId = attachment.TaskItemId,
            FileName = attachment.FileName ?? string.Empty,
            FileUrl = attachment.FileUrl ?? string.Empty,
            UploadedAt = attachment.UploadedAt
        };
}
