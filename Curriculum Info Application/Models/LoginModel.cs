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
        private static readonly string _filePath = "user.json";

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


        public static string getUsernameByEmail(string email)
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
                        return user.Username;
                    }
                }

                return null; // User not found or file doesn't exist
            }
            catch (Exception ex)
            {
                return null; // Handle exceptions
            }
        }
    }
}
