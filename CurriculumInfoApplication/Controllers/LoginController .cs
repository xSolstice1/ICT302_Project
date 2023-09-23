using Curriculum_Info_Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace Curriculum_Info_Application.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login(LoginModel model)
        {
            // Implement login logic here
            return RedirectToAction("Import");
        }
    }
}
