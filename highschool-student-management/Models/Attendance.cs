using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyHocSinh.Models
{
    [Table("attendances")]
    public class Attendance
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Student")]
        public int StudentId { get; set; }

        [ForeignKey("Class")]
        public int ClassId { get; set; }

        [ForeignKey("Subject")]
        public int? SubjectId { get; set; }
        // NULL = điểm danh cả ngày

        public DateOnly Date { get; set; }

        public int? Period { get; set; }
        // Tiết thứ mấy trong ngày (1-10)

        public int? Status { get; set; }
        // 1: Có mặt, 2: Vắng có phép, 3: Vắng không phép, 4: Đi trễ

        [MaxLength(255)]
        public string? Note { get; set; }

        [ForeignKey("RecordedByTeacher")]
        public int? RecordedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public Student Student { get; set; } = null!;
        public Class Class { get; set; } = null!;
        public Subject? Subject { get; set; }
        public Teacher? RecordedByTeacher { get; set; }
    }
}