using Auth.Model.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.Model.DTOs.Teachers
{
    public class TeacherDto
    {

        [Required]
        public string Email { get; set; } = string.Empty;
        [Required]
        [StringLength(15)]
        public string CNIC { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string IdentificationNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
