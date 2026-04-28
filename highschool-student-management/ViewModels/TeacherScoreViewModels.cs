namespace highschool_student_management.ViewModels
{
    // === ViewModels cho So Diem ===

    // Form loc nhap diem
    public class ScoreFilterViewModel
    {
        public List<ClassAssignmentDropdown> Classes { get; set; } = new();
        public List<SubjectDropdown> Subjects { get; set; } = new();
        public List<SemesterDropdown> Semesters { get; set; } = new();
    }

    public class SemesterDropdown
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int SchoolYearId { get; set; }
    }

    // Mon hoc theo lop (gom ca SemesterId de kiem tra phan cong)
    public class SubjectByClassItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int SemesterId { get; set; }
        public string SemesterName { get; set; } = string.Empty;
    }

    // Mot o diem trong bang
    public class ScoreCell
    {
        public int StudentId { get; set; }
        public int ScoreTypeId { get; set; }
        public int? ScoreId { get; set; }
        public decimal? Value { get; set; }
        public DateOnly? ExamDate { get; set; }
    }

    // Mot dong hoc sinh trong bang diem
    public class StudentScoreRow
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public List<ScoreCell> Scores { get; set; } = new();
    }

    // Bang diem day du
    public class ScoreGridViewModel
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public int SemesterId { get; set; }
        public string SemesterName { get; set; } = string.Empty;
        public List<ScoreTypeHeader> ScoreTypes { get; set; } = new();
        public List<StudentScoreRow> StudentRows { get; set; } = new();
    }

    public class ScoreTypeHeader
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Weight { get; set; }
    }

    // Model nhan du lieu luu diem
    public class ScoreSaveItem
    {
        public int? ScoreId { get; set; }
        public int StudentId { get; set; }
        public int ScoreTypeId { get; set; }
        public decimal? ScoreValue { get; set; }
        public DateOnly? ExamDate { get; set; }
        public string? Note { get; set; }
    }

    public class SaveBulkScoresRequest
    {
        public int ClassId { get; set; }
        public int SubjectId { get; set; }
        public int SemesterId { get; set; }
        public List<ScoreSaveItem> Scores { get; set; } = new();
    }
}
