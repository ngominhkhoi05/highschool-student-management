using System.ComponentModel.DataAnnotations;

namespace highschool_student_management.ViewModels
{
    // === ViewModel cho trang danh sach giao vien ===
    public class TeacherIndexViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public int? Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Specialization { get; set; }
        public DateOnly? JoinDate { get; set; }
        public int Status { get; set; }
        public int UserId { get; set; }
        public int IsUserActive { get; set; }

        public string GenderName => Gender switch { 1 => "Nam", 2 => "Nu", _ => "Khong xac dinh" };
        public string StatusName => Status switch
        {
            1 => "Dang day",
            2 => "Nghi phep",
            3 => "Nghi viec",
            _ => "Khong xac dinh"
        };
    }

    // === ViewModel cho form them/sua giao vien ===
    public class TeacherFormViewModel
    {
        public int Id { get; set; }
        public int? UserId { get; set; }

        [Required(ErrorMessage = "Ho ten khong duoc de trong.")]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        public int? Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }

        [MaxLength(15)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        [EmailAddress(ErrorMessage = "Email khong hop le.")]
        public string? Email { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? Specialization { get; set; }
        public DateOnly? JoinDate { get; set; }
        public int Status { get; set; } = 1;
        public string? Avatar { get; set; }
        public IFormFile? AvatarFile { get; set; }

        // Cac truong chi can khi tao moi (Id == 0)
        [MaxLength(50)]
        public string? Username { get; set; }

        [EmailAddress(ErrorMessage = "Email khong hop le.")]
        [MaxLength(100)]
        public string? UserEmail { get; set; }
    }

    // === ViewModel cho trang danh sach hoc sinh ===
    public class StudentIndexViewModel
    {
        public int Id { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public int? Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Phone { get; set; }
        public int Status { get; set; }
        public DateOnly? EnrollmentDate { get; set; }
        public int UserId { get; set; }
        public int IsUserActive { get; set; }

        public string GenderName => Gender switch { 1 => "Nam", 2 => "Nu", _ => "Khong xac dinh" };
        public string StatusName => Status switch
        {
            1 => "Dang hoc",
            2 => "Nghi hoc",
            3 => "Tot nghiep",
            4 => "Chuyen truong",
            _ => "Khong xac dinh"
        };
    }

    // === ViewModel cho form them/sua hoc sinh ===
    public class StudentFormViewModel
    {
        public int Id { get; set; }
        public int? UserId { get; set; }

        [Required(ErrorMessage = "Ma hoc sinh khong duoc de trong.")]
        [MaxLength(20)]
        public string StudentCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ho ten khong duoc de trong.")]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        public int? Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }

        [MaxLength(100)]
        public string? PlaceOfBirth { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        [MaxLength(15)]
        public string? Phone { get; set; }

        [MaxLength(50)]
        public string? Ethnicity { get; set; }

        [MaxLength(50)]
        public string? Religion { get; set; }
        public DateOnly? EnrollmentDate { get; set; }
        public int Status { get; set; } = 1;
        public string? Avatar { get; set; }
        public IFormFile? AvatarFile { get; set; }

        // Chi can khi tao moi
        [EmailAddress(ErrorMessage = "Email khong hop le.")]
        [MaxLength(100)]
        public string? UserEmail { get; set; }
    }

    // === ViewModel cho trang danh sach phu huynh ===
    public class ParentIndexViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int? Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Occupation { get; set; }
        public int? Relationship { get; set; }
        public string? Address { get; set; }
        public int UserId { get; set; }
        public int IsUserActive { get; set; }
        public List<string> AssignedStudentNames { get; set; } = new();

        public string GenderName => Gender switch { 1 => "Nam", 2 => "Nu", _ => "Khong xac dinh" };
        public string RelationshipName => Relationship switch
        {
            1 => "Cha",
            2 => "Me",
            3 => "Nguoi giam ho",
            _ => "Khong xac dinh"
        };
    }

    // === ViewModel cho form them/sua phu huynh ===
    public class ParentFormViewModel
    {
        public int Id { get; set; }
        public int? UserId { get; set; }

        [Required(ErrorMessage = "Ho ten khong duoc de trong.")]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        public int? Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }

        [MaxLength(15)]
        [Required(ErrorMessage = "So dien thoai khong duoc de trong.")]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(100)]
        [EmailAddress(ErrorMessage = "Email khong hop le.")]
        public string? Email { get; set; }

        [MaxLength(100)]
        public string? Occupation { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }
        public int? Relationship { get; set; }

        // Chi can khi tao moi (Id == 0)
        [EmailAddress(ErrorMessage = "Email khong hop le.")]
        [MaxLength(100)]
        public string? UserEmail { get; set; }
    }

    // === ViewModel cho form gan hoc sinh cho phu huynh ===
    public class AssignStudentsViewModel
    {
        public int ParentId { get; set; }
        public string ParentName { get; set; } = string.Empty;
        public List<StudentOption> AllStudents { get; set; } = new();
        public List<int> SelectedStudentIds { get; set; } = new();
    }

    public class StudentOption
    {
        public int Id { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
}
