using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IzinTakipPaneli.Models
{
    public class Department
    {
        [Key]
        public int DepartmentID { get; set; }

        [Required, MaxLength(100)]
        public string DepartmentName { get; set; }

        public bool IsActive { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation Property
        public ICollection<Employee> Employees { get; set; }
    }
}
