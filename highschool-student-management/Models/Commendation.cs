using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyHocSinh.Models
{
    [Table("commendations")]
    public class Commendation
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Student")]
        public int StudentId { get; set; }

        [ForeignKey("Semester")]
        public int SemesterId { get; set; }

        public int? Type { get; set; }
        // 1: HS Giỏi, 2: HS Tiên Tiến, 3: Thi HSG, 4: Thể thao, 5: Khác

        [MaxLength(200)]
        public string? Title { get; set; }

        public int? Level { get; set; }
        // 1: Cấp lớp, 2: Cấp trường, 3: Cấp huyện, 4: Cấp tỉnh, 5: Quốc gia

        public DateOnly? AwardedDate { get; set; }

        public string? Description { get; set; }

        public int AddPoints { get; set; } = 0;
        // Điểm cộng hạnh kiểm

        // Navigation
        public Student Student { get; set; } = null!;
        public Semester Semester { get; set; } = null!;
    }
}