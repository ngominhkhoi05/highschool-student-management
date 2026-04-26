using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyHocSinh.Models
{
    [Table("violation_types")]
    public class ViolationType
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public int? Severity { get; set; }
        // 1: Nhẹ, 2: Trung bình, 3: Nặng

        public int DeductPoints { get; set; } = 0;
        // Điểm trừ hạnh kiểm

        public string? Description { get; set; }

        // Navigation
        public ICollection<Violation> Violations { get; set; } = new List<Violation>();
    }
}