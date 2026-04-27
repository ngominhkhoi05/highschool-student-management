namespace highschool_student_management.ViewModels
{
    // ========== LOP CHU NHIEM ==========
    public class HomeroomClassViewModel
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int? GradeLevel { get; set; }
        public string? Room { get; set; }
        public int? MaxStudents { get; set; }
        public string SchoolYearName { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public List<HomeroomStudentItem> Students { get; set; } = new();
    }

    public class HomeroomStudentItem
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int? Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Phone { get; set; }
        public int Status { get; set; }
    }

    // ========== VI PHAM ==========
    public class ViolationIndexViewModel
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int ViolationTypeId { get; set; }
        public string ViolationTypeName { get; set; } = string.Empty;
        public int Severity { get; set; }
        public int DeductPoints { get; set; }
        public DateOnly ViolationDate { get; set; }
        public string? Description { get; set; }
        public int? ActionTaken { get; set; }
        public int SemesterId { get; set; }
        public string SemesterName { get; set; } = string.Empty;
    }

    public class ViolationCreateViewModel
    {
        public int StudentId { get; set; }
        public int ViolationTypeId { get; set; }
        public int SemesterId { get; set; }
        public DateOnly ViolationDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
        public string? Description { get; set; }
        public int? ActionTaken { get; set; }
    }

    // ========== HANH KIEM ==========
    public class ConductScoreIndexViewModel
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int SemesterId { get; set; }
        public string SemesterName { get; set; } = string.Empty;
        public int? ConductRank { get; set; }
        public int? Score { get; set; }
        public string? Note { get; set; }
        public bool HasViolationRecord { get; set; }
    }

    public class ConductScoreEditViewModel
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int SemesterId { get; set; }
        public int? ConductRank { get; set; }
        public int? Score { get; set; }
        public string? Note { get; set; }
    }

    // ========== DROP DOWNS ==========
    public class StudentDropdown
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
    }

    public class ViolationTypeDropdown
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Severity { get; set; }
        public int DeductPoints { get; set; }
    }
}
