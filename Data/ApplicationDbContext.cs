using Auth.Model.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
namespace Auth.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Teacher> Teachers { get; set; }

        public DbSet<Course> Courses { get; set; }
        
        public DbSet<TeacherSectionCourse> TeacherSectionCourses { get; set; }

        public DbSet<Section> Sections { get; set; }

        public DbSet<Student> Students { get; set; }

        public DbSet<StudentEnrollments> StudentEnrollments { get; set; }

        public DbSet<Attendance> Attendances { get; set; }

        public DbSet<Exam> Exams { get; set; }

        public DbSet<ExamType> ExamTypes { get; set; }

        public DbSet<ExamResult> ExamResults { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<TeacherSectionCourse>()
                .HasOne(ts => ts.Teacher)
                .WithMany(t => t.TeacherAssignments)
                .HasForeignKey(ts => ts.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);



            builder.Entity<TeacherSectionCourse>()
                .HasOne(ts => ts.Section)
                .WithMany(s => s.TeacherAssignments)
                .HasForeignKey(ts => ts.SectionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TeacherSectionCourse>()
                .HasOne(ts => ts.Course)
                .WithMany(c => c.TeacherAssignments)
                .HasForeignKey(ts => ts.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TeacherSectionCourse>()
                .HasIndex(ts => new { ts.TeacherId, ts.SectionId, ts.CourseId })
                .IsUnique();
            // one student cant assign to same course in same section with same teacher
            builder.Entity<StudentEnrollments>()
       .HasIndex(se => new { se.StudentId, se.StudentEnrollmentId })
       .IsUnique();


            builder.Entity<Attendance>()
    .HasIndex(a => new { a.StudentEnrollmentId, a.TeacherSectionCourseId, a.AttendanceDate })
    .IsUnique();


            builder.Entity<ExamResult>()
    .HasOne(x => x.Exam)
    .WithMany()
    .HasForeignKey(x => x.ExamId)
    .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ExamResult>()
                .HasOne(x => x.Student)
                .WithMany()
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<ExamResult>()
    .HasIndex(x => new { x.ExamId, x.StudentId })
    .IsUnique();
            builder.Entity<IdentityRole>().HasData(
        new IdentityRole
        {
            Name = "Admin",
            NormalizedName = "ADMIN",
        },
        new IdentityRole
        {
            Name = "Teacher",
            NormalizedName = "TEACHER",
        },
        new IdentityRole
        {
            Name = "Student",
            NormalizedName = "STUDENT",
        },
        new IdentityRole
        {
            Name = "HOD",
            NormalizedName = "HOD",
        },
        new IdentityRole
        {
            Name = "CourseCoordinator",
            NormalizedName = "COURSECOORDINATOR",
        }
    );   
        }
    }
}
