using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net.Http;

namespace BlazorDriveApp.Models
{
    public static class RegistrationValidator
    {
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            // ביטוי רגולרי לבדיקת פורמט אימייל
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return emailRegex.IsMatch(email);
        }
        // Validate birth date for minimum age of 16.5 years
        public static bool IsBirthDateValid(DateTime birthDate)
        {
            DateTime minimumDate = DateTime.Now.AddYears(-16).AddMonths(-6);
            return birthDate <= minimumDate;
        }

        // Validate password complexity
        public static bool IsPasswordValid(string password)
        {
            return password.Length >= 8 &&
                   password.Any(char.IsUpper) &&
                   password.Any(char.IsLower) &&
                   password.Any(char.IsDigit) &&
                   password.Any(ch => "!@#$%^&*".Contains(ch));
        }

        // Check if passwords match
        public static bool DoPasswordsMatch(string password, string confirmPassword)
        {
            return password == confirmPassword;
        }

        // Check if email format is valid
        public static bool IsEmailValid(string email)
        {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return emailRegex.IsMatch(email);
        }

        // Check if email is unique by calling API
        public static async Task<bool> IsEmailUniqueAsync(string email, HttpClient httpClient)
        {
            try
            {
                var response = await httpClient.GetAsync($"api/users/check-email?email={Uri.EscapeDataString(email)}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    // ניתוח תוכן התשובה כדי לבדוק אם האימייל ייחודי
                    return bool.TryParse(content, out bool isUnique) && isUnique;
                }
                else
                {
                    Console.WriteLine($"Error: Unable to validate email uniqueness. Status code: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred during email uniqueness check: {ex.Message}");
                return false; // Consider email not unique if there's an exception
            }
        }


    }
}
