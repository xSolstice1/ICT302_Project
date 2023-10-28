using System.Data;
using System.Data.OleDb;

namespace Curriculum_Info_Application.Models
{
    public class LoginModel
    {
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public bool isLogin { get; set; }

        public static bool insertNewUser(string connection, LoginModel info)
        {
            OleDbConnection conn = new OleDbConnection(connection);

            try
            {
                conn.Open();

                OleDbCommand cmdInsert = new OleDbCommand("INSERT INTO DCUSER (USER_EMAIL, USER_NAME, USER_PASSWORD) VALUES " +
                        "('" + info.Email + "', '" + info.Username + "', '" + info.Password + "');", conn);

                // execute
                int records = cmdInsert.ExecuteNonQuery();


                if (records > 0)
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                conn.Close();
            }
        }

        public static bool checkCredential(string connection, LoginModel info)
        {
            OleDbConnection conn = new OleDbConnection(connection);

            try
            {
                conn.Open();

                OleDbCommand cmd_Query = new OleDbCommand("Select * from DCUSER where USER_EMAIL = '" + info.Email + "'" +
                    "AND USER_PASSWORD = '" + info.Password + "';", conn);

                OleDbDataAdapter adapter = new OleDbDataAdapter(cmd_Query);
                DataTable dt = new DataTable();
                int result = adapter.Fill(dt);

                // check if record exist in database
                if (result > 0)
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                conn.Close();
            }
        }

        public static string getUsernameByEmail(string connection, LoginModel info)
        {
            OleDbConnection conn = new OleDbConnection(connection);

            try
            {
                conn.Open();

                OleDbCommand cmd_Query = new OleDbCommand("Select USER_NAME from DCUSER where USER_EMAIL = '" + info.Email + "';", conn);

                OleDbDataAdapter adapter = new OleDbDataAdapter(cmd_Query);
                DataTable dt = new DataTable();
                int result = adapter.Fill(dt);

                // check if record exist in database
                if (result > 0)
                {
                    return dt.Rows[0]["USER_NAME"].ToString();
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                conn.Close();
            }
        }
    }
}
