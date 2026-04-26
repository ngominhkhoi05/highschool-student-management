using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyHocSinh.Models
{
    [Table("score_types")]
    public class ScoreType
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        // Miệng, 15 phút, 1 tiết, Giữa kỳ, Cuối kỳ

        [Column(TypeName = "decimal(3,1)")]
        public decimal Weight { get; set; }
        // Hệ số: 1.0, 2.0, 3.0

        public int MinCount { get; set; } = 1;
        // Số lần kiểm tra tối thiểu/học kỳ

        // Navigation
        public ICollection<Score> Scores { get; set; } = new List<Score>();
    }
}