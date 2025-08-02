using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IzinTakipPaneli.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeID { get; set; }

        [Required, MaxLength(150)]
        public string FullName { get; set; }

        [ForeignKey("Department")]
        public int DepartmentID { get; set; }
        public Department? Department { get; set; }

        public bool IsActive { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [Required, MaxLength(20)]
        public string UserRole { get; set; }

        [Required, MaxLength(50)]
        public string Username { get; set; }

        [Required, MaxLength(255)]
        public string Password { get; set; }

        // Navigations
        public ICollection<Leave>? Leaves { get; set; }

    }
}
