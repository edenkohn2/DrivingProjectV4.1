namespace APIDrivingProject.Services
{
    public class AuthService
    {
        private string _userName;
        private string _userRole;

        public void SetUser(string userName, string userRole)
        {
            _userName = userName;
            _userRole = userRole;
        }

        public void ClearUser()
        {
            _userName = null;
            _userRole = null;
        }

        public bool IsAuthenticated => !string.IsNullOrEmpty(_userName);
        public bool IsAdmin => _userRole == "Admin";
        public bool IsInstructor => _userRole == "Instructor";
        public bool IsStudent => _userRole == "Student";
        public bool IsLoggedIn => IsAuthenticated;

        public string UserName => _userName;
        public string UserRole => _userRole;
        public void Logout()
        {
            ClearUser();
        }
    }
}
