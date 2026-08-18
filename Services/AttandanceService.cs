using Auth.Data;
using Auth.Model.DTOs.Attendance;
using Auth.Model.Entities;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auth.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly ApplicationDbContext _context;

        public AttendanceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> MarkBulkAttendance(BulkMarkAttendanceDto dto, Guid? markedByTeacherId)
        {
            if (dto.Entries == null || dto.Entries.Count == 0)
                return new BadRequestObjectResult("No attendance entries provided.");

            var tsc = await _context.TeacherSectionCourses
                .FirstOrDefaultAsync(t => t.TeacherSectionCourseId == dto.TeacherSectionCourseId && t.IsActive);

            if (tsc == null)
                return new BadRequestObjectResult("Teacher-section-course assignment not found or inactive.");

            var date = dto.AttendanceDate.Date;
            var result = new BulkAttendanceResultDto
            {
                TeacherSectionCourseId = dto.TeacherSectionCourseId,
                AttendanceDate = date
            };

            var enrollmentIds = dto.Entries.Select(e => e.StudentEnrollmentId).ToList();

            var validEnrollments = await _context.StudentEnrollments
                .Where(se => enrollmentIds.Contains(se.StudentEnrollmentId)
                             && se.SectionId == tsc.SectionId
                             && se.IsActive)
                .Select(se => se.StudentEnrollmentId)
                .ToListAsync();

            var existingRecords = await _context.Attendances
                .Where(a => a.TeacherSectionCourseId == dto.TeacherSectionCourseId
                            && a.AttendanceDate == date
                            && enrollmentIds.Contains(a.StudentEnrollmentId))
                .ToListAsync();

            foreach (var entry in dto.Entries)
            {
                if (!validEnrollments.Contains(entry.StudentEnrollmentId))
                {
                    result.RecordsSkipped++;
                    result.Errors.Add($"Enrollment {entry.StudentEnrollmentId} is not an active student of this section.");
                    continue;
                }

                var existing = existingRecords.FirstOrDefault(a => a.StudentEnrollmentId == entry.StudentEnrollmentId);
                if (existing != null)
                {
                    existing.Status = entry.Status;
                    existing.Remarks = entry.Remarks;
                    existing.MarkedByTeacherId = markedByTeacherId;
                    existing.UpdatedAt = DateTime.UtcNow;
                    result.RecordsUpdated++;
                }
                else
                {
                    _context.Attendances.Add(new Attendance
                    {
                        AttendanceId = Guid.NewGuid(),
                        StudentEnrollmentId = entry.StudentEnrollmentId,
                        TeacherSectionCourseId = dto.TeacherSectionCourseId,
                        AttendanceDate = date,
                        Status = entry.Status,
                        Remarks = entry.Remarks,
                        MarkedByTeacherId = markedByTeacherId
                    });
                    result.RecordsCreated++;
                }
            }

            await _context.SaveChangesAsync();
            return new OkObjectResult(result);
        }

        public async Task<IActionResult> MarkSingleAttendance(MarkAttendanceDto dto, Guid? markedByTeacherId)
        {
            var tsc = await _context.TeacherSectionCourses
                .FirstOrDefaultAsync(t => t.TeacherSectionCourseId == dto.TeacherSectionCourseId && t.IsActive);
            if (tsc == null)
                return new BadRequestObjectResult("Teacher-section-course assignment not found or inactive.");

            var enrollment = await _context.StudentEnrollments
                .FirstOrDefaultAsync(se => se.StudentEnrollmentId == dto.StudentEnrollmentId
                                            && se.SectionId == tsc.SectionId
                                            && se.IsActive);
            if (enrollment == null)
                return new BadRequestObjectResult("Student is not actively enrolled in this section.");

            var date = dto.AttendanceDate.Date;

            var existing = await _context.Attendances
                .FirstOrDefaultAsync(a => a.StudentEnrollmentId == dto.StudentEnrollmentId
                                           && a.TeacherSectionCourseId == dto.TeacherSectionCourseId
                                           && a.AttendanceDate == date);

            if (existing != null)
            {
                existing.Status = dto.Status;
                existing.Remarks = dto.Remarks;
                existing.MarkedByTeacherId = markedByTeacherId;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.Attendances.Add(new Attendance
                {
                    AttendanceId = Guid.NewGuid(),
                    StudentEnrollmentId = dto.StudentEnrollmentId,
                    TeacherSectionCourseId = dto.TeacherSectionCourseId,
                    AttendanceDate = date,
                    Status = dto.Status,
                    Remarks = dto.Remarks,
                    MarkedByTeacherId = markedByTeacherId
                });
            }

            await _context.SaveChangesAsync();
            return new OkResult();
        }

        public async Task<AttendanceDto?> GetAttendanceById(Guid attendanceId)
        {
            var a = await _context.Attendances
                .Include(x => x.StudentEnrollment).ThenInclude(se => se.Student).ThenInclude(s => s!.User)
                .Include(x => x.TeacherSectionCourse).ThenInclude(t => t.Section)
                .Include(x => x.TeacherSectionCourse).ThenInclude(t => t.Course)
                .FirstOrDefaultAsync(x => x.AttendanceId == attendanceId);

            return a == null ? null : MapToDto(a);
        }

        public async Task<List<AttendanceDto>> GetCourseAttendanceByDate(Guid teacherSectionCourseId, DateTime date)
        {
            var records = await _context.Attendances
                .Include(x => x.StudentEnrollment).ThenInclude(se => se.Student).ThenInclude(s => s!.User)
                .Include(x => x.TeacherSectionCourse).ThenInclude(t => t.Section)
                .Include(x => x.TeacherSectionCourse).ThenInclude(t => t.Course)
                .Where(x => x.TeacherSectionCourseId == teacherSectionCourseId && x.AttendanceDate == date.Date)
                .ToListAsync();

            return records.Select(MapToDto).ToList();
        }

        public async Task<List<AttendanceDto>> GetStudentAttendanceHistory(Guid studentId, Guid? courseId, DateTime? from, DateTime? to)
        {
            var query = _context.Attendances
                .Include(x => x.StudentEnrollment).ThenInclude(se => se.Student).ThenInclude(s => s!.User)
                .Include(x => x.TeacherSectionCourse).ThenInclude(t => t.Section)
                .Include(x => x.TeacherSectionCourse).ThenInclude(t => t.Course)
                .Where(x => x.StudentEnrollment.StudentId == studentId);

            if (courseId.HasValue)
                query = query.Where(x => x.TeacherSectionCourse.CourseId == courseId.Value);
            if (from.HasValue)
                query = query.Where(x => x.AttendanceDate >= from.Value.Date);
            if (to.HasValue)
                query = query.Where(x => x.AttendanceDate <= to.Value.Date);

            var records = await query.OrderByDescending(x => x.AttendanceDate).ToListAsync();
            return records.Select(MapToDto).ToList();
        }

        public async Task<IActionResult> UpdateAttendance(Guid attendanceId, UpdateAttendanceDto dto, Guid? markedByTeacherId)
        {
            var attendance = await _context.Attendances.FindAsync(attendanceId);
            if (attendance == null)
                return new NotFoundObjectResult("Attendance record not found.");

            attendance.Status = dto.Status;
            attendance.Remarks = dto.Remarks;
            attendance.MarkedByTeacherId = markedByTeacherId;
            attendance.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return new OkResult();
        }

        public async Task<IActionResult> DeleteAttendance(Guid attendanceId)
        {
            var attendance = await _context.Attendances.FindAsync(attendanceId);
            if (attendance == null)
                return new NotFoundResult();

            _context.Attendances.Remove(attendance);
            await _context.SaveChangesAsync();
            return new OkResult();
        }

        public async Task<List<AttendanceSummaryDto>> GetCourseSummary(Guid teacherSectionCourseId, DateTime? from, DateTime? to)
        {
            var query = _context.Attendances
                .Include(x => x.StudentEnrollment).ThenInclude(se => se.Student).ThenInclude(s => s!.User)
                .Where(x => x.TeacherSectionCourseId == teacherSectionCourseId);

            if (from.HasValue) query = query.Where(x => x.AttendanceDate >= from.Value.Date);
            if (to.HasValue) query = query.Where(x => x.AttendanceDate <= to.Value.Date);

            var records = await query.ToListAsync();

            return records
                .GroupBy(a => new
                {
                    StudentId = a.StudentEnrollment.StudentId,
                    FullName = a.StudentEnrollment.Student!.User!.FullName
                })
                .Select(g =>
                {
                    var total = g.Count();
                    var present = g.Count(x => x.Status == AttendanceStatus.Present);
                    var absent = g.Count(x => x.Status == AttendanceStatus.Absent);
                    var late = g.Count(x => x.Status == AttendanceStatus.Late);
                    var leave = g.Count(x => x.Status == AttendanceStatus.Leave);

                    return new AttendanceSummaryDto
                    {
                        StudentId = g.Key.StudentId,
                        StudentFullName = g.Key.FullName,
                        TotalClasses = total,
                        PresentCount = present,
                        AbsentCount = absent,
                        LateCount = late,
                        LeaveCount = leave,
                        AttendancePercentage = total == 0 ? 0 : Math.Round((present + late) * 100.0 / total, 2)
                    };
                })
                .ToList();
        }

        public async Task<List<StudentCourseDto>> GetActiveCoursesForStudent(Guid studentId)
        {
            var enrollment = await _context.StudentEnrollments
                .FirstOrDefaultAsync(se => se.StudentId == studentId && se.IsActive);

            if (enrollment == null)
                return new List<StudentCourseDto>();

            return await _context.TeacherSectionCourses
                .Where(t => t.SectionId == enrollment.SectionId && t.IsActive)
                .Include(t => t.Course)
                .Include(t => t.Section)
                .Select(t => new StudentCourseDto
                {
                    TeacherSectionCourseId = t.TeacherSectionCourseId,
                    CourseId = t.CourseId,
                    CourseName = t.Course!.CourseName,
                    TeacherId = t.TeacherId,
                    SectionId = t.SectionId,
                    SectionName = t.Section!.SectionName
                })
                .ToListAsync();
        }

        private static AttendanceDto MapToDto(Attendance a)
        {
            return new AttendanceDto
            {
                AttendanceId = a.AttendanceId,
                StudentEnrollmentId = a.StudentEnrollmentId,
                StudentId = a.StudentEnrollment?.StudentId ?? Guid.Empty,
                StudentFullName = a.StudentEnrollment?.Student?.User?.FullName,
                TeacherSectionCourseId = a.TeacherSectionCourseId,
                SectionId = a.TeacherSectionCourse?.SectionId ?? Guid.Empty,
                SectionName = a.TeacherSectionCourse?.Section?.SectionName,
                CourseId = a.TeacherSectionCourse?.CourseId ?? Guid.Empty,
                CourseName = a.TeacherSectionCourse?.Course?.CourseName,
                TeacherId = a.TeacherSectionCourse?.TeacherId ?? Guid.Empty,
                MarkedByTeacherId = a.MarkedByTeacherId,
                AttendanceDate = a.AttendanceDate,
                Status = a.Status,
                Remarks = a.Remarks
            };
        }
    }
}