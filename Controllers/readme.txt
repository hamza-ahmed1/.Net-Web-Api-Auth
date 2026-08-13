Controllers/

├── Auth/
│   ├── AuthController.cs
│   │   ├── Register
│   │   ├── Login
│   │   ├── Refresh
│   │   ├── Logout
│   │   ├── ForgotPassword
│   │   └── ResetPassword
│
├── Admin/
│   ├── TeachersController.cs          // Create, Update, Delete teachers
│   ├── StudentsController.cs          // Create, Update, Delete students
│   ├── HodsController.cs              // Manage HOD accounts
│   ├── CoursesController.cs           // CRUD Courses
│   ├── SectionsController.cs          // CRUD Sections
│   ├── DepartmentsController.cs       // CRUD Departments
│   ├── ProgramsController.cs          // BSCS, BBA, etc.
│   ├── SemestersController.cs         // Spring/Fall
│   ├── AssignmentsController.cs       // Assign teacher to course/section
│   ├── RolesController.cs             // Manage Roles
│   ├── PermissionsController.cs       // Optional RBAC
│   └── DashboardController.cs
│
├── HOD/
│   ├── TeachersController.cs          // View department teachers
│   ├── CoursesController.cs           // Assign courses
│   ├── SectionsController.cs          // Manage sections
│   ├── TimetableController.cs         // Schedule classes
│   ├── AttendanceReportsController.cs // View attendance reports
│   ├── ResultsController.cs           // Approve results
│   └── DashboardController.cs
│
├── Teacher/
│   ├── ProfileController.cs           // My Profile
│   ├── CoursesController.cs           // My Courses
│   ├── SectionsController.cs          // My Sections
│   ├── AttendanceController.cs        // Mark/Edit Attendance
│   ├── QuizController.cs              // Create quizzes
│   ├── AssignmentController.cs        // Create assignments
│   ├── ExamsController.cs             // Mid/Final exams
│   ├── MarksController.cs             // Upload marks
│   ├── GradeController.cs             // Calculate grades
│   ├── MaterialsController.cs         // Notes/Lectures
│   ├── AnnouncementsController.cs     // Class announcements
│   └── DashboardController.cs
│
├── Student/
│   ├── ProfileController.cs           // My Profile
│   ├── CoursesController.cs           // My Registered Courses
│   ├── AttendanceController.cs        // My Attendance
│   ├── QuizController.cs              // View quizzes
│   ├── AssignmentController.cs        // Submit assignments
│   ├── ExamsController.cs             // Exam schedule
│   ├── ResultsController.cs           // View GPA/Grades
│   ├── TimetableController.cs         // My timetable
│   ├── FeeController.cs               // Fee details
│   └── DashboardController.cs
│
├── Common/
│   ├── NotificationsController.cs
│   ├── FilesController.cs
│   └── LookupController.cs
│
└── HealthController.cs



docker build -t hamzakhankhan/auth-api:v1 .
docker push hamzakhankhan/auth-api:v1