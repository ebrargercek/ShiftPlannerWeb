// Controllers/AccountController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace ShiftPlannerWeb.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(int employeeId)
        {
            HttpContext.Session.SetInt32("EmployeeId", employeeId);
            string role = (employeeId == 99999) ? "Admin" : "Employee";
            HttpContext.Session.SetString("UserRole", role);

            return RedirectToAction("Dashboard", "Planner");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}