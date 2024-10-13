using Microsoft.AspNetCore.Identity;
using MySql.Data.MySqlClient;
using APIDrivingProject.Models;
using System.Data;
namespace APIDrivingProject.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }


        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }
        public string HashPassword(string password)
        {
            var passwordHasher = new PasswordHasher<User>();
            return passwordHasher.HashPassword(null, password);
        }

        // Verify password
        public bool VerifyPassword(string hashedPassword, string password)
        {
            var passwordHasher = new PasswordHasher<User>();
            return passwordHasher.VerifyHashedPassword(null, hashedPassword, password) == PasswordVerificationResult.Success;
        }

        // Check if a user exists by email
        public async Task<bool> UserExists(string email)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var query = "SELECT COUNT(1) FROM users WHERE Email = @Email";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    var count = (long)await command.ExecuteScalarAsync();
                    return count > 0;
                }
            }
        }


        // Add a new user to the database
        public async Task AddUser(User user)
        {
            try
            {
                // הכנס את המשתמש לדאטהבייס
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = "INSERT INTO users (UserName, Email, PasswordHash, Role) VALUES (@UserName, @Email, @PasswordHash, @Role)";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserName", user.UserName);
                        command.Parameters.AddWithValue("@Email", user.Email);
                        command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                        command.Parameters.AddWithValue("@Role", user.Role);
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                // הוסף לוג כדי לזהות בעיות
                Console.WriteLine($"Error inserting user: {ex.Message}");
                throw; // כדי להחזיר שגיאה ל-API
            }
        }


        // Get user by email
        public async Task<User> GetUserByEmail(string email)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var query = "SELECT * FROM Users WHERE Email = @Email";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new User
                            {
                                UserId = reader.GetInt32("UserId"),
                                UserName = reader.GetString("UserName"),
                                Email = reader.GetString("Email"),
                                PasswordHash = reader.GetString("PasswordHash"),
                                Role = reader.GetString("Role")
                            };
                        }
                    }
                }
            }
            return null;
        }

        // Get all users
        public async Task<List<User>> GetAllUsers()
        {
            var users = new List<User>();
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var query = "SELECT * FROM Users";
                using (var command = new MySqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var user = new User
                            {
                                UserId = reader.GetInt32("UserId"),
                                UserName = reader.GetString("UserName"),
                                Email = reader.GetString("Email"),
                                PasswordHash = reader.GetString("PasswordHash"),
                                Role = reader.GetString("Role")
                            };
                            users.Add(user);
                        }
                    }
                }
            }
            return users;
        }
        public async Task<bool> StudentExists(string email)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var query = "SELECT COUNT(1) FROM Students WHERE Email = @Email";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    var count = (long)await command.ExecuteScalarAsync();
                    return count > 0;
                }
            }
        }


        public async Task AddStudent(Student student)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var query = "INSERT INTO Students (FirstName, LastName, Email, PhoneNumber, LessonsTaken, HasPassedTheory) VALUES (@FirstName, @LastName, @Email, @PhoneNumber, @LessonsTaken, @HasPassedTheory)";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FirstName", student.FirstName);
                    command.Parameters.AddWithValue("@LastName", student.LastName);
                    command.Parameters.AddWithValue("@Email", student.Email);
                    command.Parameters.AddWithValue("@PhoneNumber", student.PhoneNumber);
                    command.Parameters.AddWithValue("@LessonsTaken", student.LessonsTaken);
                    command.Parameters.AddWithValue("@HasPassedTheory", student.HasPassedTheory);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
        public async Task<List<Student>> GetAllStudents()
        {
            var students = new List<Student>();

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var query = "SELECT * FROM Students";
                using (var command = new MySqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var student = new Student
                            {
                                StudentId = reader.GetInt32("StudentId"),
                                FirstName = reader.GetString("FirstName"),
                                LastName = reader.GetString("LastName"),
                                Email = reader.GetString("Email"),
                                PhoneNumber = reader.GetString("PhoneNumber"),
                                LessonsTaken = reader.GetInt32("LessonsTaken"),
                                HasPassedTheory = reader.GetBoolean("HasPassedTheory")
                            };

                            students.Add(student);
                        }
                    }
                }
            }

            return students;
        }




    }

}
