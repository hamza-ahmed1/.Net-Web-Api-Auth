using Auth.Data;
using Auth.Model.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace Auth.Data
{
    public static class SeedDate    
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("SeedDate");

            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var context = services.GetRequiredService<ApplicationDbContext>();

            // Ensure database is available
            try
            {
                await context.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Database migrate failed during seeding — continuing if database exists.");
            }

            // Ensure roles
            string[] roles = new[] { "Admin", "Teacher", "Student", "HOD", "CourseCoordinator" };
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Helper to create user if missing
            async Task<ApplicationUser> EnsureUser(string email, string fullName, string password, string role)
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user != null)
                    return user;

                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FullName = fullName
                };

                var createResult = await userManager.CreateAsync(user, password);
                if (!createResult.Succeeded)
                {
                    logger.LogWarning("Failed to create user {Email}: {Errors}", email, string.Join(";", createResult.Errors.Select(e => e.Description)));
                    return user;
                }

                if (!string.IsNullOrEmpty(role))
                {
                    await userManager.AddToRoleAsync(user, role);
                }

                return user;
            }

            // Create 3 sections if needed
            var desiredSectionCount = 3;
            if (await context.Sections.CountAsync() < desiredSectionCount)
            {
                var sectionsToAdd = new List<Section>();
                for (int i = 1; i <= desiredSectionCount; i++)
                {
                    var name = $"Section-{i}";
                    if (!await context.Sections.AnyAsync(s => s.SectionName == name))
                    {
                        sectionsToAdd.Add(new Section
                        {
                            SectionId = Guid.NewGuid(),
                            SectionName = name,
                            IntermediateClass = "9th",
                            StartDate = DateTime.UtcNow.AddMonths(-2)
                        });
                    }
                }
                if (sectionsToAdd.Count > 0)
                {
                    context.Sections.AddRange(sectionsToAdd);
                    await context.SaveChangesAsync();
                }
            }

            // Ensure at least 3 courses exist (used to assign teachers)
            var desiredCourseCount = 3;
            if (await context.Courses.CountAsync() < desiredCourseCount)
            {
                var coursesToAdd = new List<Course>();
                for (int i = 1; i <= desiredCourseCount; i++)
                {
                    var courseName = $"Course-{i}";
                    if (!await context.Courses.AnyAsync(c => c.CourseName == courseName))
                    {
                        coursesToAdd.Add(new Course
                        {
                            CourseId = Guid.NewGuid(),
                            CourseName = courseName,
                            CourseDescription = $"Sample course {i}",
                            CourseDuration = 12
                        });
                    }
                }
                if (coursesToAdd.Count > 0)
                {
                    context.Courses.AddRange(coursesToAdd);
                    await context.SaveChangesAsync();
                }
            }

            // Create 5 teachers and persist
            var desiredTeacherCount = 5;
            var existingTeachers = await context.Teachers.ToListAsync();
            for (int i = 1; i <= desiredTeacherCount; i++)
            {
                var email = $"teacher{i}@example.local";
                var name = $"Teacher {i}";
                var user = await EnsureUser(email, name, "Password@123", "Teacher");
                if (!existingTeachers.Any(t => t.UserId == user.Id))
                {
                    var teacher = new Teacher
                    {
                        Teacher_Id = Guid.NewGuid(),
                        CNIC = ($"{4200000000000 + i}").PadLeft(13, '0'),
                        Qualification = "M.Ed",
                        IdentificationNumber = $"TCHR-{1000 + i}",
                        Department = "General",
                        DateOfBirth = new DateTime(1980 + (i % 10), 1, 1),
                        HireDate = DateTime.UtcNow.AddYears(-1 - i),
                        Address = $"Address {i}",
                        Salary = 40000m + (i * 1000),
                        UserId = user.Id,
                        IsActive = true
                    };
                    context.Teachers.Add(teacher);
                }
            }
            await context.SaveChangesAsync();

            // Create students: 10 per section
            var sections = await context.Sections.ToListAsync();
            var studentsToAdd = new List<Student>();
            foreach (var section in sections)
            {
                // count existing active enrollments for this section
                var enrolledCount = await context.StudentEnrollments.CountAsync(se => se.SectionId == section.SectionId && se.IsActive);
                var toCreate = Math.Max(0, 10 - enrolledCount);
                for (int s = 1; s <= toCreate; s++)
                {
                    var studentIndex = Guid.NewGuid();
                    var email = $"student_{section.SectionName}_{s}@example.local";
                    var name = $"Student {section.SectionName}-{s}";
                    var user = await EnsureUser(email, name, "Password@123", "Student");

                    // create student domain entity if not exists
                    var student = await context.Students.FirstOrDefaultAsync(st => st.UserId == user.Id);
                    if (student == null)
                    {
                        student = new Student
                        {
                            StudentId = Guid.NewGuid(),
                            UserId = user.Id,
                            DateOfBirth = DateTime.UtcNow.AddYears(-14).AddDays(s),
                            EnrollmentDate = DateTime.UtcNow,
                            CNIC = ($"{1000000000000 + new Random().Next(1000000)}")
                        };
                        studentsToAdd.Add(student);
                        context.Students.Add(student);
                        await context.SaveChangesAsync();
                    }

                    // create enrollment if missing
                    if (!await context.StudentEnrollments.AnyAsync(se => se.StudentId == student.StudentId && se.SectionId == section.SectionId && se.IsActive))
                    {
                        context.StudentEnrollments.Add(new StudentEnrollments
                        {
                            StudentEnrollmentId = Guid.NewGuid(),
                            StudentId = student.StudentId,
                            SectionId = section.SectionId,
                            EnrolledDate = DateTime.UtcNow,
                            IsActive = true
                        });
                        await context.SaveChangesAsync();
                    }
                }
            }

            // Assign teachers to sections and courses (TeacherSectionCourse)
            var teachers = await context.Teachers.ToListAsync();
            var courses = await context.Courses.ToListAsync();
            int tIndex = 0;
            foreach (var teacher in teachers)
            {
                var targetSection = sections[tIndex % sections.Count];
                var targetCourse = courses[tIndex % courses.Count];
                // Avoid duplicate assignment
                if (!await context.TeacherSectionCourses.AnyAsync(tsc => tsc.TeacherId == teacher.Teacher_Id && tsc.SectionId == targetSection.SectionId && tsc.CourseId == targetCourse.CourseId))
                {
                    context.TeacherSectionCourses.Add(new TeacherSectionCourse
                    {
                        TeacherSectionCourseId = Guid.NewGuid(),
                        TeacherId = teacher.Teacher_Id,
                        SectionId = targetSection.SectionId,
                        CourseId = targetCourse.CourseId,
                        AssignedDate = DateTime.UtcNow,
                        IsActive = true
                    });
                }
                tIndex++;
            }
            await context.SaveChangesAsync();

            logger.LogInformation("Seed data initialization completed.");
            // --- Seed ExamTypes and Exams ---
            // Ensure common exam types exist
            var examTypes = new[] { "Midterm", "Final", "Quiz" };
            var createdExamTypes = new List<ExamType>();
            foreach (var t in examTypes)
            {
                var existing = await context.ExamTypes.FirstOrDefaultAsync(et => et.Type == t);
                if (existing == null)
                {
                    existing = new ExamType { ExamTypeId = Guid.NewGuid(), Type = t };
                    context.ExamTypes.Add(existing);
                    await context.SaveChangesAsync();
                }
                createdExamTypes.Add(existing);
            }

            // Create at least one exam per TeacherSectionCourse if missing
            var tscList = await context.TeacherSectionCourses.ToListAsync();
            foreach (var tsc in tscList)
            {
                // if any exam exists for this TSC, skip
                if (await context.Exams.AnyAsync(e => e.TeacherSectionCourseId == tsc.TeacherSectionCourseId))
                    continue;

                var exam = new Exam
                {
                    ExamID = Guid.NewGuid(),
                    Title = $"{createdExamTypes.First().Type} for {tsc.CourseId}",
                    ExamTypeId = createdExamTypes.First().ExamTypeId,
                    TotalMarks = 100,
                    IsPublished = false,
                    ExamDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
                    TeacherSectionCourseId = tsc.TeacherSectionCourseId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.Exams.Add(exam);
            }
            await context.SaveChangesAsync();
        }
    }
}
