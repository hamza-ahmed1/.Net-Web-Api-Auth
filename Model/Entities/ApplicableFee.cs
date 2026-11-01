using System.ComponentModel.DataAnnotations;

namespace Auth.Model.Entities
{
    public class ApplicableFee
    {
        [Key]
        public Guid AfId { get; set; }

        public Guid StudentId { get; set; }
        public Student? Student { get; set; }

        public Guid FeeTypeId { get; set; }
        public FeeType? FeeType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>Worked out from the student's invoices each time it is read, so it cannot go stale.</summary>
    public enum FeeStatus
    {
        Pending,
        Clear,
        Overdue
    }
}
