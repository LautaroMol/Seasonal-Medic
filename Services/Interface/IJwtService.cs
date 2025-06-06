using APISeasonalMedic.DTOs;
using System.Security.Claims;

namespace APISeasonalMedic.Services.Interface
{
    public interface IJwtService
    {
        string GenerateRefreshToken();
        JwtResult GenerateToken(Guid userId, string email, List<string> roles, bool emailConfirmed = false);
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}