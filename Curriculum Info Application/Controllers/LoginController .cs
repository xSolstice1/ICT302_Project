using System.Data.OleDb;
using Curriculum_Info_Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace Curriculum_Info_Application.Controllers
{
    public class LoginController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string _connection;
        private OleDbConnection _conn;

        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connection = _configuration.GetConnectionString("DefaultConnection");
            _conn = new OleDbConnection(_connection);
        }
        public IActionResult Index()
        {
            TempData["LoginErrorMessage"] = null;
            TempData["LoginSuccessMessage"] = null;
            TempData["LoginWarningMessage"] = null;
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginModel model)
        {
            if (LoginModel.checkCredential(_connection, model))
            {
                LoginModel loginModel = new LoginModel();
                loginModel.isLogin = true;
                TempData["LoginWarningMessage"] = null;
                return View("~/Views/Home/Import.cshtml");
            }
            else
            {
                TempData["LoginErrorMessage"] = "Invalid username or password.";
                return View("~/Views/Home/Login.cshtml");
            }
        }

        [HttpPost]
        public IActionResult Signup(LoginModel model)
        {
            if (LoginModel.insertNewUser(_connection, model))
            {
                TempData["LoginSuccessMessage"] = "Account created.";
                return View("~/Views/Home/Login.cshtml");
            }
            else
            {
                TempData["LoginErrorMessage"] = "Registration failed. Please try again.";
                return View("~/Views/Home/Login.cshtml");
            }
        }
    }
}
