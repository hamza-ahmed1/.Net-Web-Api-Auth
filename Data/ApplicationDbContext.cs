using Auth.Model.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
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
