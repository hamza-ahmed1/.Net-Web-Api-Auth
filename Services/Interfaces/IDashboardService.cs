using Auth.Model.DTOs.Dashboard;

namespace Auth.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<AdminDashboardDto> GetAdminAsync();

        /// <summary>Null when the HOD has no teacher profile or department.</summary>
        Task<HodDashboardDto?> GetHodAsync(string userId);

        /// <summary>Null when the user has no teacher profile.</summary>
        Task<TeacherDashboardDto?> GetTeacherAsync(string userId);
    }
}
