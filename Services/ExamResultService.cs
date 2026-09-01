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
        public async Task<IActionResult> UploadBulkResult(List<ExamResultCreateDto> dtos)
        {
            // Check if request is empty
            if (dtos == null || !dtos.Any())
            {
                return new BadRequestObjectResult("No exam results were provided.");
            }

            // Get unique Exam and Student IDs
            var examIds = dtos
                .Select(d => d.ExamId)
                .Distinct()
                .ToList();

            var studentIds = dtos
                .Select(d => d.StudentId)
                .Distinct()
                .ToList();

            // Get existing exams and students
            var exams = await _context.Exams
                .Where(e => examIds.Contains(e.ExamID))
                .ToListAsync();

            var students = await _context.Students
                .Where(s => studentIds.Contains(s.StudentId))
                .ToListAsync();

            // Check invalid exams
            var existingExamIds = exams
                .Select(e => e.ExamID)
                .ToHashSet();

            var invalidExamIds = examIds
                .Where(id => !existingExamIds.Contains(id))
                .ToList();

            if (invalidExamIds.Any())
            {
                return new BadRequestObjectResult(new
                {
                    message = "Some exams do not exist.",
                    invalidExamIds
                });
            }

            // Check invalid students
            var existingStudentIds = students
                .Select(s => s.StudentId)
                .ToHashSet();

            var invalidStudentIds = studentIds
                .Where(id => !existingStudentIds.Contains(id))
                .ToList();

            if (invalidStudentIds.Any())
            {
                return new BadRequestObjectResult(new
                {
                    message = "Some students do not exist.",
                    invalidStudentIds
                });
            }

            // Check duplicate results inside the uploaded list
            var duplicateResults = dtos
                .GroupBy(d => new { d.ExamId, d.StudentId })
                .Where(g => g.Count() > 1)
                .Select(g => new
                {
                    g.Key.ExamId,
                    g.Key.StudentId
                })
                .ToList();

            if (duplicateResults.Any())
            {
                return new BadRequestObjectResult(new
                {
                    message = "Duplicate student results found in the uploaded data.",
                    duplicates = duplicateResults
                });
            }

            // Check if results already exist in database
            var existingResults = await _context.ExamResults
                .Where(r =>
                    examIds.Contains(r.ExamId) &&
                    studentIds.Contains(r.StudentId))
                .Select(r => new
                {
                    r.ExamId,
                    r.StudentId
                })
                .ToListAsync();

            var existingResultSet = existingResults
                .Select(r => $"{r.ExamId}-{r.StudentId}")
                .ToHashSet();

            var alreadyExistingResults = dtos
                .Where(d => existingResultSet.Contains($"{d.ExamId}-{d.StudentId}"))
                .Select(d => new
                {
                    d.ExamId,
                    d.StudentId
                })
                .ToList();

            if (alreadyExistingResults.Any())
            {
                return new BadRequestObjectResult(new
                {
                    message = "Some exam results already exist.",
                    existingResults = alreadyExistingResults
                });
            }

            // Create exam results
            var examResults = dtos.Select(dto => new ExamResult
            {
                ExamId = dto.ExamId,
                StudentId = dto.StudentId,
                ObtainMarks = dto.ObtainMarks,
                IsAbsent = dto.IsAbsent,
                Remarks = dto.Remarks
            }).ToList();

            // Add all results
            await _context.ExamResults.AddRangeAsync(examResults);

            // Save once
            await _context.SaveChangesAsync();

            return new OkObjectResult(new
            {
                message = "Exam results uploaded successfully.",
                totalResults = examResults.Count,
                examResultIds = examResults.Select(r => r.ExamResultId)
            });
        }
        public async Task<IActionResult> UpdateBulkResult(List<ExamResultCreateDto> dtos)
        {
            // Check if request is empty
            if (dtos == null || !dtos.Any())
            {
                return new BadRequestObjectResult("No exam results were provided.");
            }

            // Check duplicate ExamId + StudentId combinations in request
            var duplicates = dtos
                .GroupBy(d => new { d.ExamId, d.StudentId })
                .Where(g => g.Count() > 1)
                .Select(g => new
                {
                    g.Key.ExamId,
                    g.Key.StudentId
                })
                .ToList();

            if (duplicates.Any())
            {
                return new BadRequestObjectResult(new
                {
                    message = "Duplicate ExamId and StudentId combinations found.",
                    duplicates
                });
            }

            // Get unique IDs
            var examIds = dtos
                .Select(d => d.ExamId)
                .Distinct()
                .ToList();

            var studentIds = dtos
                .Select(d => d.StudentId)
                .Distinct()
                .ToList();

            // Validate exams
            var existingExamIds = await _context.Exams
                .Where(e => examIds.Contains(e.ExamID))
                .Select(e => e.ExamID)
                .ToListAsync();

            var invalidExamIds = examIds
                .Except(existingExamIds)
                .ToList();

            if (invalidExamIds.Any())
            {
                return new BadRequestObjectResult(new
                {
                    message = "Some exams do not exist.",
                    invalidExamIds
                });
            }

            // Validate students
            var existingStudentIds = await _context.Students
                .Where(s => studentIds.Contains(s.StudentId))
                .Select(s => s.StudentId)
                .ToListAsync();

            var invalidStudentIds = studentIds
                .Except(existingStudentIds)
                .ToList();

            if (invalidStudentIds.Any())
            {
                return new BadRequestObjectResult(new
                {
                    message = "Some students do not exist.",
                    invalidStudentIds
                });
            }

            // Get all possible existing results
            var existingResults = await _context.ExamResults
                .Where(r =>
                    examIds.Contains(r.ExamId) &&
                    studentIds.Contains(r.StudentId))
                .ToListAsync();

            // Create lookup for fast searching
            var resultLookup = existingResults
                .ToDictionary(
                    r => (r.ExamId, r.StudentId)
                );

            // Check results that do not exist
            var notFoundResults = dtos
                .Where(d => !resultLookup.ContainsKey((d.ExamId, d.StudentId)))
                .Select(d => new
                {
                    d.ExamId,
                    d.StudentId
                })
                .ToList();

            if (notFoundResults.Any())
            {
                return new BadRequestObjectResult(new
                {
                    message = "Some exam results do not exist and cannot be updated.",
                    results = notFoundResults
                });
            }

            // Update results
            foreach (var dto in dtos)
            {
                var examResult = resultLookup[(dto.ExamId, dto.StudentId)];

                examResult.ObtainMarks = dto.ObtainMarks;
                examResult.IsAbsent = dto.IsAbsent;
                examResult.Remarks = dto.Remarks;
            }

            // Save all changes once
            await _context.SaveChangesAsync();

            return new OkObjectResult(new
            {
                message = "Exam results updated successfully.",
                totalUpdated = dtos.Count
            });
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
