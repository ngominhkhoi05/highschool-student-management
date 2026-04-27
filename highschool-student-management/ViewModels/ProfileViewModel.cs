using System.ComponentModel.DataAnnotations;

namespace highschool_student_management.ViewModels
{
    public class ProfileViewModel
    {
        // === Thong tin tai khoan (User) ===
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string RoleName { get; set; } = string.Empty;

        // === FK rieng cho tung loai nguoi dung ===
        public int? TeacherId { get; set; }
        public int? StudentId { get; set; }
        public int? ParentId { get; set; }

        // === Cac truong chung cho Teacher/Student/Parent ===
        public string? FullName { get; set; }
        public int? Gender { get; set; }
        // 1: Nam, 2: Nu
        public DateOnly? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }

        // === Cac truong rieng cho Teacher ===
        public string? Specialization { get; set; }
        public DateOnly? JoinDate { get; set; }
        public int? TeacherStatus { get; set; }

        // === Cac truong rieng cho Student ===
        public string? StudentCode { get; set; }
        public string? PlaceOfBirth { get; set; }
        public string? Ethnicity { get; set; }
        public string? Religion { get; set; }
        public DateOnly? EnrollmentDate { get; set; }
        public int? StudentStatus { get; set; }

        // === Cac truong rieng cho Parent ===
        public string? Occupation { get; set; }
        public int? Relationship { get; set; }
        // 1: Cha, 2: Me, 3: Nguoi giam ho

        // === Helper: Tra ve ten gioi tinh ===
        public string GenderName => Gender switch
        {
            1 => "Nam",
            2 => "Nu",
            _ => "Khong xac dinh"
        };

        // === Helper: Tra ve ten vai tro hien thi ===
        public string RoleDisplayName => RoleName switch
        {
            "Admin" => "Quan tri vien",
            "Teacher" => "Giao vien",
            "Student" => "Hoc sinh",
            "Parent" => "Phu huynh",
            _ => RoleName
        };

        // === Helper: Tra ve RelatedType tu Role de xac dinh loai nguoi dung ===
        public string RelatedType => RoleName;
    }
}
