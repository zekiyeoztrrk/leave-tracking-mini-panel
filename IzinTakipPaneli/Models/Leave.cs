using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IzinTakipPaneli.Models
{
    public class Leave
    {
        [Key]
        public int LeaveID { get; set; }

        [ForeignKey(nameof(Employee))]
        public int EmployeeID { get; set; }

        [ForeignKey(nameof(LeaveType))]
        public int LeaveTypeID { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public int TotalDays { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        public Employee? Employee { get; set; }
        public LeaveType? LeaveType { get; set; }
    }
}
