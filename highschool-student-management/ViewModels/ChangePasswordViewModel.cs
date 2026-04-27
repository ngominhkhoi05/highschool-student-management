using System.ComponentModel.DataAnnotations;

namespace highschool_student_management.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Mat khau cu khong duoc de trong.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mat khau cu")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mat khau moi khong duoc de trong.")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mat khau phai tu 6 ky tu tro len.")]
        [Display(Name = "Mat khau moi")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui long xac nhan mat khau moi.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Mat khau xac nhan khong khop.")]
        [Display(Name = "Xac nhan mat khau moi")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
