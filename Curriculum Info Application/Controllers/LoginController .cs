using System.Data.OleDb;
using System.Text.Json;
using System.Xml.Linq;
using Curriculum_Info_Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace Curriculum_Info_Application.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            TempData["LoginErrorMessage"] = null;
            TempData["LoginSuccessMessage"] = null;
            TempData["LoginWarningMessage"] = null;
            TempData["LoginInfoMessage"] = null;
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginModel model)
        {
            if (LoginModel.checkCredential(model))
            {
                TempData["LoginWarningMessage"] = null;
                var data = new LoginModel
                {
                    Email = model.Email,
                    Username = LoginModel.getUsernameByEmail(model.Email)
                };

                string filePath = SystemConstant.LOGIN_FILEPATH;

                // Use a StreamWriter to write to the JSON file
                using (StreamWriter streamWriter = new StreamWriter(filePath))
                {
                    string json = JsonSerializer.Serialize(data);
                    streamWriter.Write(json);
                }

                TempData["LoginInfoMessage"] = "Welcome " + data.Username;

                return View("~/Views/Home/Import.cshtml");
            }
            else
            {
                TempData["LoginErrorMessage"] = "Invalid username or password.";
                return View("~/Views/Home/Index.cshtml");
            }
        }

        [HttpPost]
        public IActionResult Signup(LoginModel model)
        {
            if (LoginModel.insertNewUser(model))
            {
                TempData["LoginSuccessMessage"] = "Account created.";
                return View("~/Views/Home/Index.cshtml");
            }
            else
            {
                TempData["LoginErrorMessage"] = "Registration failed. Please try again.";
                return View("~/Views/Home/Index.cshtml");
            }
        }
    }
}
