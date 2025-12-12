using System;
using BCrypt.Net;
using System.Data.SQLite;


namespace Server
{
    public class DatabaseHelper
    {
        private readonly string connectionString = "Data Source=User.db;Version=3";
        
        private readonly SQLiteConnection connection;

        public DatabaseHelper()
        {
            connection = new SQLiteConnection(connectionString);
            try
            {
                connection.Open();
                string Create_User = @"CREATE TABLE IF NOT EXISTS Users (
                                                Username TEXT PRIMARY KEY,
                                                Password TEXT NOT NULL,
                                                FullName TEXT,
                                                SecurityQuestion TEXT,
                                                SecurityAnswer TEXT
                                            );";
                using (var CreateUser = new SQLiteCommand(Create_User, connection))
                {
                   CreateUser.ExecuteNonQuery();
                }

                string adminPass = "123456";
                string adminHash = BCrypt.Net.BCrypt.HashPassword(adminPass);

                string insert_admin = "INSERT OR IGNORE INTO Users (Username, Password, FullName) VALUES ('admin', @pass, 'admin');";
                using (var insertCommand = new SQLiteCommand(insert_admin, connection))
                {
                    insertCommand.Parameters.AddWithValue("@pass", adminHash);
                    insertCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi kết nối DB: " + ex.Message);
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }


        public bool CheckUserExists(string username)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT Username FROM Users WHERE Username = @user";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@user", username);
                        object result = cmd.ExecuteScalar();
                        return result != null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Lỗi CheckUserExists: " + ex.Message);
                    return false;
                }
            }
        }
        public bool UpdatePassword(string username, string newPassword)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string passwordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

                    string query = "UPDATE Users SET Password = @pass WHERE Username = @user";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@user", username);
                        cmd.Parameters.AddWithValue("@pass", passwordHash);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Lỗi UpdatePassword: " + ex.Message);
                    return false;
                }
            }
        }
        public bool ValidateUser(string username, string password)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT Password FROM Users WHERE Username = @user";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@user", username);
                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            string dbPasswordHash = result.ToString();
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
        public string GetSecurityQuestion(string username)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT SecurityQuestion FROM Users WHERE Username = @user";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@user", username);
                        object result = cmd.ExecuteScalar();
                        return result != null ? result.ToString() : null;
                    }
                }
                catch { return null; }
            }
        }
        public bool VerifySecurityAnswer(string username, string answer)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT SecurityAnswer FROM Users WHERE Username = @user";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@user", username);
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            string dbAnswer = result.ToString();
                            return dbAnswer.Equals(answer, StringComparison.OrdinalIgnoreCase);
                        }
                    }
                }
                catch { }
            }
            return false;
        }
        public bool RegisterUser(string username, string password, string fullname, string question, string answer)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = @user";
                    using (SQLiteCommand checkCmd = new SQLiteCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@user", username);
                        long count = (long)checkCmd.ExecuteScalar();
                        if (count > 0) return false;
                    }

                    string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

                    string insertQuery = "INSERT INTO Users (Username, Password, FullName, SecurityQuestion, SecurityAnswer) VALUES (@user, @pass, @name, @quest, @ans)";

                    using (SQLiteCommand cmd = new SQLiteCommand(insertQuery, conn))
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