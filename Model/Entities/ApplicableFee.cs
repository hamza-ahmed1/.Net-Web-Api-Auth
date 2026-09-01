using System.ComponentModel.DataAnnotations;

namespace Auth.Model.Entities
{
    public class ApplicableFee
    {
        [Key]
        public Guid AfId { get; set; }

        public Guid StudentId { get; set; }
        public Student Student { get; set; }

        public Guid FeeTypeId { get; set; }
        public FeeType FeeType { get; set; }

        public FeeStatus Status { get; set; } = FeeStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum FeeStatus
    {
        Pending,
        Clear,
        Overdue
    }
}