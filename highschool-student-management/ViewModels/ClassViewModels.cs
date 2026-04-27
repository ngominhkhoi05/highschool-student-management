using System.ComponentModel.DataAnnotations;

namespace highschool_student_management.ViewModels
{
    // === ViewModel cho trang danh sach lop hoc ===
    public class ClassIndexViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? GradeLevel { get; set; }
        public int SchoolYearId { get; set; }
        public string SchoolYearName { get; set; } = string.Empty;
        public int? HomeroomTeacherId { get; set; }
        public string? HomeroomTeacherName { get; set; }
        public int? MaxStudents { get; set; }
        public string? Room { get; set; }
        public int CurrentStudentCount { get; set; }
    }

    // === ViewModel cho form tao/sua lop hoc ===
    public class ClassFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ten lop khong duoc de trong.")]
        [MaxLength(20)]
        public string Name { get; set; } = string.Empty;

        public int? GradeLevel { get; set; }

        [Required(ErrorMessage = "Nam hoc khong duoc de trong.")]
        public int SchoolYearId { get; set; }

        public int? HomeroomTeacherId { get; set; }
        public int? MaxStudents { get; set; }
        public string? Room { get; set; }

        public List<SchoolYearOption> SchoolYears { get; set; } = new();
        public List<TeacherOption> Teachers { get; set; } = new();
    }

    // === ViewModel cho trang xep lop hoc sinh ===
    public class ManageStudentsViewModel
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int SchoolYearId { get; set; }
        public string SchoolYearName { get; set; } = string.Empty;
        public int MaxStudents { get; set; }
        public List<StudentInClass> StudentsInClass { get; set; } = new();
        public List<StudentOption> AvailableStudents { get; set; } = new();
    }

    public class StudentInClass
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int? Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string GenderName => Gender switch { 1 => "Nam", 2 => "Nu", _ => "Khong xac dinh" };
    }

    // === ViewModel cho trang phan cong giao vien ===
    public class ManageTeacherAssignmentsViewModel
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public List<AssignmentItem> CurrentAssignments { get; set; } = new();
        public List<SubjectOption> Subjects { get; set; } = new();
        public List<TeacherOption> Teachers { get; set; } = new();
        public List<SemesterOption> Semesters { get; set; } = new();
    }

    public class AssignmentItem
    {
        public int Id { get; set; }
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public int SemesterId { get; set; }
        public string SemesterName { get; set; } = string.Empty;
        public int? PeriodsPerWeek { get; set; }
    }

    // === Cac class helper cho dropdown ===
    public class SchoolYearOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class TeacherOption
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Specialization { get; set; }
    }

    public class SubjectOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class SemesterOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int SchoolYearId { get; set; }
    }
}
