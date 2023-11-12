using System.Data.OleDb;
using System.Text.Json;
using System.Xml.Linq;
using Curriculum_Info_Application.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Curriculum_Info_Application.Controllers
{
    public class LoginController : Controller
    {
        private static readonly string _filePath = SystemConstant.USER_FILEPATH;
        public IActionResult Index()
        {
            TempData["LoginErrorMessage"] = null;
            TempData["LoginSuccessMessage"] = null;
            TempData["LoginWarningMessage"] = null;
            TempData["LoginInfoMessage"] = null;
            List<LoginModel> users = GetUserList();
            ViewBag.CurrentUserEmail = LoginModel.GetCurrentEmail();
            return View("~/Views/Home/User.cshtml", users);
        }

        [HttpPost]
        public ActionResult DeleteUser(string email)
        {
            LoginModel.DeleteUserByEmail(email);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ToggleAdminStatus(string email)
        {
            ToggleAdminStatusByEmail(email);
            return RedirectToAction("Index");
        }

        private List<LoginModel> GetUserList()
        {
            try
            {
                if (System.IO.File.Exists(_filePath))
                {
                    string json = System.IO.File.ReadAllText(_filePath);
                    List<LoginModel> users = Newtonsoft.Json.JsonConvert.DeserializeObject<List<LoginModel>>(json);
                    return users;
                }
                else
                {
                    ViewBag.ErrorMessage = "File not found.";
                    return new List<LoginModel>();
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"An error occurred: {ex.Message}";
                return new List<LoginModel>();
            }
        }

        private void ToggleAdminStatusByEmail(string email)
        {
            try
            {
                List<LoginModel> users = GetUserList();

                // Find the user with the specified email
                var userToUpdate = users.Find(u => u.Email == email);

                if (userToUpdate != null)
                {
                    // Toggle the isAdmin property
                    userToUpdate.isAdmin = !userToUpdate.isAdmin;

                    // Serialize the updated list back to JSON and overwrite the file
                    string updatedJson = JsonConvert.SerializeObject(users, Formatting.Indented);
                    System.IO.File.WriteAllText(_filePath, updatedJson);

                    ViewBag.SuccessMessage = $"Admin status for user with email '{email}' toggled successfully.";
                }
                else
                {
                    ViewBag.ErrorMessage = $"User with email '{email}' not found.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"An error occurred: {ex.Message}";
            }
        }

        [HttpPost]
        public IActionResult Login(LoginModel model)
        {
            if (LoginModel.checkCredential(model))
            {
                TempData["LoginWarningMessage"] = null;
                LoginModel currentUser = LoginModel.getUserByEmail(model.Email);
                var data = new LoginModel
                {
                    Email = model.Email,
                    Username = currentUser.Username, 
                    Password = currentUser.Password,
                    isAdmin = currentUser.isAdmin
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
            model.isAdmin = false;
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
