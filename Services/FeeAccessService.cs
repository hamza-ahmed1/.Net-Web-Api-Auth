using System.Security.Claims;
using Auth.Data;
using Auth.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth.Services
{
    /// <summary>What a fee manager may see: Admin sees everything, an HOD only their own department.</summary>
    public sealed record FeeScope(bool All, Guid? DepartmentId);

    public class FeeAccessService
    {
        private readonly ApplicationDbContext _context;

        public FeeAccessService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<FeeScope> GetScopeAsync(ClaimsPrincipal user)
        {
            if (user.IsInRole("Admin"))
                return new FeeScope(true, null);

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var departmentId = await _context.Teachers
                .Where(t => t.UserId == userId)
                .Select(t => t.DepartmentId)
                .FirstOrDefaultAsync();
            return new FeeScope(false, departmentId);
        }

        public IQueryable<Student> Students(FeeScope scope)
        {
            if (scope.All) return _context.Students;
            if (scope.DepartmentId == null) return _context.Students.Where(s => false);
            return _context.Students.Where(s => s.DepartmentId == scope.DepartmentId);
        }

        public IQueryable<Invoice> Invoices(FeeScope scope)
        {
            if (scope.All) return _context.Invoices;
            if (scope.DepartmentId == null) return _context.Invoices.Where(i => false);
            return _context.Invoices.Where(i => i.Student!.DepartmentId == scope.DepartmentId);
        }

        /// <summary>Admin, the student's HOD, or the student themselves.</summary>
        public async Task<bool> CanReadStudentAsync(ClaimsPrincipal user, Guid studentId)
        {
            if (user.IsInRole("Admin") || user.IsInRole("HOD"))
            {
                var scope = await GetScopeAsync(user);
                return await Students(scope).AnyAsync(s => s.StudentId == studentId);
            }

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            return await _context.Students.AnyAsync(s => s.StudentId == studentId && s.UserId == userId);
        }
    }
}
