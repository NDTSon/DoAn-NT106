using System;
using System.Data.SqlClient;
using BCrypt.Net;

namespace Server
{
    public class DatabaseHelper
    {
        private string connectionString = @"Data Source=localhost\SQLEXPRESS;Initial Catalog=UserDB;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";

        public bool ValidateUser(string username, string password)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT Password FROM Users WHERE Username = @user";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@user", username);
                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            string dbPasswordHash = result.ToString();
                            // Kiểm tra mật khẩu băm
                            return BCrypt.Net.BCrypt.Verify(password, dbPasswordHash);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Lỗi ValidateUser: " + ex.Message);
                }
            }
            return false;
        }

        public bool RegisterUser(string username, string password, string fullname, string question, string answer)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = @user";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@user", username);
                        int count = (int)checkCmd.ExecuteScalar();
                        if (count > 0) return false;
                    }

                    string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

                    string insertQuery = "INSERT INTO Users (Username, Password, FullName, SecurityQuestion, SecurityAnswer) VALUES (@user, @pass, @name, @quest, @ans)";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@user", username);
                        cmd.Parameters.AddWithValue("@pass", passwordHash);

                        cmd.Parameters.AddWithValue("@name", string.IsNullOrEmpty(fullname) ? (object)DBNull.Value : fullname);
                        cmd.Parameters.AddWithValue("@quest", string.IsNullOrEmpty(question) ? (object)DBNull.Value : question);
                        cmd.Parameters.AddWithValue("@ans", string.IsNullOrEmpty(answer) ? (object)DBNull.Value : answer);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Lỗi RegisterUser DB: " + ex.Message);
                    return false;
                }
            }
        }
    }
}