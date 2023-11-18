using System.Data;
using System.Data.OleDb;
using Newtonsoft.Json;

namespace Curriculum_Info_Application.Models
{
    public class LoginModel
    {
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool isAdmin { get; set; }
        private static readonly string _filePath = SystemConstant.USER_FILEPATH;

        public static bool insertNewUser(LoginModel info)
        {
            try
            {
                List<LoginModel> users;

                // Read the existing JSON data from the file
                if (File.Exists(_filePath))
                {
                    string json = File.ReadAllText(_filePath);
                    users = JsonConvert.DeserializeObject<List<LoginModel>>(json);
                }
                else
                {
                    users = new List<LoginModel>();
                }

                // Add the new user to the list
                users.Add(info);

                // Serialize the list back to JSON and write it to the file
                string updatedJson = JsonConvert.SerializeObject(users, Formatting.Indented);
                File.WriteAllText(_filePath, updatedJson);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool checkCredential(LoginModel info)
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    string json = File.ReadAllText(_filePath);
                    List<LoginModel> users = JsonConvert.DeserializeObject<List<LoginModel>>(json);

                    // Check if user credentials match any user in the list
                    return users.Any(user => user.Email == info.Email && user.Password == info.Password);
                }

                return false; // File not found, no matching user
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool valiadateSignup(LoginModel info)
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    string json = File.ReadAllText(_filePath);
                    List<LoginModel> users = JsonConvert.DeserializeObject<List<LoginModel>>(json);

                    // Check if user credentials match any user in the list
                    return users.Any(user => user.Email == info.Email);
                }

                return false; // File not found, no matching user
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public static LoginModel getUserByEmail(string email)
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    string json = File.ReadAllText(_filePath);
                    List<LoginModel> users = JsonConvert.DeserializeObject<List<LoginModel>>(json);

                    // Find the user with the specified email
                    var user = users.Find(u => u.Email == email);

                    if (user != null)
                    {
                        return user;
                    }
                }

                return null; // User not found or file doesn't exist
            }
            catch (Exception ex)
            {
                return null; // Handle exceptions
            }
        }

        public static bool isAdminAcc()
        {
            try
            {
                string jsonContent = File.ReadAllText(SystemConstant.LOGIN_FILEPATH);
                LoginModel user = JsonConvert.DeserializeObject<LoginModel>(jsonContent);
                if(user.isAdmin == false)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                return false; // Handle exceptions
            }
        }

        public string GetCurrentUsername()
        {
            try
            {
                string jsonContent = File.ReadAllText(SystemConstant.LOGIN_FILEPATH);
                LoginModel user = JsonConvert.DeserializeObject<LoginModel>(jsonContent);

                return user.Username;
            }
            catch (Exception ex)
            {
                return null; // Handle exceptions
            }
        }

        public static string GetCurrentEmail()
        {
            try
            {
                string jsonContent = File.ReadAllText(SystemConstant.LOGIN_FILEPATH);
                LoginModel user = JsonConvert.DeserializeObject<LoginModel>(jsonContent);

                return user.Email;
            }
            catch (Exception ex)
            {
                return null; // Handle exceptions
            }
        }

        public static bool DeleteUserByEmail(string email)
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    string json = File.ReadAllText(_filePath);
                    List<LoginModel> users = JsonConvert.DeserializeObject<List<LoginModel>>(json);

                    // Find the user with the specified email
                    var userToRemove = users.Find(u => u.Email == email);

                    if (userToRemove != null)
                    {
                        users.Remove(userToRemove); // Remove the user from the list

                        // Serialize the updated list back to JSON and overwrite the file
                        string updatedJson = JsonConvert.SerializeObject(users, Formatting.Indented);
                        File.WriteAllText(_filePath, updatedJson);

                        return true; // User deleted successfully
                    }
                }

                return false; // User not found or file doesn't exist
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return false;
            }
        }

        public static bool UpdateUserAdminStatus(string email, bool isAdmin)
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    string json = File.ReadAllText(_filePath);
                    List<LoginModel> users = JsonConvert.DeserializeObject<List<LoginModel>>(json);

                    // Find the user with the specified email
                    var userToUpdate = users.Find(u => u.Email == email);

                    if (userToUpdate != null)
                    {
                        // Update the isAdmin property
                        userToUpdate.isAdmin = isAdmin;

                        // Serialize the updated list back to JSON and overwrite the file
                        string updatedJson = JsonConvert.SerializeObject(users, Formatting.Indented);
                        File.WriteAllText(_filePath, updatedJson);

                        return true; // User updated successfully
                    }
                }

                return false; // User not found or file doesn't exist
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return false;
            }
        }

    }
}
