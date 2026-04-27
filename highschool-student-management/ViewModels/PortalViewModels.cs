namespace highschool_student_management.ViewModels
{
    // ========== DASHBOARD ==========
    public class DashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalClasses { get; set; }
        public string SchoolYearName { get; set; } = "";
        public string SemesterName { get; set; } = "";
        public List<AcademicRankStatViewModel> AcademicRankStats { get; set; } = new();
    }

    public class AcademicRankStatViewModel
    {
        public int Rank { get; set; }
        public int Count { get; set; }
        public string RankName => Rank switch
        {
            1 => "Gioi",
            2 => "Kha",
            3 => "Trung binh",
            4 => "Yeu",
            5 => "Kem",
            _ => "Khong xep loai"
        };
    }

    // ========== NOTIFICATION ==========
    public class NotificationViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string? Content { get; set; }
        public int Type { get; set; }
        public string SenderName { get; set; } = "";
        public int IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }

        public string TypeName => Type switch
        {
            1 => "Thong bao chung",
            2 => "Hoc vu",
            3 => "Su kien",
            4 => "Khan",
            _ => "Khong ro"
        };

        public string TypeIcon => Type switch
        {
            1 => "campaign",
            2 => "school",
            3 => "event",
            4 => "priority_high",
            _ => "info"
        };

        public string TypeColor => Type switch
        {
            1 => "bg-blue-100 text-blue-700",
            2 => "bg-purple-100 text-purple-700",
            3 => "bg-orange-100 text-orange-700",
            4 => "bg-red-100 text-red-700",
            _ => "bg-slate-100 text-slate-700"
        };
    }

    public class NotificationCreateViewModel
    {
        public string Title { get; set; } = "";
        public string? Content { get; set; }
        public int Type { get; set; } = 1;
        public int TargetType { get; set; } = 1;
        public List<int>? GradeLevels { get; set; }
        public List<int>? ClassIds { get; set; }
        public List<int>? UserIds { get; set; }

        public List<SemesterDropdown> Semesters { get; set; } = new();
        public List<ClassDropdown> Classes { get; set; } = new();
        public List<UserDropdown> Users { get; set; } = new();
    }

    public class UserDropdown
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string RoleName { get; set; } = "";
    }

    public class ClassDropdown
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int GradeLevel { get; set; }
    }

    // ========== STUDENT PORTAL ==========
    public class StudentPortalViewModel
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = "";
        public string FullName { get; set; } = "";
        public List<QuanLyHocSinh.Models.Semester> Semesters { get; set; } = new();
        public int CurrentSemesterId { get; set; }
    }

    public class StudentScoresViewModel : StudentPortalViewModel
    {
        public List<StudentScoreViewModel> Scores { get; set; } = new();
    }

    public class StudentScoreViewModel
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = "";
        public int ScoreTypeId { get; set; }
        public string ScoreTypeName { get; set; } = "";
        public decimal? ScoreValue { get; set; }
        public DateOnly? ExamDate { get; set; }
    }

    public class StudentAttendancesViewModel : StudentPortalViewModel
    {
        public List<StudentAttendanceViewModel> Attendances { get; set; } = new();
    }

    public class StudentAttendanceViewModel
    {
        public DateOnly Date { get; set; }
        public int? Period { get; set; }
        public int Status { get; set; }
        public string? Note { get; set; }

        public string StatusName => Status switch
        {
            1 => "Co mat",
            2 => "Vang co phep",
            3 => "Vang khong phep",
            4 => "Di tre",
            _ => "Khong ro"
        };

        public string StatusClass => Status switch
        {
            1 => "bg-green-100 text-green-700",
            2 => "bg-yellow-100 text-yellow-700",
            3 => "bg-red-100 text-red-700",
            4 => "bg-orange-100 text-orange-700",
            _ => "bg-slate-100 text-slate-600"
        };
    }

    public class StudentConductViewModel : StudentPortalViewModel
    {
        public ConductDetailViewModel? ConductScore { get; set; }
        public List<StudentViolationViewModel> Violations { get; set; } = new();
    }

    public class ConductDetailViewModel
    {
        public int ConductRank { get; set; }
        public int? Score { get; set; }
        public string? Note { get; set; }
        public DateTime? EvaluatedAt { get; set; }

        public string RankName => ConductRank switch
        {
            1 => "Tot",
            2 => "Kha",
            3 => "Trung binh",
            4 => "Yeu",
            _ => "Chua danh gia"
        };

        public string RankClass => ConductRank switch
        {
            1 => "bg-green-100 text-green-700",
            2 => "bg-blue-100 text-blue-700",
            3 => "bg-yellow-100 text-yellow-700",
            4 => "bg-red-100 text-red-700",
            _ => "bg-slate-100 text-slate-600"
        };
    }

    public class StudentViolationViewModel
    {
        public DateOnly ViolationDate { get; set; }
        public string ViolationTypeName { get; set; } = "";
        public int Severity { get; set; }
        public string? Description { get; set; }
        public int? ActionTaken { get; set; }

        public string SeverityClass => Severity switch
        {
            1 => "bg-yellow-100 text-yellow-700",
            2 => "bg-orange-100 text-orange-700",
            3 => "bg-red-100 text-red-700",
            _ => "bg-slate-100 text-slate-600"
        };

        public string ActionName => ActionTaken switch
        {
            1 => "Nhac nho",
            2 => "Phat lao dong",
            3 => "Moi phu huynh",
            4 => "Ky luat",
            _ => ""
        };
    }

    // ========== PARENT PORTAL ==========
    public class ParentPortalViewModel
    {
        public int ParentId { get; set; }
        public string ParentName { get; set; } = "";
        public List<StudentDropdown> Children { get; set; } = new();
        public int SelectedStudentId { get; set; }
        public List<QuanLyHocSinh.Models.Semester> Semesters { get; set; } = new();
        public int CurrentSemesterId { get; set; }
    }

    public class ParentScoresViewModel : ParentPortalViewModel
    {
        public string StudentCode { get; set; } = "";
        public string FullName { get; set; } = "";
        public List<StudentScoreViewModel> Scores { get; set; } = new();
    }

    public class ParentAttendancesViewModel : ParentPortalViewModel
    {
        public string StudentCode { get; set; } = "";
        public string FullName { get; set; } = "";
        public List<StudentAttendanceViewModel> Attendances { get; set; } = new();
    }

    public class ParentConductViewModel : ParentPortalViewModel
    {
        public string StudentCode { get; set; } = "";
        public string FullName { get; set; } = "";
        public ConductDetailViewModel? ConductScore { get; set; }
        public List<StudentViolationViewModel> Violations { get; set; } = new();
    }
}
