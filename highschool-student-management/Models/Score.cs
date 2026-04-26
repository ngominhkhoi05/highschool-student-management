using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyHocSinh.Models
{
    [Table("scores")]
    public class Score
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Student")]
        public int StudentId { get; set; }

        [ForeignKey("Subject")]
        public int SubjectId { get; set; }

        [ForeignKey("Semester")]
        public int SemesterId { get; set; }

        [ForeignKey("ScoreType")]
        public int ScoreTypeId { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        public decimal? ScoreValue { get; set; }
        // Điểm số, thang 10

        public DateOnly? ExamDate { get; set; }

        [MaxLength(255)]
        public string? Note { get; set; }

        [ForeignKey("Teacher")]
        public int? TeacherId { get; set; }
        // Giáo viên nhập điểm

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public Student Student { get; set; } = null!;
        public Subject Subject { get; set; } = null!;
        public Semester Semester { get; set; } = null!;
        public ScoreType ScoreType { get; set; } = null!;
        public Teacher? Teacher { get; set; }
    }
}