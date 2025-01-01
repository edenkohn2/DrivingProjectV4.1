using APIDrivingProject.Models;
using DrivingProjectSharedModels.Models;

namespace APIDrivingProject.Services
{
    public class AuthService
    {
        private string _userName;
        private string _userRole;
        private int _userId;

        public void SetUser(string userName, string userRole, int userId)
        {
            _userName = userName;
            _userRole = userRole;
            _userId = userId;

            Console.WriteLine($"Setting user: {_userName}, Role: {_userRole}, UserId: {_userId}");
        }

        public void ClearUser()
        {
            _userName = null;
            _userRole = null;
        }

        public bool IsAuthenticated => !string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(UserRole);
        public bool IsAdmin => _userRole == "Admin";
        public bool IsInstructor => _userRole == "Instructor";
        public bool IsStudent => _userRole == "Student";
        public int UserId => _userId;

        public string UserName => _userName;
        public string UserRole => _userRole;

        public void Logout()
        {
            ClearUser();
        }
    }
}
