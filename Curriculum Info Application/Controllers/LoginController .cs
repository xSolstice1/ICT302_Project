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
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginModel model)
        {
            if (LoginModel.checkCredential(_connection, model))
            {
                return RedirectToAction("Import");
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid username or password.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public IActionResult Signup(LoginModel model)
        {
            if (LoginModel.insertNewUser(_connection, model))
            {
                return RedirectToAction("Index");
            }
            else
            {
                TempData["ErrorMessage"] = "Registration failed. Please try again.";
                return RedirectToAction("Index");
            }
        }
    }
}
