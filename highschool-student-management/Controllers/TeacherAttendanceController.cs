using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyHocSinh.Data;
using highschool_student_management.ViewModels;
using System.Security.Claims;

namespace highschool_student_management.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherAttendanceController : Controller
    {
        private readonly AppDbContext _context;

        public TeacherAttendanceController(AppDbContext context)
        {
            _context = context;
        }

        // Lay TeacherId tu tai khoan dang nhap
        private int GetCurrentTeacherId()
        {
            // Uu tien doc tu Claim duoc set luc dang nhap
            var teacherIdClaim = User.FindFirstValue("TeacherId");
            if (!string.IsNullOrEmpty(teacherIdClaim) && int.TryParse(teacherIdClaim, out int teacherId) && teacherId > 0)
                return teacherId;

            // Fallback: doc NameIdentifier (User.Id) roi tim TeacherId trong DB
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return 0;

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            return user?.TeacherId ?? 0;
        }

        // GET: /TeacherAttendance
        public async Task<IActionResult> Index()
        {
            var teacherId = GetCurrentTeacherId();
            if (teacherId == 0)
                return Forbid();

            var teacher = await _context.Teachers.FindAsync(teacherId);
            if (teacher == null)
                return Forbid();

            // Nam hoc hien tai
            var currentYear = await _context.SchoolYears.FirstOrDefaultAsync(sy => sy.IsCurrent == 1);

            // Cac lop duoc phan cong
            var assignments = await _context.TeacherClasses
                .Where(tc => tc.TeacherId == teacherId)
                .Include(tc => tc.Class)
                .Include(tc => tc.Semester)
                    .ThenInclude(s => s.SchoolYear)
                .Include(tc => tc.Subject)
                .ToListAsync();

            // Lay cac lop cua nam hoc hien tai
            var classAssignments = assignments
                .Where(tc => tc.Semester.SchoolYear.IsCurrent == 1)
                .GroupBy(tc => new { tc.ClassId, tc.SemesterId })
                .Select(g => new ClassAssignmentItem
                {
                    ClassId = g.Key.ClassId,
                    ClassName = g.First().Class.Name,
                    GradeLevel = g.First().Class.GradeLevel ?? 0,
                    SchoolYearName = g.First().Semester.SchoolYear.Name,
                    Subjects = g.Select(tc => tc.Subject.Name).Distinct().ToList()
                })
                .ToList();

            var model = new TeacherDashboardViewModel
            {
                TeacherId = teacherId,
                TeacherName = teacher.FullName,
                Avatar = teacher.Avatar,
                TotalClasses = classAssignments.Select(c => c.ClassId).Distinct().Count(),
                TotalSubjects = assignments.Select(tc => tc.SubjectId).Distinct().Count(),
                CurrentSchoolYearId = currentYear?.Id ?? 0,
                CurrentSchoolYearName = currentYear?.Name ?? "Khong co",
                ClassAssignments = classAssignments
            };

            ViewData["Title"] = "So diem danh";
            return View(model);
        }

        // GET: /TeacherAttendance/GetFilterData
        public async Task<IActionResult> GetFilterData()
        {
            var teacherId = GetCurrentTeacherId();
            if (teacherId == 0)
                return Json(new { success = false, message = "Khong xac dinh giao vien (TeacherId = 0). Vui long lien he quan tri." });

            var currentYear = await _context.SchoolYears.FirstOrDefaultAsync(sy => sy.IsCurrent == 1);
            if (currentYear == null)
                return Json(new { success = false, message = "Khong co nam hoc hien tai." });

            // Lay cac lop duoc phan cong trong nam hoc hien tai
            var assignments = await _context.TeacherClasses
                .Where(tc => tc.TeacherId == teacherId && tc.Semester.SchoolYearId == currentYear.Id)
                .Include(tc => tc.Class)
                .Include(tc => tc.Semester)
                .Include(tc => tc.Subject)
                .ToListAsync();

            var classes = assignments
                .GroupBy(tc => new { tc.ClassId, tc.SemesterId })
                .Select(g => new ClassAssignmentDropdown
                {
                    ClassId = g.Key.ClassId,
                    SemesterId = g.Key.SemesterId,
                    ClassName = g.First().Class.Name,
                    SemesterName = g.First().Semester.Name
                })
                .ToList();

            var subjects = assignments
                .Select(tc => new SubjectDropdown { Id = tc.SubjectId, Name = tc.Subject.Name, Code = tc.Subject.Code })
                .DistinctBy(s => s.Id)
                .ToList();

            return Json(new
            {
                success = true,
                classes = classes,
                subjects = subjects,
                schoolYearId = currentYear.Id,
                schoolYearName = currentYear.Name
            });
        }

        // GET: /TeacherAttendance/GetSubjectsByClass?classId=X&semesterId=Y
        public async Task<IActionResult> GetSubjectsByClass(int classId, int semesterId)
        {
            var teacherId = GetCurrentTeacherId();
            if (teacherId == 0)
                return Json(new { success = false, message = "Khong xac dinh giao vien." });

            var subjects = await _context.TeacherClasses
                .Where(tc => tc.TeacherId == teacherId
                    && tc.ClassId == classId
                    && tc.SemesterId == semesterId)
                .Include(tc => tc.Subject)
                .Select(tc => new
                {
                    id = tc.SubjectId,
                    name = tc.Subject.Name,
                    code = tc.Subject.Code
                })
                .ToListAsync();

            return Json(new { success = true, subjects = subjects });
        }

        // GET: /TeacherAttendance/GetStudentListForAttendance?classId=X&subjectId=Y&date=Z&period=W
        public async Task<IActionResult> GetStudentListForAttendance(int classId, int subjectId, DateOnly date, int period)
        {
            var teacherId = GetCurrentTeacherId();
            if (teacherId == 0)
                return Json(new { success = false, message = "Khong xac dinh giao vien." });

            // Lay danh sach hoc sinh trong lop theo nam hoc hien tai
            var currentYear = await _context.SchoolYears.FirstOrDefaultAsync(sy => sy.IsCurrent == 1);
            if (currentYear == null)
                return Json(new { success = false, message = "Khong co nam hoc hien tai." });

            var studentClassRecords = await _context.StudentClasses
                .Where(sc => sc.ClassId == classId && sc.SchoolYearId == currentYear.Id)
                .Include(sc => sc.Student)
                .ToListAsync();

            if (!studentClassRecords.Any())
                return Json(new { success = false, message = "Lop nay chua co hoc sinh nao." });

            var studentIds = studentClassRecords.Select(sc => sc.StudentId).ToList();

            // Lay diem danh da co (neu co)
            var existingAttendances = await _context.Attendances
                .Where(a => studentIds.Contains(a.StudentId)
                    && a.ClassId == classId
                    && a.SubjectId == (subjectId > 0 ? subjectId : (int?)null)
                    && a.Date == date
                    && a.Period == period)
                .ToDictionaryAsync(a => a.StudentId);

            var items = studentClassRecords
                .Select(sc => new StudentAttendanceItem
                {
                    StudentId = sc.StudentId,
                    StudentCode = sc.Student.StudentCode,
                    FullName = sc.Student.FullName,
                    Gender = sc.Student.Gender,
                    ExistingAttendanceId = existingAttendances.TryGetValue(sc.StudentId, out var att) ? att.Id : 0,
                    ExistingStatus = existingAttendances.TryGetValue(sc.StudentId, out att) ? att.Status : null,
                    ExistingNote = existingAttendances.TryGetValue(sc.StudentId, out att) ? att.Note : null
                })
                .ToList();

            return PartialView("_StudentListForAttendance", items);
        }

        public class SaveAttendanceRequest
        {
            public int ClassId { get; set; }
            public int SubjectId { get; set; }
            public DateOnly Date { get; set; }
            public int Period { get; set; }
            public List<AttendanceSaveItem> Items { get; set; } = new();
        }

        // POST: /TeacherAttendance/SaveBulkAttendance
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveBulkAttendance([FromBody] SaveAttendanceRequest request)
        {
            var teacherId = GetCurrentTeacherId();
            if (teacherId == 0)
                return Json(new { success = false, message = "Khong xac dinh giao vien." });

            try
            {
                var items = request.Items;
                var classId = request.ClassId;
                var subjectId = request.SubjectId;
                var date = request.Date;
                var period = request.Period;

                if (items == null || !items.Any())
                    return Json(new { success = false, message = "Khong co du lieu de luu." });

                var currentYear = await _context.SchoolYears.FirstOrDefaultAsync(sy => sy.IsCurrent == 1);
                if (currentYear == null)
                    return Json(new { success = false, message = "Khong co nam hoc hien tai." });

                var now = DateTime.Now;

                foreach (var item in items.Where(i => i.Status > 0))
                {
                    // Tim ban ghi da co
                    var existing = await _context.Attendances.FirstOrDefaultAsync(a =>
                        a.StudentId == item.StudentId &&
                        a.ClassId == classId &&
                        a.SubjectId == (subjectId > 0 ? subjectId : (int?)null) &&
                        a.Date == date &&
                        a.Period == period);

                    if (existing != null)
                    {
                        existing.Status = item.Status;
                        existing.Note = item.Note;
                        existing.RecordedBy = teacherId;
                    }
                    else
                    {
                        _context.Attendances.Add(new QuanLyHocSinh.Models.Attendance
                        {
                            StudentId = item.StudentId,
                            ClassId = classId,
                            SubjectId = subjectId > 0 ? subjectId : null,
                            Date = date,
                            Period = period,
                            Status = item.Status,
                            Note = item.Note,
                            RecordedBy = teacherId,
                            CreatedAt = now
                        });
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Luu diem danh thanh cong." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Da xay ra loi: " + ex.Message });
            }
        }
    }
}
