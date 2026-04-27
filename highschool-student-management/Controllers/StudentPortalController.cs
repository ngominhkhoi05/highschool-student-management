using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyHocSinh.Data;
using highschool_student_management.ViewModels;

namespace highschool_student_management.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentPortalController : Controller
    {
        private readonly AppDbContext _context;

        public StudentPortalController(AppDbContext context)
        {
            _context = context;
        }

        private async Task<int> GetCurrentStudentId()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            return await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.StudentId)
                .FirstOrDefaultAsync() ?? 0;
        }

        // GET: /StudentPortal
        public async Task<IActionResult> Index()
        {
            var studentId = await GetCurrentStudentId();
            if (studentId == 0) return Forbid();

            var student = await _context.Students.FindAsync(studentId);
            if (student == null) return Forbid();

            var currentYear = await _context.SchoolYears.FirstOrDefaultAsync(sy => sy.IsCurrent == 1);
            var semesters = currentYear != null
                ? await _context.Semesters
                    .Where(s => s.SchoolYearId == currentYear.Id)
                    .OrderBy(s => s.SemesterNumber)
                    .ToListAsync()
                : new List<QuanLyHocSinh.Models.Semester>();

            var currentSemester = semesters.LastOrDefault();

            var vm = new StudentPortalViewModel
            {
                StudentId = studentId,
                StudentCode = student.StudentCode,
                FullName = student.FullName,
                Semesters = semesters,
                CurrentSemesterId = currentSemester?.Id ?? 0
            };

            return View(vm);
        }

        // GET: /StudentPortal/Scores?semesterId=X
        public async Task<IActionResult> Scores(int? semesterId)
        {
            var studentId = await GetCurrentStudentId();
            if (studentId == 0) return Forbid();

            var student = await _context.Students.FindAsync(studentId);
            if (student == null) return Forbid();

            var currentYear = await _context.SchoolYears.FirstOrDefaultAsync(sy => sy.IsCurrent == 1);
            var semesters = currentYear != null
                ? await _context.Semesters
                    .Where(s => s.SchoolYearId == currentYear.Id)
                    .OrderBy(s => s.SemesterNumber)
                    .ToListAsync()
                : new List<QuanLyHocSinh.Models.Semester>();

            var selectedSemesterId = semesterId ?? semesters.LastOrDefault()?.Id ?? 0;

            var scores = await _context.Scores
                .Where(s => s.StudentId == studentId && s.SemesterId == selectedSemesterId)
                .Include(s => s.Subject)
                .Include(s => s.ScoreType)
                .OrderBy(s => s.Subject.Name)
                .ThenBy(s => s.ScoreType.Name)
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

            var vm = new StudentScoresViewModel
            {
                StudentId = studentId,
                StudentCode = student.StudentCode,
                FullName = student.FullName,
                Semesters = semesters,
                CurrentSemesterId = selectedSemesterId,
                Scores = scores
            };

            return View(vm);
        }

        // GET: /StudentPortal/Attendances?semesterId=X
        public async Task<IActionResult> Attendances(int? semesterId)
        {
            var studentId = await GetCurrentStudentId();
            if (studentId == 0) return Forbid();

            var student = await _context.Students.FindAsync(studentId);
            if (student == null) return Forbid();

            var currentYear = await _context.SchoolYears.FirstOrDefaultAsync(sy => sy.IsCurrent == 1);
            var semesters = currentYear != null
                ? await _context.Semesters
                    .Where(s => s.SchoolYearId == currentYear.Id)
                    .OrderBy(s => s.SemesterNumber)
                    .ToListAsync()
                : new List<QuanLyHocSinh.Models.Semester>();

            var selectedSemesterId = semesterId ?? semesters.LastOrDefault()?.Id ?? 0;

            var semesterStart = semesters.FirstOrDefault(s => s.Id == selectedSemesterId)?.StartDate;
            var semesterEnd = semesters.FirstOrDefault(s => s.Id == selectedSemesterId)?.EndDate;

            var attendances = await _context.Attendances
                .Where(a => a.StudentId == studentId
                    && a.Date >= (semesterStart ?? DateOnly.MinValue)
                    && a.Date <= (semesterEnd ?? DateOnly.MaxValue))
                .OrderBy(a => a.Date)
                .ThenBy(a => a.Period)
                .Select(a => new StudentAttendanceViewModel
                {
                    Date = a.Date,
                    Period = a.Period,
                    Status = a.Status ?? 1,
                    Note = a.Note
                })
                .ToListAsync();

            var vm = new StudentAttendancesViewModel
            {
                StudentId = studentId,
                StudentCode = student.StudentCode,
                FullName = student.FullName,
                Semesters = semesters,
                CurrentSemesterId = selectedSemesterId,
                Attendances = attendances
            };

            return View(vm);
        }

        // GET: /StudentPortal/Conduct?semesterId=X
        public async Task<IActionResult> Conduct(int? semesterId)
        {
            var studentId = await GetCurrentStudentId();
            if (studentId == 0) return Forbid();

            var student = await _context.Students.FindAsync(studentId);
            if (student == null) return Forbid();

            var currentYear = await _context.SchoolYears.FirstOrDefaultAsync(sy => sy.IsCurrent == 1);
            var semesters = currentYear != null
                ? await _context.Semesters
                    .Where(s => s.SchoolYearId == currentYear.Id)
                    .OrderBy(s => s.SemesterNumber)
                    .ToListAsync()
                : new List<QuanLyHocSinh.Models.Semester>();

            var selectedSemesterId = semesterId ?? semesters.LastOrDefault()?.Id ?? 0;

            var conduct = await _context.ConductScores
                .Where(cs => cs.StudentId == studentId && cs.SemesterId == selectedSemesterId)
                .Select(cs => new ConductDetailViewModel
                {
                    ConductRank = cs.ConductRank ?? 0,
                    Score = cs.Score,
                    Note = cs.Note,
                    EvaluatedAt = cs.EvaluatedAt
                })
                .FirstOrDefaultAsync();

            var violations = await _context.Violations
                .Where(v => v.StudentId == studentId && v.SemesterId == selectedSemesterId)
                .Include(v => v.ViolationType)
                .OrderByDescending(v => v.ViolationDate)
                .Select(v => new StudentViolationViewModel
                {
                    ViolationDate = v.ViolationDate,
                    ViolationTypeName = v.ViolationType.Name,
                    Severity = v.ViolationType.Severity ?? 1,
                    Description = v.Description,
                    ActionTaken = v.ActionTaken
                })
                .ToListAsync();

            var vm = new StudentConductViewModel
            {
                StudentId = studentId,
                StudentCode = student.StudentCode,
                FullName = student.FullName,
                Semesters = semesters,
                CurrentSemesterId = selectedSemesterId,
                ConductScore = conduct,
                Violations = violations
            };

            return View(vm);
        }
    }
}
