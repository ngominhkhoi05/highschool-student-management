using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyHocSinh.Models
{
    [Table("semester_results")]
    public class SemesterResult
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Student")]
        public int StudentId { get; set; }

        [ForeignKey("Semester")]
        public int SemesterId { get; set; }

        [ForeignKey("Subject")]
        public int? SubjectId { get; set; }
        // NULL = tổng kết toàn kỳ, có giá trị = tổng kết theo môn

        [Column(TypeName = "decimal(4,2)")]
        public decimal? AverageScore { get; set; }

        public int? AcademicRank { get; set; }
        // 1: Giỏi, 2: Khá, 3: TB, 4: Yếu, 5: Kém

        public int? ConductRank { get; set; }
        // 1: Tốt, 2: Khá, 3: TB, 4: Yếu — chỉ có ở bản ghi tổng kỳ (SubjectId = NULL)

        public int? IsPromoted { get; set; }
        // 1: lên lớp, 0: ở lại — chỉ áp dụng ở HK2

        public string? Note { get; set; }

        // Navigation
        public Student Student { get; set; } = null!;
        public Semester Semester { get; set; } = null!;
        public Subject? Subject { get; set; }
    }
}