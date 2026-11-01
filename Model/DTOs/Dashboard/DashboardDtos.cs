namespace Auth.Model.DTOs.Dashboard
{
    public record CountPoint(string Label, int Value);

    /// <summary>Average score as a percentage, with the number of results behind it.</summary>
    public record ScorePoint(string Label, decimal Average, int Results);

    public record SubjectResult(string Label, decimal Average, decimal Highest, decimal Lowest, int Results);

    public record AttendanceWeek(string Label, decimal Rate, int Marked);

    public class AttendanceSummaryDto
    {
        public string? Label { get; set; }
        public int Total { get; set; }
        public int Present { get; set; }
        public int Absent { get; set; }
        public int Late { get; set; }
        public int Leave { get; set; }
        /// <summary>(present + late) / total, as a percentage; null when nothing has been marked.</summary>
        public decimal? Rate { get; set; }
    }

    public record DepartmentStat(string Name, int Students, int Teachers);

    public record FeeMonth(string Label, decimal Billed, decimal Collected);

    public class FeeOverviewDto
    {
        public string Currency { get; set; } = "PKR";
        /// <summary>Invoices in other currencies, which these totals leave out.</summary>
        public int OtherCurrencyInvoices { get; set; }
        public int Invoices { get; set; }
        public decimal Billed { get; set; }
        public decimal Collected { get; set; }
        public decimal Outstanding { get; set; }
        public decimal Overdue { get; set; }
        public List<FeeMonth> Months { get; set; } = new();
    }

    public class AdminDashboardDto
    {
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int Teachers { get; set; }
        public int Sections { get; set; }
        public int Courses { get; set; }
        public List<CountPoint> EnrolmentByCohort { get; set; } = new();
        public List<DepartmentStat> Departments { get; set; } = new();
        public AttendanceSummaryDto Attendance { get; set; } = new();
        public FeeOverviewDto Fees { get; set; } = new();
    }

    public record TeacherPerformance(string Name, int Exams, int Results, decimal? AverageScore, decimal? AttendanceRate);

    public class HodDashboardDto
    {
        public string Department { get; set; } = string.Empty;
        public int Students { get; set; }
        public int Teachers { get; set; }
        public int Sections { get; set; }
        public decimal? AttendanceRate { get; set; }
        public decimal? AverageScore { get; set; }
        public List<CountPoint> StudentsBySection { get; set; } = new();
        public List<AttendanceWeek> AttendanceTrend { get; set; } = new();
        public List<ScorePoint> CoursePerformance { get; set; } = new();
        public List<TeacherPerformance> TeacherPerformance { get; set; } = new();
    }

    public class TeacherDashboardDto
    {
        public int Sections { get; set; }
        public int Students { get; set; }
        public int Exams { get; set; }
        public int ExamsAwaitingResults { get; set; }
        public decimal? AttendanceRate { get; set; }
        public decimal? AverageScore { get; set; }
        public List<AttendanceSummaryDto> AttendanceBySection { get; set; } = new();
        public List<AttendanceWeek> AttendanceTrend { get; set; } = new();
        public List<ScorePoint> ExamPerformance { get; set; } = new();
        public List<SubjectResult> SubjectResults { get; set; } = new();
        public List<CountPoint> ScoreBands { get; set; } = new();
    }
}
