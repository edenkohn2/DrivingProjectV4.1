using Microsoft.AspNetCore.Identity;
using MySql.Data.MySqlClient;
using APIDrivingProject.Models;
using DrivingProjectSharedModels.Models;
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

        // הוספת אדם לטבלת Person
        public async Task<int> AddPerson(Person person)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var query = "INSERT INTO Person (FirstName, LastName, Email, PhoneNumber, Role) VALUES (@FirstName, @LastName, @Email, @PhoneNumber, @Role)";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FirstName", person.FirstName);
                    command.Parameters.AddWithValue("@LastName", person.LastName);
                    command.Parameters.AddWithValue("@Email", person.Email);
                    command.Parameters.AddWithValue("@PhoneNumber", person.PhoneNumber);
                    command.Parameters.AddWithValue("@Role", person.Role);
                    await command.ExecuteNonQueryAsync();
                    return (int)command.LastInsertedId; // מחזיר את מזהה האדם שנוסף
                }
            }
        }

        // הוספת סטודנט עם קשר ל-PersonId
        public async Task AddStudent(Student student)
        {
            int personId = await AddPerson(student);
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var query = "INSERT INTO Student (StudentId, LessonsTaken, HasPassedTheory) VALUES (@StudentId, @LessonsTaken, @HasPassedTheory)";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StudentId", personId);
                    command.Parameters.AddWithValue("@LessonsTaken", student.LessonsTaken);
                    command.Parameters.AddWithValue("@HasPassedTheory", student.HasPassedTheory);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        // הוספת מורה עם קשר ל-PersonId
        public async Task AddInstructor(Instructor instructor)
        {
            int personId = await AddPerson(instructor);
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var query = "INSERT INTO Instructor (InstructorId, ExperienceYears) VALUES (@InstructorId, @ExperienceYears)";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@InstructorId", personId);
                    command.Parameters.AddWithValue("@ExperienceYears", instructor.ExperienceYears);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        // הוספת מנהל עם קשר ל-PersonId
        public async Task AddAdmin(Admin admin)
        {
            int personId = await AddPerson(admin);
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var query = "INSERT INTO Admin (AdminId, AdminRole) VALUES (@AdminId, @AdminRole)";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AdminId", personId);
                    command.Parameters.AddWithValue("@AdminRole", admin.AdminRole);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        // קבלת כל האנשים מסוג Role מסוים
        public async Task<List<Person>> GetPeopleByRole(string role)
        {
            var people = new List<Person>();
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var query = "SELECT * FROM Person WHERE Role = @Role";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Role", role);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var person = new Person
                            {
                                PersonId = reader.GetInt32("PersonId"),
                                FirstName = reader.GetString("FirstName"),
                                LastName = reader.GetString("LastName"),
                                Email = reader.GetString("Email"),
                                PhoneNumber = reader.GetString("PhoneNumber"),
                                Role = reader.GetString("Role")
                            };
                            people.Add(person);
                        }
                    }
                }
            }
            return people;
        }
        public async Task<List<Student>> GetAllStudents()
        {
            var students = new List<Student>();

            using var connection = GetConnection();
            await connection.OpenAsync();

            var query = @"SELECT s.StudentId, s.LessonsTaken, s.HasPassedTheory,
                         p.FirstName, p.LastName, p.Email, p.PhoneNumber, p.Role
                  FROM student s
                  INNER JOIN person p ON s.StudentId = p.PersonId";

            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var student = new Student
                {
                    PersonId = reader.GetInt32("StudentId"),
                    LessonsTaken = reader.GetInt32("LessonsTaken"),
                    HasPassedTheory = reader.GetBoolean("HasPassedTheory"),
                    FirstName = reader.GetString("FirstName"),
                    LastName = reader.GetString("LastName"),
                    Email = reader.GetString("Email"),
                    PhoneNumber = reader.GetString("PhoneNumber"),
                    Role = reader.GetString("Role")
                };
                students.Add(student);
            }
            return students;
        }


        // פונקציות קיימות (לדוגמה: GetAllStudents, UserExists) יכולות להתעדכן באותו אופן במידת הצורך.
    }
}
