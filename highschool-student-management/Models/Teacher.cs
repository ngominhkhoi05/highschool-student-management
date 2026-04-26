using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyHocSinh.Models
{
    [Table("teachers")]
    public class Teacher
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        public int? Gender { get; set; }
        // 1: Nam, 2: Nữ

        public DateOnly? DateOfBirth { get; set; }

        [MaxLength(15)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        public string? Address { get; set; }

        [MaxLength(100)]
        public string? Specialization { get; set; }
        // Chuyên môn: Toán học, Ngữ Văn...

        public DateOnly? JoinDate { get; set; }

        public int Status { get; set; } = 1;
        // 1: đang dạy, 2: nghỉ phép, 3: nghỉ việc

        [MaxLength(255)]
        public string? Avatar { get; set; }

        // Navigation
        public ICollection<Class> HomeroomClasses { get; set; } = new List<Class>();
        public ICollection<TeacherClass> TeacherClasses { get; set; } = new List<TeacherClass>();
        public ICollection<Score> Scores { get; set; } = new List<Score>();
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<ConductScore> ConductScores { get; set; } = new List<ConductScore>();
        public ICollection<Violation> RecordedViolations { get; set; } = new List<Violation>();
    }
}