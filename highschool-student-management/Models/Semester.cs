using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyHocSinh.Models
{
    [Table("semesters")]
    public class Semester
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("SchoolYear")]
        public int SchoolYearId { get; set; }

        [MaxLength(20)]
        public string? Name { get; set; }
        // "Học kỳ 1", "Học kỳ 2"

        public int? SemesterNumber { get; set; }
        // 1: HK1, 2: HK2

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        // Navigation
        public SchoolYear SchoolYear { get; set; } = null!;
        public ICollection<TeacherClass> TeacherClasses { get; set; } = new List<TeacherClass>();
        public ICollection<Score> Scores { get; set; } = new List<Score>();
        public ICollection<SemesterResult> SemesterResults { get; set; } = new List<SemesterResult>();
        public ICollection<ConductScore> ConductScores { get; set; } = new List<ConductScore>();
        public ICollection<Violation> Violations { get; set; } = new List<Violation>();
        public ICollection<Commendation> Commendations { get; set; } = new List<Commendation>();
    }
}