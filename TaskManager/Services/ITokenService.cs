using TaskManager.Models;

namespace TaskManager.Services;

public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateToken(User user);
}
