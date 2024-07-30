using Microsoft.AspNetCore.Mvc;

namespace AutoPartsHub.Controllers
{
    public class AdminDashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
