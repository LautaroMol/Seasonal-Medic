using APISeasonalMedic.DTOs;
using System.Security.Claims;

namespace APISeasonalMedic.Services
{
    public interface IJwtService
    {
        JwtResult GenerateToken(Guid userId, string email, List<string> roles);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}