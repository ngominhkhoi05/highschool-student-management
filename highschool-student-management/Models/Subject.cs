using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyHocSinh.Models
{
    [Table("subjects")]
    public class Subject
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Code { get; set; } = string.Empty;
        // Mã môn: MATH, LIT, ENG...

        public int? GradeLevel { get; set; }
        // 10, 11, 12. NULL = áp dụng tất cả khối

        public int? PeriodsPerWeek { get; set; }

        public int IsActive { get; set; } = 1;
        // 1: đang dạy, 0: ngừng

        // Navigation
        public ICollection<TeacherClass> TeacherClasses { get; set; } = new List<TeacherClass>();
        public ICollection<Score> Scores { get; set; } = new List<Score>();
        public ICollection<SemesterResult> SemesterResults { get; set; } = new List<SemesterResult>();
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    }
}