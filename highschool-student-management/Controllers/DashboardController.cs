using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyHocSinh.Data;
using highschool_student_management.ViewModels;

namespace highschool_student_management.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Dashboard (Admin Dashboard)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var currentYear = await _context.SchoolYears.FirstOrDefaultAsync(sy => sy.IsCurrent == 1);

            // Dem so lieu tong quan
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

            var schoolYearName = currentYear?.Name ?? "Chua co nam hoc";

            // Thong ke xep loai hoc luc tu ket qua hoc ky cuoi cung
            var latestSemester = currentYear != null
                ? await _context.Semesters
                    .Where(s => s.SchoolYearId == currentYear.Id)
                    .OrderByDescending(s => s.SemesterNumber)
                    .FirstOrDefaultAsync()
                : null;

            var academicRankStats = new List<AcademicRankStatViewModel>();

            if (latestSemester != null)
            {
                academicRankStats = await _context.SemesterResults
                    .Where(sr => sr.SemesterId == latestSemester.Id && sr.SubjectId == null)
                    .GroupBy(sr => sr.AcademicRank)
                    .Select(g => new AcademicRankStatViewModel
                    {
                        Rank = g.Key ?? 0,
                        Count = g.Count()
                    })
                    .ToListAsync();
            }

            var viewModel = new DashboardViewModel
            {
                TotalStudents = totalStudents,
                TotalTeachers = totalTeachers,
                TotalClasses = totalClasses,
                SchoolYearName = schoolYearName,
                SemesterName = latestSemester?.Name ?? "",
                AcademicRankStats = academicRankStats
            };

            return View(viewModel);
        }

        // GET: /Dashboard/Student (Tra cuu cho Hoc sinh - chi doc)
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Student()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var student = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.Student)
                .FirstOrDefaultAsync();

            if (student == null)
                return Forbid();

            var currentYear = await _context.SchoolYears.FirstOrDefaultAsync(sy => sy.IsCurrent == 1);
            var semesters = currentYear != null
                ? await _context.Semesters
                    .Where(s => s.SchoolYearId == currentYear.Id)
                    .OrderBy(s => s.SemesterNumber)
                    .ToListAsync()
                : new List<QuanLyHocSinh.Models.Semester>();

            var currentSemester = semesters.LastOrDefault();

            var viewModel = new StudentPortalViewModel
            {
                StudentId = student.Id,
                StudentCode = student.StudentCode,
                FullName = student.FullName,
                Semesters = semesters,
                CurrentSemesterId = currentSemester?.Id ?? 0
            };

            return View(viewModel);
        }

        // GET: /Dashboard/Parent (Tra cuu cho Phu huynh - chi doc)
        [Authorize(Roles = "Parent")]
        public async Task<IActionResult> Parent()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var parent = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.Parent)
                .FirstOrDefaultAsync();

            if (parent == null)
                return Forbid();

            var children = await _context.ParentStudents
                .Where(ps => ps.ParentId == parent.Id)
                .Include(ps => ps.Student)
                .Select(ps => new StudentDropdown
                {
                    StudentId = ps.Student.Id,
                    StudentCode = ps.Student.StudentCode,
                    FullName = ps.Student.FullName,
                    ClassId = 0,
                    ClassName = ""
                })
                .ToListAsync();

            var currentYear = await _context.SchoolYears.FirstOrDefaultAsync(sy => sy.IsCurrent == 1);
            var semesters = currentYear != null
                ? await _context.Semesters
                    .Where(s => s.SchoolYearId == currentYear.Id)
                    .OrderBy(s => s.SemesterNumber)
                    .ToListAsync()
                : new List<QuanLyHocSinh.Models.Semester>();

            var currentSemester = semesters.LastOrDefault();

            var viewModel = new ParentPortalViewModel
            {
                ParentId = parent.Id,
                ParentName = parent.FullName,
                Children = children,
                SelectedStudentId = children.FirstOrDefault()?.StudentId ?? 0,
                Semesters = semesters,
                CurrentSemesterId = currentSemester?.Id ?? 0
            };

            return View(viewModel);
        }
    }
}
