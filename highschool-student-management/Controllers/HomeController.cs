using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyHocSinh.Data;
using QuanLyHocSinh.Models;
using highschool_student_management.ViewModels;
using highschool_student_management.Models;

namespace highschool_student_management.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            if (User.IsInRole("Admin"))
            {
                return await DashboardAdmin();
            }
            else if (User.IsInRole("Teacher"))
            {
                return await DashboardTeacher(userId);
            }
            else if (User.IsInRole("Student"))
            {
                return await DashboardStudent(userId);
            }
            else if (User.IsInRole("Parent"))
            {
                return await DashboardParent(userId);
            }

            return RedirectToAction("Index", "Home");
        }

        private async Task<IActionResult> DashboardAdmin()
        {
            var currentYear = await _context.SchoolYears.FirstOrDefaultAsync(sy => sy.IsCurrent == 1);
            var schoolYearName = currentYear?.Name ?? "Chưa có năm học";

            var totalStudents = currentYear != null
                ? await _context.StudentClasses
                    .Where(sc => sc.SchoolYearId == currentYear.Id)
                    .Select(sc => sc.StudentId)
                    .Distinct()
                    .CountAsync()
                : 0;

            var totalTeachers = await _context.Teachers
                .Where(t => t.Status == 1)
                .CountAsync();

            var totalClasses = currentYear != null
                ? await _context.Classes
                    .Where(c => c.SchoolYearId == currentYear.Id)
                    .CountAsync()
                : 0;

            var totalUsers = await _context.Users.CountAsync();

            var studentByGrade = new List<GradeStatViewModel>();
            if (currentYear != null)
            {
                studentByGrade = await _context.StudentClasses
                    .Where(sc => sc.SchoolYearId == currentYear.Id)
                    .Include(sc => sc.Class)
                    .GroupBy(sc => sc.Class.GradeLevel ?? 0)
                    .Select(g => new GradeStatViewModel
                    {
                        GradeLevel = g.Key,
                        StudentCount = g.Select(sc => sc.StudentId).Distinct().Count()
                    })
                    .OrderBy(g => g.GradeLevel)
                    .ToListAsync();
            }

            ViewData["Title"] = "Bảng điều khiển";
            ViewBag.TotalStudents = totalStudents;
            ViewBag.TotalTeachers = totalTeachers;
            ViewBag.TotalClasses = totalClasses;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.SchoolYearName = schoolYearName;
            ViewBag.StudentByGrade = studentByGrade;

            return View("DashboardAdmin");
        }

        private async Task<IActionResult> DashboardTeacher(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Teacher)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Teacher == null)
                return Forbid();

            var teacher = user.Teacher;
            var currentYear = await _context.SchoolYears.FirstOrDefaultAsync(sy => sy.IsCurrent == 1);
            var schoolYearName = currentYear?.Name ?? "Chưa có năm học";

            var currentSemester = currentYear != null
                ? await _context.Semesters
                    .Where(s => s.SchoolYearId == currentYear.Id)
                    .OrderByDescending(s => s.SemesterNumber)
                    .FirstOrDefaultAsync()
                : null;

            var homeroomClass = currentYear != null
                ? await _context.Classes
                    .Where(c => c.HomeroomTeacherId == teacher.Id && c.SchoolYearId == currentYear.Id)
                    .Include(c => c.StudentClasses)
                    .FirstOrDefaultAsync()
                : null;

            var homeroomStudentCount = homeroomClass?.StudentClasses?.Count ?? 0;

            var teachingClasses = new List<TeacherClassItemViewModel>();
            if (currentSemester != null)
            {
                teachingClasses = await _context.TeacherClasses
                    .Where(tc => tc.TeacherId == teacher.Id && tc.SemesterId == currentSemester.Id)
                    .Include(tc => tc.Class)
                    .Include(tc => tc.Subject)
                    .Select(tc => new TeacherClassItemViewModel
                    {
                        ClassId = tc.Class.Id,
                        ClassName = tc.Class.Name,
                        GradeLevel = tc.Class.GradeLevel ?? 0,
                        SubjectId = tc.SubjectId,
                        SubjectName = tc.Subject.Name,
                        PeriodsPerWeek = tc.PeriodsPerWeek
                    })
                    .ToListAsync();
            }

            ViewData["Title"] = "Bảng điều khiển";
            ViewBag.TeacherName = teacher.FullName;
            ViewBag.SchoolYearName = schoolYearName;
            ViewBag.CurrentSemesterName = currentSemester?.Name ?? "";
            ViewBag.IsHomeroomTeacher = homeroomClass != null;
            ViewBag.HomeroomClass = homeroomClass != null
                ? new HomeroomClassInfoViewModel
                {
                    ClassId = homeroomClass.Id,
                    ClassName = homeroomClass.Name,
                    GradeLevel = homeroomClass.GradeLevel ?? 0,
                    Room = homeroomClass.Room,
                    StudentCount = homeroomStudentCount
                }
                : null;
            ViewBag.TeachingClasses = teachingClasses;

            return View("DashboardTeacher");
        }

        private async Task<IActionResult> DashboardStudent(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Student)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Student == null)
                return Forbid();

            var student = user.Student;

            var currentYear = await _context.SchoolYears.FirstOrDefaultAsync(sy => sy.IsCurrent == 1);
            var semesters = currentYear != null
                ? await _context.Semesters
                    .Where(s => s.SchoolYearId == currentYear.Id)
                    .OrderBy(s => s.SemesterNumber)
                    .ToListAsync()
                : new List<Semester>();

            var currentSemester = semesters.LastOrDefault();

            var latestScores = new List<StudentScoreViewModel>();
            if (currentSemester != null)
            {
                latestScores = await _context.Scores
                    .Where(s => s.StudentId == student.Id && s.SemesterId == currentSemester.Id)
                    .Include(s => s.Subject)
                    .Include(s => s.ScoreType)
                    .OrderByDescending(s => s.ExamDate)
                    .Take(20)
                    .Select(s => new StudentScoreViewModel
                    {
                        SubjectId = s.SubjectId,
                        SubjectName = s.Subject.Name,
                        ScoreTypeId = s.ScoreTypeId,
                        ScoreTypeName = s.ScoreType.Name,
                        ScoreValue = s.ScoreValue,
                        ExamDate = s.ExamDate
                    })
                    .ToListAsync();
            }

            decimal? avgScore = null;
            if (currentSemester != null)
            {
                avgScore = await _context.SemesterResults
                    .Where(sr => sr.StudentId == student.Id
                        && sr.SemesterId == currentSemester.Id
                        && sr.SubjectId == null)
                    .Select(sr => sr.AverageScore)
                    .FirstOrDefaultAsync();
            }

            var attendanceStats = new StudentAttendanceSummary();
            if (currentSemester != null)
            {
                var attendances = await _context.Attendances
                    .Where(a => a.StudentId == student.Id
                        && a.Date >= currentSemester.StartDate
                        && a.Date <= currentSemester.EndDate)
                    .ToListAsync();

                attendanceStats.TotalPresent = attendances.Count(a => a.Status == 1);
                attendanceStats.TotalAbsentPermit = attendances.Count(a => a.Status == 2);
                attendanceStats.TotalAbsentNoPermit = attendances.Count(a => a.Status == 3);
                attendanceStats.TotalLate = attendances.Count(a => a.Status == 4);
                attendanceStats.TotalRecords = attendances.Count;
            }

            ViewData["Title"] = "Bảng điều khiển";
            ViewBag.StudentName = student.FullName;
            ViewBag.StudentCode = student.StudentCode;
            ViewBag.SchoolYearName = currentYear?.Name ?? "Chưa có năm học";
            ViewBag.CurrentSemesterName = currentSemester?.Name ?? "";
            ViewBag.LatestScores = latestScores;
            ViewBag.AvgScore = avgScore;
            ViewBag.AttendanceStats = attendanceStats;

            return View("DashboardStudent");
        }

        private async Task<IActionResult> DashboardParent(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Parent)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Parent == null)
                return Forbid();

            var parent = user.Parent;

            var children = await _context.ParentStudents
                .Where(ps => ps.ParentId == parent.Id)
                .Include(ps => ps.Student)
                .ThenInclude(s => s.StudentClasses)
                .Select(ps => ps.Student)
                .ToListAsync();

            var selectedStudent = children.FirstOrDefault();
            if (selectedStudent == null)
            {
                ViewData["Title"] = "Bảng điều khiển";
                ViewBag.ParentName = parent.FullName;
                ViewBag.Children = new List<ParentChildSummary>();
                ViewBag.SelectedStudentName = "";
                ViewBag.Scores = new List<StudentScoreViewModel>();
                ViewBag.AttendanceStats = new StudentAttendanceSummary();
                ViewBag.AvgScore = (decimal?)null;
                return View("DashboardStudent");
            }

            var currentYear = await _context.SchoolYears.FirstOrDefaultAsync(sy => sy.IsCurrent == 1);
            var semesters = currentYear != null
                ? await _context.Semesters
                    .Where(s => s.SchoolYearId == currentYear.Id)
                    .OrderBy(s => s.SemesterNumber)
                    .ToListAsync()
                : new List<Semester>();

            var currentSemester = semesters.LastOrDefault();

            var latestScores = new List<StudentScoreViewModel>();
            if (currentSemester != null)
            {
                latestScores = await _context.Scores
                    .Where(s => s.StudentId == selectedStudent.Id && s.SemesterId == currentSemester.Id)
                    .Include(s => s.Subject)
                    .Include(s => s.ScoreType)
                    .OrderByDescending(s => s.ExamDate)
                    .Take(20)
                    .Select(s => new StudentScoreViewModel
                    {
                        SubjectId = s.SubjectId,
                        SubjectName = s.Subject.Name,
                        ScoreTypeId = s.ScoreTypeId,
                        ScoreTypeName = s.ScoreType.Name,
                        ScoreValue = s.ScoreValue,
                        ExamDate = s.ExamDate
                    })
                    .ToListAsync();
            }

            decimal? avgScore = null;
            if (currentSemester != null)
            {
                avgScore = await _context.SemesterResults
                    .Where(sr => sr.StudentId == selectedStudent.Id
                        && sr.SemesterId == currentSemester.Id
                        && sr.SubjectId == null)
                    .Select(sr => sr.AverageScore)
                    .FirstOrDefaultAsync();
            }

            var attendanceStats = new StudentAttendanceSummary();
            if (currentSemester != null)
            {
                var attendances = await _context.Attendances
                    .Where(a => a.StudentId == selectedStudent.Id
                        && a.Date >= currentSemester.StartDate
                        && a.Date <= currentSemester.EndDate)
                    .ToListAsync();

                attendanceStats.TotalPresent = attendances.Count(a => a.Status == 1);
                attendanceStats.TotalAbsentPermit = attendances.Count(a => a.Status == 2);
                attendanceStats.TotalAbsentNoPermit = attendances.Count(a => a.Status == 3);
                attendanceStats.TotalLate = attendances.Count(a => a.Status == 4);
                attendanceStats.TotalRecords = attendances.Count;
            }

            var childSummaries = children.Select(c => new ParentChildSummary
            {
                StudentId = c.Id,
                StudentCode = c.StudentCode,
                FullName = c.FullName,
                ClassName = c.StudentClasses
                    .Where(sc => currentYear != null && sc.SchoolYearId == currentYear.Id)
                    .Select(sc => sc.Class.Name)
                    .FirstOrDefault() ?? "Chưa phân lớp"
            }).ToList();

            ViewData["Title"] = "Bảng điều khiển";
            ViewBag.ParentName = parent.FullName;
            ViewBag.Children = childSummaries;
            ViewBag.SelectedStudentName = selectedStudent.FullName;
            ViewBag.SchoolYearName = currentYear?.Name ?? "Chưa có năm học";
            ViewBag.CurrentSemesterName = currentSemester?.Name ?? "";
            ViewBag.Scores = latestScores;
            ViewBag.AvgScore = avgScore;
            ViewBag.AttendanceStats = attendanceStats;
            ViewBag.IsParentView = true;

            return View("DashboardStudent");
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            ViewData["Title"] = "Chính sách bảo mật";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

}
    }

