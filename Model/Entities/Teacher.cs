using Auth.Model.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Teacher
{
    [Key]
    public Guid Teacher_Id { get; set; }

    [Required]
    [StringLength(15)]
    public string CNIC { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Qualification { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string IdentificationNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Department { get; set; } = string.Empty;

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required]
    public DateTime HireDate { get; set; }

    [Required]
    [StringLength(200)]
    public string Address { get; set; } = string.Empty;

    [Required]
    public decimal Salary { get; set; }

    public bool IsActive { get; set; } = true;


    // Foreign Key to Identity User
    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser User { get; set; } = null!;


    //    // Navigation properties
    public ICollection<TeacherSectionCourse> TeacherAssignments { get; set; }
        = new List<TeacherSectionCourse>();
}