using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyHocSinh.Models
{
    [Table("settings")]
    public class Setting
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Key { get; set; } = string.Empty;
        // VD: school_name, logo_url, pass_score

        public string? Value { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [ForeignKey("UpdatedByUser")]
        public int? UpdatedBy { get; set; }

        // Navigation
        public User? UpdatedByUser { get; set; }
    }
}