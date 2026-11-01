using System.Globalization;
using Auth.Data;
using Auth.Model.DTOs.Dashboard;
using Auth.Model.Entities;
using Auth.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Auth.Services
{
    /// <summary>Read-only figures for the role dashboards. Nothing here writes to the database.</summary>
    public class DashboardService : IDashboardService
    {
        private const int TrendWeeks = 12;
        private const int MaxExamsShown = 8;

        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardDto> GetAdminAsync()
        {
            var cohorts = await _context.Students.AsNoTracking()
                .Where(s => s.IsActive)
                .GroupBy(s => s.Cohort != null ? s.Cohort.Name : null)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToListAsync();

            return new AdminDashboardDto
            {
                TotalStudents = await _context.Students.CountAsync(),
                ActiveStudents = await _context.Students.CountAsync(s => s.IsActive),
                Teachers = await _context.Teachers.CountAsync(t => t.IsActive),
                Sections = await _context.Sections.CountAsync(s => s.IsActive),
                Courses = await _context.Courses.CountAsync(),
                EnrolmentByCohort = cohorts
                    .Select(c => new CountPoint(c.Key ?? "Unassigned", c.Count))
                    .OrderByDescending(c => c.Value)
                    .ToList(),
                Departments = await _context.Departments.AsNoTracking()
                    .Where(d => d.IsActive)
                    .OrderBy(d => d.Name)
                    .Select(d => new DepartmentStat(d.Name, d.Students.Count(s => s.IsActive), d.Teachers.Count(t => t.IsActive)))
                    .ToListAsync(),
                Attendance = await SummarizeAsync(_context.Attendances),
                Fees = await GetFeeOverviewAsync()
            };
        }

        public async Task<HodDashboardDto?> GetHodAsync(string userId)
        {
            var departmentId = await _context.Teachers
                .Where(t => t.UserId == userId)
                .Select(t => t.DepartmentId)
                .FirstOrDefaultAsync();
            if (departmentId == null)
                return null;

            var department = await _context.Departments.Where(d => d.DepartmentId == departmentId).Select(d => d.Name).FirstAsync();

            var sectionRows = await _context.StudentEnrollments.AsNoTracking()
                .Where(e => e.IsActive && e.Student!.IsActive && e.Student.DepartmentId == departmentId)
                .GroupBy(e => new { e.SectionId, e.EnrolledSection!.IntermediateClass, e.EnrolledSection.SectionName })
                .Select(g => new { g.Key.IntermediateClass, g.Key.SectionName, Count = g.Count() })
                .ToListAsync();

            var attendance = _context.Attendances.Where(a => a.StudentEnrollment.Student!.DepartmentId == departmentId);
            var scores = await LoadScoresAsync(_context.ExamResults.Where(r => r.Student.DepartmentId == departmentId));
            var teachers = await LoadTeacherPerformanceAsync(departmentId.Value);

            return new HodDashboardDto
            {
                Department = department,
                Students = await _context.Students.CountAsync(s => s.IsActive && s.DepartmentId == departmentId),
                Teachers = teachers.Count,
                Sections = sectionRows.Count,
                AttendanceRate = (await SummarizeAsync(attendance)).Rate,
                AverageScore = Average(scores),
                StudentsBySection = sectionRows
                    .Select(s => new CountPoint(SectionLabel(s.IntermediateClass, s.SectionName), s.Count))
                    .OrderBy(s => s.Label)
                    .ToList(),
                AttendanceTrend = await LoadTrendAsync(attendance),
                CoursePerformance = scores
                    .GroupBy(s => s.Course)
                    .Select(g => new ScorePoint(g.Key, Round(g.Average(x => x.Percent)), g.Count()))
                    .OrderByDescending(c => c.Average)
                    .ToList(),
                TeacherPerformance = teachers
            };
        }

        public async Task<TeacherDashboardDto?> GetTeacherAsync(string userId)
        {
            var teacherId = await _context.Teachers
                .Where(t => t.UserId == userId)
                .Select(t => (Guid?)t.Teacher_Id)
                .FirstOrDefaultAsync();
            if (teacherId == null)
                return null;

            var assignments = await _context.TeacherSectionCourses.AsNoTracking()
                .Where(t => t.TeacherId == teacherId && t.IsActive)
                .Select(t => new { t.TeacherSectionCourseId, t.SectionId })
                .ToListAsync();
            var assignmentIds = assignments.Select(a => a.TeacherSectionCourseId).ToList();
            var sectionIds = assignments.Select(a => a.SectionId).Distinct().ToList();

            var attendance = _context.Attendances.Where(a => assignmentIds.Contains(a.TeacherSectionCourseId));
            var scores = await LoadScoresAsync(_context.ExamResults.Where(r => assignmentIds.Contains(r.Exam.TeacherSectionCourseId)));

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var exams = await _context.Exams.AsNoTracking()
                .Where(e => assignmentIds.Contains(e.TeacherSectionCourseId))
                .Select(e => new
                {
                    e.ExamID,
                    e.ExamDate,
                    HasResults = _context.ExamResults.Any(r => r.ExamId == e.ExamID && (r.ObtainMarks != null || r.IsAbsent))
                })
                .ToListAsync();

            var bySectionRows = await attendance
                .GroupBy(a => new { a.TeacherSectionCourse.SectionId, a.TeacherSectionCourse.Section!.IntermediateClass, a.TeacherSectionCourse.Section.SectionName, a.Status })
                .Select(g => new { g.Key.SectionId, g.Key.IntermediateClass, g.Key.SectionName, g.Key.Status, Count = g.Count() })
                .ToListAsync();

            return new TeacherDashboardDto
            {
                Sections = sectionIds.Count,
                Students = await _context.StudentEnrollments
                    .Where(e => e.IsActive && sectionIds.Contains(e.SectionId))
                    .Select(e => e.StudentId)
                    .Distinct()
                    .CountAsync(),
                Exams = exams.Count,
                ExamsAwaitingResults = exams.Count(e => e.ExamDate <= today && !e.HasResults),
                AttendanceRate = (await SummarizeAsync(attendance)).Rate,
                AverageScore = Average(scores),
                AttendanceBySection = bySectionRows
                    .GroupBy(r => r.SectionId)
                    .Select(g =>
                    {
                        var summary = BuildSummary(g.Select(r => (r.Status, r.Count)));
                        summary.Label = SectionLabel(g.First().IntermediateClass, g.First().SectionName);
                        return summary;
                    })
                    .OrderBy(s => s.Label)
                    .ToList(),
                AttendanceTrend = await LoadTrendAsync(attendance),
                ExamPerformance = scores
                    .GroupBy(s => new { s.ExamId, s.Exam, s.ExamDate })
                    .OrderByDescending(g => g.Key.ExamDate)
                    .Take(MaxExamsShown)
                    .OrderBy(g => g.Key.ExamDate)
                    .Select(g => new ScorePoint(g.Key.Exam, Round(g.Average(x => x.Percent)), g.Count()))
                    .ToList(),
                SubjectResults = scores
                    .GroupBy(s => s.Course)
                    .Select(g => new SubjectResult(g.Key, Round(g.Average(x => x.Percent)), Round(g.Max(x => x.Percent)), Round(g.Min(x => x.Percent)), g.Count()))
                    .OrderByDescending(s => s.Average)
                    .ToList(),
                ScoreBands = new List<CountPoint>
                {
                    new("Under 40%", scores.Count(s => s.Percent < 40)),
                    new("40-59%", scores.Count(s => s.Percent >= 40 && s.Percent < 60)),
                    new("60-79%", scores.Count(s => s.Percent >= 60 && s.Percent < 80)),
                    new("80%+", scores.Count(s => s.Percent >= 80))
                }
            };
        }

        private async Task<FeeOverviewDto> GetFeeOverviewAsync()
        {
            var allInvoices = await _context.Invoices.AsNoTracking()
                .Where(i => !i.IsVoided)
                .Select(i => new { Amount = i.Amount + i.LateFee - i.Discount, i.AmountPaid, i.DueDate, i.Currency })
                .ToListAsync();
            var today = DateTime.UtcNow.Date;

            // Different currencies are never added together: the totals cover the most used one
            var currency = allInvoices.GroupBy(i => i.Currency).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault() ?? "PKR";
            var invoices = allInvoices.Where(i => i.Currency == currency).ToList();

            return new FeeOverviewDto
            {
                Currency = currency,
                OtherCurrencyInvoices = allInvoices.Count - invoices.Count,
                Invoices = invoices.Count,
                Billed = invoices.Sum(i => i.Amount),
                Collected = invoices.Sum(i => i.AmountPaid),
                Outstanding = invoices.Sum(i => Math.Max(i.Amount - i.AmountPaid, 0)),
                Overdue = invoices.Where(i => i.DueDate.Date < today).Sum(i => Math.Max(i.Amount - i.AmountPaid, 0)),
                Months = invoices
                    .GroupBy(i => new DateTime(i.DueDate.Year, i.DueDate.Month, 1))
                    .OrderByDescending(g => g.Key)
                    .Take(12)
                    .OrderBy(g => g.Key)
                    .Select(g => new FeeMonth(g.Key.ToString("MMM yyyy", CultureInfo.InvariantCulture), g.Sum(i => i.Amount), g.Sum(i => i.AmountPaid)))
                    .ToList()
            };
        }

        private async Task<List<TeacherPerformance>> LoadTeacherPerformanceAsync(Guid departmentId)
        {
            var teachers = await _context.Teachers.AsNoTracking()
                .Where(t => t.IsActive && t.DepartmentId == departmentId)
                .Select(t => new { t.Teacher_Id, t.User.FullName })
                .ToListAsync();
            var teacherIds = teachers.Select(t => t.Teacher_Id).ToList();

            var assignments = await _context.TeacherSectionCourses.AsNoTracking()
                .Where(t => t.IsActive && teacherIds.Contains(t.TeacherId))
                .Select(t => new { t.TeacherSectionCourseId, t.TeacherId })
                .ToListAsync();
            var assignmentIds = assignments.Select(a => a.TeacherSectionCourseId).ToList();

            var examCounts = await _context.Exams
                .Where(e => assignmentIds.Contains(e.TeacherSectionCourseId))
                .GroupBy(e => e.TeacherSectionCourseId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToListAsync();
            var attendance = await _context.Attendances
                .Where(a => assignmentIds.Contains(a.TeacherSectionCourseId))
                .GroupBy(a => new { a.TeacherSectionCourseId, a.Status })
                .Select(g => new { g.Key.TeacherSectionCourseId, g.Key.Status, Count = g.Count() })
                .ToListAsync();
            var scores = await LoadScoresAsync(_context.ExamResults.Where(r => assignmentIds.Contains(r.Exam.TeacherSectionCourseId)));

            return teachers
                .Select(teacher =>
                {
                    var own = assignments.Where(a => a.TeacherId == teacher.Teacher_Id).Select(a => a.TeacherSectionCourseId).ToHashSet();
                    var ownScores = scores.Where(s => own.Contains(s.AssignmentId)).ToList();
                    var ownAttendance = BuildSummary(attendance.Where(a => own.Contains(a.TeacherSectionCourseId)).Select(a => (a.Status, a.Count)));
                    return new TeacherPerformance(
                        teacher.FullName,
                        examCounts.Where(e => own.Contains(e.Key)).Sum(e => e.Count),
                        ownScores.Count,
                        Average(ownScores),
                        ownAttendance.Rate);
                })
                .OrderByDescending(t => t.AverageScore.HasValue)
                .ThenByDescending(t => t.AverageScore)
                .ThenBy(t => t.Name)
                .ToList();
        }

        private static Task<List<ScoreRow>> LoadScoresAsync(IQueryable<ExamResult> results) =>
            results.AsNoTracking()
                .Where(r => !r.IsAbsent && r.ObtainMarks != null && r.Exam.TotalMarks > 0)
                .Select(r => new ScoreRow(
                    r.ExamId,
                    r.Exam.Title,
                    r.Exam.ExamDate,
                    r.Exam.TeacherSectionCourseId,
                    r.Exam.TeacherSectionCourse.Course!.CourseName,
                    r.ObtainMarks!.Value * 100m / r.Exam.TotalMarks))
                .ToListAsync();

        private static async Task<AttendanceSummaryDto> SummarizeAsync(IQueryable<Attendance> attendance)
        {
            var rows = await attendance
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();
            return BuildSummary(rows.Select(r => (r.Status, r.Count)));
        }

        private static AttendanceSummaryDto BuildSummary(IEnumerable<(AttendanceStatus Status, int Count)> rows)
        {
            var list = rows.ToList();
            int Count(AttendanceStatus status) => list.Where(r => r.Status == status).Sum(r => r.Count);

            var summary = new AttendanceSummaryDto
            {
                Present = Count(AttendanceStatus.Present),
                Absent = Count(AttendanceStatus.Absent),
                Late = Count(AttendanceStatus.Late),
                Leave = Count(AttendanceStatus.Leave),
                Total = list.Sum(r => r.Count)
            };
            summary.Rate = AttendanceRate(summary.Present + summary.Late, summary.Total);
            return summary;
        }

        private static async Task<List<AttendanceWeek>> LoadTrendAsync(IQueryable<Attendance> attendance)
        {
            var from = StartOfWeek(DateTime.UtcNow.Date).AddDays(-7 * (TrendWeeks - 1));
            var rows = await attendance
                .Where(a => a.AttendanceDate >= from)
                .Select(a => new { a.AttendanceDate, a.Status })
                .ToListAsync();

            return rows
                .GroupBy(r => StartOfWeek(r.AttendanceDate.Date))
                .OrderBy(g => g.Key)
                .Select(g => new AttendanceWeek(
                    g.Key.ToString("d MMM", CultureInfo.InvariantCulture),
                    AttendanceRate(g.Count(r => r.Status is AttendanceStatus.Present or AttendanceStatus.Late), g.Count()) ?? 0,
                    g.Count()))
                .ToList();
        }

        private static DateTime StartOfWeek(DateTime date) => date.AddDays(-(((int)date.DayOfWeek + 6) % 7));

        private static decimal? AttendanceRate(int attended, int total) =>
            total == 0 ? null : Round(attended * 100m / total);

        private static decimal? Average(IReadOnlyCollection<ScoreRow> scores) =>
            scores.Count == 0 ? null : Round(scores.Average(s => s.Percent));

        private static decimal Round(decimal value) => Math.Round(value, 1, MidpointRounding.AwayFromZero);

        private static string SectionLabel(string level, string name) =>
            string.IsNullOrWhiteSpace(level) || name.StartsWith($"{level} ") ? name : $"{level} - {name}";

        private sealed record ScoreRow(Guid ExamId, string Exam, DateOnly ExamDate, Guid AssignmentId, string Course, decimal Percent);
    }
}
