using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyHocSinh.Models
{
    [Table("notifications")]
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Content { get; set; }

        public int? Type { get; set; }
        // 1: Thông báo chung, 2: Học vụ, 3: Sự kiện, 4: Khẩn

        [ForeignKey("Sender")]
        public int? SenderId { get; set; }

        public int? TargetType { get; set; }
        // 1: Toàn trường, 2: Theo khối, 3: Theo lớp, 4: Cá nhân

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? PublishedAt { get; set; }

        // Navigation
        public User? Sender { get; set; }
        public ICollection<NotificationRecipient> Recipients { get; set; } = new List<NotificationRecipient>();
    }
}