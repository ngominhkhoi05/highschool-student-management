using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyHocSinh.Data;
using highschool_student_management.ViewModels;

namespace highschool_student_management.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ClassController : Controller
    {
        private readonly AppDbContext _context;

        public ClassController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Class
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Classes
                .Include(c => c.SchoolYear)
                .Include(c => c.HomeroomTeacher)
                .Include(c => c.StudentClasses)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(search) ||
                    (c.Room != null && c.Room.ToLower().Contains(search)));
            }

            var classes = await query
                .OrderBy(c => c.GradeLevel)
                .ThenBy(c => c.Name)
                .Select(c => new ClassIndexViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    GradeLevel = c.GradeLevel,
                    SchoolYearId = c.SchoolYearId,
                    SchoolYearName = c.SchoolYear.Name,
                    HomeroomTeacherId = c.HomeroomTeacherId,
                    HomeroomTeacherName = c.HomeroomTeacher != null ? c.HomeroomTeacher.FullName : null,
                    MaxStudents = c.MaxStudents,
                    Room = c.Room,
                    CurrentStudentCount = c.StudentClasses.Count
                })
                .ToListAsync();

            ViewData["Search"] = search;
            ViewData["Title"] = "Lop hoc";
            return View(classes);
        }

        // GET: /Class/_CreateOrEdit?id=0 hoac id=X
        [HttpGet]
        public IActionResult _CreateOrEdit(int id)
        {
            var model = new ClassFormViewModel();

            var schoolYears = _context.SchoolYears
                .OrderByDescending(sy => sy.IsCurrent)
                .ThenByDescending(sy => sy.StartDate)
                .Select(sy => new SchoolYearOption { Id = sy.Id, Name = sy.Name })
                .ToList();

            var teachers = _context.Teachers
                .OrderBy(t => t.FullName)
                .Select(t => new TeacherOption { Id = t.Id, FullName = t.FullName, Specialization = t.Specialization })
                .ToList();

            if (id > 0)
            {
                var cls = _context.Classes.Find(id);
                if (cls == null)
                    return NotFound("Lop hoc khong ton tai.");

                model = new ClassFormViewModel
                {
                    Id = cls.Id,
                    Name = cls.Name,
                    GradeLevel = cls.GradeLevel,
                    SchoolYearId = cls.SchoolYearId,
                    HomeroomTeacherId = cls.HomeroomTeacherId,
                    MaxStudents = cls.MaxStudents,
                    Room = cls.Room,
                    SchoolYears = schoolYears,
                    Teachers = teachers
                };
            }
            else
            {
                model = new ClassFormViewModel
                {
                    SchoolYears = schoolYears,
                    Teachers = teachers
                };
            }

            return PartialView(model);
        }

        // POST: /Class/SaveClass
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveClass(ClassFormViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    return Json(new { success = false, message = errors });
                }

                if (string.IsNullOrWhiteSpace(model.Name))
                    return Json(new { success = false, message = "Ten lop khong duoc de trong." });

                // Kiem tra trung ten lop trong cung nam hoc
                var existing = _context.Classes
                    .Where(c => c.Name == model.Name && c.SchoolYearId == model.SchoolYearId && c.Id != model.Id)
                    .FirstOrDefault();
                if (existing != null)
                    return Json(new { success = false, message = $"Lop '{model.Name}' da ton tai trong nam hoc nay." });

                if (model.Id == 0)
                {
                    var newClass = new QuanLyHocSinh.Models.Class
                    {
                        Name = model.Name,
                        GradeLevel = model.GradeLevel,
                        SchoolYearId = model.SchoolYearId,
                        HomeroomTeacherId = model.HomeroomTeacherId > 0 ? model.HomeroomTeacherId : null,
                        MaxStudents = model.MaxStudents,
                        Room = model.Room
                    };
                    _context.Classes.Add(newClass);
                }
                else
                {
                    var cls = _context.Classes.Find(model.Id);
                    if (cls == null)
                        return Json(new { success = false, message = "Lop hoc khong ton tai." });

                    cls.Name = model.Name;
                    cls.GradeLevel = model.GradeLevel;
                    cls.SchoolYearId = model.SchoolYearId;
                    cls.HomeroomTeacherId = model.HomeroomTeacherId > 0 ? model.HomeroomTeacherId : null;
                    cls.MaxStudents = model.MaxStudents;
                    cls.Room = model.Room;
                }

                _context.SaveChanges();
                return Json(new { success = true, message = model.Id == 0 ? "Them lop hoc thanh cong." : "Cap nhat lop hoc thanh cong." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Da xay ra loi: " + ex.Message });
            }
        }

        // POST: /Class/DeleteClass
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteClass(int id)
        {
            try
            {
                var cls = _context.Classes
                    .Include(c => c.StudentClasses)
                    .Include(c => c.TeacherClasses)
                    .FirstOrDefault(c => c.Id == id);

                if (cls == null)
                    return Json(new { success = false, message = "Lop hoc khong ton tai." });

                if (cls.StudentClasses.Any())
                    return Json(new { success = false, message = $"Khong the xoa vi lop co {cls.StudentClasses.Count} hoc sinh. Vui long xoa hoc sinh khoi lop truoc." });

                if (cls.TeacherClasses.Any())
                    return Json(new { success = false, message = $"Khong the xoa vi lop co {cls.TeacherClasses.Count} phan cong. Vui long xoa phan cong truoc." });

                _context.Classes.Remove(cls);
                _context.SaveChanges();
                return Json(new { success = true, message = "Xoa lop hoc thanh cong." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Da xay ra loi: " + ex.Message });
            }
        }

        // ============================
        // XEP LOP HOC SINH
        // ============================

        // GET: /Class/_ManageStudents?classId=X
        [HttpGet]
        public IActionResult _ManageStudents(int classId)
        {
            var cls = _context.Classes
                .Include(c => c.SchoolYear)
                .FirstOrDefault(c => c.Id == classId);

            if (cls == null)
                return NotFound("Lop hoc khong ton tai.");

            // Hoc sinh hien dang co trong lop
            var studentsInClass = _context.StudentClasses
                .Where(sc => sc.ClassId == classId && sc.SchoolYearId == cls.SchoolYearId)
                .Include(sc => sc.Student)
                .Select(sc => new StudentInClass
                {
                    Id = sc.Id,
                    StudentId = sc.StudentId,
                    StudentCode = sc.Student.StudentCode,
                    FullName = sc.Student.FullName,
                    Gender = sc.Student.Gender,
                    DateOfBirth = sc.Student.DateOfBirth
                })
                .ToList();

            // Hoc sinh chua co lop trong nam hoc nay
            var assignedStudentIds = _context.StudentClasses
                .Where(sc => sc.SchoolYearId == cls.SchoolYearId)
                .Select(sc => sc.StudentId)
                .ToHashSet();

            var availableStudents = _context.Students
                .Where(s => s.Status == 1 && !assignedStudentIds.Contains(s.Id))
                .OrderBy(s => s.FullName)
                .Select(s => new StudentOption
                {
                    Id = s.Id,
                    StudentCode = s.StudentCode,
                    FullName = s.FullName
                })
                .ToList();

            var model = new ManageStudentsViewModel
            {
                ClassId = cls.Id,
                ClassName = cls.Name,
                SchoolYearId = cls.SchoolYearId,
                SchoolYearName = cls.SchoolYear.Name,
                MaxStudents = cls.MaxStudents ?? 0,
                StudentsInClass = studentsInClass,
                AvailableStudents = availableStudents
            };

            return PartialView(model);
        }

        // POST: /Class/AddStudentsToClass
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddStudentsToClass(int classId, List<int> studentIds)
        {
            try
            {
                var cls = _context.Classes.Find(classId);
                if (cls == null)
                    return Json(new { success = false, message = "Lop hoc khong ton tai." });

                if (studentIds == null || !studentIds.Any())
                    return Json(new { success = false, message = "Vui long chon it nhat mot hoc sinh." });

                // Kiem tra si so
                var currentCount = _context.StudentClasses.Count(sc => sc.ClassId == classId && sc.SchoolYearId == cls.SchoolYearId);
                var maxStudents = cls.MaxStudents ?? 999;
                if (currentCount + studentIds.Count > maxStudents)
                    return Json(new { success = false, message = $"Si so toi da cua lop la {maxStudents}. Hien co {currentCount} hoc sinh, chi con the them {maxStudents - currentCount} hoc sinh." });

                var added = 0;
                var skipped = 0;

                foreach (var studentId in studentIds.Where(id => id > 0))
                {
                    var exists = _context.StudentClasses.Any(sc =>
                        sc.StudentId == studentId &&
                        sc.ClassId == classId &&
                        sc.SchoolYearId == cls.SchoolYearId);

                    if (exists)
                    {
                        skipped++;
                        continue;
                    }

                    _context.StudentClasses.Add(new QuanLyHocSinh.Models.StudentClass
                    {
                        StudentId = studentId,
                        ClassId = classId,
                        SchoolYearId = cls.SchoolYearId,
                        IsCurrent = 1,
                        JoinedAt = DateOnly.FromDateTime(DateTime.Now)
                    });
                    added++;
                }

                _context.SaveChanges();

                return Json(new { success = true, message = $"Da them {added} hoc sinh vao lop. {skipped} hoc sinh bo qua (da co trong lop)." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Da xay ra loi: " + ex.Message });
            }
        }

        // POST: /Class/RemoveStudentFromClass
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveStudentFromClass(int id)
        {
            try
            {
                var sc = _context.StudentClasses.Find(id);
                if (sc == null)
                    return Json(new { success = false, message = "Ban ghi khong ton tai." });

                _context.StudentClasses.Remove(sc);
                _context.SaveChanges();

                return Json(new { success = true, message = "Da xoa hoc sinh khoi lop." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Da xay ra loi: " + ex.Message });
            }
        }

        // ============================
        // PHAN CONG GIAO VIEN
        // ============================

        // GET: /Class/_ManageTeacherAssignments?classId=X
        [HttpGet]
        public IActionResult _ManageTeacherAssignments(int classId)
        {
            var cls = _context.Classes.Find(classId);
            if (cls == null)
                return NotFound("Lop hoc khong ton tai.");

            var currentAssignments = _context.TeacherClasses
                .Where(tc => tc.ClassId == classId)
                .Include(tc => tc.Teacher)
                .Include(tc => tc.Subject)
                .Include(tc => tc.Semester)
                    .ThenInclude(s => s.SchoolYear)
                .Select(tc => new AssignmentItem
                {
                    Id = tc.Id,
                    TeacherId = tc.TeacherId,
                    TeacherName = tc.Teacher.FullName,
                    SubjectId = tc.SubjectId,
                    SubjectName = tc.Subject.Name,
                    SemesterId = tc.SemesterId,
                    SemesterName = tc.Semester.Name + " (" + tc.Semester.SchoolYear.Name + ")",
                    PeriodsPerWeek = tc.PeriodsPerWeek
                })
                .ToList();

            var subjects = _context.Subjects
                .Where(s => s.IsActive == 1)
                .OrderBy(s => s.Name)
                .Select(s => new SubjectOption { Id = s.Id, Name = s.Name, Code = s.Code })
                .ToList();

            var teachers = _context.Teachers
                .Where(t => t.Status == 1)
                .OrderBy(t => t.FullName)
                .Select(t => new TeacherOption { Id = t.Id, FullName = t.FullName, Specialization = t.Specialization })
                .ToList();

            var semesters = _context.Semesters
                .Include(s => s.SchoolYear)
                .OrderByDescending(s => s.SchoolYear.StartDate)
                .ThenBy(s => s.SemesterNumber)
                .Select(s => new SemesterOption { Id = s.Id, Name = s.Name + " (" + s.SchoolYear.Name + ")", SchoolYearId = s.SchoolYearId })
                .ToList();

            var model = new ManageTeacherAssignmentsViewModel
            {
                ClassId = cls.Id,
                ClassName = cls.Name,
                CurrentAssignments = currentAssignments,
                Subjects = subjects,
                Teachers = teachers,
                Semesters = semesters
            };

            return PartialView(model);
        }

        // POST: /Class/SaveTeacherAssignment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveTeacherAssignment(int classId, int subjectId, int teacherId, int semesterId, int? periodsPerWeek)
        {
            try
            {
                var cls = _context.Classes.Find(classId);
                if (cls == null)
                    return Json(new { success = false, message = "Lop hoc khong ton tai." });

                if (subjectId <= 0 || teacherId <= 0 || semesterId <= 0)
                    return Json(new { success = false, message = "Vui long dien day du thong tin." });

                // Kiem tra trung: cung lop, cung mon, cung hoc ky
                var existing = _context.TeacherClasses
                    .FirstOrDefault(tc =>
                        tc.ClassId == classId &&
                        tc.SubjectId == subjectId &&
                        tc.SemesterId == semesterId);

                if (existing != null)
                {
                    // Cap nhat giao vien va so tiet
                    existing.TeacherId = teacherId;
                    existing.PeriodsPerWeek = periodsPerWeek;
                    _context.SaveChanges();
                    return Json(new { success = true, message = "Cap nhat phan cong thanh cong." });
                }
                else
                {
                    // Tao moi
                    var assignment = new QuanLyHocSinh.Models.TeacherClass
                    {
                        ClassId = classId,
                        SubjectId = subjectId,
                        TeacherId = teacherId,
                        SemesterId = semesterId,
                        PeriodsPerWeek = periodsPerWeek
                    };
                    _context.TeacherClasses.Add(assignment);
                    _context.SaveChanges();
                    return Json(new { success = true, message = "Phan cong giao vien thanh cong." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Da xay ra loi: " + ex.Message });
            }
        }

        // POST: /Class/RemoveTeacherAssignment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveTeacherAssignment(int id)
        {
            try
            {
                var tc = _context.TeacherClasses.Find(id);
                if (tc == null)
                    return Json(new { success = false, message = "Phan cong khong ton tai." });

                _context.TeacherClasses.Remove(tc);
                _context.SaveChanges();

                return Json(new { success = true, message = "Da xoa phan cong." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Da xay ra loi: " + ex.Message });
            }
        }
    }
}
