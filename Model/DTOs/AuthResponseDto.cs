using Auth.Model.DTOs.AuthApi.Models.Dtos;

namespace Auth.Model.DTOs
{
    public class AuthResponseDto
    {
        public string AccessToken { get; set; }
        public DateTime AccessTokenExpiresAt { get; set; }
        public UserDto User { get; set; }
    }
}
