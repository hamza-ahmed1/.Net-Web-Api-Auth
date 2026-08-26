using Auth.Data;
using Auth.Model.DTOs.Exam;
using Auth.Model.Entities;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auth.Services
{
    public class ExamService:IExamService
    {

        private readonly ApplicationDbContext _context;

        public ExamService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Create Exam:
        public async Task<IActionResult> CreateExam(CreateExamDto examDto)
        {
            // check validity of teacher
            var tscExists = await _context.TeacherSectionCourses.FirstOrDefaultAsync(t => t.TeacherSectionCourseId == examDto.TeacherSectionCourseId);
            if (tscExists==null)
            {
                return new BadRequestObjectResult($"TeacherSectionCourse with id {examDto.TeacherSectionCourseId} does not exist.");
            }

            var exam = new Exam
            {
                ExamTypeId = examDto.ExamTypeId,
                IsPublished = examDto.IsPublished,
                TeacherSectionCourseId = examDto.TeacherSectionCourseId,
                Title = examDto.Title,
                TotalMarks = examDto.TotalMarks,
                ExamDate = examDto.ExamDate,
            };
                await _context.Exams.AddAsync(exam);
                await _context.SaveChangesAsync();
                return new OkObjectResult(new { examId = exam.ExamID });
        
        }

        public async Task<IActionResult> UpdateExam(Guid examId, CreateExamDto examDto)
        {
            var exam = await _context.Exams.FindAsync(examId);
            if (exam == null)
            {
                return new NotFoundObjectResult($"Exam with id {examId} does not exist.");
            }
            // check validity of teacher
            var tscExists = await _context.TeacherSectionCourses.FirstOrDefaultAsync(t => t.TeacherSectionCourseId == examDto.TeacherSectionCourseId);
            if (tscExists == null)
            {
                return new BadRequestObjectResult($"TeacherSectionCourse with id {examDto.TeacherSectionCourseId} does not exist.");
            }
            exam.ExamTypeId = examDto.ExamTypeId;
            exam.IsPublished = examDto.IsPublished;
            exam.TeacherSectionCourseId = examDto.TeacherSectionCourseId;
            exam.Title = examDto.Title;
            exam.TotalMarks = examDto.TotalMarks;
            exam.ExamDate = examDto.ExamDate;
            _context.Exams.Update(exam);
            await _context.SaveChangesAsync();
            return new OkObjectResult(new { message = "Exam updated successfully." });
        }

        public async Task<IActionResult> DeleteExam(Guid examId)
        {
            var exam = await _context.Exams.FindAsync(examId);
            if (exam == null)
            {
                return new NotFoundObjectResult($"Exam with id {examId} does not exist.");
            }
            _context.Exams.Remove(exam);
            await _context.SaveChangesAsync();
            return new OkObjectResult(new { message = "Exam deleted successfully." });
        }

        // this api for getting all exams by teacher wise
        public async Task<IActionResult> GetAllExamByTeacherSection(Guid id)
        {
            var exams = await _context.Exams.Where(e => e.TeacherSectionCourseId == id).ToListAsync();
            return new OkObjectResult(new { exams = exams });
        }

        public async Task<IActionResult> GetExamByTeacherId(Guid id)
        {
            var exams = await _context.Exams.Where(e => e.TeacherSectionCourse.TeacherId == id).ToListAsync();
            return new OkObjectResult(new { exams = exams });
        }


    }
}
