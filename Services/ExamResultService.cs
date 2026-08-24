using Auth.Data;
using Auth.Model.DTOs.ExamResult;
using Auth.Model.Entities;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auth.Services
{
    public class ExamResultService:IExamResultService
    {
        private readonly ApplicationDbContext _context;
        public ExamResultService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> CreateExamResult(ExamResultCreateDto dto)
        {
            // check validity of exam
            var examExists = await _context.Exams.FirstOrDefaultAsync(e => e.ExamID == dto.ExamId);
            if (examExists == null)
            {
                return new BadRequestObjectResult($"Exam with id {dto.ExamId} does not exist.");
            }
            // check validity of student
            var studentExists = await _context.Students.FirstOrDefaultAsync(s => s.StudentId == dto.StudentId);
            if (studentExists == null)
            {
                return new BadRequestObjectResult($"Student with id {dto.StudentId} does not exist.");
            }
            var examResult = new ExamResult
            {
                ExamId = dto.ExamId,
                StudentId = dto.StudentId,
                ObtainMarks = dto.ObtainMarks,
                IsAbsent = dto.IsAbsent,
                Remarks = dto.Remarks
            };
            await _context.ExamResults.AddAsync(examResult);
            await _context.SaveChangesAsync();
            return new OkObjectResult(new { examResultId = examResult.ExamResultId });
        }

        public async Task DeleteExamResultById(Guid examResultId)
        {
            var examResult = await _context.ExamResults.FirstOrDefaultAsync(er => er.ExamResultId == examResultId);
            if (examResult == null)
            {
                throw new InvalidOperationException($"Exam result with id {examResultId} does not exist.");
            }
            _context.ExamResults.Remove(examResult);
            await _context.SaveChangesAsync();
        }

        public async Task<IActionResult> GetExamResultById(Guid examResultId)
        {
            var examResult = await _context.ExamResults
                .Include(er => er.Exam)
                .Include(er => er.Student)
                .FirstOrDefaultAsync(er => er.ExamResultId == examResultId);
            if (examResult == null)
            {
                return new NotFoundObjectResult($"Exam result with id {examResultId} does not exist.");
            }
            var resultDto = new ExamResultDto
            {
                ExamResultId = examResult.ExamResultId,
                ExamId = examResult.ExamId,
                StudentId = examResult.StudentId,
                ObtainMarks = examResult.ObtainMarks,
                IsAbsent = examResult.IsAbsent,
                Remarks = examResult.Remarks,
                ExamTitle = examResult.Exam.Title,
                StudentName = examResult.Student.User.FullName
            };
            return new OkObjectResult(resultDto);
        }

        public async Task<IActionResult> GetExamsByStudentId(Guid studentId)
        {
            var examResults = await _context.ExamResults
                .Where(er => er.StudentId == studentId)
                .Include(er => er.Exam)
                .ToListAsync();

            return new OkObjectResult(examResults);
        }
        public async Task<IActionResult> UpdateExamResult(Guid examResultId, ExamResultCreateDto dto)
        {
            var examResult = await _context.ExamResults.FirstOrDefaultAsync(er => er.ExamResultId == examResultId);
            if (examResult == null)
            {
                return new NotFoundObjectResult($"Exam result with id {examResultId} does not exist.");
            }
            // check validity of exam
            var examExists = await _context.Exams.FirstOrDefaultAsync(e => e.ExamID == dto.ExamId);
            if (examExists == null)
            {
                return new BadRequestObjectResult($"Exam with id {dto.ExamId} does not exist.");
            }
            // check validity of student
            var studentExists = await _context.Students.FirstOrDefaultAsync(s => s.StudentId == dto.StudentId);
            if (studentExists == null)
            {
                return new BadRequestObjectResult($"Student with id {dto.StudentId} does not exist.");
            }
            examResult.ExamId = dto.ExamId;
            examResult.StudentId = dto.StudentId;
            examResult.ObtainMarks = dto.ObtainMarks;
            examResult.IsAbsent = dto.IsAbsent;
            examResult.Remarks = dto.Remarks;
            _context.ExamResults.Update(examResult);
            await _context.SaveChangesAsync();
            return new OkObjectResult(new { message = "Exam result updated successfully." });
        }
    }
}
