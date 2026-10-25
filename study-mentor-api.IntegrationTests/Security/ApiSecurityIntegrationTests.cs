using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StudyMentorApi.Data;
using StudyMentorApi.Data.Models;

namespace StudyMentorApi.IntegrationTests.Security;

public class ApiSecurityIntegrationTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
    private Guid _userAId;
    private Guid _userBId;
    private Guid _adminId;
    private Guid _groupId;
    private Guid _lectureId;
    private Guid _sessionBId;
    private Guid _messageBId;
    private string _jwtKey = null!;
    private string _jwtIssuer = null!;
    private string _jwtAudience = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var dbName = "TestSecurityDb_" + Guid.NewGuid();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("Testing:DatabaseName", dbName);
        });

        _client = _factory.CreateClient();

        var config = _factory.Services.GetRequiredService<IConfiguration>();
        _jwtKey = config["Jwt:Campus:Key"] ?? "TestingDefaultSecretKeyMinimum32CharactersLong!";
        _jwtIssuer = config["Jwt:Campus:Issuer"] ?? "StudyMentorApi";
        _jwtAudience = config["Jwt:Campus:Audience"] ?? "StudyMentorClient";

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var group = new Group { Name = "Test Group" };
        db.Groups.Add(group);
        db.SaveChanges();
        _groupId = group.Id;

        var userA = new User
        {
            Name = "User A",
            Email = "usera@test.com",
            Password = "PasswordA123",
            GroupId = _groupId,
            Roles = ["User"]
        };
        db.Users.Add(userA);

        var userB = new User
        {
            Name = "User B",
            Email = "userb@test.com",
            Password = "PasswordB123",
            GroupId = _groupId,
            Roles = ["User"]
        };
        db.Users.Add(userB);

        var admin = new User
        {
            Name = "Admin User",
            Email = "admin@test.com",
            Password = "AdminPassword123",
            GroupId = _groupId,
            Roles = ["Admin"]
        };
        db.Users.Add(admin);

        var major = new Major { Name = "Computer Science" };
        db.Majors.Add(major);

        db.SaveChanges();

        _userAId = userA.Id;
        _userBId = userB.Id;
        _adminId = admin.Id;

        var subject = new Subject { Name = "Cybersecurity", MajorId = major.Id };
        db.Subjects.Add(subject);
        db.SaveChanges();

        var lecture = new Lecture { Name = "API Security", SubjectId = subject.Id };
        db.Lectures.Add(lecture);
        db.SaveChanges();
        _lectureId = lecture.Id;

        var sessionB = new ChatSession
        {
            UserId = _userBId,
            LectureId = _lectureId
        };
        db.ChatSessions.Add(sessionB);
        db.SaveChanges();
        _sessionBId = sessionB.Id;

        var messageB = new ChatMessage
        {
            ChatSessionId = _sessionBId,
            Content = "Secret message of User B",
            Role = MessageRole.User,
            SequenceNumber = 1,
            Status = "completed",
            Timestamp = DateTime.UtcNow
        };
        db.ChatMessages.Add(messageB);
        db.SaveChanges();
        _messageBId = messageB.Id;
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private string GenerateJwt(
        Guid userId,
        string[] roles,
        string name = "TestUser",
        string email = "test@test.com",
        TimeSpan? lifetime = null,
        string? signingKey = null)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(signingKey ?? _jwtKey);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, name),
            new(ClaimTypes.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var expires = lifetime.HasValue
            ? DateTime.UtcNow.Add(lifetime.Value)
            : DateTime.UtcNow.AddMinutes(60);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Audience = _jwtAudience,
            Issuer = _jwtIssuer,
            NotBefore = (lifetime.HasValue && lifetime.Value < TimeSpan.Zero ? expires.AddMinutes(-5) : DateTime.UtcNow.AddMinutes(-1)),
            Expires = expires,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private HttpClient CreateAuthenticatedClient(Guid userId, string[] roles)
    {
        var client = _factory.CreateClient();
        var token = GenerateJwt(userId, roles);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Test]
    public async Task AnonymousRequest_ToProtectedEndpoint_ShouldReturn401Unauthorized()
    {
        var response = await _client.GetAsync("/users");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));

        var chatResponse = await _client.GetAsync("/chat-sessions");
        Assert.That(chatResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));

        var aiResponse = await _client.GetAsync($"/api/ai-chat/messages?chatId={_sessionBId}");
        Assert.That(aiResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));

        var flashcardsResponse = await _client.GetAsync("/flashcards");
        Assert.That(flashcardsResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));

        var testsResponse = await _client.GetAsync("/tests");
        Assert.That(testsResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task RegularUser_GetAllUsers_ShouldReturn403Forbidden()
    {
        using var userClient = CreateAuthenticatedClient(_userAId, ["User"]);
        var response = await userClient.GetAsync("/users");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task RegularUser_GetOtherUserProfile_ShouldReturn403Forbidden()
    {
        using var userClient = CreateAuthenticatedClient(_userAId, ["User"]);
        var response = await userClient.GetAsync($"/users/{_userBId}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task RegularUser_UpdateOrDeleteOtherUserProfile_ShouldReturn403Forbidden()
    {
        using var userClient = CreateAuthenticatedClient(_userAId, ["User"]);

        var updateResponse = await userClient.PutAsJsonAsync($"/users/{_userBId}", new
        {
            name = "Hacked Name",
            password = "NewPassword",
            groupId = _groupId
        });
        Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        var deleteResponse = await userClient.DeleteAsync($"/users/{_userBId}");
        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task RegularUser_GetOrModifyOtherUserChatSession_ShouldReturn403Forbidden()
    {
        using var userClient = CreateAuthenticatedClient(_userAId, ["User"]);

        var getResponse = await userClient.GetAsync($"/chat-sessions/{_sessionBId}");
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        var getByUserResponse = await userClient.GetAsync($"/chat-sessions/user/{_userBId}");
        Assert.That(getByUserResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        var putResponse = await userClient.PutAsJsonAsync($"/chat-sessions/{_sessionBId}", new
        {
            userId = _userAId,
            lectureId = _lectureId
        });
        Assert.That(putResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        var deleteResponse = await userClient.DeleteAsync($"/chat-sessions/{_sessionBId}");
        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task RegularUser_AccessOrPostMessagesInOtherUserSession_ShouldReturn403Forbidden()
    {
        using var userClient = CreateAuthenticatedClient(_userAId, ["User"]);

        var getMessagesResponse = await userClient.GetAsync($"/chat-messages/session/{_sessionBId}");
        Assert.That(getMessagesResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        var getSingleMessageResponse = await userClient.GetAsync($"/chat-messages/{_messageBId}");
        Assert.That(getSingleMessageResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        var postMessageResponse = await userClient.PostAsJsonAsync("/chat-messages", new
        {
            chatSessionId = _sessionBId,
            content = "Injected unauthorized message",
            role = 0,
            sequenceNumber = 2
        });
        Assert.That(postMessageResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        var putMessageResponse = await userClient.PutAsJsonAsync($"/chat-messages/{_messageBId}", new
        {
            chatSessionId = _sessionBId,
            content = "Modified message content",
            role = 0,
            sequenceNumber = 1
        });
        Assert.That(putMessageResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        var deleteMessageResponse = await userClient.DeleteAsync($"/chat-messages/{_messageBId}");
        Assert.That(deleteMessageResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task RegularUser_AccessAiOrGenerateContentInOtherUserSession_ShouldReturn403Forbidden()
    {
        using var userClient = CreateAuthenticatedClient(_userAId, ["User"]);

        var aiGetMessagesResponse = await userClient.GetAsync($"/api/ai-chat/messages?chatId={_sessionBId}");
        Assert.That(aiGetMessagesResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        var aiSendMessageResponse = await userClient.PostAsJsonAsync("/api/ai-chat/messages", new
        {
            chatId = _sessionBId.ToString(),
            content = "Hello AI in another user's session"
        });
        Assert.That(aiSendMessageResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        var generateContentResponse = await userClient.PostAsJsonAsync("/api/learning-content/generate/chat", new
        {
            chatId = _sessionBId.ToString(),
            type = "flashcards",
            explicitGeneration = true
        });
        Assert.That(generateContentResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task AdminUser_CanAccessOtherUserChatSession_ShouldReturn200Ok()
    {
        using var adminClient = CreateAuthenticatedClient(_adminId, ["Admin"]);

        var getSessionResponse = await adminClient.GetAsync($"/chat-sessions/{_sessionBId}");
        Assert.That(getSessionResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var getMessagesResponse = await adminClient.GetAsync($"/chat-messages/session/{_sessionBId}");
        Assert.That(getMessagesResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var getAllUsersResponse = await adminClient.GetAsync("/users");
        Assert.That(getAllUsersResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task PublicRegistration_WithAdminRoleSupplied_ShouldIgnoreRolesAndAssignUserRole()
    {
        var newEmail = $"newstudent_{Guid.NewGuid():N}@test.com";
        var response = await _client.PostAsJsonAsync("/users", new
        {
            name = "Malicious Register",
            email = newEmail,
            password = "Password123",
            groupId = _groupId,
            roles = new[] { "Admin" }
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var createdUser = await db.Users.FirstOrDefaultAsync(u => u.Email == newEmail);

        Assert.That(createdUser, Is.Not.Null);
        Assert.That(createdUser!.Roles, Does.Not.Contain("Admin"));
        Assert.That(createdUser.Roles, Does.Contain("User"));
    }

    [Test]
    public async Task Owner_CanReadOwnProfileAndSession_ShouldReturn200Ok()
    {
        using var userClient = CreateAuthenticatedClient(_userBId, ["User"]);

        var profileResponse = await userClient.GetAsync($"/users/{_userBId}");
        Assert.That(profileResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var sessionResponse = await userClient.GetAsync($"/chat-sessions/{_sessionBId}");
        Assert.That(sessionResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var messagesResponse = await userClient.GetAsync($"/chat-messages/session/{_sessionBId}");
        Assert.That(messagesResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Admin_CreateChatSessionWithEmptyGuid_ShouldReturn400BadRequest()
    {
        using var adminClient = CreateAuthenticatedClient(_adminId, ["Admin"]);

        var response = await adminClient.PostAsJsonAsync("/chat-sessions", new
        {
            userId = Guid.Empty,
            lectureId = _lectureId
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task RegularUser_CreateChatSession_ShouldForceOwnerUserId()
    {
        using var userClient = CreateAuthenticatedClient(_userAId, ["User"]);

        var response = await userClient.PostAsJsonAsync("/chat-sessions", new
        {
            userId = _userBId,
            lectureId = _lectureId
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var sessionResult = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var assignedUserId = sessionResult.GetProperty("userId").GetString();

        Assert.That(assignedUserId, Is.EqualTo(_userAId.ToString()));
    }
    [Test]
    public async Task ExpiredToken_ShouldReturn401Unauthorized()
    {
        var expiredToken = GenerateJwt(_userAId, ["User"], lifetime: TimeSpan.FromMinutes(-10));
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", expiredToken);

        var response = await client.GetAsync("/users");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task InvalidSignatureToken_ShouldReturn401Unauthorized()
    {
        var forgedToken = GenerateJwt(_userAId, ["Admin"], signingKey: "TotallyInvalidForgedKeyForTestingPurposesOnly32Chars!");
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", forgedToken);

        var response = await client.GetAsync("/users");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task CreateUser_WithMissingEmail_ShouldReturn400BadRequest()
    {
        var response = await _client.PostAsJsonAsync("/users", new
        {
            name = "Incomplete User",
            email = "",
            password = "Password123",
            groupId = _groupId
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}