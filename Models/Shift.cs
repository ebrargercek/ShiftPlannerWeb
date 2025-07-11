using System;

namespace ShiftPlannerWeb.Models
{
    public class Shift
    {
        public int EmployeeId { get; set; }
        public DateTime ShiftStart { get; set; }
        public DateTime ShiftEnd { get; set; }
        public int AssignType { get; set; }

        // Artık yazılabilir hale getirildi
        public string? AssignLabel { get; set; }
    }
}
