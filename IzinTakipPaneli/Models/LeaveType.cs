using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IzinTakipPaneli.Models
{
    public class LeaveType
    {
        [Key]
        public int LeaveTypeID { get; set; }

        [Required, MaxLength(100)]
        public string LeaveTypeName { get; set; }

        public bool IsActive { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation Property
        public ICollection<Leave> Leaves { get; set; }
    }
}
