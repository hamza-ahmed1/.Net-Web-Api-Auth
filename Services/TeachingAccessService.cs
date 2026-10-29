using Auth.Data;
using Microsoft.EntityFrameworkCore;

namespace Auth.Services
{
    /// <summary>
    /// Ownership checks for teaching data: a teacher (or HOD acting as a teacher) may only manage exams,
    /// results and attendance for the course-section assignments they hold. Used by the controllers so the
    /// rules are enforced on the server, whatever the UI shows.
    /// </summary>
    public class TeachingAccessService
    {
        private readonly ApplicationDbContext _context;

        public TeachingAccessService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<bool> OwnsAssignmentAsync(string userId, Guid teacherSectionCourseId)
        {
            return _context.TeacherSectionCourses.AnyAsync(t =>
                t.TeacherSectionCourseId == teacherSectionCourseId && t.Teacher != null && t.Teacher.UserId == userId);
        }

        public Task<bool> OwnsExamAsync(string userId, Guid examId)
        {
            return _context.Exams.AnyAsync(e =>
                e.ExamID == examId && e.TeacherSectionCourse.Teacher != null && e.TeacherSectionCourse.Teacher.UserId == userId);
        }

        /// <summary>True when every exam exists and belongs to the user's assignments.</summary>
        public async Task<bool> OwnsAllExamsAsync(string userId, IEnumerable<Guid> examIds)
        {
            var ids = examIds.Distinct().ToList();
            if (ids.Count == 0) return true;
            var owned = await _context.Exams.CountAsync(e =>
                ids.Contains(e.ExamID) && e.TeacherSectionCourse.Teacher != null && e.TeacherSectionCourse.Teacher.UserId == userId);
            return owned == ids.Count;
        }

        public Task<bool> OwnsResultAsync(string userId, Guid examResultId)
        {
            return _context.ExamResults.AnyAsync(r =>
                r.ExamResultId == examResultId && r.Exam.TeacherSectionCourse.Teacher != null && r.Exam.TeacherSectionCourse.Teacher.UserId == userId);
        }

        public Task<bool> OwnsAttendanceAsync(string userId, Guid attendanceId)
        {
            return _context.Attendances.AnyAsync(a =>
                a.AttendanceId == attendanceId && a.TeacherSectionCourse.Teacher != null && a.TeacherSectionCourse.Teacher.UserId == userId);
        }

        /// <summary>The StudentId belonging to a signed-in student, or null for other users.</summary>
        public Task<Guid?> StudentIdForUserAsync(string userId)
        {
            return _context.Students.Where(s => s.UserId == userId).Select(s => (Guid?)s.StudentId).FirstOrDefaultAsync();
        }
    }
}
