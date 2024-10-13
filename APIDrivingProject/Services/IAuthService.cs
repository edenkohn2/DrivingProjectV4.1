using APIDrivingProject.Models;

namespace APIDrivingProject.Services
{
    public interface IAuthService
    {
        Task<RegisterResult> Register(RegisterModel registerModel);
        Task<LoginResult> Login(LoginModel loginModel);
    }

}
