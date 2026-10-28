using System.ComponentModel.DataAnnotations;

namespace Auth.Model.Entities
{
    /// <summary>Academic department (e.g. Science) that teachers and students belong to.</summary>
    public class Department
    {
        [Key]
        public Guid DepartmentId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();

        public ICollection<Student> Students { get; set; } = new List<Student>();
    }
}
