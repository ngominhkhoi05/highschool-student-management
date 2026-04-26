using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyHocSinh.Models
{
    [Table("conduct_scores")]
    public class ConductScore
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Student")]
        public int StudentId { get; set; }

        [ForeignKey("Semester")]
        public int SemesterId { get; set; }

        public int? ConductRank { get; set; }
        // 1: Tốt, 2: Khá, 3: Trung bình, 4: Yếu

        public int? Score { get; set; }
        // Điểm hạnh kiểm tích lũy

        public string? Note { get; set; }

        [ForeignKey("EvaluatedByTeacher")]
        public int? EvaluatedBy { get; set; }

        public DateTime EvaluatedAt { get; set; } = DateTime.Now;

        // Navigation
        public Student Student { get; set; } = null!;
        public Semester Semester { get; set; } = null!;
        public Teacher? EvaluatedByTeacher { get; set; }
    }
}