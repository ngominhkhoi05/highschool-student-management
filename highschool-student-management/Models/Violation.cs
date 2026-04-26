using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyHocSinh.Models
{
    [Table("violations")]
    public class Violation
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Student")]
        public int StudentId { get; set; }

        [ForeignKey("ViolationType")]
        public int ViolationTypeId { get; set; }

        [ForeignKey("Semester")]
        public int SemesterId { get; set; }

        public DateOnly ViolationDate { get; set; }

        public string? Description { get; set; }

        public int? ActionTaken { get; set; }
        // 1: Nhắc nhở, 2: Phạt lao động, 3: Mời phụ huynh, 4: Kỷ luật

        [ForeignKey("RecordedByTeacher")]
        public int? RecordedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public Student Student { get; set; } = null!;
        public ViolationType ViolationType { get; set; } = null!;
        public Semester Semester { get; set; } = null!;
        public Teacher? RecordedByTeacher { get; set; }
    }
}