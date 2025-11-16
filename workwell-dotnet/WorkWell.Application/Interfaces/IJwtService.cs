using System.Security.Claims;

namespace WorkWell.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(int userId, string email, string role);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
}

