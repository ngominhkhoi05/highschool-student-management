using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyHocSinh.Data;
using highschool_student_management.ViewModels;

namespace highschool_student_management.Controllers
{
    [Authorize(Roles = "Parent")]
    public class ParentPortalController : Controller
    {
        private readonly AppDbContext _context;

        public ParentPortalController(AppDbContext context)
        {
            _context = context;
        }

        private async Task<int> GetCurrentParentId()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            return await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.ParentId)
                .FirstOrDefaultAsync() ?? 0;
        }

        private async Task<bool> ValidateChildAccess(int parentId, int studentId)
        {
            return await _context.ParentStudents
                .AnyAsync(ps => ps.ParentId == parentId && ps.StudentId == studentId);
        }

        // GET: /ParentPortal?studentId=X
        public async Task<IActionResult> Index(int? studentId)
        {
            var parentId = await GetCurrentParentId();
            if (parentId == 0) return Forbid();

            var parent = await _context.Parents.FindAsync(parentId);
            if (parent == null) return Forbid();

            var children = await _context.ParentStudents
                .Where(ps => ps.ParentId == parentId)
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

            if (!children.Any())
                return View("Error", new { message = "Ban khong co thong tin hoc sinh lien ket." });

            var selectedStudentId = studentId ?? children.First().StudentId;

            var currentYear = await _context.SchoolYears.FirstOrDefaultAsync(sy => sy.IsCurrent == 1);
            var semesters = currentYear != null
                ? await _context.Semesters
                    .Where(s => s.SchoolYearId == currentYear.Id)
                    .OrderBy(s => s.SemesterNumber)
                    .ToListAsync()
                : new List<QuanLyHocSinh.Models.Semester>();

            var currentSemester = semesters.LastOrDefault();

            var vm = new ParentPortalViewModel
            {
                ParentId = parentId,
                ParentName = parent.FullName,
                Children = children,
                SelectedStudentId = selectedStudentId,
                Semesters = semesters,
                CurrentSemesterId = currentSemester?.Id ?? 0
            };

            return View(vm);
        }

        // GET: /ParentPortal/Scores?studentId=X&semesterId=Y
        public async Task<IActionResult> Scores(int? studentId, int? semesterId)
        {
            var parentId = await GetCurrentParentId();
            if (parentId == 0) return Forbid();

            var parent = await _context.Parents.FindAsync(parentId);
            if (parent == null) return Forbid();

            var children = await _context.ParentStudents
                .Where(ps => ps.ParentId == parentId)
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

            if (!children.Any())
                return View("Error", new { message = "Ban khong co thong tin hoc sinh lien ket." });

            var selectedStudentId = studentId ?? children.First().StudentId;

            // Bao mat: kiem tra hoc sinh co thuoc phu huynh nay khong
            if (!await ValidateChildAccess(parentId, selectedStudentId))
                return Forbid();

            var student = await _context.Students.FindAsync(selectedStudentId);
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
                .Where(s => s.StudentId == selectedStudentId && s.SemesterId == selectedSemesterId)
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

            var vm = new ParentScoresViewModel
            {
                ParentId = parentId,
                ParentName = parent.FullName,
                Children = children,
                SelectedStudentId = selectedStudentId,
                StudentCode = student.StudentCode,
                FullName = student.FullName,
                Semesters = semesters,
                CurrentSemesterId = selectedSemesterId,
                Scores = scores
            };

            return View(vm);
        }

        // GET: /ParentPortal/Attendances?studentId=X&semesterId=Y
        public async Task<IActionResult> Attendances(int? studentId, int? semesterId)
        {
            var parentId = await GetCurrentParentId();
            if (parentId == 0) return Forbid();

            var parent = await _context.Parents.FindAsync(parentId);
            if (parent == null) return Forbid();

            var children = await _context.ParentStudents
                .Where(ps => ps.ParentId == parentId)
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

            if (!children.Any())
                return View("Error", new { message = "Ban khong co thong tin hoc sinh lien ket." });

            var selectedStudentId = studentId ?? children.First().StudentId;

            if (!await ValidateChildAccess(parentId, selectedStudentId))
                return Forbid();

            var student = await _context.Students.FindAsync(selectedStudentId);
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
                .Where(a => a.StudentId == selectedStudentId
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

            var vm = new ParentAttendancesViewModel
            {
                ParentId = parentId,
                ParentName = parent.FullName,
                Children = children,
                SelectedStudentId = selectedStudentId,
                StudentCode = student.StudentCode,
                FullName = student.FullName,
                Semesters = semesters,
                CurrentSemesterId = selectedSemesterId,
                Attendances = attendances
            };

            return View(vm);
        }

        // GET: /ParentPortal/Conduct?studentId=X&semesterId=Y
        public async Task<IActionResult> Conduct(int? studentId, int? semesterId)
        {
            var parentId = await GetCurrentParentId();
            if (parentId == 0) return Forbid();

            var parent = await _context.Parents.FindAsync(parentId);
            if (parent == null) return Forbid();

            var children = await _context.ParentStudents
                .Where(ps => ps.ParentId == parentId)
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

            if (!children.Any())
                return View("Error", new { message = "Ban khong co thong tin hoc sinh lien ket." });

            var selectedStudentId = studentId ?? children.First().StudentId;

            if (!await ValidateChildAccess(parentId, selectedStudentId))
                return Forbid();

            var student = await _context.Students.FindAsync(selectedStudentId);
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
                .Where(cs => cs.StudentId == selectedStudentId && cs.SemesterId == selectedSemesterId)
                .Select(cs => new ConductDetailViewModel
                {
                    ConductRank = cs.ConductRank ?? 0,
                    Score = cs.Score,
                    Note = cs.Note,
                    EvaluatedAt = cs.EvaluatedAt
                })
                .FirstOrDefaultAsync();

            var violations = await _context.Violations
                .Where(v => v.StudentId == selectedStudentId && v.SemesterId == selectedSemesterId)
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

            var vm = new ParentConductViewModel
            {
                ParentId = parentId,
                ParentName = parent.FullName,
                Children = children,
                SelectedStudentId = selectedStudentId,
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
