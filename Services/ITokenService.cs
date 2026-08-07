using Auth.Model.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Auth.Services
{
    // Services/ITokenService.cs
    public interface ITokenService
    {
        Task<string> GenerateAccessToken(ApplicationUser user, UserManager<ApplicationUser> userManager);
        string GenerateRefreshToken();
    }

    
}
