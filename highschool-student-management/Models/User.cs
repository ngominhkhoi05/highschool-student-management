using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyHocSinh.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string Password { get; set; } = string.Empty;
        // Lưu mật khẩu đã hash (BCrypt)

        [MaxLength(100)]
        public string? Email { get; set; }

        [ForeignKey("Role")]
        public int RoleId { get; set; }

        public int? RelatedId { get; set; }
        // ID tương ứng trong bảng teachers / students / parents

        [MaxLength(20)]
        public string? RelatedType { get; set; }
        // "teacher", "student", "parent", "admin"

        public int IsActive { get; set; } = 1;
        // 1: đang hoạt động, 0: bị khóa

        public DateTime? LastLogin { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation
        public Role Role { get; set; } = null!;
        public ICollection<Notification> SentNotifications { get; set; } = new List<Notification>();
        public ICollection<NotificationRecipient> NotificationRecipients { get; set; } = new List<NotificationRecipient>();
        public ICollection<Setting> UpdatedSettings { get; set; } = new List<Setting>();
    }
}