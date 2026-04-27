using System.ComponentModel.DataAnnotations;

namespace highschool_student_management.ViewModels
{
    public class UserAccountViewModel
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public int IsActive { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime CreatedAt { get; set; }

        public string RoleDisplayName => RoleName switch
        {
            "Admin" => "Quan tri vien",
            "Teacher" => "Giao vien",
            "Student" => "Hoc sinh",
            "Parent" => "Phu huynh",
            _ => RoleName
        };
    }

    public class ResetPasswordViewModel
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mat khau moi khong duoc de trong.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mat khau phai tu 6 ky tu tro len.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui long xac nhan mat khau moi.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
