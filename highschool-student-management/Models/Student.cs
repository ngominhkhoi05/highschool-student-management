using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyHocSinh.Models
{
    [Table("students")]
    public class Student
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(20)]
        public string StudentCode { get; set; } = string.Empty;
        // Mã HS: HS2024001

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        public int? Gender { get; set; }
        // 1: Nam, 2: Nữ

        public DateOnly? DateOfBirth { get; set; }

        [MaxLength(100)]
        public string? PlaceOfBirth { get; set; }

        public string? Address { get; set; }

        [MaxLength(15)]
        public string? Phone { get; set; }

        [MaxLength(50)]
        public string? Ethnicity { get; set; }

        [MaxLength(50)]
        public string? Religion { get; set; }

        public DateOnly? EnrollmentDate { get; set; }

        public int Status { get; set; } = 1;
        // 1: đang học, 2: nghỉ học, 3: tốt nghiệp, 4: chuyển trường

        [MaxLength(255)]
        public string? Avatar { get; set; }

        // Navigation
        public ICollection<StudentClass> StudentClasses { get; set; } = new List<StudentClass>();
        public ICollection<ParentStudent> ParentStudents { get; set; } = new List<ParentStudent>();
        public ICollection<Score> Scores { get; set; } = new List<Score>();
        public ICollection<SemesterResult> SemesterResults { get; set; } = new List<SemesterResult>();
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<ConductScore> ConductScores { get; set; } = new List<ConductScore>();
        public ICollection<Violation> Violations { get; set; } = new List<Violation>();
        public ICollection<Commendation> Commendations { get; set; } = new List<Commendation>();
    }
}