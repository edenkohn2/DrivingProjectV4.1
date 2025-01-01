using APIDrivingProject.Models;
using DrivingProjectSharedModels.Models;
using APIDrivingProject.Services;
using MySql.Data.MySqlClient;
using System.Data;

public class UserService
{
    private readonly DatabaseService _databaseService;

    public UserService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task<Person> Login(string email, string password)
    {
        using var connection = _databaseService.GetConnection();
        await connection.OpenAsync();

        // עדכון השם לשם הנכון של עמודת הסיסמה
        var query = @"SELECT * FROM person 
                  WHERE Email = @Email AND PasswordHash = @Password";
        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@Email", email);
        command.Parameters.AddWithValue("@Password", password); // השם הנכון במקום PasswordHash

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Person
            {
                PersonId = reader.GetInt32("PersonId"),
                FirstName = reader.GetString("FirstName"),
                LastName = reader.GetString("LastName"),
                Email = reader.GetString("Email"),
                PasswordHash = reader.GetString("PasswordHash"), // השם הנכון של עמודת הסיסמה
                PhoneNumber = reader.GetString("PhoneNumber"),
                Address = reader.GetString("Address"),
                BirthDate = reader.GetDateTime("BirthDate"),
                Role = reader.GetString("Role")
            };
        }
        return null;
    }


    public async Task<List<Student>> GetStudentsForInstructor(int instructorId)
    {
        var students = new List<Student>();

        using var connection = _databaseService.GetConnection();
        await connection.OpenAsync();

        var query = @"
        SELECT s.StudentId, p.FirstName, p.LastName, p.Email, p.PhoneNumber, s.LessonsTaken, s.HasPassedTheory
        FROM lessons l
        INNER JOIN student s ON l.StudentId = s.StudentId
        INNER JOIN person p ON s.StudentId = p.PersonId
        WHERE l.InstructorId = @InstructorId";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@InstructorId", instructorId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var student = new Student
            {
                PersonId = reader.GetInt32("StudentId"),
                FirstName = reader.GetString("FirstName"),
                LastName = reader.GetString("LastName"),
                Email = reader.GetString("Email"),
                PhoneNumber = reader.GetString("PhoneNumber"),
                LessonsTaken = reader.GetInt32("LessonsTaken"),
                HasPassedTheory = reader.GetBoolean("HasPassedTheory")
            };
            students.Add(student);
        }

        return students;
    }


    public async Task<bool> RegisterPerson(Person person)
    {
        using var connection = _databaseService.GetConnection();
        await connection.OpenAsync();

        // שלב 1: הוספת הנתונים לטבלת person
        var queryPerson = @"INSERT INTO person (FirstName, LastName, Email, PasswordHash, PhoneNumber, Address, BirthDate, Role)
                        VALUES (@FirstName, @LastName, @Email, @PasswordHash, @PhoneNumber, @Address, @BirthDate, @Role)";
        using var commandPerson = new MySqlCommand(queryPerson, connection);
        commandPerson.Parameters.AddWithValue("@FirstName", person.FirstName);
        commandPerson.Parameters.AddWithValue("@LastName", person.LastName);
        commandPerson.Parameters.AddWithValue("@Email", person.Email);
        commandPerson.Parameters.AddWithValue("@PasswordHash", person.PasswordHash);
        commandPerson.Parameters.AddWithValue("@PhoneNumber", person.PhoneNumber);
        commandPerson.Parameters.AddWithValue("@Address", person.Address);
        commandPerson.Parameters.AddWithValue("@BirthDate", person.BirthDate);
        commandPerson.Parameters.AddWithValue("@Role", person.Role);

        var result = await commandPerson.ExecuteNonQueryAsync();
        if (result <= 0)
        {
            return false; // אם ההוספה ל-person נכשלה
        }

        // שלב 2: שליפת ה-ID שנוסף בטבלת person
        var personId = (int)commandPerson.LastInsertedId;

        // שלב 3: הוספת הנתונים לטבלה היורשת
        if (person.Role == "Student")
        {
            var queryStudent = @"INSERT INTO student (StudentId, LessonsTaken, HasPassedTheory)
                             VALUES (@StudentId, @LessonsTaken, @HasPassedTheory)";
            using var commandStudent = new MySqlCommand(queryStudent, connection);
            commandStudent.Parameters.AddWithValue("@StudentId", personId);
            commandStudent.Parameters.AddWithValue("@LessonsTaken", 0); // ברירת מחדל
            commandStudent.Parameters.AddWithValue("@HasPassedTheory", false); // ברירת מחדל
            await commandStudent.ExecuteNonQueryAsync();
        }
        else if (person.Role == "Instructor")
        {
            var queryInstructor = @"INSERT INTO instructor (InstructorId, ExperienceYears)
                                VALUES (@InstructorId, @ExperienceYears)";
            using var commandInstructor = new MySqlCommand(queryInstructor, connection);
            commandInstructor.Parameters.AddWithValue("@InstructorId", personId);
            commandInstructor.Parameters.AddWithValue("@ExperienceYears", 0); // ברירת מחדל
            await commandInstructor.ExecuteNonQueryAsync();
        }

        return true;
    }

    public MySqlConnection GetDatabaseConnection()
    {
        return _databaseService.GetConnection();
    }
}
