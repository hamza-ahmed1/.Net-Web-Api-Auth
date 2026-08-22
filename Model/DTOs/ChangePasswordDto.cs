using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Auth.Model.DTOs
{
    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required,StringLength(100,MinimumLength =8)]
        public string NewPasword { get; set; }

        [Required,Compare(nameof(NewPasword))]
        public string ConfirmPassword { get; set; }
    }
}
