using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExitInterviewSystem.Controllers
{
    [Authorize]
    public class ResolutionsController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Resolutions";
            return View();
        }
    }
}
