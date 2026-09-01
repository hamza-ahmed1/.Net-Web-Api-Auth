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
        public DbSet<FeeCategory> FeeCategories { get; set; }
        public DbSet<FeeType> FeeTypes { get; set; }
        public DbSet<ApplicableFee> ApplicableFees { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<TransactionHistory> TransactionHistories { get; set; }
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


            // Fee : Properties configuration for decimal precision
            builder.Entity<FeeType>().Property(f => f.Amount).HasColumnType("decimal(10,2)");
            builder.Entity<Invoice>().Property(i => i.TotalAmount).HasColumnType("decimal(10,2)");
            builder.Entity<Invoice>().Property(i => i.AmountPaid).HasColumnType("decimal(10,2)");
            // InvoiceItem and Payment entities were removed/renamed in the updated fee model.
            // Configure TransactionHistory amount precision instead.
            builder.Entity<TransactionHistory>().Property(t => t.Amount).HasColumnType("decimal(10,2)");

            // Fee:
            // Unique constraint on invoice number — prevents duplicate invoice numbers
            builder.Entity<Invoice>()
                .HasIndex(i => i.InvoiceNum)
                .IsUnique();

            // Prevent accidental cascade-delete chains across financial records —
            // deleting a student should NOT silently delete their payment history
            builder.Entity<ApplicableFee>()
                .HasOne(af => af.Student)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Invoice>()
                .HasOne(i => i.Student)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<ApplicableFee>()
                .Property(x => x.Status)
                .HasConversion<string>();

            // TransactionHistory -> Invoice: invoice can have many transaction history records
            // Invoice currently doesn't declare a collection navigation property, so configure the relationship
            // using WithMany() without a lambda to avoid requiring an Invoice.Navigation property.
            builder.Entity<TransactionHistory>()
                .HasOne(th => th.Invoice)
                .WithMany()
                .HasForeignKey(th => th.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
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
