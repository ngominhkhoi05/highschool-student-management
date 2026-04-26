using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyHocSinh.Models
{
    [Table("notification_recipients")]
    public class NotificationRecipient
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Notification")]
        public int NotificationId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        public int IsRead { get; set; } = 0;
        // 1: đã đọc, 0: chưa đọc

        public DateTime? ReadAt { get; set; }

        // Navigation
        public Notification Notification { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}