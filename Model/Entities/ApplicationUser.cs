using Microsoft.AspNetCore.Identity;

namespace Auth.Model.Entities
{
    public class ApplicationUser:IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        public string ResetCode { get; set; } = string.Empty;
        public DateTime? ResetCodeExpiry { get; set; }
        
        public ICollection<RefreshToken>? RefreshTokens { get; set; }
        public ICollection<Teacher>? Teachers { get; set; }
        public Student? Student { get; set; }
    }
}
