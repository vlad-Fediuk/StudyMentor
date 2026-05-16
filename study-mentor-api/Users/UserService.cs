using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using StudyMentorApi.Data.Models;
using StudyMentorApi.Services;

namespace StudyMentorApi.Users;

public class UserService(MongoDbService dbService, IConfiguration configuration)
    : BaseCrudService<User, string>
{
    private const string CollectionName = "users";

    private readonly IMongoCollection<User> _collection =
        dbService.GetCollection<User>(CollectionName);

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var user = await _collection
            .Find(u => u.Email == request.Email.ToLower())
            .FirstOrDefaultAsync(ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Невірний email або пароль.");

        var (token, expiresAt) = GenerateToken(user);
        return new LoginResponse(token, expiresAt, user.Id, user.Name, user.Email, user.GroupId);
    }

    public User MapFromRequest(UserRequest request) => new()
    {
        Name         = request.Name,
        Email        = request.Email.ToLower(),
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
        GroupId      = request.GroupId
    };

    public async Task<User?> GetByGroupAsync(string groupId, CancellationToken ct)
        => await _collection
            .Find(u => u.GroupId == groupId)
            .FirstOrDefaultAsync(ct);

    protected override IQueryable<User> Query()
        => _collection.AsQueryable();

    protected override async Task<User?> FindByIdAsync(string id, CancellationToken ct)
        => await _collection
            .Find(u => u.Id == id)
            .FirstOrDefaultAsync(ct);

    protected override async Task<User> AddEntityAsync(User entity, CancellationToken ct)
    {
        await _collection.InsertOneAsync(entity, cancellationToken: ct);
        return entity;
    }

    protected override async Task<User> SaveUpdatedEntityAsync(User entity, CancellationToken ct)
    {
        await _collection.ReplaceOneAsync(
            u => u.Id == entity.Id,
            entity,
            cancellationToken: ct);
        return entity;
    }

    protected override async Task DeleteEntityAsync(User entity, CancellationToken ct)
        => await _collection.DeleteOneAsync(u => u.Id == entity.Id, ct);

    protected override void UpdateEntityValues(User existing, User updated)
    {
        existing.Name         = updated.Name;
        existing.Email        = updated.Email;
        existing.PasswordHash = updated.PasswordHash;
        existing.GroupId      = updated.GroupId;
    }

    private (string Token, DateTime ExpiresAt) GenerateToken(User user)
    {
        var jwt       = configuration.GetSection("JwtSettings");
        var key       = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["SecretKey"]!));
        var expiresAt = DateTime.UtcNow.AddHours(int.Parse(jwt["ExpiresInHours"] ?? "24"));

        var token = new JwtSecurityToken(
            issuer:   jwt["Issuer"],
            audience: jwt["Audience"],
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub,   user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("name",                        user.Name),
                new Claim("group_id",                    user.GroupId),
                new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
            ],
            expires:            expiresAt,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}