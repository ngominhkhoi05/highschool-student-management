using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyHocSinh.Models
{
    [Table("parents")]
    public class Parent
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

        [MaxLength(100)]
        public string? Occupation { get; set; }

        public string? Address { get; set; }

        public int? Relationship { get; set; }
        // 1: Cha, 2: Mẹ, 3: Người giám hộ

        // Navigation
        public ICollection<ParentStudent> ParentStudents { get; set; } = new List<ParentStudent>();
    }
}