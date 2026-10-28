using Auth.Model.DTOs.AuthApi.Models.Dtos;

namespace Auth.Model.DTOs
{
    public class AuthResponseDto
    {
        public string AccessToken { get; set; }
        public DateTime AccessTokenExpiresAt { get; set; }
        public UserDto User { get; set; }

        /// <summary>
        /// Only sent to clients that cannot hold the httpOnly cookie (mobile apps, via the X-Client-Type: mobile header).
        /// Browsers keep using the cookie, so the token never reaches page scripts.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public string? RefreshToken { get; set; }

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? RefreshTokenExpiresAt { get; set; }
    }
}
