namespace Auth.Model.DTOs
{
    namespace AuthApi.Models.Dtos
    {
        // Safe user info to expose to client — never expose PasswordHash etc.
        public class UserDto
        {
            public string Id { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
            public List<string> Roles { get; set; }
        }
    }
}
