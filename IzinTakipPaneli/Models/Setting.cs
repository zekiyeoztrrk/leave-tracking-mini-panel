using System;
using System.ComponentModel.DataAnnotations;

namespace IzinTakipPaneli.Models
{
    public class Setting
    {
        [Key]
        public int SettingID { get; set; }

        [Required]
        public int MaxAnnualLeaveDays { get; set; }

        [Required]
        public bool IsActive { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
