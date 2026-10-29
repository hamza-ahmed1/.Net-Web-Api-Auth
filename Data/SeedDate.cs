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

            try
            {
                await context.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Database migrate failed during seeding — continuing if database exists.");
            }

            string[] roles = new[] { "Admin", "Teacher", "Student", "HOD", "CourseCoordinator" };
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            async Task<ApplicationUser> EnsureUser(string email, string fullName, string password, string role)
            {
                var user = await userManager.FindByEmailAsync(email) ?? await userManager.FindByNameAsync(email);

                if (user == null)
                {
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
                }
                else if (string.IsNullOrWhiteSpace(user.FullName) || user.FullName != fullName)
                {
                    user.FullName = fullName;
                    await userManager.UpdateAsync(user);
                }

                if (!string.IsNullOrEmpty(role) && !await userManager.IsInRoleAsync(user, role))
                {
                    var addRoleResult = await userManager.AddToRoleAsync(user, role);
                    if (!addRoleResult.Succeeded)
                    {
                        logger.LogWarning("Failed to add user {Email} to role {Role}: {Errors}", email, role, string.Join(';', addRoleResult.Errors.Select(e => e.Description)));
                    }
                }

                return await userManager.FindByIdAsync(user.Id);
            }

            // XI / XII — Pre-Engineering (A–F); section E is Pre-Medical
            var sectionDefs = new List<(string IntermediateClass, string SectionName, string Stream)>
            {
                ("XI", "Pre-Engineering (A)", "Pre-Engineering"),
                ("XI", "Pre-Engineering (B)", "Pre-Engineering"),
                ("XI", "Pre-Engineering (C)", "Pre-Engineering"),
                ("XI", "Pre-Engineering (D)", "Pre-Engineering"),
                ("XI", "Pre-Medical (E)", "Pre-Medical"),
                ("XI", "Pre-Engineering (F)", "Pre-Engineering"),
                ("XII", "Pre-Engineering (A)", "Pre-Engineering"),
                ("XII", "Pre-Engineering (B)", "Pre-Engineering"),
                ("XII", "Pre-Engineering (C)", "Pre-Engineering"),
                ("XII", "Pre-Engineering (D)", "Pre-Engineering"),
                ("XII", "Pre-Medical (E)", "Pre-Medical"),
                ("XII", "Pre-Engineering (F)", "Pre-Engineering"),
            };

            foreach (var (intermediateClass, sectionName, _) in sectionDefs)
            {
                if (!await context.Sections.AnyAsync(s => s.IntermediateClass == intermediateClass && s.SectionName == sectionName))
                {
                    context.Sections.Add(new Section
                    {
                        SectionId = Guid.NewGuid(),
                        SectionName = sectionName,
                        IntermediateClass = intermediateClass,
                        StartDate = DateTime.UtcNow.AddMonths(-3),
                        IsActive = true
                    });
                }
            }
            await context.SaveChangesAsync();

            var courseDefs = new[]
            {
                ("Physics", "Intermediate Physics for Pre-Engineering and Pre-Medical streams", 12),
                ("Chemistry", "Intermediate Chemistry covering organic, inorganic and physical chemistry", 12),
                ("Mathematics", "Intermediate Mathematics for Pre-Engineering (algebra, calculus, trigonometry)", 12),
                ("Biology", "Intermediate Biology for Pre-Medical (botany and zoology)", 12),
                ("English", "Compulsory English for Intermediate Part I & II", 12),
                ("Urdu", "Compulsory Urdu for Intermediate Part I & II", 12),
                ("Computer Science", "Introduction to Computing and Programming", 12),
                ("Pakistan Studies", "Pakistan Studies for Intermediate classes", 6),
                ("Islamiyat", "Islamic Studies / Ethics for Intermediate classes", 6),
            };

            foreach (var (name, description, duration) in courseDefs)
            {
                if (!await context.Courses.AnyAsync(c => c.CourseName == name))
                {
                    context.Courses.Add(new Course
                    {
                        CourseId = Guid.NewGuid(),
                        CourseName = name,
                        CourseDescription = description,
                        CourseDuration = duration
                    });
                }
            }
            await context.SaveChangesAsync();

            var teacherDefs = new[]
            {
                new { Email = "ahmed.khan@lms.edu.pk", Name = "Dr. Muhammad Ahmed Khan", Cnic = "35202-1456789-1", Qual = "Ph.D Physics (PU)", Dept = "Science", IdNo = "TCHR-1001", Address = "House 42, Street 7, Gulberg III, Lahore", Salary = 95000m, Dob = new DateTime(1978, 3, 12), Subject = "Physics" },
                new { Email = "fatima.siddiqui@lms.edu.pk", Name = "Prof. Fatima Zahra Siddiqui", Cnic = "42101-2345678-2", Qual = "M.Phil Chemistry (KU)", Dept = "Science", IdNo = "TCHR-1002", Address = "Flat 12-B, DHA Phase 5, Karachi", Salary = 88000m, Dob = new DateTime(1982, 7, 21), Subject = "Chemistry" },
                new { Email = "ali.raza@lms.edu.pk", Name = "Sir Ali Hassan Raza", Cnic = "37405-9876543-3", Qual = "M.Sc Mathematics (QAU)", Dept = "Mathematics", IdNo = "TCHR-1003", Address = "House 18, G-11/2, Islamabad", Salary = 82000m, Dob = new DateTime(1985, 1, 9), Subject = "Mathematics" },
                new { Email = "ayesha.noor@lms.edu.pk", Name = "Madam Ayesha Noor", Cnic = "33100-1122334-4", Qual = "M.Phil Biology (UAF)", Dept = "Science", IdNo = "TCHR-1004", Address = "B-204, Canal View, Faisalabad", Salary = 80000m, Dob = new DateTime(1987, 11, 4), Subject = "Biology" },
                new { Email = "usman.malik@lms.edu.pk", Name = "Mr. Usman Malik", Cnic = "61101-5566778-5", Qual = "M.A English Literature (NUML)", Dept = "Languages", IdNo = "TCHR-1005", Address = "House 9, Satellite Town, Rawalpindi", Salary = 72000m, Dob = new DateTime(1984, 5, 18), Subject = "English" },
                new { Email = "sana.malik@lms.edu.pk", Name = "Ms. Sana Malik", Cnic = "35202-7788990-6", Qual = "M.A Urdu (GCU)", Dept = "Languages", IdNo = "TCHR-1006", Address = "House 55, Model Town Block C, Lahore", Salary = 70000m, Dob = new DateTime(1990, 9, 27), Subject = "Urdu" },
                new { Email = "bilal.sheikh@lms.edu.pk", Name = "Engr. Bilal Ahmed Sheikh", Cnic = "42201-3344556-7", Qual = "MS Computer Science (FAST-NU)", Dept = "Computer Science", IdNo = "TCHR-1007", Address = "House 3, Gulshan-e-Iqbal Block 13-D, Karachi", Salary = 85000m, Dob = new DateTime(1988, 2, 14), Subject = "Computer Science" },
                new { Email = "imran.qureshi@lms.edu.pk", Name = "Mr. Imran Qureshi", Cnic = "31303-6677889-8", Qual = "M.A Pakistan Studies (IUB)", Dept = "Social Sciences", IdNo = "TCHR-1008", Address = "House 21, Satellite Town, Bahawalpur", Salary = 68000m, Dob = new DateTime(1983, 12, 2), Subject = "Pakistan Studies" },
                new { Email = "zainab.hassan@lms.edu.pk", Name = "Madam Zainab Hassan", Cnic = "35202-9900112-9", Qual = "M.A Islamic Studies (PU)", Dept = "Islamic Studies", IdNo = "TCHR-1009", Address = "House 11, Johar Town Phase 1, Lahore", Salary = 67000m, Dob = new DateTime(1986, 6, 30), Subject = "Islamiyat" },
                new { Email = "hamza.tariq@lms.edu.pk", Name = "Sir Hamza Tariq", Cnic = "38403-2233445-1", Qual = "M.Sc Physics (UET Taxila)", Dept = "Science", IdNo = "TCHR-1010", Address = "House 67, Civil Lines, Sargodha", Salary = 78000m, Dob = new DateTime(1991, 4, 8), Subject = "Physics" },
            };

            // Departments; Pre-Engineering and Pre-Medical students both belong to Science
            const string scienceDepartment = "Science";
            foreach (var name in teacherDefs.Select(d => d.Dept).Append(scienceDepartment).Distinct())
            {
                if (!await context.Departments.AnyAsync(d => d.Name == name))
                {
                    context.Departments.Add(new Department { DepartmentId = Guid.NewGuid(), Name = name });
                }
            }
            await context.SaveChangesAsync();
            var departmentIds = await context.Departments.ToDictionaryAsync(d => d.Name, d => d.DepartmentId, StringComparer.OrdinalIgnoreCase);

            // Cohorts and campus students are assigned to (both admin-managed afterwards)
            string[] cohortNames = { "Cohort-2025", "Cohort-2026" };
            foreach (var name in cohortNames)
            {
                if (!await context.Cohorts.AnyAsync(c => c.Name == name))
                    context.Cohorts.Add(new Cohort { Id = Guid.NewGuid(), Name = name });
            }
            const string campusName = "Qayyum-Abad Campus";
            if (!await context.Campuses.AnyAsync(c => c.Name == campusName))
                context.Campuses.Add(new Campus { Id = Guid.NewGuid(), Name = campusName });
            await context.SaveChangesAsync();
            var cohortIds = await context.Cohorts.ToDictionaryAsync(c => c.Name, c => c.Id, StringComparer.OrdinalIgnoreCase);
            var campusId = await context.Campuses.Where(c => c.Name == campusName).Select(c => c.Id).FirstAsync();

            for (var ti = 0; ti < teacherDefs.Length; ti++)
            {
                var def = teacherDefs[ti];
                var user = await EnsureUser(def.Email, def.Name, "Password@123", "Teacher");
                if (!await context.Teachers.AnyAsync(t => t.UserId == user.Id))
                {
                    context.Teachers.Add(new Teacher
                    {
                        Teacher_Id = Guid.NewGuid(),
                        CNIC = def.Cnic,
                        Qualification = def.Qual,
                        IdentificationNumber = def.IdNo,
                        // All seeded teachers start in Science (def.Dept only seeds the department list)
                        DepartmentId = departmentIds[scienceDepartment],
                        DateOfBirth = def.Dob,
                        HireDate = DateTime.UtcNow.AddYears(-(2 + (ti % 5))),
                        Address = def.Address,
                        Salary = def.Salary,
                        UserId = user.Id,
                        IsActive = true
                    });
                }
            }
            await context.SaveChangesAsync();

            var maleFirst = new[] { "Ahmed", "Ali", "Hassan", "Usman", "Bilal", "Hamza", "Omar", "Zain", "Ibrahim", "Yousuf", "Saad", "Taha", "Rayyan", "Daniyal", "Fahad" };
            var femaleFirst = new[] { "Ayesha", "Fatima", "Sana", "Zainab", "Maryam", "Hira", "Noor", "Laiba", "Areeba", "Mahnoor", "Iqra", "Hafsa", "Eman", "Alina", "Rida" };
            var lastNames = new[] { "Khan", "Malik", "Ahmed", "Raza", "Siddiqui", "Qureshi", "Sheikh", "Butt", "Chaudhry", "Hashmi", "Mirza", "Javed", "Rehman", "Abbas", "Nawaz" };

            var sectionKeys = sectionDefs
                .Select(d => (d.IntermediateClass, d.SectionName))
                .ToHashSet();
            var allSections = await context.Sections.ToListAsync();
            var sections = allSections
                .Where(s => sectionKeys.Contains((s.IntermediateClass, s.SectionName)))
                .ToList();

            var fatherFirst = new[] { "Muhammad", "Abdul", "Ghulam", "Syed", "Chaudhry", "Malik", "Haji", "Rana" };
            var cities = new[]
            {
                ("Gulberg III, Lahore", "Lahore"),
                ("Model Town, Lahore", "Lahore"),
                ("DHA Phase 5, Karachi", "Karachi"),
                ("Gulshan-e-Iqbal, Karachi", "Karachi"),
                ("G-11/2, Islamabad", "Islamabad"),
                ("Satellite Town, Rawalpindi", "Rawalpindi"),
                ("Canal View, Faisalabad", "Faisalabad"),
                ("Civil Lines, Multan", "Multan"),
            };

            var studentsPerSection = 8;
            var nameIndex = 0;

            // Student codes continue after the highest existing one so re-seeding never collides
            var existingCodes = await context.Students.Select(st => st.StudentCode).ToListAsync();
            var nextStudentCode = existingCodes
                .Select(code => long.TryParse(code, out var n) ? n : 0)
                .DefaultIfEmpty(0)
                .Max();
            nextStudentCode = Math.Max(nextStudentCode, 2001000);
            foreach (var section in sections)
            {
                var stream = section.SectionName.StartsWith("Pre-Medical", StringComparison.OrdinalIgnoreCase)
                    ? "Pre-Medical"
                    : "Pre-Engineering";
                var enrolledCount = await context.StudentEnrollments.CountAsync(se => se.SectionId == section.SectionId && se.IsActive);
                var toCreate = Math.Max(0, studentsPerSection - enrolledCount);

                for (int s = 1; s <= toCreate; s++)
                {
                    nameIndex++;
                    var isFemale = nameIndex % 2 == 0;
                    var first = isFemale
                        ? femaleFirst[nameIndex % femaleFirst.Length]
                        : maleFirst[nameIndex % maleFirst.Length];
                    var last = lastNames[(nameIndex * 3) % lastNames.Length];
                    var fullName = $"{first} {last}";
                    var sectionKey = $"{section.IntermediateClass}{section.SectionName.Replace(" ", "").Replace("(", "").Replace(")", "").Replace("-", "")}";
                    var email = $"{first.ToLower()}.{last.ToLower()}.{sectionKey.ToLower()}{s}@student.lms.edu.pk";
                    var (area, city) = cities[nameIndex % cities.Length];
                    var fatherName = $"{fatherFirst[nameIndex % fatherFirst.Length]} {last}";

                    var user = await EnsureUser(email, fullName, "Password@123", "Student");

                    var student = await context.Students.FirstOrDefaultAsync(st => st.UserId == user.Id);
                    if (student == null)
                    {
                        var cnicSerial = 1000000 + nameIndex;
                        if (string.IsNullOrEmpty(user.PhoneNumber))
                        {
                            user.PhoneNumber = $"0300-{1000000 + nameIndex:D7}";
                            await userManager.UpdateAsync(user);
                        }
                        student = new Student
                        {
                            StudentId = Guid.NewGuid(),
                            UserId = user.Id,
                            StudentCode = (++nextStudentCode).ToString(),
                            IsActive = true,
                            DateOfBirth = DateTime.UtcNow.AddYears(-(16 + (nameIndex % 3))).AddDays(nameIndex),
                            EnrollmentDate = DateTime.UtcNow.AddMonths(-2),
                            CNIC = $"35202-{cnicSerial:D7}-{(nameIndex % 9) + 1}",
                            // Intake year for the 2026-27 session: XII joined in 2025, XI in 2026
                            CohortId = cohortIds[section.IntermediateClass == "XII" ? "Cohort-2025" : "Cohort-2026"],
                            CampusId = campusId,
                            MajorSubject = stream,
                            Gender = isFemale ? "Female" : "Male",
                            FatherName = fatherName,
                            CurrentAdress = $"House {(nameIndex % 90) + 1}, {area}",
                            PermanentAddress = $"House {(nameIndex % 90) + 1}, {area}, {city}",
                            DepartmentId = departmentIds[scienceDepartment]
                        };
                        context.Students.Add(student);
                        await context.SaveChangesAsync();
                    }

                    if (!await context.StudentEnrollments.AnyAsync(se => se.StudentId == student.StudentId && se.SectionId == section.SectionId && se.IsActive))
                    {
                        context.StudentEnrollments.Add(new StudentEnrollments
                        {
                            StudentEnrollmentId = Guid.NewGuid(),
                            StudentId = student.StudentId,
                            SectionId = section.SectionId,
                            EnrolledDate = DateTime.UtcNow.AddMonths(-2),
                            IsActive = true
                        });
                        await context.SaveChangesAsync();
                    }
                }
            }

            // Assign each teacher to their subject across relevant sections
            var teachers = await context.Teachers.Include(t => t.User).ToListAsync();
            var courses = await context.Courses.ToListAsync();
            var teacherByEmail = teachers.ToDictionary(t => t.User.Email!, t => t, StringComparer.OrdinalIgnoreCase);

            string FormatSectionLabel(Section section) => $"{section.IntermediateClass} - {section.SectionName}";

            async Task EnsureAssignment(Teacher teacher, Section section, Course course)
            {
                if (await context.TeacherSectionCourses.AnyAsync(tsc =>
                        tsc.TeacherId == teacher.Teacher_Id &&
                        tsc.SectionId == section.SectionId &&
                        tsc.CourseId == course.CourseId))
                {
                    return;
                }

                context.TeacherSectionCourses.Add(new TeacherSectionCourse
                {
                    TeacherSectionCourseId = Guid.NewGuid(),
                    TeacherId = teacher.Teacher_Id,
                    SectionId = section.SectionId,
                    CourseId = course.CourseId,
                    AssignedDate = DateTime.UtcNow,
                    IsActive = true
                });
            }

            Course? FindCourse(string name) => courses.FirstOrDefault(c => c.CourseName == name);

            var preEngSections = sections.Where(s => s.SectionName.StartsWith("Pre-Engineering", StringComparison.OrdinalIgnoreCase)).ToList();
            var preMedSections = sections.Where(s => s.SectionName.StartsWith("Pre-Medical", StringComparison.OrdinalIgnoreCase)).ToList();
            var allTargetSections = sections.ToList();

            // Physics — split XI/XII Pre-Eng between two teachers; both Pre-Med get Physics too
            if (teacherByEmail.TryGetValue("ahmed.khan@lms.edu.pk", out var physics1) && FindCourse("Physics") is { } physics)
            {
                foreach (var section in preEngSections.Where(s => s.IntermediateClass == "XI").Concat(preMedSections))
                    await EnsureAssignment(physics1, section, physics);
            }
            if (teacherByEmail.TryGetValue("hamza.tariq@lms.edu.pk", out var physics2) && FindCourse("Physics") is { } physicsB)
            {
                foreach (var section in preEngSections.Where(s => s.IntermediateClass == "XII"))
                    await EnsureAssignment(physics2, section, physicsB);
            }

            if (teacherByEmail.TryGetValue("fatima.siddiqui@lms.edu.pk", out var chemTeacher) && FindCourse("Chemistry") is { } chemistry)
            {
                foreach (var section in allTargetSections)
                    await EnsureAssignment(chemTeacher, section, chemistry);
            }

            if (teacherByEmail.TryGetValue("ali.raza@lms.edu.pk", out var mathTeacher) && FindCourse("Mathematics") is { } mathematics)
            {
                foreach (var section in preEngSections)
                    await EnsureAssignment(mathTeacher, section, mathematics);
            }

            if (teacherByEmail.TryGetValue("ayesha.noor@lms.edu.pk", out var bioTeacher) && FindCourse("Biology") is { } biology)
            {
                foreach (var section in preMedSections)
                    await EnsureAssignment(bioTeacher, section, biology);
            }

            if (teacherByEmail.TryGetValue("usman.malik@lms.edu.pk", out var engTeacher) && FindCourse("English") is { } english)
            {
                foreach (var section in allTargetSections)
                    await EnsureAssignment(engTeacher, section, english);
            }

            if (teacherByEmail.TryGetValue("sana.malik@lms.edu.pk", out var urduTeacher) && FindCourse("Urdu") is { } urdu)
            {
                foreach (var section in allTargetSections)
                    await EnsureAssignment(urduTeacher, section, urdu);
            }

            if (teacherByEmail.TryGetValue("bilal.sheikh@lms.edu.pk", out var csTeacher) && FindCourse("Computer Science") is { } cs)
            {
                foreach (var section in preEngSections.Where((_, i) => i % 2 == 0))
                    await EnsureAssignment(csTeacher, section, cs);
            }

            if (teacherByEmail.TryGetValue("imran.qureshi@lms.edu.pk", out var pstTeacher) && FindCourse("Pakistan Studies") is { } pst)
            {
                foreach (var section in allTargetSections)
                    await EnsureAssignment(pstTeacher, section, pst);
            }

            if (teacherByEmail.TryGetValue("zainab.hassan@lms.edu.pk", out var islTeacher) && FindCourse("Islamiyat") is { } islamiyat)
            {
                foreach (var section in allTargetSections)
                    await EnsureAssignment(islTeacher, section, islamiyat);
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Seed data initialization completed with Pakistani Intermediate sections ({Count} sections).", sections.Count);

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

            var tscList = await context.TeacherSectionCourses
                .Include(tsc => tsc.Course)
                .Include(tsc => tsc.Section)
                .ToListAsync();

            foreach (var tsc in tscList)
            {
                if (await context.Exams.AnyAsync(e => e.TeacherSectionCourseId == tsc.TeacherSectionCourseId))
                    continue;

                var courseName = tsc.Course?.CourseName ?? "Course";
                var sectionLabel = tsc.Section != null ? FormatSectionLabel(tsc.Section) : "Section";
                context.Exams.Add(new Exam
                {
                    ExamID = Guid.NewGuid(),
                    Title = $"{createdExamTypes.First().Type} — {courseName} ({sectionLabel})",
                    ExamTypeId = createdExamTypes.First().ExamTypeId,
                    TotalMarks = 100,
                    IsPublished = false,
                    ExamDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
                    TeacherSectionCourseId = tsc.TeacherSectionCourseId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            await context.SaveChangesAsync();
        }
    }
}
