namespace highschool_student_management.ViewModels
{
    // === ViewModels cho So Diem Danh ===

    // Dashboard - thong tin giao vien
    public class TeacherDashboardViewModel
    {
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public int TotalClasses { get; set; }
        public int TotalSubjects { get; set; }
        public int CurrentSchoolYearId { get; set; }
        public string CurrentSchoolYearName { get; set; } = string.Empty;
        public List<ClassAssignmentItem> ClassAssignments { get; set; } = new();
    }

    public class ClassAssignmentItem
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int GradeLevel { get; set; }
        public string SchoolYearName { get; set; } = string.Empty;
        public List<string> Subjects { get; set; } = new();
    }

    // Form loc diem danh
    public class AttendanceFilterViewModel
    {
        public List<ClassAssignmentDropdown> Classes { get; set; } = new();
        public List<SubjectDropdown> Subjects { get; set; } = new();
    }

    public class ClassAssignmentDropdown
    {
        public int ClassId { get; set; }
        public int SemesterId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string SemesterName { get; set; } = string.Empty;
    }

    public class SubjectDropdown
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    // Danh sach hoc sinh de diem danh
    public class StudentAttendanceItem
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int? Gender { get; set; }
        public int? ExistingStatus { get; set; }
        public string? ExistingNote { get; set; }
        public int ExistingAttendanceId { get; set; }
        public bool HasExistingRecord => ExistingAttendanceId > 0;
    }

    // Model nhan du lieu luu diem danh
    public class AttendanceSaveItem
    {
        public int StudentId { get; set; }
        public int Status { get; set; }
        public string? Note { get; set; }
        public DateOnly Date { get; set; }
        public int Period { get; set; }
    }
}
