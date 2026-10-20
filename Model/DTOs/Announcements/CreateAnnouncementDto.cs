using Auth.Model.Entities;

namespace Auth.Model.DTOs.Announcements
{
    public class CreateAnnouncementDto
    {
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public Priority Priority { get; set; } = Priority.Normal;
        public AudienceType Audience { get; set; }
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// CourseId: required for Teacher role, triggers Teacher mode.
        /// If present for HOD, HOD acts in Teacher mode (students in courses they teach).
        /// If absent for HOD, HOD acts in HOD mode (all teachers/students in their department).
        /// </summary>
        public Guid? CourseId { get; set; }

        /// <summary>
        /// SectionId: optional for Teacher mode. If provided, limits recipients to students in that specific section.
        /// </summary>
        public Guid? SectionId { get; set; }
    }
}
