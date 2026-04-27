using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyHocSinh.Data;
using highschool_student_management.ViewModels;

namespace highschool_student_management.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class HomeroomController : Controller
    {
        private readonly AppDbContext _context;

        public HomeroomController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCurrentTeacherId()
        {
            var teacherIdClaim = User.FindFirstValue("TeacherId");
            if (!string.IsNullOrEmpty(teacherIdClaim) && int.TryParse(teacherIdClaim, out int teacherId) && teacherId > 0)
                return teacherId;

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return 0;

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            return user?.TeacherId ?? 0;
        }

        // GET: /Homeroom/MyClass
        public async Task<IActionResult> MyClass()
        {
            var teacherId = GetCurrentTeacherId();
            if (teacherId == 0)
                return Forbid();

            var currentYear = await _context.SchoolYears.FirstOrDefaultAsync(sy => sy.IsCurrent == 1);
            if (currentYear == null)
                return View("Error", new { message = "Khong co nam hoc hien tai." });

            // Lay lop chu nhiem cua giao vien trong nam hoc hien tai
            var homeroomClass = await _context.Classes
                .Where(c => c.HomeroomTeacherId == teacherId && c.SchoolYearId == currentYear.Id)
                .Include(c => c.StudentClasses)
                    .ThenInclude(sc => sc.Student)
                .FirstOrDefaultAsync();

            if (homeroomClass == null)
            {
                ViewData["Title"] = "Lop chu nhiem";
                ViewData["NoClass"] = true;
                return View();
            }

            var model = new HomeroomClassViewModel
            {
                ClassId = homeroomClass.Id,
                ClassName = homeroomClass.Name,
                GradeLevel = homeroomClass.GradeLevel,
                Room = homeroomClass.Room,
                MaxStudents = homeroomClass.MaxStudents,
                SchoolYearName = currentYear.Name,
                StudentCount = homeroomClass.StudentClasses.Count,
                Students = homeroomClass.StudentClasses
                    .Select(sc => new HomeroomStudentItem
                    {
                        StudentId = sc.StudentId,
                        StudentCode = sc.Student.StudentCode,
                        FullName = sc.Student.FullName,
                        Gender = sc.Student.Gender,
                        DateOfBirth = sc.Student.DateOfBirth,
                        Phone = sc.Student.Phone,
                        Status = sc.Student.Status
                    })
                    .OrderBy(s => s.FullName)
                    .ToList()
            };

            ViewData["Title"] = "Lop chu nhiem";
            return View(model);
        }

        // ========== VI PHAM ==========

        // GET: /Homeroom/Violations
        public async Task<IActionResult> Violations()
        {
            var teacherId = GetCurrentTeacherId();
            if (teacherId == 0)
                return Forbid();

            var currentYear = await _context.SchoolYears.FirstOrDefaultAsync(sy => sy.IsCurrent == 1);
            if (currentYear == null)
                return View("Error", new { message = "Khong co nam hoc hien tai." });

            var semesters = await _context.Semesters
                .Where(s => s.SchoolYearId == currentYear.Id)
                .OrderBy(s => s.SemesterNumber)
                .Select(s => new SemesterDropdown { Id = s.Id, Name = s.Name, SchoolYearId = s.SchoolYearId })
                .ToListAsync();

            var violationTypes = await _context.ViolationTypes
                .OrderBy(v => v.Severity)
                .ThenBy(v => v.Name)
                .Select(v => new ViolationTypeDropdown
                {
                    Id = v.Id,
                    Name = v.Name,
                    Severity = v.Severity ?? 1,
                    DeductPoints = v.DeductPoints
                })
                .ToListAsync();

            // Lay lop chu nhiem
            var homeroomClass = await _context.Classes
                .Where(c => c.HomeroomTeacherId == teacherId && c.SchoolYearId == currentYear.Id)
                .FirstOrDefaultAsync();

            var studentIdsInClass = homeroomClass != null
                ? await _context.StudentClasses
                    .Where(sc => sc.ClassId == homeroomClass.Id && sc.SchoolYearId == currentYear.Id)
                    .Select(sc => sc.StudentId)
                    .ToListAsync()
                : new List<int>();

            var selectedSemesterId = semesters.FirstOrDefault()?.Id ?? 0;

            var hcId = homeroomClass?.Id ?? 0;
            var hcName = homeroomClass?.Name ?? "";
            var hccName = homeroomClass != null ? homeroomClass.Name : "";

            var violations = await _context.Violations
                .Where(v => studentIdsInClass.Contains(v.StudentId) && v.SemesterId == selectedSemesterId)
                .Include(v => v.Student)
                .Include(v => v.ViolationType)
                .Include(v => v.Semester)
                .OrderByDescending(v => v.ViolationDate)
                .Select(v => new ViolationIndexViewModel
                {
                    Id = v.Id,
                    StudentId = v.StudentId,
                    StudentCode = v.Student.StudentCode,
                    StudentName = v.Student.FullName,
                    ClassId = hcId,
                    ClassName = hcName,
                    ViolationTypeId = v.ViolationTypeId,
                    ViolationTypeName = v.ViolationType.Name,
                    Severity = v.ViolationType.Severity ?? 1,
                    DeductPoints = v.ViolationType.DeductPoints,
                    ViolationDate = v.ViolationDate,
                    Description = v.Description,
                    ActionTaken = v.ActionTaken,
                    SemesterId = v.SemesterId,
                    SemesterName = v.Semester.Name
                })
                .ToListAsync();

            var studentList = await _context.StudentClasses
                .Where(sc => homeroomClass != null && sc.ClassId == homeroomClass.Id && sc.SchoolYearId == currentYear.Id)
                .Include(sc => sc.Student)
                .Select(sc => new StudentDropdown
                {
                    StudentId = sc.StudentId,
                    StudentCode = sc.Student.StudentCode,
                    FullName = sc.Student.FullName,
                    ClassId = sc.ClassId,
                    ClassName = hccName
                })
                .OrderBy(s => s.FullName)
                .ToListAsync();

            ViewData["Semesters"] = semesters;
            ViewData["ViolationTypes"] = violationTypes;
            ViewData["Students"] = studentList;
            ViewData["SelectedSemesterId"] = selectedSemesterId;
            ViewData["ClassId"] = hcId;
            ViewData["ClassName"] = hcName;
            ViewData["Title"] = "Theo doi vi pham";
            return View(violations);
        }

        // GET: /Homeroom/GetViolationsBySemester?semesterId=X
        public async Task<IActionResult> GetViolationsBySemester(int semesterId)
        {
            var teacherId = GetCurrentTeacherId();
            if (teacherId == 0)
                return Json(new { success = false, message = "Khong xac dinh giao vien." });

            var currentYear = await _context.SchoolYears.FirstOrDefaultAsync(sy => sy.IsCurrent == 1);
            if (currentYear == null)
                return Json(new { success = false, message = "Khong co nam hoc hien tai." });

            var homeroomClass = await _context.Classes
                .Where(c => c.HomeroomTeacherId == teacherId && c.SchoolYearId == currentYear.Id)
                .FirstOrDefaultAsync();

            var studentIdsInClass = homeroomClass != null
                ? await _context.StudentClasses
                    .Where(sc => sc.ClassId == homeroomClass.Id && sc.SchoolYearId == currentYear.Id)
                    .Select(sc => sc.StudentId)
                    .ToListAsync()
                : new List<int>();

            var homeroomClassId = homeroomClass?.Id ?? 0;
            var homeroomClassName = homeroomClass?.Name ?? "";

            var violations = await _context.Violations
                .Where(v => studentIdsInClass.Contains(v.StudentId) && v.SemesterId == semesterId)
                .Include(v => v.Student)
                .Include(v => v.ViolationType)
                .Include(v => v.Semester)
                .OrderByDescending(v => v.ViolationDate)
                .Select(v => new ViolationIndexViewModel
                {
                    Id = v.Id,
                    StudentId = v.StudentId,
                    StudentCode = v.Student.StudentCode,
                    StudentName = v.Student.FullName,
                    ClassId = homeroomClassId,
                    ClassName = homeroomClassName,
                    ViolationTypeId = v.ViolationTypeId,
                    ViolationTypeName = v.ViolationType.Name,
                    Severity = v.ViolationType.Severity ?? 1,
                    DeductPoints = v.ViolationType.DeductPoints,
                    ViolationDate = v.ViolationDate,
                    Description = v.Description,
                    ActionTaken = v.ActionTaken,
                    SemesterId = v.SemesterId,
                    SemesterName = v.Semester.Name
                })
                .ToListAsync();

            return PartialView("_ViolationList", violations);
        }

        // POST: /Homeroom/CreateViolation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateViolation([FromBody] ViolationCreateViewModel model)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] CreateViolation called. SemesterId={model.SemesterId}, StudentId={model.StudentId}, ViolationTypeId={model.ViolationTypeId}, ViolationDate={model.ViolationDate}");
            var teacherId = GetCurrentTeacherId();
            if (teacherId == 0)
                return Json(new { success = false, message = "Khong xac dinh giao vien." });

            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Json(new { success = false, message = errors });
            }

            if (model.SemesterId <= 0)
                return Json(new { success = false, message = "Vui long chon hoc ky hop le." });

            var semesterExists = await _context.Semesters.AnyAsync(s => s.Id == model.SemesterId);
            if (!semesterExists)
                return Json(new { success = false, message = "Hoc ky khong ton tai. Vui long chon lai." });

            try
            {
                var violation = new QuanLyHocSinh.Models.Violation
                {
                    StudentId = model.StudentId,
                    ViolationTypeId = model.ViolationTypeId,
                    SemesterId = model.SemesterId,
                    ViolationDate = model.ViolationDate,
                    Description = model.Description,
                    ActionTaken = model.ActionTaken,
                    RecordedBy = teacherId,
                    CreatedAt = DateTime.Now
                };

                _context.Violations.Add(violation);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Ghi nhan vi pham thanh cong." });
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException;
                var detail = inner != null ? " [" + inner.GetType().Name + ": " + inner.Message + "]" : "";
                return Json(new { success = false, message = "Loi: " + ex.Message + detail });
            }
        }

        // DELETE: /Homeroom/DeleteViolation/{id}
        [HttpDelete]
        public async Task<IActionResult> DeleteViolation(int id)
        {
            var teacherId = GetCurrentTeacherId();
            if (teacherId == 0)
                return Json(new { success = false, message = "Khong xac dinh giao vien." });

            var violation = await _context.Violations.FindAsync(id);
            if (violation == null)
                return Json(new { success = false, message = "Khong tim thay vi pham." });

            _context.Violations.Remove(violation);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Xoa vi pham thanh cong." });
        }

        // ========== HANH KIEM ==========

        // GET: /Homeroom/ConductScores
        public async Task<IActionResult> ConductScores()
        {
            var teacherId = GetCurrentTeacherId();
            if (teacherId == 0)
                return Forbid();

            var currentYear = await _context.SchoolYears.FirstOrDefaultAsync(sy => sy.IsCurrent == 1);
            if (currentYear == null)
                return View("Error", new { message = "Khong co nam hoc hien tai." });

            var semesters = await _context.Semesters
                .Where(s => s.SchoolYearId == currentYear.Id)
                .OrderBy(s => s.SemesterNumber)
                .Select(s => new SemesterDropdown { Id = s.Id, Name = s.Name, SchoolYearId = s.SchoolYearId })
                .ToListAsync();

            var homeroomClass = await _context.Classes
                .Where(c => c.HomeroomTeacherId == teacherId && c.SchoolYearId == currentYear.Id)
                .FirstOrDefaultAsync();

            var selectedSemesterId = semesters.FirstOrDefault()?.Id ?? 0;

            var studentRecords = await _context.StudentClasses
                .Where(sc => homeroomClass != null && sc.ClassId == homeroomClass.Id && sc.SchoolYearId == currentYear.Id)
                .Include(sc => sc.Student)
                .Include(sc => sc.Student.Violations)
                .Select(sc => sc.Student)
                .OrderBy(s => s.FullName)
                .ToListAsync();

            var existingConductScores = await _context.ConductScores
                .Where(cs => cs.SemesterId == selectedSemesterId)
                .ToDictionaryAsync(cs => cs.StudentId);

            var conductList = new List<ConductScoreIndexViewModel>();
            foreach (var student in studentRecords)
            {
                var semesterViolationIds = student.Violations
                    .Where(v => v.SemesterId == selectedSemesterId)
                    .Select(v => v.Id)
                    .ToList();

                var hasViolation = semesterViolationIds.Any();

                existingConductScores.TryGetValue(student.Id, out var cs);
                var classId = homeroomClass?.Id ?? 0;
                var className = homeroomClass?.Name ?? "";

                conductList.Add(new ConductScoreIndexViewModel
                {
                    Id = cs?.Id ?? 0,
                    StudentId = student.Id,
                    StudentCode = student.StudentCode,
                    StudentName = student.FullName,
                    ClassId = classId,
                    ClassName = className,
                    SemesterId = selectedSemesterId,
                    SemesterName = semesters.FirstOrDefault(s => s.Id == selectedSemesterId)?.Name ?? "",
                    ConductRank = cs?.ConductRank,
                    Score = cs?.Score,
                    Note = cs?.Note,
                    HasViolationRecord = hasViolation
                });
            }

            ViewData["Semesters"] = semesters;
            ViewData["SelectedSemesterId"] = selectedSemesterId;
            ViewData["ClassId"] = homeroomClass?.Id ?? 0;
            ViewData["ClassName"] = homeroomClass?.Name ?? "";
            ViewData["Title"] = "Danh gia hanh kiem";
            return View(conductList);
        }

        // GET: /Homeroom/GetConductScoresBySemester?semesterId=X
        public async Task<IActionResult> GetConductScoresBySemester(int semesterId)
        {
            var teacherId = GetCurrentTeacherId();
            if (teacherId == 0)
                return Json(new { success = false, message = "Khong xac dinh giao vien." });

            var currentYear = await _context.SchoolYears.FirstOrDefaultAsync(sy => sy.IsCurrent == 1);
            if (currentYear == null)
                return Json(new { success = false, message = "Khong co nam hoc hien tai." });

            var semesterName = (await _context.Semesters.FindAsync(semesterId))?.Name ?? "";

            var homeroomClass = await _context.Classes
                .Where(c => c.HomeroomTeacherId == teacherId && c.SchoolYearId == currentYear.Id)
                .FirstOrDefaultAsync();

            var studentRecords = await _context.StudentClasses
                .Where(sc => homeroomClass != null && sc.ClassId == homeroomClass.Id && sc.SchoolYearId == currentYear.Id)
                .Include(sc => sc.Student)
                    .ThenInclude(s => s.Violations)
                .Select(sc => sc.Student)
                .OrderBy(s => s.FullName)
                .ToListAsync();

            var existingConductScores = await _context.ConductScores
                .Where(cs => cs.SemesterId == semesterId)
                .ToDictionaryAsync(cs => cs.StudentId);

            var classId = homeroomClass?.Id ?? 0;
            var className = homeroomClass?.Name ?? "";

            var conductList = studentRecords.Select(student =>
            {
                var hasViolation = student.Violations.Any(v => v.SemesterId == semesterId);
                existingConductScores.TryGetValue(student.Id, out var cs);
                return new ConductScoreIndexViewModel
                {
                    Id = cs?.Id ?? 0,
                    StudentId = student.Id,
                    StudentCode = student.StudentCode,
                    StudentName = student.FullName,
                    ClassId = classId,
                    ClassName = className,
                    SemesterId = semesterId,
                    SemesterName = semesterName,
                    ConductRank = cs?.ConductRank,
                    Score = cs?.Score,
                    Note = cs?.Note,
                    HasViolationRecord = hasViolation
                };
            }).ToList();

            return PartialView("_ConductScoreList", conductList);
        }

        // POST: /Homeroom/SaveConductScores
        [HttpPost]
        public async Task<IActionResult> SaveConductScores([FromBody] List<ConductScoreEditViewModel> items)
        {
            var teacherId = GetCurrentTeacherId();
            if (teacherId == 0)
                return Json(new { success = false, message = "Khong xac dinh giao vien." });

            if (items == null || !items.Any())
                return Json(new { success = false, message = "Khong co du lieu." });

            try
            {
                foreach (var item in items)
                {
                    var existing = await _context.ConductScores
                        .FirstOrDefaultAsync(cs => cs.Id == item.Id);

                    if (existing != null)
                    {
                        existing.ConductRank = item.ConductRank;
                        existing.Score = item.Score;
                        existing.Note = item.Note;
                        existing.EvaluatedBy = teacherId;
                        existing.EvaluatedAt = DateTime.Now;
                    }
                    else if (item.ConductRank > 0 || item.Score > 0)
                    {
                        _context.ConductScores.Add(new QuanLyHocSinh.Models.ConductScore
                        {
                            StudentId = item.StudentId,
                            SemesterId = item.SemesterId,
                            ConductRank = item.ConductRank,
                            Score = item.Score,
                            Note = item.Note,
                            EvaluatedBy = teacherId,
                            EvaluatedAt = DateTime.Now
                        });
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Luu hanh kiem thanh cong." });
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException;
                var detail = inner != null ? " [" + inner.GetType().Name + ": " + inner.Message + "]" : "";
                return Json(new { success = false, message = "Loi: " + ex.Message + detail });
            }
        }
    }
}
