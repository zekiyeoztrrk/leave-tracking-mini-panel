using Microsoft.AspNetCore.Mvc.Rendering;

namespace IzinTakipPaneli.Models.ViewModels
{
    public class LeaveRowVm
    {
        public string LeaveType { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalDays { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UserDashboardVm
    {
        public string Username { get; set; } = "";
        public string FullName { get; set; } = "";
        public string DepartmentName { get; set; } = "";

        public int MaxAnnualDays { get; set; }
        public int UsedAnnualDays { get; set; }
        public int RemainingAnnualDays => Math.Max(0, MaxAnnualDays - UsedAnnualDays);

        public List<LeaveRowVm> Leaves { get; set; } = new();

        // Modal dropdown için
        public int? SelectedLeaveTypeId { get; set; }
        public IEnumerable<SelectListItem> LeaveTypes { get; set; } = new List<SelectListItem>();
    }
}
