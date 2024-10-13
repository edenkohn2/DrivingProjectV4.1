using APIDrivingProject.Models;

namespace APIDrivingProject.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<RegisterResult> Register(RegisterModel registerModel)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Students/register", registerModel);
            string Error = "";

            if (response.IsSuccessStatusCode)
            {
                // Ensure that the response contains valid JSON
                var content = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(content))
                {
                    return new RegisterResult { Successful = false,  };
                }

                // Deserializing the content to RegisterResult
                return await response.Content.ReadFromJsonAsync<RegisterResult>();
            }
            else
            {
                // Handle error case if the request fails
                return new RegisterResult { Successful = false,  };
            }
        }


        public async Task<LoginResult> Login(LoginModel loginModel)
        {
            var response = await _httpClient.PostAsJsonAsync("api/accounts/login", loginModel);
            return await response.Content.ReadFromJsonAsync<LoginResult>();
        }
    }

}
