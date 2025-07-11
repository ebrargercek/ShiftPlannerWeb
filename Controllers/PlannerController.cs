using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ShiftPlannerWeb.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;

namespace ShiftPlannerWeb.Controllers
{
    public class PlannerController : Controller
    {
        private readonly string _excelPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "planlama_verisi.xlsx");

        public IActionResult Dashboard()
        {
            int? employeeId = HttpContext.Session.GetInt32("EmployeeId");
            if (employeeId == null)
                return RedirectToAction("Login", "Account");

            string? role = HttpContext.Session.GetString("UserRole");

            ViewBag.EmployeeId = employeeId;
            ViewBag.Role = role;

            return View();
        }

        public IActionResult Index()
        {
            var shifts = ReadShiftsFromExcel();
            return View(shifts);
        }

        public IActionResult MyShifts()
        {
            int? employeeId = HttpContext.Session.GetInt32("EmployeeId");
            if (employeeId == null)
                return RedirectToAction("Login", "Account");

            var allShifts = ReadShiftsFromExcel();
            var userShifts = allShifts.Where(s => s.EmployeeId == employeeId.Value).ToList();

            return View("Index", userShifts);
        }

        [HttpGet]
        public IActionResult Manage()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
                return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpPost]
        public IActionResult AssignShift(int EmployeeId, DateTime ShiftStart, DateTime ShiftEnd, int AssignType)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
                return RedirectToAction("Login", "Account");

            var workbook = new XLWorkbook(_excelPath);
            var worksheet = workbook.Worksheet("ScheduleAssignedShift");
            var lastRow = worksheet.LastRowUsed().RowNumber();
            var newRow = worksheet.Row(lastRow + 1);

            newRow.Cell(3).Value = EmployeeId;
            newRow.Cell(5).Value = ShiftStart;
            newRow.Cell(6).Value = ShiftEnd;
            newRow.Cell(7).Value = AssignType;

            workbook.Save();
            return RedirectToAction("Manage");
        }

        [HttpGet]
        public IActionResult AssignTask()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
                return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpPost]
        public IActionResult AssignTask(int EmployeeId, DateTime ShiftStart, DateTime ShiftEnd, string TaskNote)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
                return RedirectToAction("Login", "Account");

            var workbook = new XLWorkbook(_excelPath);
            var worksheet = workbook.Worksheet("ScheduleAssignedShift");
            var lastRow = worksheet.LastRowUsed().RowNumber();
            var newRow = worksheet.Row(lastRow + 1);

            newRow.Cell(3).Value = EmployeeId;
            newRow.Cell(5).Value = ShiftStart;
            newRow.Cell(6).Value = ShiftEnd;
            newRow.Cell(7).Value = 2;
            newRow.Cell(8).Value = TaskNote;

            workbook.Save();
            return RedirectToAction("Dashboard");
        }

        public IActionResult Weekly()
        {
            int? employeeId = HttpContext.Session.GetInt32("EmployeeId");
            if (employeeId == null)
                return RedirectToAction("Login", "Account");

            var allShifts = ReadShiftsFromExcel();
            var userShifts = allShifts
                .Where(s => s.EmployeeId == employeeId.Value)
                .OrderBy(s => s.ShiftStart)
                .ToList();

            return View(userShifts);
        }

        public IActionResult AdminStats()
        {
            var shifts = ReadShiftsFromExcel();

            var grouped = shifts
                .GroupBy(s => s.AssignLabel ?? "Bilinmiyor")
                .ToDictionary(g => g.Key, g => g.Count());

            return View(grouped);
        }

        private List<Shift> ReadShiftsFromExcel()
        {
            var shifts = new List<Shift>();
            if (!System.IO.File.Exists(_excelPath)) return shifts;

            try
            {
                var workbook = new XLWorkbook(_excelPath);
                var worksheet = workbook.Worksheet("ScheduleAssignedShift");
                var rows = worksheet.RangeUsed().RowsUsed();

                foreach (var row in rows.Skip(1))
                {
                    try
                    {
                        shifts.Add(new Shift
                        {
                            EmployeeId = int.Parse(row.Cell(3).GetValue<string>()),
                            ShiftStart = DateTime.Parse(row.Cell(5).GetValue<string>()),
                            ShiftEnd = DateTime.Parse(row.Cell(6).GetValue<string>()),
                            AssignType = int.Parse(row.Cell(7).GetValue<string>()),
                            AssignLabel = row.Cell(8).GetValue<string>()
                        });
                    }
                    catch { }
                }
            }
            catch { }

            return shifts;
        }
    }
}
