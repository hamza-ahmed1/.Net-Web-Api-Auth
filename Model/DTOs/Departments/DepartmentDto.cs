using System.ComponentModel.DataAnnotations;

namespace Auth.Model.DTOs.Departments
{
    public class DepartmentDto
    {
        public Guid DepartmentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int TeacherCount { get; set; }
        public int StudentCount { get; set; }
    }

    public class SaveDepartmentDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
