using System.Linq.Expressions;
using Auth.Data;
using Auth.Model.DTOs.Announcements;
using Auth.Model.DTOs.Lookups;
using Auth.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth.Services
{
    /// <summary>
    /// CRUD for admin-managed lists that students are assigned to (cohorts, campuses).
    /// Names are unique (case-insensitive, matching SQL Server's collation); an item that still has
    /// students cannot be deleted, only deactivated, and inactive items can't be chosen for new students.
    /// </summary>
    public class NamedLookupService<TEntity> where TEntity : class, INamedLookup, new()
    {
        private readonly ApplicationDbContext _context;
        private readonly Expression<Func<Student, Guid?>> _studentKey;
        private readonly string _label;

        public NamedLookupService(ApplicationDbContext context, Expression<Func<Student, Guid?>> studentKey, string label)
        {
            _context = context;
            _studentKey = studentKey;
            _label = label;
        }

        public async Task<List<LookupDto>> GetAllAsync(bool activeOnly)
        {
            var query = _context.Set<TEntity>().AsNoTracking();
            if (activeOnly)
                query = query.Where(x => x.IsActive);

            var items = await query.OrderBy(x => x.Name).ToListAsync();
            var counts = await StudentCountsAsync();

            return items.Select(x => ToDto(x, counts)).ToList();
        }

        public async Task<LookupDto?> GetByIdAsync(Guid id)
        {
            var item = await _context.Set<TEntity>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return item == null ? null : ToDto(item, await StudentCountsAsync());
        }

        /// <summary>True when the id refers to an existing, active item (used when assigning students).</summary>
        public Task<bool> IsActiveAsync(Guid id)
        {
            return _context.Set<TEntity>().AnyAsync(x => x.Id == id && x.IsActive);
        }

        public async Task<Result<LookupDto>> CreateAsync(SaveLookupDto dto)
        {
            var name = dto.Name?.Trim() ?? string.Empty;
            if (name.Length == 0)
                return Result<LookupDto>.Failure($"The {_label} name is required.");
            if (await _context.Set<TEntity>().AnyAsync(x => x.Name == name))
                return Result<LookupDto>.Failure($"A {_label} named \"{name}\" already exists.");

            var item = new TEntity { Id = Guid.NewGuid(), Name = name, IsActive = dto.IsActive, CreatedAt = DateTime.UtcNow };
            _context.Set<TEntity>().Add(item);
            await _context.SaveChangesAsync();

            return Result<LookupDto>.Ok((await GetByIdAsync(item.Id))!);
        }

        public async Task<Result<LookupDto>> UpdateAsync(Guid id, SaveLookupDto dto)
        {
            var item = await _context.Set<TEntity>().FirstOrDefaultAsync(x => x.Id == id);
            if (item == null)
                return Result<LookupDto>.NotFound($"The {_label} was not found.");

            var name = dto.Name?.Trim() ?? string.Empty;
            if (name.Length == 0)
                return Result<LookupDto>.Failure($"The {_label} name is required.");
            if (await _context.Set<TEntity>().AnyAsync(x => x.Name == name && x.Id != id))
                return Result<LookupDto>.Failure($"A {_label} named \"{name}\" already exists.");

            item.Name = name;
            item.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();

            return Result<LookupDto>.Ok((await GetByIdAsync(id))!);
        }

        public async Task<Result<bool>> DeleteAsync(Guid id)
        {
            var item = await _context.Set<TEntity>().FirstOrDefaultAsync(x => x.Id == id);
            if (item == null)
                return Result<bool>.NotFound($"The {_label} was not found.");

            if (await _context.Students.AnyAsync(StudentIn(id)))
                return Result<bool>.Failure($"This {_label} still has students. Move them first, or deactivate the {_label} instead.");

            _context.Set<TEntity>().Remove(item);
            await _context.SaveChangesAsync();
            return Result<bool>.Ok(true);
        }

        private async Task<Dictionary<Guid, int>> StudentCountsAsync()
        {
            var rows = await _context.Students
                .GroupBy(_studentKey)
                .Select(g => new { Id = g.Key, Count = g.Count() })
                .ToListAsync();
            return rows.Where(r => r.Id.HasValue).ToDictionary(r => r.Id!.Value, r => r.Count);
        }

        /// <summary>Builds "student => student.{Key} == id" from the key selector.</summary>
        private Expression<Func<Student, bool>> StudentIn(Guid id)
        {
            var body = Expression.Equal(_studentKey.Body, Expression.Constant((Guid?)id, typeof(Guid?)));
            return Expression.Lambda<Func<Student, bool>>(body, _studentKey.Parameters);
        }

        private static LookupDto ToDto(TEntity item, Dictionary<Guid, int> counts) => new()
        {
            Id = item.Id,
            Name = item.Name,
            IsActive = item.IsActive,
            StudentCount = counts.GetValueOrDefault(item.Id)
        };
    }
}
