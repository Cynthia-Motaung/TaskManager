using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TaskManager.Tests;

public class TaskApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public TaskApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Root_Redirects_To_Swagger()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/swagger", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Login_WithSeededManager_ReturnsToken()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "manager@taskmanager.local",
            password = "Manager@123"
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var body = await loginResponse.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);
        Assert.False(string.IsNullOrWhiteSpace(json.RootElement.GetProperty("accessToken").GetString()));
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_ReturnsOk()
    {
        await AuthenticateAsManagerAsync();
        var response = await _client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateTask_WithInvalidStatus_ReturnsBadRequest()
    {
        await AuthenticateAsManagerAsync();
        var payload = new
        {
            title = "Invalid status task",
            description = "Should fail validation",
            status = "INVALID",
            priority = "High",
            dueDate = DateTime.UtcNow.AddDays(7),
            projectId = 1
        };

        var response = await _client.PostAsJsonAsync("/api/tasks", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AllEndpoints_SmokeTest_Workflow_Passes()
    {
        await AuthenticateAsManagerAsync();
        var unique = Guid.NewGuid().ToString("N")[..8];

        // Users
        var userCreateResponse = await _client.PostAsJsonAsync("/api/users", new
        {
            name = $"Workflow User {unique}",
            email = $"workflow-{unique}@example.com",
            role = "User"
        });
        Assert.Equal(HttpStatusCode.Created, userCreateResponse.StatusCode);
        var userId = await ReadIdAsync(userCreateResponse);

        var usersGetResponse = await _client.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.OK, usersGetResponse.StatusCode);

        var userGetResponse = await _client.GetAsync($"/api/users/{userId}");
        Assert.Equal(HttpStatusCode.OK, userGetResponse.StatusCode);

        var userPutResponse = await _client.PutAsJsonAsync($"/api/users/{userId}", new
        {
            name = $"Workflow User Updated {unique}",
            email = $"workflow-updated-{unique}@example.com",
            role = "User"
        });
        Assert.Equal(HttpStatusCode.NoContent, userPutResponse.StatusCode);

        // Projects
        var projectCreateResponse = await _client.PostAsJsonAsync("/api/projects", new
        {
            name = $"Workflow Project {unique}",
            description = "Integration workflow project"
        });
        Assert.Equal(HttpStatusCode.Created, projectCreateResponse.StatusCode);
        var projectId = await ReadIdAsync(projectCreateResponse);

        var projectsGetResponse = await _client.GetAsync("/api/projects");
        Assert.Equal(HttpStatusCode.OK, projectsGetResponse.StatusCode);

        var projectGetResponse = await _client.GetAsync($"/api/projects/{projectId}");
        Assert.Equal(HttpStatusCode.OK, projectGetResponse.StatusCode);

        var projectPutResponse = await _client.PutAsJsonAsync($"/api/projects/{projectId}", new
        {
            name = $"Workflow Project Updated {unique}",
            description = "Updated description"
        });
        Assert.Equal(HttpStatusCode.NoContent, projectPutResponse.StatusCode);

        // Tasks
        var task1CreateResponse = await _client.PostAsJsonAsync("/api/tasks", new
        {
            title = $"Workflow Task 1 {unique}",
            description = "Task 1",
            status = "Pending",
            priority = "High",
            dueDate = DateTime.UtcNow.AddDays(3),
            projectId
        });
        Assert.Equal(HttpStatusCode.Created, task1CreateResponse.StatusCode);
        var task1Id = await ReadIdAsync(task1CreateResponse);

        var task2CreateResponse = await _client.PostAsJsonAsync("/api/tasks", new
        {
            title = $"Workflow Task 2 {unique}",
            description = "Task 2",
            status = "InProgress",
            priority = "Medium",
            dueDate = DateTime.UtcNow.AddDays(5),
            projectId
        });
        Assert.Equal(HttpStatusCode.Created, task2CreateResponse.StatusCode);
        var task2Id = await ReadIdAsync(task2CreateResponse);

        var tasksGetResponse = await _client.GetAsync("/api/tasks");
        Assert.Equal(HttpStatusCode.OK, tasksGetResponse.StatusCode);

        var taskGetResponse = await _client.GetAsync($"/api/tasks/{task1Id}");
        Assert.Equal(HttpStatusCode.OK, taskGetResponse.StatusCode);

        var taskPutResponse = await _client.PutAsJsonAsync($"/api/tasks/{task1Id}", new
        {
            title = $"Workflow Task 1 Updated {unique}",
            description = "Task 1 updated",
            status = "Done",
            priority = "Critical",
            dueDate = DateTime.UtcNow.AddDays(10),
            projectId
        });
        Assert.Equal(HttpStatusCode.NoContent, taskPutResponse.StatusCode);

        var taskFilterResponse = await _client.GetAsync("/api/tasks/filter?status=Done&priority=Critical");
        Assert.Equal(HttpStatusCode.OK, taskFilterResponse.StatusCode);

        var assignViaTaskRouteResponse = await _client.PostAsync($"/api/tasks/{task1Id}/assign/{userId}", null);
        Assert.Equal(HttpStatusCode.OK, assignViaTaskRouteResponse.StatusCode);

        // Task assignments
        var assignCreateResponse = await _client.PostAsJsonAsync("/api/taskassignments", new
        {
            userId,
            taskItemId = task2Id
        });
        Assert.Equal(HttpStatusCode.Created, assignCreateResponse.StatusCode);

        var assignmentsGetResponse = await _client.GetAsync("/api/taskassignments");
        Assert.Equal(HttpStatusCode.OK, assignmentsGetResponse.StatusCode);

        var assignmentsByTaskResponse = await _client.GetAsync($"/api/taskassignments/task/{task2Id}");
        Assert.Equal(HttpStatusCode.OK, assignmentsByTaskResponse.StatusCode);

        var assignmentsByUserResponse = await _client.GetAsync($"/api/taskassignments/user/{userId}");
        Assert.Equal(HttpStatusCode.OK, assignmentsByUserResponse.StatusCode);

        var assignmentDeleteResponse = await _client.DeleteAsync($"/api/taskassignments/{userId}/{task2Id}");
        Assert.Equal(HttpStatusCode.NoContent, assignmentDeleteResponse.StatusCode);

        // Task dependencies
        var dependencyCreateResponse = await _client.PostAsJsonAsync("/api/taskdependencies", new
        {
            taskItemId = task2Id,
            dependsOnTaskId = task1Id
        });
        Assert.Equal(HttpStatusCode.Created, dependencyCreateResponse.StatusCode);

        var dependenciesGetResponse = await _client.GetAsync("/api/taskdependencies");
        Assert.Equal(HttpStatusCode.OK, dependenciesGetResponse.StatusCode);

        var dependencyGetResponse = await _client.GetAsync($"/api/taskdependencies/{task2Id}/{task1Id}");
        Assert.Equal(HttpStatusCode.OK, dependencyGetResponse.StatusCode);

        var dependencyDeleteResponse = await _client.DeleteAsync($"/api/taskdependencies/{task2Id}/{task1Id}");
        Assert.Equal(HttpStatusCode.NoContent, dependencyDeleteResponse.StatusCode);

        // Comments
        var commentCreateResponse = await _client.PostAsJsonAsync($"/api/tasks/{task1Id}/comments", new
        {
            userId,
            content = "Workflow comment"
        });
        Assert.Equal(HttpStatusCode.Created, commentCreateResponse.StatusCode);
        var commentId = await ReadIdAsync(commentCreateResponse);

        var commentsGetResponse = await _client.GetAsync($"/api/tasks/{task1Id}/comments");
        Assert.Equal(HttpStatusCode.OK, commentsGetResponse.StatusCode);

        var commentDeleteResponse = await _client.DeleteAsync($"/api/tasks/{task1Id}/comments/{commentId}");
        Assert.Equal(HttpStatusCode.NoContent, commentDeleteResponse.StatusCode);

        // Attachments
        var attachmentCreateResponse = await _client.PostAsJsonAsync($"/api/tasks/{task1Id}/attachments", new
        {
            fileName = "workflow-doc.txt",
            fileUrl = "https://example.com/workflow-doc.txt"
        });
        Assert.Equal(HttpStatusCode.Created, attachmentCreateResponse.StatusCode);
        var attachmentId = await ReadIdAsync(attachmentCreateResponse);

        var attachmentsGetResponse = await _client.GetAsync($"/api/tasks/{task1Id}/attachments");
        Assert.Equal(HttpStatusCode.OK, attachmentsGetResponse.StatusCode);

        var attachmentDeleteResponse = await _client.DeleteAsync($"/api/tasks/{task1Id}/attachments/{attachmentId}");
        Assert.Equal(HttpStatusCode.NoContent, attachmentDeleteResponse.StatusCode);

        // Cleanup deletions for CRUD coverage
        var task2DeleteResponse = await _client.DeleteAsync($"/api/tasks/{task2Id}");
        Assert.Equal(HttpStatusCode.NoContent, task2DeleteResponse.StatusCode);

        var task1DeleteResponse = await _client.DeleteAsync($"/api/tasks/{task1Id}");
        Assert.Equal(HttpStatusCode.NoContent, task1DeleteResponse.StatusCode);

        var projectDeleteResponse = await _client.DeleteAsync($"/api/projects/{projectId}");
        Assert.Equal(HttpStatusCode.NoContent, projectDeleteResponse.StatusCode);

        var userDeleteResponse = await _client.DeleteAsync($"/api/users/{userId}");
        Assert.Equal(HttpStatusCode.NoContent, userDeleteResponse.StatusCode);
    }

    private static async Task<int> ReadIdAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);
        return json.RootElement.GetProperty("id").GetInt32();
    }

    private async Task AuthenticateAsManagerAsync()
    {
        var token = await LoginAndGetTokenAsync("manager@taskmanager.local", "Manager@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task<string> LoginAndGetTokenAsync(string email, string password)
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var body = await loginResponse.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);
        return json.RootElement.GetProperty("accessToken").GetString() ?? string.Empty;
    }
}
